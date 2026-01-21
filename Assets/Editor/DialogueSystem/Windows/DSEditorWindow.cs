using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


    using System;
    using UnityEngine;

    public class DSEditorWindow : EditorWindow
    {
        private DSGraphView graphView;

        private readonly string defaultFileName = "DialoguesFileName";

        private static TextField fileNameTextField;
        private Button saveButton;
        private Button miniMapButton;

        [MenuItem("Graph/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<DSEditorWindow>("Dialogue Graph");
        }

        // INITIALISATION DE LA FENETRE//
        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();

            AddStyles();
        }

        // AJOUTE LA GRAPHVIEW A LA FENETRE//
        private void AddGraphView()
        {
            graphView = new DSGraphView(this);

            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
        }

        // MENU ET BOUTONS EN HAUT DE LA FENETRE POUR SAUVEGARDER/CHARGER ETC// VOUS POUVEZ AJOUTER VOS BOUTONS ICI OU/RENOMMER CEUX EXISTANTS (mais attention les noms ont une logique très profonde derrière) //
        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            fileNameTextField = DSElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });
            
            Button openGraphEditorButton = DSElementUtility.CreateButton("<- Open data graph wiew", () => OpenDataGraphView(fileNameTextField.value));
            saveButton = DSElementUtility.CreateButton("Save Graph Wiew", () => Save());

            Button loadButton = DSElementUtility.CreateButton("Load Node From File", () => Load());
            Button clearButton = DSElementUtility.CreateButton("Clear Graph Wiew", () => Clear());
            Button resetButton = DSElementUtility.CreateButton("Reset Data", () => ResetGraph());

            miniMapButton = DSElementUtility.CreateButton("Minimap (prototype)", () => ToggleMiniMap());

            toolbar.Add(fileNameTextField);
            toolbar.Add(openGraphEditorButton);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(miniMapButton);

            toolbar.AddStyleSheets("DialogueSystem/DSToolbarStyles.uss");

            rootVisualElement.Add(toolbar);
        }

        // PERMET D'OUVRIR DIRECTEMENT L'ASSET DE TYPE SCRIPTABLEOBJECT QUI CONTIENT LE GRAPHE DE DIALOGUE //

private void OpenDataGraphView(string pathOrId)
{
    string fileName = (fileNameTextField != null) ? fileNameTextField.value.Trim() : pathOrId?.Trim();

    if (string.IsNullOrEmpty(fileName))
    {
        Debug.LogError("[DSEditorWindow] nom de fichier vide.");
        return;
    }

    string baseName = Path.GetFileNameWithoutExtension(fileName);

    string graphsFolder = "Assets/Editor/DialogueSystem/Graphs";

    string[] candidates = new[]
    {
        Path.Combine(graphsFolder, baseName + "Graph.asset").Replace("\\","/"),
        Path.Combine(graphsFolder, baseName + ".asset").Replace("\\","/"),
        Path.Combine(graphsFolder, baseName).Replace("\\","/"), // cas improbable mais tenté
    };

    UnityEngine.Object asset = null;
    foreach (var candidate in candidates)
    {
        Debug.Log($"[DSEditorWindow] Tentative de chargement : {candidate}");
        asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(candidate);
        if (asset != null) break;
    }
    
    if (asset == null)
    {
        Debug.LogError($"[DSEditorWindow] Impossible de localiser l'asset pour '{fileName}'. Assure-toi que le fichier existe dans '{graphsFolder}' avec le suffixe 'Graph.asset' ou '.asset'.");
        return;
    }

    Selection.activeObject = asset;
    EditorGUIUtility.PingObject(asset);
    EditorUtility.FocusProjectWindow();
}
        // return;
    //
    //     if (string.IsNullOrEmpty(pathOrId))
    //     {
    //         Debug.LogWarning("[DSEditorWindow] pathOrId vide.");
    //         return;
    //     }
    //
    //     string input = pathOrId.Replace("\\", "/").Trim();
    //
    //     string projectRelativePath = input;
    //     string assetsAbsolute = Application.dataPath.Replace("\\", "/");
    //     if (input.StartsWith(assetsAbsolute, StringComparison.OrdinalIgnoreCase))
    //     {
    //         projectRelativePath = "Assets/Editor" + input.Substring(assetsAbsolute.Length);
    //     }
    //
    //     if (!projectRelativePath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
    //     {
    //         string guidToPath = AssetDatabase.GUIDToAssetPath(input);
    //         if (!string.IsNullOrEmpty(guidToPath))
    //         {
    //             projectRelativePath = guidToPath;
    //         }
    //     }
    //
    //     UnityEngine.Object asset = null;
    //     if (projectRelativePath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
    //     {
    //         AssetDatabase.Refresh();
    //         asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(projectRelativePath);
    //     }
    //
    //     if (asset == null)
    //     {
    //         string fileName = Path.GetFileNameWithoutExtension(input);
    //
    //         if (!string.IsNullOrEmpty(fileName))
    //         {
    //             string[] guids = AssetDatabase.FindAssets($"{fileName} t:ScriptableObject");
    //             if (guids == null || guids.Length == 0)
    //             {
    //                 guids = AssetDatabase.FindAssets(fileName);
    //             }
    //
    //             if (guids != null && guids.Length > 0)
    //             {
    //                 if (guids.Length > 1)
    //                     Debug.LogWarning($"[DSEditorWindow] Plusieurs assets trouvés pour '{fileName}', ouverture du premier (GUID = {guids[0]}).");
    //
    //                 string foundPath = AssetDatabase.GUIDToAssetPath(guids[0]);
    //                 asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(foundPath);
    //                 projectRelativePath = foundPath;
    //             }
    //         }
    //     }
    //
    //     if (asset == null)
    //     {
    //         Debug.LogError($"[DSEditorWindow] Impossible de localiser l'asset pour '{pathOrId}'. Assure-toi que le chemin est dans le dossier Assets ou fournis un nom/ GUID valide.");
    //         return;
    //     }
    //
    //     Selection.activeObject = asset;
    //     EditorGUIUtility.PingObject(asset);
    //
    //     EditorUtility.FocusProjectWindow();

        
        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueSystem/DSVariables.uss");
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog("Invalid file name.", "Please ensure the file name you've typed in is valid.", "Roger!");

                return;
            }

            DSIOUtility.Initialize(graphView, fileNameTextField.value);
            DSIOUtility.Save();
        }

        private void Load()
        {
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/Editor/DialogueSystem/Graphs", "asset");

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            Clear();

            DSIOUtility.Initialize(graphView, Path.GetFileNameWithoutExtension(filePath));
            DSIOUtility.Load();
        }

        private void Clear()
        {
            graphView.ClearGraph();
        }

        private void ResetGraph()
        {
            Clear();
            UpdateFileName(defaultFileName);
        }

        private void ToggleMiniMap()
        {
            graphView.ToggleMiniMap();

            miniMapButton.ToggleInClassList("ds-toolbar__button__selected");
        }

        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }

        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
    }
