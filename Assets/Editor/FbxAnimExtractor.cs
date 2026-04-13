using UnityEngine;
using UnityEditor;
using System.IO;

public class FbxAnimExtractor
{
    [MenuItem("Tools/FBX → Humanoid Anim 추출 및 삭제")]
    public static void ExtractAndDelete()
    {
        string folderPath = "Assets/Animation";
        string[] guids = AssetDatabase.FindAssets("t:Object", new[] { folderPath });

        int successCount = 0;
        int skipCount = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                continue;

            string fbxName = Path.GetFileNameWithoutExtension(assetPath);

            // 1. Rig → Humanoid 설정
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[FbxAnimExtractor] ModelImporter 없음: {assetPath}");
                skipCount++;
                continue;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.SaveAndReimport();

            // 2. mixamo.com 클립 찾기
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            AnimationClip sourceClip = null;
            foreach (Object obj in allAssets)
            {
                if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                {
                    sourceClip = clip;
                    break;
                }
            }

            if (sourceClip == null)
            {
                Debug.LogWarning($"[FbxAnimExtractor] AnimationClip 없음: {assetPath}");
                skipCount++;
                continue;
            }

            // 3. 클립 복사 후 FBX 이름으로 저장
            AnimationClip newClip = new AnimationClip();
            EditorUtility.CopySerialized(sourceClip, newClip);
            newClip.name = fbxName;

            string animPath = $"{folderPath}/{fbxName}.anim";
            AssetDatabase.CreateAsset(newClip, animPath);

            // 4. FBX 삭제
            AssetDatabase.DeleteAsset(assetPath);

            Debug.Log($"[FbxAnimExtractor] 완료: {fbxName}.anim 생성, FBX 삭제");
            successCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "FBX 추출 완료",
            $"성공: {successCount}개\n스킵: {skipCount}개",
            "확인"
        );
    }
}
