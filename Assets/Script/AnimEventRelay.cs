using UnityEngine;

// Animator가 있는 자식 오브젝트에 부착 — Animation Event를 부모로 중계
public class AnimEventRelay : MonoBehaviour
{
    private Player_UpperBody playerUpper;

    void Start()
    {
        playerUpper = GetComponentInParent<Player_UpperBody>();
    }

    // 애니메이션 클립 이벤트 진입점 — 이름은 클립의 이벤트 함수명과 일치해야 함
    void OnFireSkill()
    {
        // Player_UpperBody.FireSkill()로 중계 (루트 Animator 이동 후 이중 호출 방지)
        playerUpper?.FireSkill();
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
