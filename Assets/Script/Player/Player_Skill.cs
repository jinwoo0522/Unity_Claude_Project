using Unity.Netcode;
using UnityEngine;

// Q(Buff)·마우스 우클릭(Attack_Skill) 스킬 공통 베이스.
// 쿨타임·발동 가부 검증은 서버에서만 수행 (서버 권위적).
// HUD 표시용 클라 타이머는 서버가 발동을 확정한 뒤 owner에게 통지 → StartCooldown 호출.
public abstract class Player_Skill : NetworkBehaviour
{
    private const int SkillLayer = 2; // Animator Skill Layer index
    [SerializeField] protected float fAnimLerpSpeed = 3f;

    // 서브클래스(Golem_Skill)에서 deltaPosition 접근용
    protected Animator       anim;
    protected SkillCooldownUI _cooldownUI;
    private   Player_UpperBody playerUpper;


    // 서버 로컬 쿨타임 타임스탬프 — 슬롯별 마지막 사용 시간, 클라 입력 불신 원칙
    private readonly double[] _lastUseTimes = new double[2];
    // 현재 재생 중인 스킬 스테이트명 (종료 감지용, 서버 전용)
    private string _activeState;

    // owner 로컬 HUD — 씬 단일 오브젝트로 OnNetworkSpawn에서 탐색
    
    private NetworkVariable<float> net_SkillWeight = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // 스킬 레이어 weight > 0 → 스킬 발동 중
    public bool IsSkilling => net_SkillWeight.Value > 0f;

    private StatusEffect_Airborne _airborne;

    public override void OnNetworkSpawn()
    {
        // Animator가 루트로 이동됐으므로 GetComponent로 직접 참조
        anim        = GetComponent<Animator>();
        playerUpper = GetComponent<Player_UpperBody>();
        _airborne   = GetComponent<StatusEffect_Airborne>();

        // 스폰 시점 초기 weight 즉시 반영
        anim.SetLayerWeight(SkillLayer, net_SkillWeight.Value);
        net_SkillWeight.OnValueChanged = (_, next) =>
            anim.SetLayerWeight(SkillLayer, next);

        // owner만 HUD를 탐색해 슬롯별 쿨타임 길이를 등록한다.
        // 씬에 HUD가 1개뿐이므로 FindObjectOfType으로 충분.
        if (IsOwner)
        {
            _cooldownUI = FindAnyObjectByType<SkillCooldownUI>();
        }
    }

    // PlayerInput SendMessages — Q 키(Buff 액션)
    abstract protected void OnBuff();

    // PlayerInput SendMessages — 마우스 우클릭(Attack_Skill 액션)
    abstract protected void OnAttack_Skill();

    protected SkillData CheckCanUseSkill(SkillType tag, int slot)
    {
        SkillData data;
        
        data = GameManager.Instance.skillPool.GetSkillData(tag);

        if (data == null) {
            Debug.Log($"SkillData not found for tag: {tag}");
            return null;
        }

        double now = NetworkManager.ServerTime.Time;

        // 서버 검증: 쿨타임 / 스킬 중복 / 기본공격 중 / 피격 중 / 공중 중
        if (now - _lastUseTimes[slot] < data.fCooldown) return null;
        if (IsSkilling) return null;
        if (playerUpper != null && playerUpper.IsAttacking) return null;
        if (playerUpper != null && playerUpper.IsHit) return null;
        if (_airborne != null && _airborne.IsAirborne) return null;

        return data;
    }

    [ServerRpc]
    protected void UseSkill_ServerRpc(SkillType tag , Vector3 point = default) // 서버에서 스킬 사용 명령 수신
    {
        if(point == default(Vector3))
        {
            point = transform.position;
        }
        GameManager.Instance.skillPool
        .UseSkill(tag, point, transform.forward, OwnerClientId);
        return;
    }

    [ServerRpc]
    protected void Animation_Play_ServerRpc(SkillType tag , int slot)
    {
        SkillData data;
        data = GameManager.Instance.skillPool.GetSkillData(tag);

        if (data == null) {
            Debug.Log($"SkillData not found for tag: {tag}");
            return;
        }

        double now = NetworkManager.ServerTime.Time;
        net_SkillWeight.Value = 1f;
        _activeState = data.strSkillState;
        anim.Play(data.strSkillState, SkillLayer, 0f);
        PlaySkill_ClientRpc(data.strSkillState);

        _lastUseTimes[slot]   = now;
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
            net_SkillWeight.Value = Mathf.Clamp01(net_SkillWeight.Value -= Time.deltaTime * fAnimLerpSpeed);

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

    protected void SetCooldownLength(SkillType tag, int slot)
    {
        if (!IsOwner || _cooldownUI == null) return;
        float cooldown = GameManager.Instance.skillPool.GetSkillData(tag)?.fCooldown ?? 0f;
        _cooldownUI.SetCooldownLength(slot, cooldown);
    }
}
