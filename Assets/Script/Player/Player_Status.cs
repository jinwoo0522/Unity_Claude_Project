using System.Collections;
using Unity.Netcode;
using UnityEngine;

// 상태이상 통합 관리 컴포넌트 — Airborne/Slow/Freeze 부여·조회·시간처리를 단일 NetworkBehaviour로 위임받아 처리
public class Player_Status : NetworkBehaviour
{
    // 클라 입력 게이트(점프·공격 차단)에서 읽혀야 하므로 NetworkVariable로 복제
    private NetworkVariable<bool> net_isAirborne = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> net_isFrozen = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private StatusEffect_Airborne _airborne;
    private StatusEffect_Slow     _slow;
    private StatusEffect_Freeze   _freeze;

    private CharacterController _cct;
    private Animator _animator;

    // 외부 조회 API
    public bool  IsAirborne      => net_isAirborne.Value;
    public bool  IsFrozen        => net_isFrozen.Value;
    public float SpeedMultiplier => _slow.SpeedMultiplier;

    public override void OnNetworkSpawn()
    {
        // 순수 C# 상태이상 객체 생성 — 코루틴·네트워크 처리는 이 컴포넌트가 대신함
        _airborne = new StatusEffect_Airborne(this);
        _slow     = new StatusEffect_Slow(this);
        _freeze   = new StatusEffect_Freeze(this);
        _cct      = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsServer) return;
        _slow.Tick(Time.deltaTime);
        _freeze.Tick(Time.deltaTime);
    }

    // Airborne의 착지 감지 코루틴 — StatusEffect_Airborne.Apply()에서 StartLandingRoutine()으로 위임
    private IEnumerator LandingRoutine()
    {
        yield return new WaitUntil(() => !_cct.isGrounded);
        yield return new WaitUntil(() => _cct.isGrounded);
        net_isAirborne.Value = false;
    }

    // StatusEffect_Airborne에서 호출 — 공중 상태 set 및 착지 감지 코루틴 시작
    public void SetAirborne(bool value)
    {
        net_isAirborne.Value = value;
    }

    public void StartLandingRoutine()
    {
        StartCoroutine(LandingRoutine());
    }

    // StatusEffect_Freeze.Tick()에서 만료 시 호출
    public void SetFrozen(bool value)
    {
        net_isFrozen.Value = value;
        SetAnimatorSpeed_ClientRpc(1f);
    }

    // 외부 조회 — Player_Move(서버)에서 1회 소비
    public bool ConsumePendingLaunch(out float force)
    {
        return _airborne.ConsumePendingLaunch(out force);
    }

    // 외부 부여 API — 모두 서버 전용 가드
    public void ApplyAirborne(float force)
    {
        if (!IsServer) return;
        _airborne.Apply(force);
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (!IsServer) return;
        _slow.Apply(multiplier, duration);
    }

    public void ApplyFreeze(float duration)
    {
        if (!IsServer) return;
        net_isFrozen.Value = true;
        _freeze.Apply(duration);
        SetAnimatorSpeed_ClientRpc(0f);
    }

    [ClientRpc]
    public void SetAnimatorSpeed_ClientRpc(float fValue)
    {
        _animator.speed = fValue;
    }
}
