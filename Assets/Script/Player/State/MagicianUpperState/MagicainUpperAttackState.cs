using UnityEngine;

// 마법사 상체 공격 — 콤보 없이 1타, 0.7초 타이밍에 Electric 스킬을 풀에서 꺼내 투사체로 발사
public class MagicainUpperAttackState : EntityState
{
    readonly private Magician_Player _player;
    readonly private EntityAnimator _upperAniController;
    readonly private Transform _shootPos;

    const float fFireTime = 0.7f;                              // 스킬 발사 타이밍(초)
    static readonly Vector3 vSpawnOffset = new Vector3(0f, 1f, 1f); // 발사 위치 오프셋(전방/높이, 로컬 기준)

    public MagicainUpperAttackState(Magician_Player player , Transform ShootPos)
    {
        _player = player;
        _upperAniController = player._upperAniController;
        _shootPos = ShootPos;

    }

    public override void Create()
    {
        // 공격 애니메이션이 끝나면 IDLE로 복귀 (1타 종료)
        TransitionList.Add(new StateToIdle_Player(_upperAniController));
        StateEvents.Add((fFireTime, FireElectric));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)MAGICIAN.UpperStateType.ATTACK;
    }

    public override void Exit() { }

    protected override void UpdateState(float fTimedelta, ushort curState) { }

    // 서버 권위 — 상체 상태머신은 서버에서만 돌아가므로 여기서 스킬 스폰

    void FireElectric()
    {
        Vector3 vDir = GameManager.Instance.cameraManager.Get_Camera(CameraManager.CameraTag.MAIN).transform.forward;

        GameManager.Instance.skillFactory.Create(
            NetworkObjectType.ELECTRONIC_SKILL, _shootPos.position, vDir, _player.OwnerClientId, _player.gameObject);
    }
}
