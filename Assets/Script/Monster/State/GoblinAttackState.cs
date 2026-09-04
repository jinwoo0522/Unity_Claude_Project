using UnityEngine;
using UnityEngine.AI;

// 정지 거리 안에서의 공격 — 모션이 끝날 때까지 이동을 막고, 끝나면 대기로 돌아간다
public class GoblinAttackState : EntityState
{
    private IEntityMovement _move;
    private EntityAnimator _aniController;
    private NavMeshAgent _agent;
    private Transform _transform;
    private IHitter _hitter;
    private Stat _stat;
    Vector3 vCentor = new Vector3(0f,1f,1.5f);
    Vector3 vHalfExtents = new Vector3(1.2f,0.5f,0.7f);
    const float fHitDuration = 0.3f;   // 판정 지속시간
    const float fKnockbackPower = 18f;
    const float fKnockbackDecay = 9f;
    public GoblinAttackState(Goblin goblin)
    {
        _move = goblin._move;
        _aniController = goblin._aniController;
        _agent = goblin._agent;
        _transform = goblin.transform;
        _hitter = goblin._hitter;
        _stat = goblin._stat;   
    }

    public override void Create()
    {
        TransitionList.Add(new AttackToIdle_Goblin(_aniController));
        StateEvents.Add((0.6f , EventFunc));
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
    void EventFunc()
    {
        _hitter.DoHitCheck(vCentor , vHalfExtents , fHitDuration, HitHandler, false);
    }

    void HitHandler(IHitter.HitInfo hitInfo)
    {
        hitInfo.Target.Hit(_stat.Get_Stat(Stat.STAT_TAG.DAMAGE));

        Vector3 vKnocbackDir = Vector3.Normalize(hitInfo.Point - _stat.gameObject.transform.position);
        
        if(hitInfo.Collider.TryGetComponent(out CrowdController crowdController)== true)
            crowdController.Apply(CrowdController.CC_TAG.KNOCKBACK, 
                new ICrowdControl.CCData(fKnockbackPower, fKnockbackDecay, vKnocbackDir));
    }
}
