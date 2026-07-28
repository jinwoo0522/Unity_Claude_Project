using UnityEngine;

public class PlayerWalkState : EntityState
{
    IEntityMoveInput _moveInput;
    IEntityInputState _inputState;
    IEntityMovement _playerMove;
    IEntityRotate _rotate;
    EntityAnimator _aniController;
    Vector2 vAnimLerp;
    public PlayerWalkState(Player player)
    {
        _moveInput = player._input;
        _playerMove = player._move;
        _rotate = player._rotate;
        _aniController = player._aniController;
        _inputState = player._input;
    }

    public override void Create()
    {
        TransitionList.Add(new WalkToIdle_Player(_inputState));
        TransitionList.Add(new WalkToRun_Player(_inputState));
    }
    public override void Enter()
    {
       _aniController._state.Value = (ushort)ENTITY.StateType.WALK;
    }

    public override void Exit()
    {
        vAnimLerp = Vector2.zero;
    }

    protected override void UpdateState(float fTimedelta , ushort curState)
    {
        // 이동 구현
        _playerMove.Move(_moveInput.MoveInput , _moveInput.isSprint);
        _playerMove.Gravity();
        _rotate.Rotate();

        // 애니메이션    
        Vector2 vTarget = _moveInput.MoveInput.magnitude > 0.1f ? _moveInput.MoveInput.normalized
        : Vector2.zero;

        vAnimLerp = Vector2.Lerp(vAnimLerp, vTarget, Time.deltaTime * 3f);

        _aniController._animator.SetFloat("MoveX", vAnimLerp.x);
        _aniController._animator.SetFloat("MoveZ", vAnimLerp.y);
    }
}
