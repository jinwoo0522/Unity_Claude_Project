using Unity.VisualScripting;
using UnityEngine;


public class Magician_Player : Player
{
    [SerializeField] Transform _rightHand;
    [SerializeField] Transform _LeftHand;
    [SerializeField] Transform _HeadPos;
    [SerializeField] private LayerMask _groundMask;   // 마우스 스킬 타겟 레이캐스트용 그라운드 레이어
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        CreateState();
        CreateUpperState();

        if (IsServer == false) return;

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
        _stateMachine.CreateState((ushort)MAGICIAN.StateType.MOUSE_SKILL, new MagicianMouseSkillState(this, _groundMask, _HeadPos));
        _stateMachine.CreateState((ushort)ENTITY.StateType.FROZEN, new EntityFrozenState(this, _upperStateMachine));
        _stateMachine.CreateState((ushort)ENTITY.StateType.DIE, new PlayerDieState(this));

        // 등록 순서가 곧 우선순위 — 사망이 최우선이다
        _stateMachine.CreateAnyTransition(new AnyToDie_Player(_stat, _damagable));
        _stateMachine.CreateAnyTransition(new AnyToJump_Player(_input, _aniController, _jump, _crowdController));
        _stateMachine.CreateAnyTransition(new AnyToAirborne_Entity(_crowdController));
        _stateMachine.CreateAnyTransition(new AnyToFrozen_Entity(_crowdController));

        _stateMachine.AddTransition((ushort)ENTITY.StateType.IDLE, new StateToMouseSkill_Magician(this, _groundMask, _HeadPos));
        _stateMachine.AddTransition((ushort)ENTITY.StateType.WALK, new StateToMouseSkill_Magician(this, _groundMask, _HeadPos));
        _stateMachine.AddTransition((ushort)ENTITY.StateType.RUN,  new StateToMouseSkill_Magician(this, _groundMask, _HeadPos));
    }

    void CreateUpperState()
    {
        _upperStateMachine.CreateState((ushort)ENTITY.UpperStateType.IDLE, new PlayerUpperIdleState(this));
        _upperStateMachine.CreateState((ushort)ENTITY.UpperStateType.HIT,  new PlayerHitState(this));
        _upperStateMachine.CreateState((ushort)ENTITY.UpperStateType.EMPTY, new PlayerUpperEmptyState(this));
        _upperStateMachine.CreateState((ushort)MAGICIAN.UpperStateType.ATTACK, new MagicainUpperAttackState(this, _rightHand));
        _upperStateMachine.CreateState((ushort)MAGICIAN.UpperStateType.QSKILL, new MagicianUpperQSkillState(this));

        // 피격 반응 — 서버에서 데미지 판정 후 IDamagable(Stat)을 통해 HIT 전환 트리거
        _upperStateMachine.CreateAnyTransition(new AnyToHit_Player(_upperAniController, _damagable));

        _upperStateMachine.AddTransition((ushort)ENTITY.UpperStateType.IDLE, new IdleToMagicianAttack_Magician(_input));
        _upperStateMachine.AddTransition((ushort)ENTITY.UpperStateType.IDLE, new IdleToMagicianQSkill_Magician(_input));
    }
}
