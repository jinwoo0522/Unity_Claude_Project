using UnityEngine;

// 마법사 하체 마우스 스킬 — 진입 시 카메라 포워드 그라운드 히트 지점에 Storm 시전,
// 수평 이동은 차단하고 중력만 적용, 애니메이션이 끝나면 IDLE로 복귀
public class MagicianMouseSkillState : EntityState
{
    private Magician_Player _player;
    private EntityAnimator _aniController;
    private IEntityMovement _move;
    private IEntityRotate _rotate;
    private Player_Input _input;
    private LayerMask _groundMask;
    private Transform _headPos;
    readonly private IEffector _effector;
    private const float _fMaxRayDistance = 15f;   // 전환 조건과 동일한 사거리
    private const float _fSkillHeight = 3.5f;    // 레이 원점 눈높이 오프셋 (전환과 동일해야 함)

    private const float _fSkillTime = 0.7f;

    public MagicianMouseSkillState(Magician_Player player, LayerMask groundMask , Transform headPos)
    {
        _player = player;
        _aniController = player._aniController;
        _move = player._move;
        _rotate = player._rotate;
        _input = player._input;
        _groundMask = groundMask;
        _headPos = headPos;
        _effector = player._effector;
    }

    public override void Create()
    {
        // 마우스 스킬 애니메이션이 끝까지 재생되면 IDLE로 복귀
        TransitionList.Add(new StateToIdle_Player(_aniController));
        StateEvents.Add((_fSkillTime, CastStorm));
        StateEvents.Add((_fSkillTime + 0.1f, () => _effector.StopTrail((int)MAGICIAN.MagicianTrail.Left_Hand)));
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)MAGICIAN.StateType.MOUSE_SKILL;
        _effector.PlayTrail((int)MAGICIAN.MagicianTrail.Left_Hand);     // 왼손 트레일 시작
        _player._upperStateMachine.TransitionTo((ushort)ENTITY.UpperStateType.EMPTY);   // 상체 잠금
    }

    public override void Exit()
    {
        _player._upperStateMachine.TransitionTo((ushort)ENTITY.UpperStateType.IDLE);    // 상체 복귀
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        // 이동 입력은 무시하고 중력만 적용 (스킬 중 제자리 고정)
        _move.Gravity();
        _rotate.Rotate();
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
