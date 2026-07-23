using UnityEngine;

// 마법사 하체 마우스 스킬 — 진입 시 카메라 포워드 그라운드 히트 지점에 Storm 시전,
// 수평 이동은 차단하고 중력만 적용, 애니메이션이 끝나면 IDLE로 복귀
public class MagicianMouseSkillState : EntityState
{
    private Magician_Player _player;
    private EntityAnimator _aniController;
    private IEntityMovement _move;
    private Player_Input _input;
    private LayerMask _groundMask;
    private Transform _headPos;
    private const float _fMaxRayDistance = 10f;   // 전환 조건과 동일한 사거리
    private const float _fSkillHeight = 3.5f;    // 레이 원점 눈높이 오프셋 (전환과 동일해야 함)

    public MagicianMouseSkillState(Magician_Player player, LayerMask groundMask , Transform headPos)
    {
        _player = player;
        _aniController = player._aniController;
        _move = player._move;
        _input = player._input;
        _groundMask = groundMask;
        _headPos = headPos;
    }

    public override void Create()
    {
        // 마우스 스킬 애니메이션이 끝까지 재생되면 IDLE로 복귀
        TransitionList.Add(new StateToIdle_Player(_aniController));
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)MAGICIAN.StateType.MOUSE_SKILL;
        CastStorm();
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        // 이동 입력은 무시하고 중력만 적용 (스킬 중 제자리 고정)
        _move.Gravity();
    }

    // 카메라 포워드 방향으로 그라운드 레이를 다시 쏘아 첫 히트 지점에 Storm 시전
    void CastStorm()
    {
        if (Physics.Raycast(_headPos.position, _input.AimDir, out RaycastHit hit, _fMaxRayDistance, _groundMask) == false)
            return;

        Vector3 vSkillPos = new Vector3(0f , _fSkillHeight , 0f) + hit.point;

        GameManager.Instance.skillFactory.Create(
            NetworkObjectType.STORM, vSkillPos, Vector2.zero, _player.OwnerClientId, _player.gameObject);
    }
}
