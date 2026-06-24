using UnityEngine;

public class ElfUpperAttackLastState : EntityState
{
    EntityAnimator _upperAniController;
    
    public ElfUpperAttackLastState(Player player)
    {
        _upperAniController = player._upperAniController;
    }
    public override void Create()
    {
        TransitionList.Add(new StateToIdle_Player(_upperAniController));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)ELF.UpperStateType.ATTACK_LAST;
    }

    public override void Exit()
    {

    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {

    }
}
