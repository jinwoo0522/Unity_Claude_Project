using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SkillSystemSetup
{
    [MenuItem("Tools/Setup Skill System")]
    static void Setup()
    {
        // 1. SkillPool 오브젝트 생성
        SkillPool pool = Object.FindFirstObjectByType<SkillPool>();
        if (pool == null)
        {
            var poolGo = new GameObject("SkillPool");
            pool = poolGo.AddComponent<SkillPool>();
            Undo.RegisterCreatedObjectUndo(poolGo, "Create SkillPool");
            Debug.Log("SkillPool 생성 완료");
        }

        // 2. Player 찾기
        var playerUpperBody = Object.FindFirstObjectByType<Player_UpperBody>();
        if (playerUpperBody == null)
        {
            Debug.LogError("씬에서 Player_UpperBody를 찾을 수 없습니다.");
            return;
        }

        // 3. ElectricSkillData 로드 후 Player_UpperBody에 할당
        var skillData = AssetDatabase.LoadAssetAtPath<SkillData>(
            "Assets/Data/SkillData/ElectricSkillData.asset");
        if (skillData != null)
        {
            SerializedObject so = new SerializedObject(playerUpperBody);
            so.FindProperty("skillData").objectReferenceValue = skillData;
            so.ApplyModifiedProperties();
            Debug.Log("skillData 할당 완료");
        }

        // 4. Animator 자식 오브젝트에 AnimEventRelay 추가
        var animator = playerUpperBody.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            if (animator.GetComponent<AnimEventRelay>() == null)
            {
                Undo.AddComponent<AnimEventRelay>(animator.gameObject);
                Debug.Log($"AnimEventRelay 추가: {animator.gameObject.name}");
            }

            // 5. 오른손 뼈 자동 검색
            Transform rightHand = FindBone(animator.transform,
                "hand.r", "mixamorig:RightHand", "RightHand", "Bip001 R Hand",
                "right_hand", "Hand_R", "mixamorig_RightHand");

            if (rightHand != null)
            {
                SerializedObject so = new SerializedObject(playerUpperBody);
                so.FindProperty("rightHandBone").objectReferenceValue = rightHand;
                so.ApplyModifiedProperties();
                Debug.Log($"rightHandBone 할당 완료: {rightHand.name}");
            }
            else
            {
                Debug.LogWarning("오른손 뼈를 자동으로 찾지 못했습니다. Inspector에서 rightHandBone을 직접 할당해주세요.");
            }
        }

        // 6. Player 태그 설정
        playerUpperBody.gameObject.tag = "Player";
        EditorUtility.SetDirty(playerUpperBody.gameObject);

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("스킬 시스템 설정 완료");
    }

    static Transform FindBone(Transform root, params string[] names)
    {
        foreach (string boneName in names)
        {
            var result = FindInChildren(root, boneName);
            if (result != null) return result;
        }
        return null;
    }

    static Transform FindInChildren(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform child in t)
        {
            var result = FindInChildren(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
