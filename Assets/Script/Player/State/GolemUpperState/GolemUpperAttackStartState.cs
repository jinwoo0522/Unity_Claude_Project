using UnityEngine;

public class GolemUpperAttackStartState : EntityState
{
    EntityAnimator _upperAniController;
    IEntityInputState _input;
    IHitter _hitter;
    IEffector _effector;
    Stat _stat;
    Vector3 vCentor = new Vector3(0f,1f,1.5f);
    Vector3 vHalfExtents = new Vector3(1.2f,0.5f,0.7f);

    const float fHitDuration = 0.5f;   // 판정 지속시간(초)
    const float fKnockbackPower = 25f;
    const float fKnockbackDecay = 12f;

    public GolemUpperAttackStartState(Golem_Player player)
    {
        _upperAniController = player._upperAniController;
        _input = player._input;
        _hitter = player._hitter;
        _effector = player._effector;
        _stat = player._stat;
    }
    public override void Create()
    {
        TransitionList.Add(new AttackStartToLast_Golem(_input, _upperAniController));
        TransitionList.Add(new StateToIdle_Player(_upperAniController , 1f));
        StateEvents.Add((0.8f , EventFunc));
        StateEvents.Add((2f , ()=> _effector.StopTrail((int)GOLEM.GolemTrail.WEAPON_TRAIL)));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)GOLEM.UpperStateType.ATTACK_START;
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
        _hitter.DoHitCheck(vCentor , vHalfExtents , fHitDuration, HitHandler);
    }

    void HitHandler(IHitter.HitInfo hitInfo)
    {
        hitInfo.Target.Hit(new IDamagable.DamageInfo{
            Damage = _stat.Get_Stat(Stat.STAT_TAG.DAMAGE), Attacker = _stat.transform, Point = hitInfo.Point});
        _effector.PlayEffect((int)GOLEM.GolemEffect.HIT_EFFECT , hitInfo.Point);

        Vector3 vKnocbackDir = Vector3.Normalize(hitInfo.Point - _stat.gameObject.transform.position);
        
        if(hitInfo.Collider.TryGetComponent(out CrowdController crowdController)== true)
            crowdController.Apply(CrowdController.CC_TAG.KNOCKBACK, 
                new ICrowdControl.CCData(fKnockbackPower, fKnockbackDecay, vKnocbackDir));
    }
}
