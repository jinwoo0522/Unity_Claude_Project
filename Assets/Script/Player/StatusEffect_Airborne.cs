using System.Collections;
using Unity.Netcode;
using UnityEngine;

// 공중 띄움 상태이상 — Apply() 호출 시 서버에서 발동, 착지 코루틴이 자동 해제
public class StatusEffect_Airborne : NetworkBehaviour
{
    // 공중 상태 전 클라 공개 — Player_Skill.CheckCanUseSkill 등 외부 게이트가 읽음
    private NetworkVariable<bool> net_isAirborne = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private float               _pendingLaunchForce;
    private CharacterController _cct;

    public bool IsAirborne => net_isAirborne.Value;

    public override void OnNetworkSpawn()
    {
        _cct = GetComponent<CharacterController>();
    }

    // 서버 전용 — 상승속도 펜딩 후 착지 감지 코루틴 시작
    public void Apply(float launchForce)
    {
        if (!IsServer) return;
        _pendingLaunchForce  = launchForce;
        net_isAirborne.Value = true;
        StartCoroutine(LandingRoutine());
    }

    // Player_Move.FixedUpdate(서버)에서 1회 소비 — verticalVelocity에 주입
    public bool ConsumePendingLaunch(out float force)
    {
        force               = _pendingLaunchForce;
        bool hasPending     = _pendingLaunchForce != 0f;
        _pendingLaunchForce = 0f;
        return hasPending;
    }

    // JumpRoutine 패턴과 동일 — 이륙 후 착지 시점에만 net_isAirborne 해제
    private IEnumerator LandingRoutine()
    {
        yield return new WaitUntil(() => !_cct.isGrounded);
        yield return new WaitUntil(() =>  _cct.isGrounded);
        net_isAirborne.Value = false;
    }
}
