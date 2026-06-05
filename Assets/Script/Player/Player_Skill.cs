using Unity.Netcode;
using UnityEngine;

// Q(Buff)·마우스 우클릭(Attack_Skill) 스킬 공통 베이스.
// 쿨타임·발동 가부 검증은 서버에서만 수행 (서버 권위적).
// HUD 표시용 클라 타이머는 서버가 발동을 확정한 뒤 owner에게 통지 → StartCooldown 호출.
public abstract class Player_Skill : NetworkBehaviour
{
    private const int SkillLayer = 2; // Animator Skill Layer index

    [SerializeField] protected SkillData qSkillData;
    [SerializeField] protected SkillData mouseSkillData;

    [SerializeField] protected float fAnimmLerpSpeed = 3f;

    // 서브클래스(Golem_Skill)에서 deltaPosition 접근용
    protected Animator       anim;
    private   Player_UpperBody playerUpper;

    // 서버 로컬 쿨타임 타임스탬프 — 슬롯별 마지막 사용 시간, 클라 입력 불신 원칙
    private readonly double[] _lastUseTimes = new double[2];
    // 현재 재생 중인 스킬 스테이트명 (종료 감지용, 서버 전용)
    private string _activeState;

    // owner 로컬 HUD — 씬 단일 오브젝트로 OnNetworkSpawn에서 탐색
    private SkillCooldownUI _cooldownUI;

    private NetworkVariable<float> net_SkillWeight = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // 스킬 레이어 weight > 0 → 스킬 발동 중
    public bool IsSkilling => net_SkillWeight.Value > 0f;

    public override void OnNetworkSpawn()
    {
        // Animator가 루트로 이동됐으므로 GetComponent로 직접 참조
        anim        = GetComponent<Animator>();
        playerUpper = GetComponent<Player_UpperBody>();

        // 스폰 시점 초기 weight 즉시 반영
        anim.SetLayerWeight(SkillLayer, net_SkillWeight.Value);
        net_SkillWeight.OnValueChanged = (_, next) =>
            anim.SetLayerWeight(SkillLayer, next);

        // owner만 HUD를 탐색해 슬롯별 쿨타임 길이를 등록한다.
        // 씬에 HUD가 1개뿐이므로 FindObjectOfType으로 충분.
        if (IsOwner)
        {
            _cooldownUI = FindObjectOfType<SkillCooldownUI>();
            if (_cooldownUI != null)
            {
                _cooldownUI.SetCooldownLength(0, qSkillData != null ? qSkillData.fCooldown : 0f);
                _cooldownUI.SetCooldownLength(1, mouseSkillData != null ? mouseSkillData.fCooldown : 0f);
            }
        }
    }

    // PlayerInput SendMessages — Q 키(Buff 액션)
    void OnBuff()
    {
        if (!IsOwner) return;
        UseSkill_ServerRpc(0);
    }

    // PlayerInput SendMessages — 마우스 우클릭(Attack_Skill 액션)
    void OnAttack_Skill()
    {
        if (!IsOwner) return;
        UseSkill_ServerRpc(1);
    }

    [ServerRpc]
    private void UseSkill_ServerRpc(int slot)
    {
        SkillData data = GetSkillData(slot);
        if (data == null) return;

        double now = NetworkManager.ServerTime.Time;

        // 서버 검증: 쿨타임 / 스킬 중복 / 기본공격 중 / 피격 중
        if (now - _lastUseTimes[slot] < data.fCooldown) return;
        if (IsSkilling) return;
        if (playerUpper != null && playerUpper.IsAttacking) return;
        if (playerUpper != null && playerUpper.IsHit) return;

        _lastUseTimes[slot]   = now;
        _activeState          = data.strSkillState;
        net_SkillWeight.Value = 1f;
        anim.Play(data.strSkillState, SkillLayer, 0f);
        PlaySkill_ClientRpc(data.strSkillState);
        OnSkillStart(slot);
        // 서버가 발동을 확정한 뒤에만 owner에게 HUD 타이머 시작을 통지
        NotifyCooldown_ClientRpc(slot);
    }

    // 오너·기타 클라이언트에 스킬 애니 동기화
    [ClientRpc]
    private void PlaySkill_ClientRpc(string stateName)
    {
        // 서버(호스트 포함)는 ServerRpc에서 이미 재생됨
        if (IsServer) return;
        anim.Play(stateName, SkillLayer, 0f);
    }

    // owner에게만 HUD 쿨타임 타이머 시작을 통지 — 서버 확정 후에만 호출되므로 UI 오작동 없음
    [ClientRpc]
    private void NotifyCooldown_ClientRpc(int slot)
    {
        if (!IsOwner) return;
        _cooldownUI?.StartCooldown(slot);
    }

    // 서버 전용 — 스킬 클립 95% 도달 시 weight 복귀
    protected virtual void Update()
    {
        if (!IsServer || net_SkillWeight.Value <= 0f || _activeState == null) return;

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(SkillLayer);
        if (info.IsName(_activeState) && info.normalizedTime >= 0.7f)
        {
            net_SkillWeight.Value = Mathf.Clamp01(net_SkillWeight.Value -= Time.deltaTime * fAnimmLerpSpeed);

            if(net_SkillWeight.Value <= 0f)
                OnSkillEnd();
        }
    }

    // 서브클래스 훅 — 스킬 시작/종료 시 추가 처리용
    protected virtual void OnSkillStart(int slot) { }
    protected virtual void OnSkillEnd()
    {
       _activeState          = null; 
    }

    private SkillData GetSkillData(int slot) => slot == 0 ? qSkillData : mouseSkillData;
}
