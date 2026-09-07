using UnityEngine;

// 사망 상태 — 애니메이션 대신 래그돌로 쓰러지는 것을 표현한다
// 래그돌이 Animator·NavMeshAgent·CharacterController를 모두 끄므로 이동·중력은 여기서 다루지 않는다
public class MonsterDieState : EntityState
{
    private StateMachine _stateMachine;
    private RagdollController _rc;

    public MonsterDieState(Monster monster, RagdollController rc)
    {
        _stateMachine = monster._stateMachine;
        _rc = rc;
    }

    public override void Create()
    {
        // 사망은 종료 상태 — 빠져나가는 전환이 없다
    }

    public override void Enter()
    {
        _rc.SetRagdollState_ClientRpc(true);
        _rc.Explode(_rc.transform.position + Vector3.up * 0.5f, 20.0f, 2.0f);
        _stateMachine.Lock();
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
    }
}
