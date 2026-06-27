using Unity.VisualScripting;
using UnityEngine;

public class Elf_Player : Player
{

    public IHitter _hitter {get; private set;}
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _hitter = GetComponent<IHitter>();

        CreateState();
        CreateUpperState();

       // 시작 시 IDLE 시작
       _stateMachine.TransitionTo((ushort)ENTITY.StateType.IDLE);
       _upperStateMachine.TransitionTo((ushort)ENTITY.StateType.IDLE);
    }

    protected override void Update()
    {
        base.Update();
    }

    void CreateState()
    {
       _stateMachine.CreateState((ushort)ENTITY.StateType.IDLE, new PlayerIdleState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.WALK, new PlayerWalkState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.RUN, new PlayerRunState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.JUMP, new ElfJumpState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.LAND, new PlayerLandState(this));
       _stateMachine.CreateState((ushort)ELF.StateType.MOUSE_SKILL, new ElfMouseSkillState(this));
       _stateMachine.CreateState((ushort)ELF.StateType.Q_SKILL, new ElfQSkillState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.AIRBORNE, new EntityAirborneState(this));

       _stateMachine.CreateAnyTransition(new AnyToJump_Player(_input ,_aniController, _jump));
       _stateMachine.CreateAnyTransition(new AnyToAirborne_Entity(_crowdController));
    }

    void CreateUpperState()
    {
       _upperStateMachine.CreateState((ushort)ELF.UpperStateType.IDLE, new PlayerUpperIdleState(this));
       _upperStateMachine.CreateState((ushort)ELF.UpperStateType.HIT, new PlayerHitState(this));
       _upperStateMachine.CreateState((ushort)ELF.UpperStateType.ATTACK_START, new ElfUpperAttackStartState(this));
       _upperStateMachine.CreateState((ushort)ELF.UpperStateType.ATTACK_MIDDLE, new ElfUpperAttackMiddleState(this));
       _upperStateMachine.CreateState((ushort)ELF.UpperStateType.ATTACK_LAST, new ElfUpperAttackLastState(this));

       _upperStateMachine.CreateAnyTransition(new AnyToHit_Player(_upperAniController , _stat));
    }

}
