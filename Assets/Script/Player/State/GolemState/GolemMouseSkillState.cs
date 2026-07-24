using UnityEngine;

public class GolemMouseSkillState : EntityState
{

    Golem_Player _player;
    EntityAnimator _aniController;
    IEffector _effector;

    const float _fFireCastTime = 1.8f;   // Fire_Explosion 스폰 타이밍(초)
    const float _fFireDistance = 3f;     // 플레이어 정면 방향 스폰 거리

    public GolemMouseSkillState(Golem_Player player)
    {
        _player = player;
        _aniController = player._aniController;
        _effector = player._effector;
    }
    public override void Create()
    {
        TransitionList.Add(new StateToIdle_Player(_aniController));
        StateEvents.Add((1.7f , () => _effector.StopEffect((int)GOLEM.GolemEffect.DASH_TRAIL)));
        StateEvents.Add((_fFireCastTime, FireExplosion));
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

    // 서버 권위 — 플레이어 정면 방향 +3 위치에 Fire_Explosion 스폰
    void FireExplosion()
    {
        Vector3 vPos = _player.transform.position + _player.transform.forward * _fFireDistance;

        GameManager.Instance.skillFactory.Create(
            NetworkObjectType.FIRE_EXPLOSION, vPos, Vector2.zero, _player.OwnerClientId, _player.gameObject);
    }

}
