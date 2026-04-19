using UnityEngine;
using UnityEngine.InputSystem;

public class Player_UpperBody : MonoBehaviour
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

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        anim.SetLayerWeight(UpperBodyLayer, 0f);
    }

    void Update()
    {
        if (anim.GetLayerWeight(UpperBodyLayer) <= 0f) return;

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(UpperBodyLayer);

        _ = state switch
        {
            UpperState.Attacking => PlayUpper(fAttackExitStart, "UpperAttack", info),
            UpperState.Hit       => PlayUpper(fHitExitStart,    "UpperHit",    info),
            _                    => 0
        };
    }

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

    int PlayUpper(float fCurExitStart, string strCurState , AnimatorStateInfo info)
    {
        if(info.IsName(strCurState) && info.normalizedTime >= fCurExitStart)
            FadeWeight(1f / (1f - fCurExitStart));

        return 1;
    }

    void FadeWeight(float speed)
    {
        float w = Mathf.Clamp01(fAccLerpTime -= Time.deltaTime * speed);
        anim.SetLayerWeight(UpperBodyLayer, w);
    }
}
