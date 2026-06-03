using UnityEngine;

// Golem 전용 스킬.
// 마우스 스킬(slot 1, Standing Melee Run Jump Attack)에 루트모션 적용.
// applyRootMotion은 항상 false — 루트모션을 서버 Update에서 직접 CCT에 적용해 위치 권위 유지.
public class Golem_Skill : Player_Skill
{
    private CharacterController cct;
    private bool                _rootMotionActive;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        cct = GetComponent<CharacterController>();
    }

    // 마우스 스킬(slot 1)만 루트모션 활성
    protected override void OnSkillStart(int slot)
    {
        _rootMotionActive = (slot == 1);
    }

    protected override void OnSkillEnd()
    {
        _rootMotionActive = false;
    }

    protected override void Update()
    {
        base.Update(); // 스킬 종료 감지
        ApplyRootMotion();
    }

    // 서버에서만 루트모션 소비 — 위치는 NetworkTransform으로 전 클라에 전파
    private void ApplyRootMotion()
    {
        if (!IsServer || !_rootMotionActive || cct == null) return;
        cct.Move(anim.deltaPosition);
    }
}
