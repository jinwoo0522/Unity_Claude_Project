using UnityEngine;


public class Magician_Player : Player
{
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

        _stateMachine.CreateAnyTransition(new AnyToJump_Player(_input, _aniController, _jump));
        _stateMachine.CreateAnyTransition(new AnyToAirborne_Entity(_crowdController));
    }

    void CreateUpperState()
    {
        _upperStateMachine.CreateState((ushort)ENTITY.UpperStateType.IDLE, new PlayerUpperIdleState(this));
        _upperStateMachine.CreateState((ushort)ENTITY.UpperStateType.HIT,  new PlayerHitState(this));
        _upperStateMachine.CreateState((ushort)MAGICIAN.UpperStateType.ATTACK, new MagicainUpperAttackState(this));

        // 피격 반응 — 서버에서 데미지 판정 후 IDamagable(Stat)을 통해 HIT 전환 트리거
        _upperStateMachine.CreateAnyTransition(new AnyToHit_Player(_upperAniController, _stat));

        _upperStateMachine.AddTransition((ushort)ENTITY.UpperStateType.IDLE, new IdleToMagicianAttack_Magician(_input));
    }
}
