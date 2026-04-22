using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_UpperBody : MonoBehaviour
{
    [Header("공격")]
    public float fAttackExitStart = 0.7f;
    [Header("스킬")]
    public SkillType currentSkill;  // 사용할 스킬 종류 (SkillPool에서 데이터 조회)
    public Transform rightHandBone; // 발사 위치 기준 뼈 (hand.r)
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

    // AnimEventRelay → 애니메이션 이벤트로 호출 (PlayerInput 인풋 아님)
    public void OnFireSkill()
    {
        if (SkillPool.Instance == null) return;
        Vector3 pos = rightHandBone != null ? rightHandBone.position : transform.position;
        SkillPool.Instance.Get(currentSkill, pos, transform.forward, this.GameObject());
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
