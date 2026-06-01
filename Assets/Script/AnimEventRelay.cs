using UnityEngine;

// Animator가 있는 자식 오브젝트에 부착 — Animation Event를 부모로 중계
public class AnimEventRelay : MonoBehaviour
{
    private Player_UpperBody playerUpper;

    void Start()
    {
        playerUpper = GetComponentInParent<Player_UpperBody>();
    }

    void OnFireSkill()
    {
        playerUpper?.OnFireSkill();
    }

    void OnGolemAttackStart()
    {
        playerUpper?.OnAttackHitboxOn();
    }

    void OnGolemAttackEnd()
    {
        playerUpper?.OnAttackHitboxOff();
    }
}
