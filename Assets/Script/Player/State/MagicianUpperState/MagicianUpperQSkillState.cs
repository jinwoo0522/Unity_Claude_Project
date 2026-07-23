using UnityEngine;

// 마법사 상체 Q스킬 — 캐스팅 0.7초 시점에 IceExplosion을 플레이어 중심에 스폰(스폰 즉시 폭발)
public class MagicianUpperQSkillState : EntityState
{
    readonly private Magician_Player _player;
    readonly private EntityAnimator _upperAniController;
    readonly private IEffector _effector;

    const float fCastTime = 2f;        // 폭발 스폰 타이밍(초)
    const float fTrailStopTime = 3f;   // 양손 트레일 종료 타이밍(초)

    public MagicianUpperQSkillState(Magician_Player player)
    {
        _player = player;
        _upperAniController = player._upperAniController;
        _effector = player._effector;
    }

    public override void Create()
    {
        // 캐스팅 애니메이션이 끝나면 IDLE로 복귀
        TransitionList.Add(new StateToIdle_Player(_upperAniController));
        StateEvents.Add((fCastTime, FireIceExplosion));
        StateEvents.Add((fTrailStopTime, () => _effector.StopTrail((int)MAGICIAN.MagicianTrail.Right_Hand)));
        StateEvents.Add((fTrailStopTime, () => _effector.StopTrail((int)MAGICIAN.MagicianTrail.Left_Hand)));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)MAGICIAN.UpperStateType.QSKILL;
        _effector.PlayEffect((int)MAGICIAN.MagicianEffect.QSKILL);      // 캐스팅 이펙트 시전
        _effector.PlayTrail((int)MAGICIAN.MagicianTrail.Right_Hand);    // 오른손 트레일 시작
        _effector.PlayTrail((int)MAGICIAN.MagicianTrail.Left_Hand);     // 왼손 트레일 시작
    }

    public override void Exit() { }

    protected override void UpdateState(float fTimedelta, ushort curState) { }

    // 서버 권위 — 상체 상태머신은 서버에서만 돌아가므로 여기서 스킬 스폰

    void FireIceExplosion()
    {
        Vector3 vPos = _player.transform.position + new Vector3(0f , 0.5f ,0f);
        
        GameManager.Instance.skillFactory.Create(
            NetworkObjectType.ICE_EXPLOSION, vPos, Vector2.zero, _player.OwnerClientId, _player.gameObject);
    }
}
