// Updated in https://github.com/MoureDeervarse/UnityAssist
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public class HierarchyMoveShortcuts
{
	[MenuItem("Edit/Hotkeys/Selected Move Up &UP")]
    private static void MoveObjectUp()
    {
        HierachyMoveProcess(false, selectedArr => 
        {
            foreach (Transform trans in selectedArr)
            {
                int siblingIdx = trans.GetSiblingIndex();
                if(null == trans.parent)
                {
                    if (siblingIdx > 0 && !Array.Exists(selectedArr, t => t.GetSiblingIndex() == siblingIdx - 1))
                    {
                        trans.SetSiblingIndex(siblingIdx - 1);
                    }
                }
                else if(0 == siblingIdx)
                {
                    int dstIdx = trans.parent.GetSiblingIndex();
                    trans.SetParent(trans.parent.parent);
                    trans.SetSiblingIndex(dstIdx);
                }
                else
                {
                    trans.SetSiblingIndex(siblingIdx - 1);
                }
                EditorUtility.SetDirty(trans);
            }
        });
    }

    [MenuItem("Edit/Hotkeys/Selected Move Down &DOWN")]
    private static void MoveObjectDown()
    {
        HierachyMoveProcess(true, selectedArr =>
        {
            foreach (Transform trans in selectedArr)
            {
                int siblingIdx = trans.GetSiblingIndex();
                if (null == trans.parent)
                {
                    if(!Array.Exists(selectedArr, t => t.GetSiblingIndex() == (siblingIdx + 1)))
                    {
                        trans.SetSiblingIndex(siblingIdx + 1);
                    }
                }
                else if ((trans.parent.childCount - 1) == siblingIdx)
                {
                    int dstIdx = trans.parent.GetSiblingIndex();
                    trans.SetParent(trans.parent.parent);
                    trans.SetSiblingIndex(dstIdx+1);
                }
                else
                {
                    trans.SetSiblingIndex(siblingIdx + 1);
                }
                EditorUtility.SetDirty(trans);
            }
        });
    }

    [MenuItem("Edit/Hotkeys/Selected Move In &RIGHT")]
    private static void MoveObjectInside()
    {
        HierachyMoveProcess(false, selectedArr =>
        {
            foreach (Transform trans in selectedArr)
            {
                int siblingIdx = trans.GetSiblingIndex();
                if (siblingIdx <= 0)
                {
                    continue;
                }
                if(null != trans.parent)
                {
                    trans.SetParent(trans.parent.GetChild(siblingIdx - 1));
                    trans.SetSiblingIndex(trans.parent.childCount - 1);
                }
                else
                {
                    Transform aboveTrans = Resources.FindObjectsOfTypeAll<Transform>().First(t =>
                    {
                        return (null == t.parent && t.GetSiblingIndex() == (siblingIdx - 1) && HideFlags.None == t.hideFlags);
                    });
                    trans.SetParent(aboveTrans);
                    trans.SetSiblingIndex(aboveTrans.childCount - 1);
                }
                var hierarchyType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
                var foldMethod = hierarchyType.GetMethod("SetExpandedRecursive");
                EditorApplication.ExecuteMenuItem("Window/Hierarchy");
                EditorWindow hierarchy = EditorWindow.focusedWindow;
                // Foldout folded parent hierarchy
                foldMethod.Invoke(hierarchy, new object[] { trans.gameObject.GetInstanceID(), true });
                // Fold moved object hierarchy that unfolded by below foldout process
                foldMethod.Invoke(hierarchy, new object[] { trans.gameObject.GetInstanceID(), false });
                EditorUtility.SetDirty(trans);
            }
        });
    }

    [MenuItem("Edit/Hotkeys/Selected Move Out &LEFT")]
    private static void MoveObjectOutside()
    {
        HierachyMoveProcess(true, selectedArr =>
        {
            foreach (Transform trans in selectedArr)
            {
                if(null == trans.parent)
                {
                    continue;
                }
                int dstIdx = trans.parent.GetSiblingIndex() + 1;
                trans.SetParent(trans.parent.parent);
                trans.SetSiblingIndex(dstIdx);
                EditorUtility.SetDirty(trans);
            }
        });
    }

    private static bool IsHierarchySelected()
    {
        // Exit if selection is empty or select project view objects
        return (Selection.transforms.Length > 0 && !AssetDatabase.Contains(Selection.transforms[0]));
    }

    private delegate void Callback(Transform[] transforms);
    private static void HierachyMoveProcess(bool reverseArr, Callback callback)
    {
        if (!IsHierarchySelected())
        {
            return;
        }
        Transform[] selectedArr = Selection.transforms;
        Array.Sort(selectedArr, new HierarchySiblingComparer(reverseArr));
        callback(selectedArr);
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }
    }
}

public class HierarchySiblingComparer : IComparer<Transform>
{
    private int reverseValue = 1;

    public HierarchySiblingComparer(bool isReverse)
    {
        reverseValue = isReverse ? -1 : 1;
    }

    public int Compare(Transform a, Transform b)
    {
        return (GetTotalSiblingIndex(a) - GetTotalSiblingIndex(b)) * reverseValue;
    }

    private int GetTotalSiblingIndex(Transform trans, int stackIdx = 0)
    {
        int totalIdx = stackIdx + trans.GetSiblingIndex();
        if (null == trans.parent)
        {
            return totalIdx - 1;
        }
        else
        {
            return GetTotalSiblingIndex(trans.parent, totalIdx + 1);
        }
    }
}