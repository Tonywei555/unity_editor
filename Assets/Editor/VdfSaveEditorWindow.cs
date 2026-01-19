using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityEditorVdf
{
    public sealed class VdfSaveEditorWindow : EditorWindow
    {
        private const float DescriptionWidth = 280f;

        [SerializeField] private string vdfPath;
        [SerializeField] private VdfFieldDescriptions descriptions;

        private VdfNode root;
        private Vector2 scroll;
        private Dictionary<string, string> descriptionLookup = new Dictionary<string, string>();
        private readonly Dictionary<VdfNode, bool> foldouts = new Dictionary<VdfNode, bool>();
        private string loadMessage;

        [MenuItem("Tools/VDF Save Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<VdfSaveEditorWindow>("VDF Save Editor");
            window.minSize = new Vector2(720f, 400f);
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space();

            if (!string.IsNullOrEmpty(loadMessage))
            {
                EditorGUILayout.HelpBox(loadMessage, MessageType.Info);
            }

            if (root == null)
            {
                EditorGUILayout.HelpBox("Load a VDF file to begin editing.", MessageType.None);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var child in root.Children)
            {
                DrawNode(child, child.Key);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("VDF Save File", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    vdfPath = EditorGUILayout.TextField("Path", vdfPath);
                    if (GUILayout.Button("Browse", GUILayout.Width(80f)))
                    {
                        var selected = EditorUtility.OpenFilePanel("Select VDF Save", "", "vdf");
                        if (!string.IsNullOrEmpty(selected))
                        {
                            vdfPath = selected;
                            LoadVdf();
                        }
                    }

                    if (GUILayout.Button("Reload", GUILayout.Width(80f)))
                    {
                        LoadVdf();
                    }
                }

                descriptions = (VdfFieldDescriptions)EditorGUILayout.ObjectField(
                    "Descriptions",
                    descriptions,
                    typeof(VdfFieldDescriptions),
                    false);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.enabled = root != null;
                    if (GUILayout.Button("Save", GUILayout.Width(80f)))
                    {
                        SaveVdf();
                    }

                    GUI.enabled = true;
                    if (GUILayout.Button("Create Descriptions Asset", GUILayout.Width(200f)))
                    {
                        CreateDescriptionsAsset();
                    }
                }
            }
        }

        private void DrawNode(VdfNode node, string path)
        {
            if (node.HasChildren)
            {
                if (!foldouts.TryGetValue(node, out var isExpanded))
                {
                    isExpanded = true;
                }

                foldouts[node] = EditorGUILayout.Foldout(isExpanded, node.Key, true);
                if (foldouts[node])
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        foreach (var child in node.Children)
                        {
                            DrawNode(child, $"{path}.{child.Key}");
                        }
                    }
                }

                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(node.Key, GUILayout.Width(220f));
                node.Value = EditorGUILayout.TextField(node.Value ?? string.Empty);

                if (descriptionLookup.TryGetValue(path, out var description) && !string.IsNullOrWhiteSpace(description))
                {
                    EditorGUILayout.LabelField(new GUIContent("?", description), GUILayout.Width(24f));
                }
                else
                {
                    GUILayout.Space(DescriptionWidth);
                }
            }
        }

        private void LoadVdf()
        {
            loadMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(vdfPath))
            {
                loadMessage = "Please provide a VDF file path.";
                root = null;
                return;
            }

            if (!File.Exists(vdfPath))
            {
                loadMessage = "VDF file not found.";
                root = null;
                return;
            }

            try
            {
                var contents = File.ReadAllText(vdfPath);
                root = VdfParser.Parse(contents);
                descriptionLookup = descriptions != null ? descriptions.ToDictionary() : new Dictionary<string, string>();
                loadMessage = $"Loaded {root.Children.Count} top-level entries.";
            }
            catch (Exception exception)
            {
                root = null;
                loadMessage = $"Failed to parse VDF: {exception.Message}";
            }
        }

        private void SaveVdf()
        {
            if (root == null || string.IsNullOrWhiteSpace(vdfPath))
            {
                return;
            }

            try
            {
                var contents = VdfParser.Serialize(root);
                File.WriteAllText(vdfPath, contents);
                loadMessage = "Saved VDF file.";
            }
            catch (Exception exception)
            {
                loadMessage = $"Failed to save VDF: {exception.Message}";
            }
        }

        private void CreateDescriptionsAsset()
        {
            var asset = CreateInstance<VdfFieldDescriptions>();
            var path = EditorUtility.SaveFilePanelInProject(
                "Save VDF Descriptions",
                "VdfFieldDescriptions",
                "asset",
                "Create a descriptions asset to document VDF field meanings.");

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            descriptions = asset;
            descriptionLookup = descriptions.ToDictionary();
        }
    }
}
