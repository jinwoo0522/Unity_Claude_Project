using UnityEngine;
using Unity.Netcode;
public abstract class Player_UpperBody : NetworkBehaviour
{
    [Header("공격")]
    public float fAttackExitStart = 0.7f;
    [Header("피격")]
    public float fHitExitStart = 0.7f;
    public bool IsHit => state == UpperState.Hit && anim.GetLayerWeight(UpperBodyLayer) > 0f;

    private enum UpperState { Attacking, Hit }
    private Animator anim;
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
        anim = GetComponentInChildren<Animator>();

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

    // AnimEventRelay → 애니메이션 이벤트로 호출
    virtual public void OnFireSkill() => NormalAttack();
    virtual public void OnAttackHitboxOn() {}
    virtual public void OnAttackHitboxOff() {}

    // 캐릭터별 공격 구현
    virtual public void NormalAttack()
    {
    }
    [ServerRpc]
    virtual public void NormalAttack_ServerRpc()
    {
    }

    void OnAttack()
    {
        if(IsOwner == false)  return;
        
        if (IsHit) return;
        if (anim.GetLayerWeight(UpperBodyLayer) > 0f) return;
            StartUpper("UpperAttack", UpperState.Attacking);
    }

    [ContextMenu("TakeHit 디버그")]
    public void TakeHit()
    {
        if (IsHit) return;
        StartUpper("UpperHit", UpperState.Hit);
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
