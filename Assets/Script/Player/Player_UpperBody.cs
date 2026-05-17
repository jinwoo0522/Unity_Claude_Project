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
    private float fAccLerpTime;
    private UpperState state;

    public override void OnNetworkSpawn()
    {
        anim = GetComponentInChildren<Animator>();
        anim.SetLayerWeight(UpperBodyLayer, 0f);
    }

    void Update()
    {
        if(IsOwner == false) return;
        
        if (anim.GetLayerWeight(UpperBodyLayer) <= 0f) return;

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(UpperBodyLayer);

        _ = state switch
        {
            UpperState.Attacking => PlayUpper(fAttackExitStart, "UpperAttack", info),
            UpperState.Hit       => PlayUpper(fHitExitStart,    "UpperHit",    info),
            _                    => 0
        };
    }

    // AnimEventRelay → 애니메이션 이벤트로 호출
    public void OnFireSkill() => NormalAttack();

    // 자식에서 반드시 재정의 — 캐릭터별 공격 구현
    public abstract void NormalAttack();

    void OnAttack()
    {
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
        fAccLerpTime = 1f;
        anim.SetLayerWeight(UpperBodyLayer, 1f);
        anim.Play(stateName, UpperBodyLayer, 0f);
    }

    int PlayUpper(float fCurExitStart, string strCurState, AnimatorStateInfo info)
    {
        if (info.IsName(strCurState) && info.normalizedTime >= fCurExitStart)
            FadeWeight(1f / (1f - fCurExitStart));
        return 1;
    }

    void FadeWeight(float speed)
    {
        float w = Mathf.Clamp01(fAccLerpTime -= Time.deltaTime * speed);
        anim.SetLayerWeight(UpperBodyLayer, w);
    }
}
