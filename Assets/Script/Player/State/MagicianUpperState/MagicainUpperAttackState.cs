using UnityEngine;

// 마법사 상체 공격 — 콤보 없이 1타, 0.7초 타이밍에 Electric 스킬을 풀에서 꺼내 투사체로 발사
public class MagicainUpperAttackState : EntityState
{
    readonly private Magician_Player _player;
    readonly private EntityAnimator _upperAniController;
    readonly private IEffector _effector;
    readonly private Transform _shootPos;

    const float fFireTime = 0.4f;                              // 스킬 발사 타이밍(초)
    const float fTrailStopTime = 0.7f;                         // 오른손 트레일 종료 타이밍(초)

    public MagicainUpperAttackState(Magician_Player player , Transform ShootPos)
    {
        _player = player;
        _upperAniController = player._upperAniController;
        _effector = player._effector;
        _shootPos = ShootPos;

    }

    public override void Create()
    {
        // 공격 애니메이션이 끝나면 IDLE로 복귀 (1타 종료)
        TransitionList.Add(new StateToIdle_Player(_upperAniController));
        StateEvents.Add((fFireTime, FireElectric));
        StateEvents.Add((fTrailStopTime, () => _effector.StopTrail((int)MAGICIAN.MagicianTrail.Right_Hand)));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)MAGICIAN.UpperStateType.ATTACK;
        _effector.PlayTrail((int)MAGICIAN.MagicianTrail.Right_Hand);   // 오른손 트레일 시작
    }

    public override void Exit() { }

    protected override void UpdateState(float fTimedelta, ushort curState) { }

    // 서버 권위 — 상체 상태머신은 서버에서만 돌아가므로 여기서 스킬 스폰

    void FireElectric()
    {
        // 조준 방향은 입력이 서버로 동기화 — 카메라를 직접 읽지 않고 소비만 한다 (상하 포함)
        Vector3 vDir = _player._input.AimDir;

        GameManager.Instance.skillFactory.Create(
            NetworkObjectType.ELECTRONIC_SKILL, _shootPos.position, vDir, _player.OwnerClientId, _player.gameObject);
    }
}
