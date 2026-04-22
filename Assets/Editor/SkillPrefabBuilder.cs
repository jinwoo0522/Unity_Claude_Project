using UnityEngine;
using UnityEditor;

public class SkillPrefabBuilder
{
    [MenuItem("Tools/Create Electric Skill Prefab")]
    static void CreateElectricSkillPrefab()
    {
        var projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Resources/Effect/Skill/Electric_Jap/Projectiles_electric.prefab");
        var hitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Resources/Effect/Skill/Electric_Jap/Hit_electric.prefab");

        if (projectilePrefab == null || hitPrefab == null)
        {
            Debug.LogError("이펙트 프리팹을 찾을 수 없습니다.");
            return;
        }

        var root = new GameObject("ElectricSkill");

        var skill = root.AddComponent<SkillProjectile>();

        var rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;

        var col = root.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius    = 0.3f;

        var projGo = (GameObject)PrefabUtility.InstantiatePrefab(projectilePrefab, root.transform);
        projGo.name = "ProjectileEffect";

        var hitGo  = (GameObject)PrefabUtility.InstantiatePrefab(hitPrefab, root.transform);
        hitGo.name = "HitEffect";
        hitGo.SetActive(false);

        skill.projectileEffect = projGo;
        skill.hitEffect        = hitGo;

        const string savePath = "Assets/Resources/Effect/Skill/Electric_Jap/ElectricSkill.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, savePath);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Debug.Log($"ElectricSkill 프리팹 생성 완료: {savePath}");
    }
}
