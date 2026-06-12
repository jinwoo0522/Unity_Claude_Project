// 공중 띄움 상태이상 — 순수 C# 클래스
// 네트워크 상태(net_isAirborne)와 착지 코루틴은 owner(Player_Status)에게 위임
public class StatusEffect_Airborne : StatusEffect
{
    private float _pendingLaunchForce;

    // owner의 NetworkVariable을 통해 클라도 공중 상태를 읽을 수 있음
    public bool IsAirborne => owner.IsAirborne;

    public StatusEffect_Airborne(Player_Status owner) : base(owner) { }

    // 서버 전용 — 상승속도 펜딩 후 공중 상태 set, 착지 코루틴은 owner에 위임
    public void Apply(float launchForce)
    {
        _pendingLaunchForce = launchForce;
        owner.SetAirborne(true);
        owner.StartLandingRoutine();
    }

    // Player_Move(서버)에서 1회 소비 — verticalVelocity에 주입
    public bool ConsumePendingLaunch(out float force)
    {
        force               = _pendingLaunchForce;
        bool hasPending     = _pendingLaunchForce != 0f;
        _pendingLaunchForce = 0f;
        return hasPending;
    }
}
