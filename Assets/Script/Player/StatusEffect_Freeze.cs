// 빙결 상태이상 — 순수 C#, 지속시간 만료 시 owner에게 net_isFrozen 해제 요청
public class StatusEffect_Freeze : StatusEffect
{
    private float _remainingTime = 0f;

    public StatusEffect_Freeze(Player_Status owner) : base(owner) { }

    // 지속시간 설정 — ApplyFreeze에서 net_isFrozen=true 설정 후 호출됨
    public void Apply(float duration)
    {
        _remainingTime = duration;
    }

    // 서버 Update마다 Player_Status가 호출 — 만료 시 owner에 해제 위임
    public override void Tick(float dt)
    {
        if (_remainingTime <= 0f) return;
        
        _remainingTime -= dt;
        if (_remainingTime <= 0f)
            owner.SetFrozen(false);
    }
}
