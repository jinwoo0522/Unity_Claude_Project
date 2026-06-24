using UnityEngine;

public class ElfUpperAttackStartState : EntityState
{
    EntityAnimator _upperAniController;
    IEntityInputState _input;
    IHitter _hitter;

    Vector3 vCentor = new Vector3(0f,1f,0.9f);
    Vector3 vHalfExtents = new Vector3(0.25f,0.25f,0.5f);

    public ElfUpperAttackStartState(Elf_Player player)
    {
        _upperAniController = player._upperAniController;
        _input = player._input;
        _hitter = player._hitter;
    }
    public override void Create()
    {
        TransitionList.Add(new AttackStartToMiddle_Elf(_input, _upperAniController ));
        TransitionList.Add(new StateToIdle_Player(_upperAniController , 1f));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)ELF.UpperStateType.ATTACK_START;
    }

    public override void Exit()
    {

    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _hitter.DoHitCheck(vCentor , vHalfExtents);
    }
}
