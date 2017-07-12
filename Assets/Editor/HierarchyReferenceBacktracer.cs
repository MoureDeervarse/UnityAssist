// Updated in https://github.com/MoureDeervarse/UnityAssist
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public class HierarchyReferenceBacktracer : EditorWindow
{
    // Each referencing object
    private class RefItem
    {
        public string fieldPath;
        public Object refObj;

        public RefItem(string _fieldPath, Object _refObj)
        {
            fieldPath = _fieldPath;
            refObj = _refObj;
        }
    }

    // Drawn RefItmes that groupping by fold
    private class RefItemGroup
    {
        public bool isFoldout = true;
        public Object typeOfObj;
        public List<RefItem> items;

        public RefItemGroup(Object _typeOfObj, List<RefItem> _items)
        {
            typeOfObj = _typeOfObj;
            items = _items;
        }
    }

    private List<RefItemGroup> itemGroups;
    private Component comp;
    
    public void SetTargetObject(Component obj)
    {
        comp = obj;
    }

    private const string LOADING_TITLE = "Loaindg";
    private const string LOADING_DESC = "Now backtrace objects in hierarchy...";
    public void BacktraceReferences(Transform trans)
    {
        if (null == trans)
        {
            return;
        }
        EditorUtility.DisplayProgressBar(LOADING_TITLE, LOADING_DESC, 0f);
        var refDic = new Dictionary<Object, List<RefItem>>();
        List<Object> comps = new List<Object>(new Object[]{ trans.gameObject});
        comps.AddRange(trans.GetComponents<Component>());

        for(int idx = 0; idx < comps.Count; ++idx)
        {
            List<RefItem> items = FindObjectsReferencing(comps[idx]);
            EditorUtility.DisplayProgressBar(LOADING_TITLE, LOADING_DESC, (float)idx / comps.Count);
            if (null == items || items.Count <= 0)
            {
                continue;
            }
            refDic.Add(comps[idx], items);
        }

        itemGroups = new List<RefItemGroup>();
        if (null != refDic && refDic.Count > 0)
        {
            foreach (KeyValuePair<Object, List<RefItem>> pair in refDic)
            {
                itemGroups.Add(new RefItemGroup(pair.Key, pair.Value));
            }
        }
        EditorUtility.ClearProgressBar();
    }

    private static List<RefItem> FindObjectsReferencing<T>(T comp) where T : Object
    {
        Component[] objs = Resources.FindObjectsOfTypeAll<Component>();
        if (null == objs)
        {
            return null;
        }
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        List<RefItem> items = new List<RefItem>();
        foreach (Component obj in objs)
        {
            FieldInfo[] fields = obj.GetType().GetFields(flags);
            foreach (FieldInfo fieldInfo in fields)
            {
                string refFieldPath = FieldReferencesComponent(obj, fieldInfo, comp);
                if (string.Empty == refFieldPath)
                {
                    continue;
                }
                items.Add(new RefItem(refFieldPath, obj));
            }
        }
        return items;
    }

    private static string FieldReferencesComponent<T>(Component obj, FieldInfo fieldInfo, T comp) where T : Object
    {
        if (null == obj || null == fieldInfo || null == comp)
        {
            return string.Empty;
        }
        if (fieldInfo.FieldType.IsArray)
        {
            var arr = fieldInfo.GetValue(obj) as System.Array;
            if (null == arr)
            {
                return string.Empty;
            }
            foreach (object elem in arr)
            {
                if (null != elem && comp == elem)
                {
                    int elemIdx = System.Array.IndexOf(arr, elem);
                    return string.Format("{0}.{1}[{2}]", obj.GetType().ToString(), fieldInfo.Name, elemIdx);
                }
            }
        }
        else
        {
            object value = fieldInfo.GetValue(obj);
            if (null != value && comp == value)
            {
                return string.Format("{0}.{1}", obj.GetType().ToString(), fieldInfo.Name);
            }
        }
        return string.Empty;
    }

    private Vector2 scrollPos;
    private void OnGUI()
    {
        comp = EditorGUILayout.ObjectField("Object referenced : ", comp, typeof(Component), true) as Component;
        // Block main asset backtrace that asset in project view
        if(null == comp || AssetDatabase.Contains(comp))
        {
            comp = null;
            return;
        }
        if (GUILayout.Button("Backtrace reference in hierarchy"))
        {
            BacktraceReferences(comp.transform);
        }
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
		if (null != itemGroups) 
        {
			itemGroups.ForEach (DrawRefItem);
		}
        EditorGUILayout.EndScrollView();
    }

    private void DrawRefItem(RefItemGroup group)
    {
        if (null == group)
        {
            return;
        }
        group.isFoldout = EditorGUILayout.Foldout(group.isFoldout, group.typeOfObj.GetType().ToString());
        if (!group.isFoldout)
        {
            return;
        }
        for (int idx = 0; idx < group.items.Count; ++idx)
        {
            // Referencing object field
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(22);
            string idxStr = string.Format("[{0}]", (idx+1).ToString());
            GUIStyle centerOption = GUI.skin.label;
            centerOption.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(idxStr, centerOption, GUILayout.Width(20));
            Object refObj = group.items[idx].refObj;
            EditorGUILayout.ObjectField(refObj, refObj.GetType(), true);
            EditorGUILayout.EndHorizontal();

            // That object path field
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(28);
            GUIStyle leftOption = GUI.skin.label;
            leftOption.alignment = TextAnchor.MiddleLeft;
            GUILayout.Label(string.Concat("└─ ", group.items[idx].fieldPath), leftOption);
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.Space(10);
    }

    [MenuItem("Window/Hierarchy Reference Backtracer")]
    public static HierarchyReferenceBacktracer GetWindow()
    {
        var window = GetWindow<HierarchyReferenceBacktracer>("RefBacktracer", false);
        window.Repaint();
        return window;
    }

    [MenuItem("GameObject/Backtrace References #F12", false, 0)]
    private static void ShowAllReference()
    {
        if (Selection.transforms.Length != 1)
        {
            return;
        }
        Transform selected = Selection.transforms[0];
        HierarchyReferenceBacktracer tracer = GetWindow();
        tracer.SetTargetObject(selected);
        tracer.BacktraceReferences(selected);
    }
}