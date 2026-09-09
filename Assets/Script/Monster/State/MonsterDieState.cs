using UnityEngine;

// 사망 상태 — 애니메이션 대신 래그돌로 쓰러지는 것을 표현한다
// 래그돌이 Animator·NavMeshAgent·CharacterController를 모두 끄므로 이동·중력은 여기서 다루지 않는다
public class MonsterDieState : EntityState
{
    private StateMachine _stateMachine;
    private RagdollController _rc;
    private MaterialChanger _matChanger;

    private const float _fDissolveDelay = 5.5f;   // 쓰러진 자세를 보여준 뒤 디졸브를 시작하기까지의 시간

    public MonsterDieState(Monster monster, RagdollController rc)
    {
        _stateMachine = monster._stateMachine;
        _rc = rc;
        _matChanger = monster.GetComponent<MaterialChanger>();
    }

    public override void Create()
    {
        // 사망은 종료 상태 — 빠져나가는 전환이 없다
        StateEvents.Add((_fDissolveDelay, () => _matChanger.Change(MaterialChanger.MAT_TAG.DISSOLVE)));
    }

    public override void Enter()
    {
        _rc.SetRagdollState_ClientRpc(true);
        _rc.Explode(_rc.transform.position + Vector3.up * 0.5f, 20.0f, 2.0f);
        // 전환만 막는다 — 갱신까지 멈추면 디졸브 예약 이벤트도 같이 멈춘다
        _stateMachine.LockTransition();
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
    }
}
