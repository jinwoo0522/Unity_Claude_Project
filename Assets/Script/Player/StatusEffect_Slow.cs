// 슬로우 상태이상 — 순수 C# 클래스
// SpeedMultiplier는 Player_Move(서버 전용 이동 계산)에서만 읽히므로 로컬 float으로 충분, 네트워크 복제 불필요
public class StatusEffect_Slow : StatusEffect
{
    private float _multiplier    = 1f;
    private float _remainingTime = 0f;

    public float SpeedMultiplier => _remainingTime > 0f ? _multiplier : 1f;

    public StatusEffect_Slow(Player_Status owner) : base(owner) { }

    // 서버 전용 — 슬로우 배율과 지속시간 덮어쓰기 방식으로 적용
    public void Apply(float multiplier, float duration)
    {
        _multiplier    = multiplier;
        _remainingTime = duration;
    }

    // Player_Status.Update(서버)에서 매 프레임 호출
    public override void Tick(float dt)
    {
        if (_remainingTime > 0f)
            _remainingTime -= dt;
    }
}
