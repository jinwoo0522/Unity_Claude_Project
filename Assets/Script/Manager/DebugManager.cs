using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugManager
{
    // 현재 적용 중인 렌더 파이프라인 에셋과 퀄리티 레벨별 설정을 출력한다
    public void Check()
    {
        var current = GraphicsSettings.currentRenderPipeline;
#if UNITY_EDITOR
        Debug.Log($"[활성] {current?.name ?? "없음"}  path={AssetDatabase.GetAssetPath(current)}");
#else
        Debug.Log($"[활성] {current?.name ?? "없음"}");
#endif
        Debug.Log($"[Quality 레벨] {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        Debug.Log($"[Graphics 기본] {GraphicsSettings.defaultRenderPipeline?.name ?? "없음"}");

        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            var rp = QualitySettings.GetRenderPipelineAssetAt(i);
            Debug.Log($"  레벨 {i} '{QualitySettings.names[i]}' → {rp?.name ?? "(Graphics 기본 사용)"}");
        }
    }
}
