// Updated in https://github.com/MoureDeervarse/UnityAssist
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace UnityAssist
{
    public class FilePaletteWindow : EditorWindow
    {
        private class FileItem
        {
            public Object asset;
            public int idx;

            private Object[] tempArr;

            public FileItem() { }

            public FileItem(int _idx)
            {
                idx = _idx;
            }

            public FileItem(int _idx, Object _asset)
            {
                idx = _idx;
                asset = _asset;
            }
        }

        private const float ITEM_HEIGHT = 22.0f;
        private const float ITEM_WIDTH = 20.0f;
        private const string SAVE_NAME = "FilePalette";

        private List<FileItem> items;
        private List<FileItem> drawItems;
        private Vector2 scrollPos;

        private void OnEnable()
        {
            string savedData = EditorPrefs.GetString(SAVE_NAME, string.Empty);
            items = new List<FileItem>();
            if (null == savedData || string.Empty == savedData)
            {
                items.Add(new FileItem(0));
            }
            else
            {
                string[] itemIds = savedData.Split(',');
                for (int idx = 0; idx < itemIds.Length; ++idx)
                {
                    Object asset = AssetDatabase.LoadAssetAtPath<Object>(itemIds[idx]);
                    items.Add(new FileItem(idx, asset));
                }
            }
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            drawItems = new List<FileItem>(items);
            drawItems.ForEach(DrawItemLine);
            EditorGUILayout.EndScrollView();
            CheckDragAndDrop();
        }

        private void DrawItemLine(FileItem item)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(ITEM_HEIGHT));

            // Draw index label with centering
            GUIStyle centerOption = GUI.skin.label;
            centerOption.alignment = TextAnchor.UpperCenter;
            GUILayout.Label((item.idx + 1).ToString(), centerOption, GUILayout.Width(ITEM_WIDTH));

            // Draw object field with open button if that is folder
            EditorGUI.BeginChangeCheck();
            item.asset = EditorGUILayout.ObjectField(item.asset, typeof(Object), false);
            if (EditorGUI.EndChangeCheck())
            {
                SaveFileList();
            }

            // Draw +, - buttons
            if (GUILayout.Button("+"))
            {
                AddLine(item.idx);
            }
            if (GUILayout.Button("-"))
            {
                RemoveLine(item.idx);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AddLine(int targetIdx)
        {
            // Add SceneItem to below
            items.Insert(targetIdx + 1, new FileItem());
            RefreshIndex();
        }

        private void RemoveLine(int targetIdx)
        {
            // Block remove first or unique item
            if (items.Count > 1 || targetIdx > 0)
            {
                items.RemoveAt(targetIdx);
                RefreshIndex();
            }
            else
            {
                items[0].asset = null;
                SaveFileList();
            }
        }

        private void RefreshIndex()
        {
            int idx = 0;
            items.ForEach(item => item.idx = idx++);
            SaveFileList();
        }

        private void CheckDragAndDrop()
        {
            switch (Event.current.type)
            {
                case EventType.DragPerform:
                    Object[] dragItems = DragAndDrop.objectReferences;
                    int pasteIdx = items.Count;
                    int idx = 0;
                    if (1 == items.Count && null == items[0].asset)
                    {
                        items[0].asset = dragItems[0];
                        ++idx;
                    }
                    while (idx < dragItems.Length)
                    {
                        items.Add(new FileItem(pasteIdx++, dragItems[idx]));
                        ++idx;
                    }
                    SaveFileList();
                    break;
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    break;
            }
        }

        private void SaveFileList()
        {
            string saveData = string.Empty;
            foreach (FileItem item in items)
            {
                string id = (null == item.asset) ? string.Empty : AssetDatabase.GetAssetPath(item.asset);
                saveData = string.Concat(saveData, id, ",");
            }
            if (string.Empty != saveData)
            {
                // Remove last comma
                saveData = saveData.Remove(saveData.Length - 1);
            }
            EditorPrefs.SetString(SAVE_NAME, saveData);
        }

        private void OpenFolderInBrowser(int instanceId)
        {
            System.Type projectBrowserType = System.Type.GetType("UnityEditor.ProjectBrowser,UnityEditor");
            if (null == projectBrowserType)
            {
                Debug.LogError("Can't find UnityEditor.ProjectBrowser type!");
            }
            else
            {
                FieldInfo projectBrowserField = projectBrowserType.GetField("s_LastInteractedProjectBrowser", BindingFlags.Static | BindingFlags.Public);
                if (null == projectBrowserField)
                {
                    Debug.LogError("Can't find s_LastInteractedProjectBrowser field!");
                }
                else
                {
                    FieldInfo browserViewMode = projectBrowserType.GetField("m_ViewMode", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (null == browserViewMode)
                    {
                        Debug.LogError("Can't find m_ViewMode field!");
                    }
                    else
                    {
                        object projectBrowserInst = projectBrowserField.GetValue(null);
                        // ViewMode value 0 is one column, 1 is two column mode
                        if (1 != (int)browserViewMode.GetValue(projectBrowserInst))
                        {
                            MethodInfo setTwoColumnMode = projectBrowserType.GetMethod("SetTwoColumns", BindingFlags.Instance | BindingFlags.NonPublic);
                            setTwoColumnMode.Invoke(projectBrowserInst, null);
                        }
                        MethodInfo showFolderFunc = projectBrowserType.GetMethod("ShowFolderContents", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (null == showFolderFunc)
                        {
                            Debug.LogError("Can't find ShowFolderContents method!");
                        }
                        else
                        {
                            showFolderFunc.Invoke(projectBrowserInst, new object[] { instanceId, true });
                        }
                    }
                }
            }
        }

        [OnOpenAssetAttribute]
        private static bool FileDoubleClickCallback(int instanceId, int line)
        {
            bool paletteFocused = (typeof(FilePaletteWindow) == focusedWindow.GetType());
            if (paletteFocused && ProjectWindowUtil.IsFolder(instanceId))
            {
                GetWindow<FilePaletteWindow>().OpenFolderInBrowser(instanceId);
                return true;
            }
            return false;
        }

        [MenuItem("Window/File Palette")]
        private static void ShowWindow()
        {
            GetWindow<FilePaletteWindow>("Palette", true);
        }
    }
}
