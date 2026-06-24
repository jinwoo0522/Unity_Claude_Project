using UnityEngine;

public class ElfUpperAttackMiddleState : EntityState
{
    EntityAnimator _upperAniController;
    
    IEntityInputState _input;

    IHitter _hitter;

    public ElfUpperAttackMiddleState(Elf_Player player)
    {
        _upperAniController = player._upperAniController;
        _input = player._input;
        _hitter = player._hitter;
    }
    public override void Create()
    {
        TransitionList.Add(new AttackMiddleToLast_Elf(_input, _upperAniController));
        TransitionList.Add(new StateToIdle_Player(_upperAniController , 1f));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)ELF.UpperStateType.ATTACK_MIDDLE;
    }

    public override void Exit()
    {

    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {

    }
}
