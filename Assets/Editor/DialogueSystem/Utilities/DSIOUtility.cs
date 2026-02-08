using System;
using System.Collections.Generic;
using System.Linq;
using DS.Utilities;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public static class DSIOUtility
    {
        private static DSGraphView graphView;

        private static string graphFileName;
        private static string containerFolderPath;

        private static List<DSNode> nodes = new List<DSNode>(); 
        private static List<DSGroup> groups;

        private static Dictionary<string, DSDialogueGroupSO> createdDialogueGroups;
        private static Dictionary<string, DSDialogueSO> createdDialogues;

        private static Dictionary<string, DSGroup> loadedGroups;
        private static Dictionary<string, DSNode> loadedNodes;
        
        /// <summary>
        /// LE SCRIPT LE PLUS IMPORTANT DU MONDE // IL CONTIENT BEAUCOUP DE HELPERS POUR SAUVEGARDER ET CHARGER LES GRAPHS DE DIALOGUE // AINSI QUE LES DIALOGUES ET GROUPES ASSOCIES ///
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        
        public static string CheckNameWithOthers(string name)
        {
            return name;
        }

        public static void Initialize(DSGraphView dsGraphView, string graphName)
        {
            graphView = dsGraphView;

            graphFileName = graphName;
            containerFolderPath = $"Assets/DialogueSystem/Dialogues/{graphName}";

            groups = new List<DSGroup>();

            createdDialogueGroups = new Dictionary<string, DSDialogueGroupSO>();
            createdDialogues = new Dictionary<string, DSDialogueSO>();

            loadedGroups = new Dictionary<string, DSGroup>();
            loadedNodes = new Dictionary<string, DSNode>();
        }

        public static void Save()
        {
            nodes.Clear();
            groups = new List<DSGroup>();
            
            createdDialogueGroups = new Dictionary<string, DSDialogueGroupSO>();
            createdDialogues = new Dictionary<string, DSDialogueSO>();

            CreateDefaultFolders();

            GetElementsFromGraphView();

            // Load or create asset
            DSGraphSaveDataSO graphData = CreateAsset<DSGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", $"{graphFileName}Graph");

            // Ensure asset lists are cleared before we repopulate them
            if (graphData.Nodes == null) graphData.Nodes = new List<DSNodeSaveData>();
            else graphData.Nodes.Clear();

            if (graphData.Groups == null) graphData.Groups = new List<DSGroupSaveData>();
            else graphData.Groups.Clear();

            // If graphData has old collections for names, clear them too (defensive)
            if (graphData.OldGroupNames == null) graphData.OldGroupNames = new List<string>();
            if (graphData.OldGroupedNodeNames == null) graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>();
            if (graphData.OldUngroupedNodeNames == null) graphData.OldUngroupedNodeNames = new List<string>();

            graphData.Initialize(graphFileName); // si Initialize réinitialise certains champs, ok

            DSDialogueContainerSO dialogueContainer = CreateAsset<DSDialogueContainerSO>(containerFolderPath, graphFileName);
            // clear dialogue container entries before repopulating (defensive)
            dialogueContainer.Initialize(graphFileName);
            
            
            SyncChoicesFromEdges_SingleTarget();

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);

            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }


        private static void SaveGroups(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();

            foreach (DSGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void SaveGroupToGraph(DSGroup group, DSGraphSaveDataSO graphData)
        {
            DSGroupSaveData groupData = new DSGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };

            graphData.Groups.Add(groupData);
        }

        private static void SaveGroupToScriptableObject(DSGroup group, DSDialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;

            CreateFolder($"{containerFolderPath}/Groups", groupName);
            CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");

            DSDialogueGroupSO dialogueGroup = CreateAsset<DSDialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);

            dialogueGroup.Initialize(groupName);

            createdDialogueGroups.Add(group.ID, dialogueGroup);

            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<DSDialogueSO>());

            SaveAsset(dialogueGroup);
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

                foreach (string groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{containerFolderPath}/Groups/{groupToRemove}");
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        private static void SaveNodes(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();

            foreach (DSNode node in nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);
                
                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.DialogueName);

                    continue;
                }
                ungroupedNodeNames.Add(node.DialogueName);
            }
            
            UpdateDialoguesChoicesConnections();

            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        private static void SaveNodeToGraph(DSNode node, DSGraphSaveDataSO graphData)
        {
            List<DSChoiceSaveData> choices = CloneNodeChoices(node.Saves.ChoicesInNode);

            DSNodeSaveData nodeData = new DSNodeSaveData()
            {
                ID = node.ID,
                Name = node.DialogueName,
                GroupID = node.Group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position,
                OnlyOneConditionNeeded = node.Saves.OnlyOneConditionNeeded,
            };
            
            nodeData.SetBubleType(node.BubleType);
            nodeData.SaveDropDownKeyDialogue(node.Saves.GetDropDownKeyDialogue());
            nodeData.SaveDropDownKeyEvent(node.Saves.GetDropDownKeyEvent());
            nodeData.SaveEvents(node.Saves.GetEvents()); 
            nodeData.hasEvent = node.Saves.hasEvent; // to edit (clean)
            nodeData.isMultipleChoice = node.Saves.isMultipleChoice; // to edit (clean)
            nodeData.SaveSpeaker(node.Speaker);
            nodeData.SaveHumeur(node.Humeur);
            nodeData.SetChoices(choices);
            

            graphData.Nodes.Add(nodeData);
        }


        private static void SaveNodeToScriptableObject(DSNode node, DSDialogueContainerSO dialogueContainer)
        {
            DSDialogueSO dialogue;

            if (node.Group != null)
            {
                dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Groups/{node.Group.title}/Dialogues", node.DialogueName);

                dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.Group.ID], dialogue);
            }
            else
            {
                dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Global/Dialogues", node.DialogueName);

                dialogueContainer.UngroupedDialogues.Add(dialogue);
            }

            dialogue.Initialize(
                node.DialogueName,
                node.Text,
                ConvertNodeChoicesToDialogueChoices(node.Saves.ChoicesInNode),
                node.DialogueType,
                node.IsStartingNode(),
                node.Speaker
                
            );


            node.Saves.OnlyOneConditionNeeded = node.OnlyOneConditionNeeded;
            node.Saves.SaveHumeur(node.Humeur);
            node.Saves.SetBubleType(node.BubleType);
            createdDialogues.Add(node.ID, dialogue);

            node.Saves.SaveEvents(node.Events); 

            SaveAsset(dialogue);
        }

        private static List<DSDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<DSChoiceSaveData> nodeChoices)
        {
            List<DSDialogueChoiceData> dialogueChoices = new List<DSDialogueChoiceData>();

            foreach (DSChoiceSaveData nodeChoice in nodeChoices)
            {
                DSDialogueChoiceData choiceData = new DSDialogueChoiceData()
                {
                    // copiers d'autres champs si DSDialogueChoiceData en a (ex : Text, Condition infos)
                    // NextDialogue left null for now — will be set in UpdateDialoguesChoicesConnections
                };
                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }


        private static void UpdateDialoguesChoicesConnections()
        {
            foreach (DSNode node in nodes)
            {
                DSDialogueSO dialogue = createdDialogues[node.ID];

                for (int choiceIndex = 0; choiceIndex < node.Saves.ChoicesInNode.Count; ++choiceIndex)
                {
                    DSChoiceSaveData nodeChoice = node.Saves.ChoicesInNode[choiceIndex];

                    if (string.IsNullOrEmpty(nodeChoice.NodeID))
                    {
                        continue;
                    }

                    dialogue.Choices[choiceIndex].NextDialogue = createdDialogues[nodeChoice.NodeID];

                    SaveAsset(dialogue);
                }
            }
        }


    private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                    {
                        nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();
                    }

                    foreach (string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                    }
                }
            }

            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Global/Dialogues", nodeToRemove);
                }
            }

            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }

        public static void Load()
        {
            DSGraphSaveDataSO graphData = LoadAsset<DSGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", graphFileName);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not find the file!",
                    "The file at the following path could not be found:\n\n" +
                    $"\"Assets/Editor/DialogueSystem/Graphs/{graphFileName}\".\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Thanks!"
                );

                return;
            }

            DSEditorWindow.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
        }

        // LOAD LES GROUPES // AJOUTE LES GROUPES AU DICTIONNAIRE //
        private static void LoadGroups(List<DSGroupSaveData> groups)
        {
            foreach (DSGroupSaveData groupData in groups)
            {
                DSGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);

                group.ID = groupData.ID;

                loadedGroups.Add(group.ID, group);
            }
        }
        
        public static bool DoesNameExistInLoadedNodes(string name)
        {
            if (graphView == null)
                return false;
            return graphView.GetExistingNames().Contains(name);
        }
        
        // LOAD LES NODES // UTILISE CloneNodeChoices POUR CLONER LES CHOIX // APRES AVOIR TOUT INITIALISÉ // APPELLE node.Draw() // AJOUTE LE NODE AU graphView // AJOUTE LE NODE AU DICTIONNAIRE loadedNodes // SI LE NODE A UN GroupID, L'ASSOCIE AU GROUPE CORRESPONDANT !! //
        private static void LoadNodes(List<DSNodeSaveData> nodes)
        {
            graphView.ClearExistingNames();
            FantasyDialogueTable.Load();
            foreach (DSNodeSaveData nodeData in nodes)
            {
                List<DSChoiceSaveData> choices = CloneNodeChoices(nodeData.ChoicesInNode);

                DSNode node = graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false);
                node.ID = nodeData.ID;
                
                node.Saves.SaveDropDownKeyDialogue( nodeData.GetDropDownKeyDialogue());
                node.Saves.SaveEvents(nodeData.GetEvents());
                node.Saves.SetChoices(choices);
                node.Saves.isMultipleChoice = nodeData.isMultipleChoice;
                node.Saves.hasEvent = nodeData.hasEvent; 
                node.BubleType = nodeData.GetBubleType();
                node.SetSpeaker(nodeData.Speaker);
                node.SetHumeur(nodeData.GetHumeur());
                
                node.Saves.OnlyOneConditionNeeded = nodeData.OnlyOneConditionNeeded;
                node.Draw(new Color());
                graphView.AddElement(node);

                loadedNodes.Add(node.ID, node);

                if (!string.IsNullOrEmpty(nodeData.GroupID))
                {
                    if (loadedGroups.TryGetValue(nodeData.GroupID, out DSGroup group))
                    {
                        node.Group = group;
                        group.AddElement(node);
                    }
                    else
                    {
                        Debug.LogWarning($"[LoadNodes] Group with ID {nodeData.GroupID} not found for node {nodeData.Name}");
                    }
                }
            }
        }

        // LOAD LES CONNEXIONS ENTRE NODES //
        private static void LoadNodesConnections() 
        { 
            Port FindPortInElement(VisualElement elem) 
            {
                if (elem == null) return null; 
                
                if (elem is Port directPort) return directPort;
                
                foreach (var child in elem.Children())
                {
                    var found = FindPortInElement(child);
                    if (found != null) return found;
                }
                
                return null;
            }

            foreach (KeyValuePair<string, DSNode> loadedNode in loadedNodes)
            {
                var node = loadedNode.Value;
                if (node == null || node.outputContainer == null) continue;

                foreach (var visualElement in node.outputContainer.Children())
                {
                    // chercher un Port soit à la racine, soit imbriqué
                    Port choicePort = FindPortInElement(visualElement);

                    if (choicePort == null)
                    {
                        // ce child ne contient pas de port, on l'ignore
                        continue;
                    }

                    // userData peut être null ou de type différent => vérifier
                    if (choicePort.userData == null)
                    {
                        Debug.LogWarning($"[LoadNodesConnections] Port sans userData trouvé dans node '{node.DialogueName}' (ID: {node.ID}). Ignoring.");
                        continue;
                    }

                    if (!(choicePort.userData is DSChoiceSaveData choiceData))
                    {
                        Debug.LogWarning($"[LoadNodesConnections] Port.userData n'est pas un DSChoiceSaveData pour le node '{node.DialogueName}'. Type trouvé: {choicePort.userData.GetType().Name}");
                        continue;
                    }

                    if (string.IsNullOrEmpty(choiceData.NodeID))
                    {
                        // pas de connexion pour ce choix
                        continue;
                    }

                    if (!loadedNodes.TryGetValue(choiceData.NodeID, out DSNode nextNode))
                    {
                        Debug.LogWarning($"[LoadNodesConnections] No target node with ID '{choiceData.NodeID}' found for connection from node '{node.DialogueName}'.");
                        continue;
                    }

                    // trouver l'Input port du node cible
                    if (nextNode.inputContainer == null || !nextNode.inputContainer.Children().Any())
                    {
                        Debug.LogWarning($"[LoadNodesConnections] Next node '{nextNode.DialogueName}' has no input ports.");
                        continue;
                    }

                    // trouver le premier Port dans l'inputContainer (ou imbriqué)
                    Port nextNodeInputPort = null;
                    foreach (var iv in nextNode.inputContainer.Children())
                    {
                        nextNodeInputPort = FindPortInElement(iv);
                        if (nextNodeInputPort != null) break;
                    }

                    if (nextNodeInputPort == null)
                    {
                        Debug.LogWarning($"[LoadNodesConnections] Could not find an input Port on next node '{nextNode.DialogueName}'.");
                        continue;
                    }

                    // connect and add edge
                    try
                    {
                        Edge edge = choicePort.ConnectTo(nextNodeInputPort);
                        graphView.AddElement(edge);
                        node.RefreshPorts();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[LoadNodesConnections] Failed to create edge between '{node.DialogueName}' and '{nextNode.DialogueName}': {ex.Message}");
                    }
                } 
            }
            // ----- Reconnect Start-node NextDialogueNodeID (fallback) -----
            foreach (var kv in loadedNodes)
            {
                DSNode srcNode = kv.Value;
                if (srcNode == null || srcNode.Saves == null) continue;

                string nextId = srcNode.Saves.ChoicesInNode[0].NodeID;
                if (string.IsNullOrEmpty(nextId)) continue;

                if (!loadedNodes.TryGetValue(nextId, out DSNode targetNode))
                {
                    Debug.LogWarning($"[LoadNodesConnections] Start next node ID '{nextId}' not found for {srcNode.DialogueName}");
                    continue;
                }

                // find first output port on srcNode
                Port srcOutPort = null;
                foreach (var ve in srcNode.outputContainer.Children())
                {
                    srcOutPort = FindPortInElement(ve);
                    if (srcOutPort != null) break;
                }

                if (srcOutPort == null)
                {
                    Debug.LogWarning($"[LoadNodesConnections] No output port found on source node {srcNode.DialogueName}");
                    continue;
                }

                // find first input port on target node
                Port targetInPort = null;
                foreach (var ve in targetNode.inputContainer.Children())
                {
                    targetInPort = FindPortInElement(ve);
                    if (targetInPort != null) break;
                }
                if (targetInPort == null)
                {
                    Debug.LogWarning($"[LoadNodesConnections] No input port found on target node {targetNode.DialogueName}");
                    continue;
                }

                Edge edge = srcOutPort.ConnectTo(targetInPort);
                graphView.AddElement(edge);
                srcNode.RefreshPorts();
            }

        }

        private static void CreateDefaultFolders()
        {
            CreateFolder("Assets/Editor/DialogueSystem", "Graphs");

            CreateFolder("Assets", "DialogueSystem");
            CreateFolder("Assets/DialogueSystem", "Dialogues");

            CreateFolder("Assets/DialogueSystem/Dialogues", graphFileName);
            CreateFolder(containerFolderPath, "Global");
            CreateFolder(containerFolderPath, "Groups");
            CreateFolder($"{containerFolderPath}/Global", "Dialogues");
        }

        private static void GetElementsFromGraphView()
        {
            nodes.Clear();
            groups = new List<DSGroup>();

            Type groupType = typeof(DSGroup);

            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is DSNode node)
                {
                    // On ajoute tous les nodes (groupés ou non) — Save() décidera comment les traiter
                    nodes.Add(node);
                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    DSGroup group = (DSGroup) graphElement;
                    groups.Add(group);
                    return;
                }
            });
        }


        public static void CreateFolder(string parentFolderPath, string newFolderName)
        {
            if (AssetDatabase.IsValidFolder($"{parentFolderPath}/{newFolderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(parentFolderPath, newFolderName);
        }

        public static void RemoveFolder(string path)
        {
            FileUtil.DeleteFileOrDirectory($"{path}.meta");
            FileUtil.DeleteFileOrDirectory($"{path}/");
        }

        public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";

            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();

                AssetDatabase.CreateAsset(asset, fullPath);
            }

            return asset;
        }

        public static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";

            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        public static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        private static List<DSChoiceSaveData> CloneNodeChoices(List<DSChoiceSaveData> nodeChoices)
        {
            var choices = new List<DSChoiceSaveData>();

            foreach (DSChoiceSaveData choice in nodeChoices)
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    NodeID = choice.NodeID
                };
                
                choiceData.SaveDropDownKeyChoice(choice.GetDropDownKeyChoice());
                choiceData.SavePortDirection(choice.GetPortDirection());

                if (choice.ConditionsKey != null && choice.ConditionsKey.Count > 0)
                    choiceData.ConditionsKey = new List<string>(choice.ConditionsKey);
                else
                    choiceData.ConditionsKey = new List<string>();

                choices.Add(choiceData);
            }

            return choices;
        }
        
        private static void SyncChoicesFromEdges_SingleTarget()
        {
            if (nodes == null || graphView == null) return;

            foreach (var n in nodes)
            {
                if (n?.Saves?.ChoicesInNode == null) continue;
                foreach (var c in n.Saves.ChoicesInNode)
                    c.NodeID = null;
            }

            var edges = graphView.graphElements.OfType<Edge>().ToList();
            foreach (var edge in edges)
            {
                var outPort = edge.output;
                var inPort = edge.input;
                if (outPort == null || inPort == null) continue;

                if (outPort.userData is DSChoiceSaveData choiceData && inPort.node is DSNode targetNode)
                {
                    choiceData.NodeID = targetNode.ID;
                }
                else if (outPort.node is DSStartNode startNode && inPort.node is DSNode tNode)
                {
                    startNode.Saves.ChoicesInNode[0].NodeID = tNode.ID;
                }
            }
        }

    }
