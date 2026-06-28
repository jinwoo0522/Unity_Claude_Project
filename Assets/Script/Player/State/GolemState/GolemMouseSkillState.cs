using UnityEngine;

public class GolemMouseSkillState : EntityState
{
    
    EntityAnimator _aniController;
    IHitter _hitter;
    IEntityMovement _move;
    IEffector _effector;
    Stat _stat;

    Vector3 vCentor = new Vector3(0f,0.25f,0.0f);
    Vector3 vHalfExtents = new Vector3(1f,0.25f,1f);

    const float fHitDuration = 0.2f;   // 판정 지속시간(초)

    public GolemMouseSkillState(Golem_Player player)
    {
        _aniController = player._aniController;
        _hitter = player._hitter;
        _move = player._move;
        _effector = player._effector;
        _stat = player._stat;
    }
    public override void Create()
    {
        TransitionList.Add(new StateToIdle_Player(_aniController));
        StateEvents.Add((1.7f , () => _effector.StopEffect((int)GOLEM.GolemEffect.DASH_TRAIL)));
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)GOLEM.StateType.MOUSE_SKILL;
        _effector.PlayEffect((int)GOLEM.GolemEffect.DASH_TRAIL);
        _aniController._animator.applyRootMotion = true;
        
    }

    public override void Exit()
    {
        _aniController._animator.applyRootMotion = false;
        _effector.StopEffect((int)ELF.ElfEffect.MOUSE_SKILL);
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
    }
    void EventFunc()
    {
    }

}
