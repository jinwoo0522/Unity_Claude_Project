using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Stat : NetworkBehaviour
{
    [SerializeField] private Entity_Data Stat_Data;
    [SerializeField] private Slider hpSlider;

    public float pHp { get => fHp.Value; set
        {
            float fCurHp = fHp.Value + value;
            if(fCurHp <= 0)
                Die();
            fHp.Value = Mathf.Clamp(fCurHp, 0, pMaxHp);
        }}
    public float pDamage     { get; private set; }
    public float pResistance { get; private set; }
    public float pMaxHp      { get => fMaxHp.Value; set => fMaxHp.Value = value; }

    // 클라이언트에서 마나 게이트·UI 표시에 사용 (NetworkVariable 복제값)
    public float pMana    => fMana.Value;
    public float pMaxMana => fMaxMana.Value;

    NetworkVariable<float> fHp = new NetworkVariable<float>(0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    NetworkVariable<float> fMaxHp = new NetworkVariable<float>(0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    NetworkVariable<float> fMana = new NetworkVariable<float>(0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    NetworkVariable<float> fMaxMana = new NetworkVariable<float>(0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // 서버 전용 — 마나 재생량(클라 복제 불필요)
    private float _fManaRegen;
    // 마지막 공격자 — Die() 시 킬 집계에 사용(서버 전용)
    private ulong _lastAttacker;
    // 연속 데미지로 Die()가 중복 호출되는 것을 방지
    private bool _isDead;

    public override void OnNetworkSpawn()
    {
        if (hpSlider == null)
        {
            Debug.Log("Slider Null 발생! , Stat.cs");
            return;
        }

        if(IsServer)
        {
            pDamage     = Stat_Data.fAttackDamage;
            pResistance = Stat_Data.fResistance;
            pMaxHp      = Stat_Data.fMaxHp;
            pHp         = Stat_Data.fHp;

            // Player_Data일 때만 마나 초기화 — 적(Entity_Data)은 마나 없음 (시스템 경계 검증)
            if (Stat_Data is Player_Data playerData)
            {
                fMaxMana.Value = playerData.fMaxMana;
                fMana.Value    = playerData.fMaxMana; // 스폰 시 최대 마나로 시작
                _fManaRegen    = playerData.fManaRegen;
            }
        }

        // 비owner 머리 위 HP 슬라이더 바인딩 — owner는 canvas 숨김 후 PlayerHUD로 대체
        hpSlider.maxValue = fMaxHp.Value;
        hpSlider.value    = fHp.Value;

        fHp.OnValueChanged    += (_, next) => hpSlider.value    = next;
        fMaxHp.OnValueChanged += (_, next) => hpSlider.maxValue = next;
    }

    // owner 화면 HUD 바인딩 — Player_NetworkSpawn에서 스폰 시 1회 호출
    // 초기값 세팅 + NetworkVariable 구독으로 이후 변경사항 자동 반영
    public void BindOwnerHUD(PlayerHUD hud)
    {
        hud.SetName($"client : {OwnerClientId}");
        hud.SetHp(fHp.Value, fMaxHp.Value);
        hud.SetMana(fMana.Value, fMaxMana.Value);

        fHp.OnValueChanged    += (_, next) => hud.SetHp(next, fMaxHp.Value);
        fMaxHp.OnValueChanged += (_, next) => hud.SetHp(fHp.Value, next);
        fMana.OnValueChanged  += (_, next) => hud.SetMana(next, fMaxMana.Value);
    }

    // 서버 권위적 마나 소모 — 마나 부족 시 false(소모 없음), 충분 시 차감 후 true
    public bool TryConsumeMana(float cost , bool isConsume = true)
    {
        if (!IsServer) return false;
        if (fMana.Value < cost) return false;

        if(isConsume == true)
            fMana.Value -= cost;
            
        return true;
    }

    void Update()
    {
        // 서버에서만 마나 재생 — _fManaRegen은 서버에서만 초기화되므로 클라는 자동 스킵
        if (!IsServer || _fManaRegen <= 0f || fMana.Value >= fMaxMana.Value) return;
        fMana.Value = Mathf.Min(fMana.Value + _fManaRegen * Time.deltaTime, fMaxMana.Value);
    }

    // 공격자 기록 — TakeHit(서버)에서 데미지 적용 직전 호출
    public void SetLastAttacker(ulong clientId) => _lastAttacker = clientId;

    // 서버 전용 — HP 0 시 1회만 실행, 킬/데스를 ScoreManager에 집계
    void Die()
    {
        if (_isDead) return;
        _isDead = true;
        GameManager.Instance.scoreManager.RegisterKill(_lastAttacker, OwnerClientId);
    }
}
