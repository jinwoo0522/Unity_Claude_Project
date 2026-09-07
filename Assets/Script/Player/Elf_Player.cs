using Unity.VisualScripting;
using UnityEngine;

public class Elf_Player : Player
{
    [SerializeField] private MotionTrailer _motionTrailer;   // 인스펙터로 주입 — 상태에서 잔상 재생에 사용

    public IHitter _hitter {get; private set;}
    public MotionTrailer _MotionTrailer => _motionTrailer;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _hitter = GetComponent<IHitter>();

        CreateState();
        CreateUpperState();

        if(IsServer == false) return;
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
       _stateMachine.CreateState((ushort)ENTITY.StateType.FROZEN, new EntityFrozenState(this, _upperStateMachine));

       _stateMachine.CreateAnyTransition(new AnyToJump_Player(_input ,_aniController, _jump, _crowdController));
       _stateMachine.CreateAnyTransition(new AnyToAirborne_Entity(_crowdController));
       _stateMachine.CreateAnyTransition(new AnyToFrozen_Entity(_crowdController));

       // 공통 상태에서 분리된 Elf 전용 스킬 전환을 외부 주입 (Golem은 등록하지 않음)
       _stateMachine.AddTransition((ushort)ENTITY.StateType.IDLE, new StateToMouseAttack_Elf(_input, _upperStateMachine));
       _stateMachine.AddTransition((ushort)ENTITY.StateType.IDLE, new StateToQSkill_Elf(_input, _upperStateMachine));
       _stateMachine.AddTransition((ushort)ENTITY.StateType.WALK, new StateToMouseAttack_Elf(_input, _upperStateMachine));
       _stateMachine.AddTransition((ushort)ENTITY.StateType.WALK, new StateToQSkill_Elf(_input, _upperStateMachine));
       _stateMachine.AddTransition((ushort)ENTITY.StateType.RUN, new StateToMouseAttack_Elf(_input, _upperStateMachine));
       _stateMachine.AddTransition((ushort)ENTITY.StateType.RUN, new StateToQSkill_Elf(_input, _upperStateMachine));
    }

    void CreateUpperState()
    {
       _upperStateMachine.CreateState((ushort)ELF.UpperStateType.IDLE, new PlayerUpperIdleState(this));
       _upperStateMachine.CreateState((ushort)ELF.UpperStateType.HIT, new PlayerHitState(this));
       _upperStateMachine.CreateState((ushort)ENTITY.UpperStateType.EMPTY, new PlayerUpperEmptyState(this));
       _upperStateMachine.CreateState((ushort)ELF.UpperStateType.ATTACK_START, new ElfUpperAttackStartState(this));
       _upperStateMachine.CreateState((ushort)ELF.UpperStateType.ATTACK_MIDDLE, new ElfUpperAttackMiddleState(this));
       _upperStateMachine.CreateState((ushort)ELF.UpperStateType.ATTACK_LAST, new ElfUpperAttackLastState(this));

       _upperStateMachine.CreateAnyTransition(new AnyToHit_Player(_upperAniController , _damagable));

       // 공통 상체 Idle 상태에서 분리된 Elf 전용 공격 전환을 외부 주입
       _upperStateMachine.AddTransition((ushort)ELF.UpperStateType.IDLE, new IdleToAttackStart_Elf(_input));
    }

}
