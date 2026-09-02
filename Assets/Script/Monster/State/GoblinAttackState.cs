using UnityEngine;
using UnityEngine.AI;

// 정지 거리 안에서의 공격 — 모션이 끝날 때까지 이동을 막고, 끝나면 대기로 돌아간다
public class GoblinAttackState : EntityState
{
    private IEntityMovement _move;
    private EntityAnimator _aniController;
    private NavMeshAgent _agent;
    private Transform _transform;
    public GoblinAttackState(Goblin goblin)
    {
        _move = goblin._move;
        _aniController = goblin._aniController;
        _agent = goblin._agent;
        _transform = goblin.transform;
    }

    public override void Create()
    {
        TransitionList.Add(new AttackToIdle_Goblin(_aniController));
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)ENTITY.StateType.ATTACK;
        // 앞으로 내딛는 이동량은 클립이 직접 만든다
        _aniController._animator.applyRootMotion = true;
    }

    public override void Exit()
    {
        _aniController._animator.applyRootMotion = false;
        _agent.nextPosition = _transform.transform.position;
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _move.Gravity();
    }
}
