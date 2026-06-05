using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Tools/Setup SkillCooldownHUD 메뉴로 실행하면 현재 열린 씬의 Canvas 하위에
// QSlot·MouseSlot HUD를 생성하고 SkillCooldownUI에 Overlay를 연결한다.
// 한 번만 실행하면 된다. 씬을 저장한 뒤 이 파일은 삭제해도 된다.
public static class SkillCooldownHUD_Setup
{
    private const float SlotSize    = 100f;
    private const float SlotSpacing = 10f;
    private const float EdgePadding = 20f;

    [MenuItem("Tools/Setup SkillCooldownHUD")]
    private static void Setup()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("오류", "씬에 Canvas가 없습니다.", "확인");
            return;
        }

        // 기존 HUD가 있으면 제거 후 재생성
        Transform existing = canvas.transform.Find("SkillCooldownHUD");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        // ── SkillCooldownHUD 루트 ──────────────────────────────────────
        GameObject hud = CreateUIObject("SkillCooldownHUD", canvas.transform);
        RectTransform hudRect = hud.GetComponent<RectTransform>();
        // 우측 하단 앵커
        hudRect.anchorMin = hudRect.anchorMax = hudRect.pivot = new Vector2(1f, 0f);
        float totalWidth = SlotSize * 2 + SlotSpacing;
        hudRect.anchoredPosition = new Vector2(-EdgePadding, EdgePadding);
        hudRect.sizeDelta        = new Vector2(totalWidth, SlotSize);

        SkillCooldownUI ui = hud.AddComponent<SkillCooldownUI>();

        // ── QSlot (슬롯 0) ────────────────────────────────────────────
        Image qOverlay = CreateSlot("QSlot", hud.transform, 0f);

        // ── MouseSlot (슬롯 1) ────────────────────────────────────────
        Image mOverlay = CreateSlot("MouseSlot", hud.transform, SlotSize + SlotSpacing);

        // SkillCooldownUI._overlays에 두 오버레이 연결
        SerializedObject so      = new SerializedObject(ui);
        SerializedProperty overlays = so.FindProperty("_overlays");
        overlays.arraySize = 2;
        overlays.GetArrayElementAtIndex(0).objectReferenceValue = qOverlay;
        overlays.GetArrayElementAtIndex(1).objectReferenceValue = mOverlay;
        so.ApplyModifiedPropertiesWithoutUndo();

        // 씬 변경 표시 및 저장 유도
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("[SkillCooldownHUD_Setup] HUD 생성 완료. Ctrl+S로 씬을 저장하세요.");
        Selection.activeGameObject = hud;
    }

    // 단색 배경 슬롯 + Overlay Image 생성 후 Overlay Image 반환
    private static Image CreateSlot(string name, Transform parent, float xOffset)
    {
        // 배경
        GameObject slot = CreateUIObject(name, parent);
        RectTransform slotRect = slot.GetComponent<RectTransform>();
        slotRect.anchorMin = slotRect.anchorMax = slotRect.pivot = new Vector2(0f, 0f);
        slotRect.anchoredPosition = new Vector2(xOffset, 0f);
        slotRect.sizeDelta        = new Vector2(SlotSize, SlotSize);

        Image bg = slot.AddComponent<Image>();
        bg.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        // 오버레이 (Filled / Vertical / Bottom)
        GameObject overlay = CreateUIObject("Overlay", slot.transform);
        RectTransform ovRect = overlay.GetComponent<RectTransform>();
        ovRect.anchorMin       = Vector2.zero;
        ovRect.anchorMax       = Vector2.one;
        ovRect.offsetMin       = Vector2.zero;
        ovRect.offsetMax       = Vector2.zero;

        Image img          = overlay.AddComponent<Image>();
        img.color          = new Color(0f, 0f, 0f, 0.75f);
        img.type           = Image.Type.Filled;
        img.fillMethod     = Image.FillMethod.Vertical;
        img.fillOrigin     = (int)Image.OriginVertical.Bottom;
        img.fillAmount     = 0f;
        img.enabled        = false; // 초기에는 비활성(쿨타임 없음)

        return img;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}
