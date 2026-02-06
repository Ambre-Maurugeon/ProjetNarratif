using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

    public class DSGraphView : UnityEditor.Experimental.GraphView.GraphView
    {
        private DSEditorWindow editorWindow;
        private DSSearchWindow searchWindow;

        private MiniMap miniMap;
        private SerializableDictionary<string, DSNodeErrorData> ungroupedNodes;
        private SerializableDictionary<string, DSGroupErrorData> groups;
        private SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>> groupedNodes;

        private readonly List<string> _existingNames = new List<string>();
        private int _nameErrorsAmount;
        

        // PROPRIÉTÉ QUI GÈRE LE NOMBRE D'ERREURS DE NOMMAGE (NODES ET GROUPS) // SI IL Y A 0 ERREUR ON ACTIVE LE BOUTON DE SAUVEGARDE, SI IL Y A 1 ERREUR ON LE DÉSACTIVE //
        public int NameErrorsAmount
        {
            get => _nameErrorsAmount;
            set
            {
                _nameErrorsAmount = value;

                if (_nameErrorsAmount == 0)
                {
                    editorWindow.EnableSaving();
                }
                if (_nameErrorsAmount == 1)
                {
                    editorWindow.DisableSaving();
                }
            }
        }

        // CONSTRUCTEUR DU GRAPHVIEW // S'APPELLE LORS DE LA CRÉATION DE LA FENÊTRE D'ÉDITION //
        public DSGraphView(DSEditorWindow dsEditorWindow)
        {
            editorWindow = dsEditorWindow;

            ungroupedNodes = new SerializableDictionary<string, DSNodeErrorData>();
            groups = new SerializableDictionary<string, DSGroupErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>>();

            AddManipulators();
            AddGridBackground();
            AddSearchWindow();
            AddMiniMap();

            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            AddStyles();
            AddMiniMapStyles();
        }
        

        // MÉTHODE DE VÉRIFICATION DE LA COMPATIBILITÉ DES PORTS (ENTRÉE/SORTIE) //
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            // ON PARCOURS TOUS LES PORTS DU GRAPHVIEW // SI LE PORT DE DÉPART EST LE MÊME QUE CELUI PARCOURU ON CONTINUE, SI LE NODE EST LE MÊME ON CONTINUE, SI LA DIRECTION EST LA MÊME ON CONTINUE, SINON ON AJOUTE LE PORT PARCOURU À LA LISTE DES PORTS COMPATIBLES (c long mais important mdr) //
            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return;
                }

                if (startPort.node == port.node)
                {
                    return;
                }

                if (startPort.direction == port.direction)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }
        
        public List<string> GetExistingNames()
        {
            return _existingNames;
        }

        // MÉTHODE D'AJOUT DES MANIPULATEURS (ZOOM, DRAG, CONTEXTUAL MENU, ETC) // DONC ACTION AFFICHER QUAND ON CLIQUE SOURIS DANS LE GRAPH WIEW //
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Dialogue Node", DSDialogueType.MultipleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Start Node",DSDialogueType.Start));
            this.AddManipulator(CreateNodeContextualMenu("End Node",DSDialogueType.End));
            this.AddManipulator(CreateNodeContextualMenu("Branch Node",DSDialogueType.Branch));
 
            this.AddManipulator(CreateGroupContextualMenu());
        }

        // MÉTHODE DE CRÉATION DE "CONTEXTUAL MENU" POUR LES NODES //
        private IManipulator CreateNodeContextualMenu(string actionTitle, DSDialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode("DialogueName", dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );

            return contextualMenuManipulator;
        }
        // MÉTHODE DE CRÉATION DE "CONTEXTUAL MENU" POUR LES GROUPS //
        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("DialogueGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
            );
            return contextualMenuManipulator;
        }
        
        // MÉTHODE DE CRÉATION DE GROUP ! // parametres : nom du group, position du group (mouse pos) //
        public DSGroup CreateGroup(string title, Vector2 position)
        {
            DSGroup group = new DSGroup(title, position);

            AddGroup(group);

            AddElement(group);

            foreach (GraphElement selectedElement in selection)
            {
                if (!(selectedElement is DSNode))
                {
                    continue;
                }

                DSNode node = (DSNode) selectedElement;

                group.AddElement(node);
            }
            return group;
        }


        public List<string> GetUnGroupedNodesNames()
        {
            List<string> names = new List<string>();
            foreach(var kvp in ungroupedNodes)
            {
                names.Add(kvp.Key);
            }
            return names;
        }
        public void ClearExistingNames()
        {
            _existingNames.Clear();
        }
        
        // MÉTHODE DE CRÉATION DE NODE ! // parametres : nom du node, type de dialogue *enum* (ATTENTION ! ira le chercher dans les fichiers du jeu), position du node (mouse pos), doit-on le dessiner directement (par défaut oui pas besoin de changer ça imo) //
        public DSNode CreateNode(string nodeName, DSDialogueType dialogueType, Vector2 position, bool shouldDraw = true)
        {

            if (dialogueType == DSDialogueType.Start)
            {
                foreach (var nod in nodes)
                {
                    if (nod is DSStartNode)
                    {
                        Debug.LogError("START NODE ALREADY EXISTS IN THE GRAPHVIEW");
                        return null;
                    }
                }
            }
            // ON S'ASSURE QUE LA TABLE DE DIALOGUE EST CHARGÉE // MAIS PAS GRAVE SI ON L'APPELLE PLUSIEURS FOIS ELLE NE SE CHARGE QU'UNE FOIS (promis (normalement :) )) //
            
            FantasyDialogueTable.Load();
            
            // ON RÉCUPÈRE LE TYPE DE NODE EN FONCTION DU TYPE DE DIALOGUE PASSÉ EN PARAMÈTRE // DONC ATTENTION À BIEN NOMMER LES FICHIERS DE NODE COMME CECI : DS + NOM DU DIALOGUE + Node (exemple : DSStartNode, DSEndNode, DSMultipleChoiceNode) //
            Type nodeType = Type.GetType($"DS{dialogueType}Node");
            if (nodeType == null)
            {
                throw new Exception($"Node type for dialogue type {dialogueType} not found");
            }
            
            // ON CRÉE UNE INSTANCE DE CE TYPE //
           
            DSNode node = (DSNode) Activator.CreateInstance(nodeType);
            if (node == null)
            {
                throw new Exception($"Failed to create node of type {nodeType}");
            }
            
            if (string.IsNullOrEmpty(nodeName))
            {
                nodeName = "XXX";
            }
            nodeName.RemoveWhitespaces().RemoveSpecialCharacters();
            if(!_existingNames.Contains(nodeName.ToLower()))
            {
                _existingNames.Add(nodeName.ToLower());
            }
            else
            {
                int duplicateIndex = 1;
                string newNodeName;
                do
                {
                    newNodeName = $"{nodeName}_{duplicateIndex}";
                    duplicateIndex++;
                } while (_existingNames.Contains(newNodeName.ToLower()));
                nodeName = newNodeName;
                _existingNames.Add(nodeName.ToLower());
            }
            
            // ON INITIALISE LE NODE AVEC LE NOM, LA POSITION ET LA RÉFÉRENCE AU GRAPHVIEW ACTUEL //
            node.Initialize(nodeName, this, position);

            // ON DRAW LA FONCTION DU NODE (AJOUTE LES PORTS, CHAMPS, ETC) //
            if (shouldDraw)
            {
                node.Draw(new Color());
            }

            // ON AJOUTE LE NODE AU GRAPHVIEW //
            AddUngroupedNode(node);
            return node;
        }
        
        // MÉTHODE APPELÉE LORS DE LA SUPPRESSION D'ÉLÉMENTS DANS LE GRAPHVIEW //
        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(DSGroup);
                Type edgeType = typeof(Edge);

                List<DSGroup> groupsToDelete = new List<DSGroup>();
                List<DSNode> nodesToDelete = new List<DSNode>();
                List<Edge> edgesToDelete = new List<Edge>();

                foreach (GraphElement selectedElement in selection)
                {
                    if (selectedElement is DSNode node)
                    {
                        nodesToDelete.Add(node);

                        continue;
                    }

                    if (selectedElement.GetType() == edgeType)
                    {
                        Edge edge = (Edge) selectedElement;

                        edgesToDelete.Add(edge);

                        continue;
                    }

                    if (selectedElement.GetType() != groupType)
                    {
                        continue;
                    }

                    DSGroup group = (DSGroup) selectedElement;

                    groupsToDelete.Add(group);
                }

                foreach (DSGroup groupToDelete in groupsToDelete)
                {
                    List<DSNode> groupNodes = new List<DSNode>();

                    foreach (GraphElement groupElement in groupToDelete.containedElements)
                    {
                        if (!(groupElement is DSNode))
                        {
                            continue;
                        }

                        DSNode groupNode = (DSNode) groupElement;

                        groupNodes.Add(groupNode);
                    }

                    groupToDelete.RemoveElements(groupNodes);

                    RemoveGroup(groupToDelete);

                    RemoveElement(groupToDelete);
                }

                DeleteElements(edgesToDelete);

                foreach (DSNode nodeToDelete in nodesToDelete)
                {
                    if (nodeToDelete.Group != null)
                    {
                        nodeToDelete.Group.RemoveElement(nodeToDelete);
                    }

                    RemoveUngroupedNode(nodeToDelete);

                    nodeToDelete.DisconnectAllPorts();

                    RemoveElement(nodeToDelete);
                }
            };
        }
        
        // MÉTHODE APPELÉE LORS DE L'AJOUT D'ÉLÉMENTS DANS UN GROUPE //
        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DSNode))
                    {
                        continue;
                    }

                    DSGroup dsGroup = (DSGroup) group;
                    DSNode node = (DSNode) element;

                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, dsGroup);
                }
            };
        }
        
        // MÉTHODE APPELÉE LORS DE LA SUPPRESSION D'ÉLÉMENTS D'UN GROUPE //
        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DSNode))
                    {
                        continue;
                    }

                    DSGroup dsGroup = (DSGroup) group;
                    DSNode node = (DSNode) element;

                    RemoveGroupedNode(node, dsGroup);
                    AddUngroupedNode(node);
                }
            };
        }
        
        // MÉTHODE APPELÉE LORS DU RENOMMAGE D'UN GROUPE //
        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DSGroup dsGroup = (DSGroup) group;

                dsGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(dsGroup.title))
                {
                    if (!string.IsNullOrEmpty(dsGroup.OldTitle))
                    {
                        ++NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dsGroup.OldTitle))
                    {
                        --NameErrorsAmount;
                    }
                }

                RemoveGroup(dsGroup);

                dsGroup.OldTitle = dsGroup.title;

                AddGroup(dsGroup);
            };
        }

        // MÉTHODE APPELÉE LORS DE LA MODIFICATION DU GRAPHVIEW (AJOUT/SUPPRESSION DE LIENS ENTRE NODES) //
        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        DSNode nextNode = (DSNode) edge.input.node;
                        if (nextNode == null)
                        {
                            Debug.Log("Next node is null");
                            continue;
                        }
                        DSChoiceSaveData choiceData = (DSChoiceSaveData) edge.output.userData;
                        if (choiceData == null)
                        {
                            Debug.Log("Next node or choice data is null");
                            continue;
                        }
                        choiceData.NodeID = nextNode.ID;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);

                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                        {
                            continue;
                        }

                        Edge edge = (Edge) element;

                        DSChoiceSaveData choiceData = (DSChoiceSaveData) edge.output.userData;

                        choiceData.NodeID = "";
                    }
                }
                return changes;
            };
        }
        
        public string DoesNameExist(string name)
        {
            string lowerName = name.ToLower();
            if(_existingNames.Contains(lowerName))
            {
                // CHANGER LE NOM EN AJOUTANT UN SUFFIXE //
                return lowerName + $"_{_existingNames.Count}";
            }
            return null;
        }

        // MÉTHODE D'AJOUT D'UN NODE SANS GROUPE //
        public void AddUngroupedNode(DSNode node)
        {
            if(node == null)
            {
                throw new ArgumentNullException(nameof(node), "Node cannot be null");
            }
            if(string.IsNullOrEmpty(node.DialogueName))
            {
                return;
            }
            string nodeName = node.DialogueName.ToLower();

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();
                nodeErrorData.Nodes.Add(node);
                ungroupedNodes.Add(nodeName, nodeErrorData);
                return;
            }
           
            Debug.LogError("NODE WITH SAME NAME DETECTED");
        }

        // MÉTHODE DE SUPPRESSION D'UN NODE SANS GROUPE //
        public void RemoveUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Remove(node);

            node.ResetStyle();

            if (ungroupedNodesList.Count == 1)
            {
                --NameErrorsAmount;

                ungroupedNodesList[0].ResetStyle();

                return;
            }

            if (ungroupedNodesList.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }

        // MÉTHODE D'AJOUT D'UN GROUPE //
        private void AddGroup(DSGroup group)
        {
            string groupName = group.title.ToLower();

            if (!groups.ContainsKey(groupName))
            {
                DSGroupErrorData groupErrorData = new DSGroupErrorData();

                groupErrorData.Groups.Add(group);

                groups.Add(groupName, groupErrorData);

                return;
            }

            List<DSGroup> groupsList = groups[groupName].Groups;

            groupsList.Add(group);

            Color errorColor = groups[groupName].ErrorData.Color;

            group.SetErrorStyle(errorColor);

            if (groupsList.Count == 2)
            {
                ++NameErrorsAmount;

                groupsList[0].SetErrorStyle(errorColor);
            }
        }

        // MÉTHODE DE SUPPRESSION D'UN GROUPE //
        private void RemoveGroup(DSGroup group)
        {
            string oldGroupName = group.OldTitle.ToLower();

            List<DSGroup> groupsList = groups[oldGroupName].Groups;

            groupsList.Remove(group);

            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                --NameErrorsAmount;

                groupsList[0].ResetStyle();

                return;
            }

            if (groupsList.Count == 0)
            {
                groups.Remove(oldGroupName);
            }
        }

        // MÉTHODE D'AJOUT D'UN NODE AVEC GROUPE //
        public void AddGroupedNode(DSNode node, DSGroup group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = group;

            if (!groupedNodes.ContainsKey(group))
            {
                groupedNodes.Add(group, new SerializableDictionary<string, DSNodeErrorData>());
            }

            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                groupedNodes[group].Add(nodeName, nodeErrorData);

                return;
            }

            List<DSNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Add(node);

            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;

            node.SetErrorStyle(errorColor);

            if (groupedNodesList.Count == 2)
            {
                ++NameErrorsAmount;

                groupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        // MÉTHODE DE SUPPRESSION D'UN NODE AVEC GROUPE //
        public void RemoveGroupedNode(DSNode node, DSGroup group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = null;

            List<DSNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Remove(node);

            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                --NameErrorsAmount;

                groupedNodesList[0].ResetStyle();

                return;
            }

            if (groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }

        // MÉTHODE D'AJOUT DU FOND GRILLE //
        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();

            gridBackground.StretchToParentSize();

            Insert(0, gridBackground);
        }

        // MÉTHODE D'AJOUT DE LA FENÊTRE DE RECHERCHE //
        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<DSSearchWindow>();
            }

            searchWindow.Initialize(this);

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        // MÉTHODE D'AJOUT DE LA MINI MAP //
        private void AddMiniMap()
        {
            miniMap = new MiniMap()
            {
                anchored = true
            };

            miniMap.SetPosition(new Rect(15, 50, 200, 180));

            Add(miniMap);

            miniMap.visible = false;
        }

        // MÉTHODE D'AJOUT DES STYLES CSS //
        private void AddStyles()
        {
            this.AddStyleSheets("DialogueSystem/DSGraphViewStyles.uss", "DialogueSystem/DSNodeStyles.uss");
        }

        // MÉTHODE D'AJOUT DES STYLES CSS POUR LA MINI MAP //
        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            miniMap.style.backgroundColor = backgroundColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
        }

        // MÉTHODE DE RÉCUPÉRATION DE LA POSITION LOCALE DE LA SOURIS DANS LE GRAPHVIEW //
        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, mousePosition - editorWindow.position.position);
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }

        // MÉTHODE DE VIDAGE DU GRAPHVIEW //
        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));

            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();

            NameErrorsAmount = 0;
        }
        // MÉTHODE DE TOGGLE DE LA MINI MAP // LA MINIMAP EST FAITE PAR LE PACKAGE EN LIGNE (J'aurais jamais eu la foi de la faire moi même mdr, mais en vrai un peu useless on s'en fou imo) //
        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }

    }
