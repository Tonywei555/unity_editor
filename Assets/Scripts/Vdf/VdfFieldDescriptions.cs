using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditorVdf
{
    [CreateAssetMenu(menuName = "VDF/Field Descriptions", fileName = "VdfFieldDescriptions")]
    public sealed class VdfFieldDescriptions : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string Path;
            [TextArea]
            public string Description;
        }

        public List<Entry> Entries = new List<Entry>();

        public Dictionary<string, string> ToDictionary()
        {
            var map = new Dictionary<string, string>();
            foreach (var entry in Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Path))
                {
                    continue;
                }

                map[entry.Path] = entry.Description ?? string.Empty;
            }

            return map;
        }
    }
}
