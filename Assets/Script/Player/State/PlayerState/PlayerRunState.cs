using Unity.Netcode.Components;
using UnityEngine;

public class PlayerRunState : EntityState
{
    IEntityMoveInput _moveInput;
    PlayerMovement _playerMove;
    IEntityInputState _inputState;
    EntityAnimator _aniController;
    Vector2 vAnimLerp;
    public PlayerRunState(Player player)
    {
        _moveInput = player._input;
        _playerMove = player._move;
        _inputState= player._input;
        _aniController = player._aniController;
    }

    public override void Create()
    {
        //트랜지션을 생성.
        TransitionList.Add(new RunToWalk_Player(_inputState)); 
        TransitionList.Add(new RunToIdle_Player(_inputState)); 
    }
    public override void Enter()
    {
        _aniController._state.Value = (ushort)ENTITY.StateType.RUN;
    }

    public override void Exit()
    {
        vAnimLerp = Vector2.zero;
    }

    protected override void UpdateState(float fTimedelta , ushort curState)
    {
        // 이동 구현
        _playerMove.PlayerMove(_moveInput.MoveInput , _moveInput.isSprint);
        _playerMove.Gravity();

        // 애니메이션
        Vector2 vTarget = _moveInput.MoveInput.magnitude > 0.1f ? _moveInput.MoveInput.normalized
        : Vector2.zero;

        vAnimLerp = Vector2.Lerp(vAnimLerp, vTarget, Time.deltaTime * 3f);

        _aniController._animator.SetFloat("MoveX", vAnimLerp.x);
        _aniController._animator.SetFloat("MoveZ", vAnimLerp.y);
    }
}
