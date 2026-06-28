using UnityEngine;

// Golem 플레이어 — Elf와 동일한 공통 상태를 재사용하되 스킬 전환은 등록하지 않음
public class Golem_Player : Player
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        CreateState();
        CreateUpperState();

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
        _stateMachine.CreateState((ushort)ENTITY.StateType.JUMP,     new PlayerJumpState(this));
        _stateMachine.CreateState((ushort)ENTITY.StateType.LAND,     new PlayerLandState(this));
        _stateMachine.CreateState((ushort)ENTITY.StateType.AIRBORNE, new EntityAirborneState(this));

        _stateMachine.CreateAnyTransition(new AnyToJump_Player(_input, _aniController, _jump));
        _stateMachine.CreateAnyTransition(new AnyToAirborne_Entity(_crowdController));
        // Golem은 스킬 전환을 외부 주입하지 않으므로 ELF 상태로의 전환이 없음
    }

    void CreateUpperState()
    {
        _upperStateMachine.CreateState((ushort)ENTITY.UpperStateType.IDLE, new PlayerUpperIdleState(this));
        _upperStateMachine.CreateState((ushort)ENTITY.UpperStateType.HIT,  new PlayerHitState(this));

        _upperStateMachine.CreateAnyTransition(new AnyToHit_Player(_upperAniController, _stat));
    }
}
