using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// Stat 변경 → HP 표시 갱신 전담 (데이터 Stat ─구독→ Binder ─갱신→ 표시, 단방향).
// 머리 위 월드 HP바(_hpTopSlider): 적/아군 모두 봐야 하므로 전 클라 갱신.
// 화면 좌하단 HUD(PlayerHUD): 내 것만이므로 owner 전용 갱신.
public class PlayerHUDBinder : NetworkBehaviour
{
    [SerializeField] private Slider _hpTopSlider;   // 머리 위 월드 HP바 (전 클라 표시)

    private Stat      _stat;
    private PlayerHUD _hud;   // 화면 HUD (owner 전용)

    public override void OnNetworkSpawn()
    {
        _stat = GetComponent<Stat>();

        // 머리 위 HP바는 모든 클라에서 갱신돼야 하므로 owner 무관하게 구독
        _stat.StatChanged += OnStatChanged;

        // 화면 HUD는 내 것만 — owner만 탐색
        if (IsOwner)
            _hud = FindAnyObjectByType<PlayerHUD>(FindObjectsInactive.Include);

        RefreshHp();   // 구독 전 서버가 채운 초기값을 1회 강제 반영
    }

    public override void OnNetworkDespawn()
    {
        _stat.StatChanged -= OnStatChanged;
    }

    private void OnStatChanged(Stat.STAT_TAG tag)
    {
        if (tag == Stat.STAT_TAG.HP || tag == Stat.STAT_TAG.MAX_HP)
            RefreshHp();
    }

    private void RefreshHp()
    {
        float fHp    = _stat.Get_Stat(Stat.STAT_TAG.HP);
        float fMaxHp = _stat.Get_Stat(Stat.STAT_TAG.MAX_HP);

        // 머리 위 HP바 — 전 클라
        _hpTopSlider.maxValue = fMaxHp;
        _hpTopSlider.value    = fHp;

        // 화면 HUD — owner만
        if (IsOwner)
            _hud.SetHp(fHp, fMaxHp);
    }
}
