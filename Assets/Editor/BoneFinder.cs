using UnityEngine;
using UnityEditor;
using System.Text;

public class BoneFinder
{
    [MenuItem("Tools/Find Right Hand Bone")]
    static void FindRightHandBone()
    {
        var playerUpperBody = Object.FindFirstObjectByType<Player_UpperBody>();
        if (playerUpperBody == null) { Debug.LogError("Player_UpperBody 없음"); return; }

        var animator = playerUpperBody.GetComponentInChildren<Animator>();
        if (animator == null) { Debug.LogError("Animator 없음"); return; }

        var sb = new StringBuilder();
        DumpBones(animator.transform, 0, sb, "hand");
        Debug.Log("Hand 관련 뼈:\n" + sb.ToString());
    }

    static void DumpBones(Transform t, int depth, StringBuilder sb, string filter)
    {
        if (t.name.ToLower().Contains(filter.ToLower()))
            sb.AppendLine(new string('-', depth) + t.name);

        foreach (Transform child in t)
            DumpBones(child, depth + 1, sb, filter);
    }
}
