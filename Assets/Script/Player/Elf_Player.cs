using UnityEngine;

public class Elf_Player : Player
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

       _stateMachine.CreateState((ushort)ENTITY.StateType.IDLE, new PlayerIdleState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.WALK, new PlayerWalkState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.RUN, new PlayerRunState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.JUMP, new ElfJumpState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.LAND, new PlayerLandState(this));

       _stateMachine.CreateAnyTransition(new AnyToJump_Player(_input ,_aniController, _move ));

       // 시작 시 IDLE 시작
       _stateMachine.TransitionTo((ushort)ENTITY.StateType.IDLE);
    }
}
