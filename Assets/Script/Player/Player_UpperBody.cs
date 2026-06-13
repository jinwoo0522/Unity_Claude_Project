using UnityEngine;
using Unity.Netcode;
public abstract class Player_UpperBody : NetworkBehaviour
{
    [Header("공격")]
    public float fAttackExitStart = 0.7f;
    [Header("피격")]
    public float fHitExitStart = 0.7f;
    public bool IsHit      => state == UpperState.Hit      && anim.GetLayerWeight(UpperBodyLayer) > 0f;
    // 기본공격 중 여부 — UseSkill_ServerRpc에서 스킬 발동 차단에 사용
    public bool IsAttacking => state == UpperState.Attacking && anim.GetLayerWeight(UpperBodyLayer) > 0f;

    private enum UpperState { Attacking, Hit }
    private Animator      anim;
    private Player_Skill  skill;
    private Player_Status _status;
    private Stat          _stat;
    private const int UpperBodyLayer = 1;
    private UpperState state;

    private NetworkVariable<float> net_AnimWeight = new NetworkVariable<float>(
    0f, 
    NetworkVariableReadPermission.Everyone, 
    NetworkVariableWritePermission.Server);

     private NetworkVariable<float> net_AccLerpTime = new NetworkVariable<float>(
    0f, 
    NetworkVariableReadPermission.Everyone, 
    NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        // Animator가 루트로 이동됐으므로 GetComponent로 직접 참조
        anim    = GetComponent<Animator>();
        skill   = GetComponent<Player_Skill>();
        _status = GetComponent<Player_Status>();
        _stat   = GetComponent<Stat>();

        anim.SetLayerWeight(UpperBodyLayer, net_AnimWeight.Value);
        net_AnimWeight.OnValueChanged = (pre, next) =>
        {
            anim.SetLayerWeight(UpperBodyLayer, next);
        };
     }

    void Update()
    {
        if (anim.GetLayerWeight(UpperBodyLayer) <= 0f) return;

        if(IsServer == true)
        {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(UpperBodyLayer);

            _ = state switch
            {
                UpperState.Attacking => PlayUpper(fAttackExitStart, "UpperAttack", info),
                UpperState.Hit       => PlayUpper(fHitExitStart,    "UpperHit",    info),
                _                    => 0
            };
        }
    }

    // 서브클래스(Magician_UpperBody)에서 override — 기본값 0은 골렘 기본공격(마나 소모 없음)
    protected virtual float GetBasicAttackManaCost() => 0f;

    virtual public void FireSkill() => NormalAttack();
    virtual public void OnAttackHitboxOn() {}
    virtual public void OnAttackHitboxOff() {}

    // 캐릭터별 공격 구현
    virtual public void NormalAttack()
    {
    }
    [ServerRpc]
    virtual public void NormalAttack_ServerRpc(Vector3 pos = default, Vector3 dir = default)
    {
    }

    void OnAttack()
    {
        if(IsOwner == false) return;

        if (IsHit) return;
        if (skill != null && skill.IsSkilling) return;            // 스킬 중 기본공격 차단
        if (_status.IsFrozen) return;                             // 빙결 중 기본공격 차단
        if (anim.GetLayerWeight(UpperBodyLayer) > 0f) return;
        // 마나 부족 시 스윙 애니 포함 전체 차단 — 서버는 NormalAttack_ServerRpc에서 TryConsumeMana로 최종 확정
        if (_stat != null && _stat.pMana < GetBasicAttackManaCost()) return;
        StartUpper("UpperAttack", UpperState.Attacking);
    }

    // 서버 전용 — 피격: 데미지 집계 후 체력 감소 + 피격 애니메이션
    public void TakeHit(float damage, ulong attackerClientId, bool isHitAni = true)
    {
        if (!IsServer) return;

        Stat stat = GetComponent<Stat>();
        stat.SetLastAttacker(attackerClientId);
        GameManager.Instance.scoreManager.AddDamage(attackerClientId, damage);
        stat.pHp = -damage;                          // 체력 감소(서버 권위)

        Debug.Log(isHitAni + "빙결 히트 애니 플래그");
        
        if(isHitAni == true)
        {
            anim.Play("UpperHit", UpperBodyLayer, 0f);    // 서버 즉시 재생
            PlayHitAnim_ClientRpc();                       // 전 클라 동기화
            state = UpperState.Hit;
            net_AccLerpTime.Value = 1f;
            net_AnimWeight.Value = 1f;                    // 상부 레이어 활성(전 클라 동기화)
        }

    }

    [ClientRpc]
    void PlayHitAnim_ClientRpc()
    {
        state = UpperState.Hit;          // 오너 IsHit 게이트 즉시 true
        if (IsServer) return;            // 호스트는 이미 재생
        anim.Play("UpperHit", UpperBodyLayer, 0f);
    }

    void StartUpper(string stateName, UpperState nextState)
    {
        state = nextState;
        anim.Play(stateName, UpperBodyLayer, 0f);
        SubmitAnimInfo_ServerRpc(stateName, nextState , 1f);
    }

    int PlayUpper(float fCurExitStart, string strCurState, AnimatorStateInfo info)
    {
        if (info.IsName(strCurState) && info.normalizedTime >= fCurExitStart)
            FadeWeight(1f / (1f - fCurExitStart));


        return 1;
    }

    void FadeWeight(float speed)
    {
        net_AnimWeight.Value = Mathf.Clamp01(net_AccLerpTime.Value -= Time.deltaTime * speed);
    }

    
    [ServerRpc]
    void SubmitAnimInfo_ServerRpc(string stateName, UpperState nextState, float input)
    {
        state = nextState;
        net_AccLerpTime.Value = 1f;
        net_AnimWeight.Value = input;
        anim.Play(stateName, UpperBodyLayer, 0f);
        SyncUpperAnim_ClientRpc(stateName);
    }

//NetworkObject가 붙어있는 객체의 함수를 실행시키는 것임
//예를들어 Player A가 ClientRpc로 함수를 실행햇다 치면 
// 모든 클라에 Player A의 객체에서 함수가 실행되는거지
// 같은 스크립트가 붙어잇는 모든 객체가 저걸 호출하는게 아니다.
    [ClientRpc]
    void SyncUpperAnim_ClientRpc(string stateName)
    {
        if (IsOwner) return;
        anim.Play(stateName, UpperBodyLayer, 0f);
    }

}
