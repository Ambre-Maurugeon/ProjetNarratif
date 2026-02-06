    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.PlayerLoop;

    [CreateAssetMenu(fileName = "BugsDatabase", menuName = "Scriptable Objects/BugsDatabase")]  
    public class BugsDatabase : ScriptableObject
    {
        public List<CharacterEntry> entries = new List<CharacterEntry>();
        
        private void OnValidate()
        {
            UpdateIds();
        }
    
        [ContextMenu("Refresh IDs")]
        private void RefreshIds()
        {
            UpdateIds();
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    
        private void UpdateIds()
        {
            if (entries == null) return;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null)
                    entries[i].id = i;
            }
        }
    }