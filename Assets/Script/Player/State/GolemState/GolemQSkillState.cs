using UnityEngine;

public class GolemQSkillState : EntityState
{
    
    EntityAnimator _aniController;
    IEntityMovement _move;
    IEntityInputState _input;
    IEffector _effector;
    public GolemQSkillState(Golem_Player player)
    {
        _aniController = player._aniController;
        _move = player._move;
        _input = player._input;
        _effector = player._effector;
    }
    public override void Create()
    {
        TransitionList.Add(new StateToIdle_Player(_aniController));
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)GOLEM.StateType.Q_SKILL;
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _move.Gravity();
    }

}
