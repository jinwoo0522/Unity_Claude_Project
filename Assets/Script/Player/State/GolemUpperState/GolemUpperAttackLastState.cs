using UnityEngine;

public class GolemUpperAttackLastState : EntityState
{
    EntityAnimator _upperAniController;
    IHitter _hitter;
    IEntityMovement _move;
    IEffector _effector;
    Stat _stat;
    MotionTrailer _motionTrailer;
    Vector3 vCentor = new Vector3(0f,1f,1.5f);
    Vector3 vHalfExtents = new Vector3(1.2f,0.5f,0.7f);
    const float fDashSpeed = 5f;
    const float fDashDistance = 1f;
    const float fHitDuration = 0.3f;   // 판정 지속시간(초)
    const float fAirbornePower = 13f;
    const float fAirborneDecay = 3f;
    const float fKnockbackPower = 25f;
    const float fKnockbackDecay = 12f;
    const float fTrailDuration = 0.1f; // 잔상 재생 시간(초)
    public GolemUpperAttackLastState(Golem_Player player)
    {
        _upperAniController = player._upperAniController;
        _hitter = player._hitter;
        _move = player._move;
        _effector = player._effector;
        _stat = player._stat;
        _motionTrailer = player._MotionTrailer;
    }
    public override void Create()
    {
        TransitionList.Add(new StateToIdle_Player(_upperAniController));
        StateEvents.Add((0.85f , EventFunc));
        StateEvents.Add((1.7f , EventFunc));
        StateEvents.Add((2f , () => _effector.StopTrail((int)GOLEM.GolemTrail.WEAPON_TRAIL)));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)GOLEM.UpperStateType.ATTACK_LAST;
        _effector.PlayTrail((int)GOLEM.GolemTrail.WEAPON_TRAIL);
    }

    public override void Exit()
    {
        _effector.StopEffect((int)GOLEM.GolemEffect.HIT_EFFECT);
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {

    }

    void EventFunc()
    {
        _move.Dash(fDashSpeed , fDashDistance);
        _hitter.DoHitCheck(vCentor , vHalfExtents , fHitDuration, HitHandler);
        _motionTrailer.Play_Trail(fTrailDuration);
    }

    void HitHandler(IHitter.HitInfo hitInfo)
    {
        hitInfo.Target.Hit(new IDamagable.DamageInfo{
            Damage = _stat.Get_Stat(Stat.STAT_TAG.DAMAGE), Attacker = _stat.transform, Point = hitInfo.Point});
        _effector.PlayEffect((int)GOLEM.GolemEffect.HIT_EFFECT , hitInfo.Point);

        if(hitInfo.Collider.TryGetComponent(out CrowdController crowdController)== true)
        {
            Vector3 vKnocbackDir = Vector3.Normalize(hitInfo.Point - _stat.gameObject.transform.position);
            crowdController.Apply(CrowdController.CC_TAG.AIRBORNE , new ICrowdControl.CCData(fAirbornePower , fAirborneDecay));
            crowdController.Apply(CrowdController.CC_TAG.KNOCKBACK, new ICrowdControl.CCData(fKnockbackPower, fKnockbackDecay, vKnocbackDir));
        }


    }
}
