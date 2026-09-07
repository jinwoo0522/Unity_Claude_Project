using Unity.VisualScripting;
using UnityEngine;

// Golem 플레이어 — Elf와 동일한 공통 상태를 재사용하되 스킬 전환은 등록하지 않음
public class Golem_Player : Player
{
    [SerializeField] private Transform _bodyPosition;
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

        _stateMachine.TransitionTo((ushort)ENTITY.StateType.IDLE);
        _upperStateMachine.TransitionTo((ushort)ENTITY.UpperStateType.IDLE);
    }

    protected override void Update()
    {
        base.Update();
    }

    void CreateState()
    {
        _stateMachine.CreateState((ushort)ENTITY.StateType.IDLE,     new PlayerIdleState(this));
        _stateMachine.CreateState((ushort)ENTITY.StateType.WALK,     new PlayerWalkState(this));
        _stateMachine.CreateState((ushort)ENTITY.StateType.RUN,      new PlayerRunState(this));
        _stateMachine.CreateState((ushort)ENTITY.StateType.JUMP,     new PlayerJumpState(this, 0.1f));
        _stateMachine.CreateState((ushort)ENTITY.StateType.LAND,     new PlayerLandState(this));
        _stateMachine.CreateState((ushort)ENTITY.StateType.AIRBORNE, new EntityAirborneState(this));
        _stateMachine.CreateState((ushort)GOLEM.StateType.Q_SKILL, new GolemQSkillState(this, _bodyPosition));
        _stateMachine.CreateState((ushort)GOLEM.StateType.MOUSE_SKILL, new GolemMouseSkillState(this));
        _stateMachine.CreateState((ushort)ENTITY.StateType.FROZEN, new EntityFrozenState(this, _upperStateMachine));


        _stateMachine.CreateAnyTransition(new AnyToJump_Player(_input, _aniController, _jump, _crowdController));
        _stateMachine.CreateAnyTransition(new AnyToAirborne_Entity(_crowdController));
        _stateMachine.CreateAnyTransition(new AnyToFrozen_Entity(_crowdController));


        _stateMachine.AddTransition((ushort)ENTITY.StateType.IDLE, new StateToMouseAttack_Golem(_input, _upperStateMachine));
       _stateMachine.AddTransition((ushort)ENTITY.StateType.IDLE, new StateToQSkill_Golem(_input, _upperStateMachine));
       _stateMachine.AddTransition((ushort)ENTITY.StateType.WALK, new StateToMouseAttack_Golem(_input, _upperStateMachine));
       _stateMachine.AddTransition((ushort)ENTITY.StateType.WALK, new StateToQSkill_Golem(_input, _upperStateMachine));
       _stateMachine.AddTransition((ushort)ENTITY.StateType.RUN, new StateToMouseAttack_Golem(_input, _upperStateMachine));
       _stateMachine.AddTransition((ushort)ENTITY.StateType.RUN, new StateToQSkill_Golem(_input, _upperStateMachine));
    }

    void CreateUpperState()
    {
        _upperStateMachine.CreateState((ushort)ENTITY.UpperStateType.IDLE, new PlayerUpperIdleState(this));
        _upperStateMachine.CreateState((ushort)ENTITY.UpperStateType.HIT,  new PlayerHitState(this));
        _upperStateMachine.CreateState((ushort)ENTITY.UpperStateType.EMPTY, new PlayerUpperEmptyState(this));
        _upperStateMachine.CreateState((ushort)GOLEM.UpperStateType.ATTACK_START,  new GolemUpperAttackStartState(this));
        _upperStateMachine.CreateState((ushort)GOLEM.UpperStateType.ATTACK_LAST,  new GolemUpperAttackLastState(this));

        _upperStateMachine.CreateAnyTransition(new AnyToHit_Player(_upperAniController, _damagable));

        _upperStateMachine.AddTransition((ushort)GOLEM.UpperStateType.IDLE, new IdleToAttackStart_Golem(_input));
    }
}
