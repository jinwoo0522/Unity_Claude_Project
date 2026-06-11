using Unity.Netcode;
using UnityEngine;

// 슬로우 상태이상 — Apply() 호출 시 서버에서 발동, 시간 경과로 자동 만료
// SpeedMultiplier는 Player_Move(서버 전용 이동 계산)에서만 읽히므로 로컬 float으로 충분
public class StatusEffect_Slow : NetworkBehaviour
{
    private float _multiplier    = 1f;
    private float _remainingTime = 0f;

    public float SpeedMultiplier => _remainingTime > 0f ? _multiplier : 1f;

    // 서버 전용 — 슬로우 배율과 지속시간 덮어쓰기 방식으로 적용
    public void Apply(float multiplier, float duration)
    {
        if (!IsServer) return;
        _multiplier    = multiplier;
        _remainingTime = duration;
    }

    private void Update()
    {
        if (!IsServer || _remainingTime <= 0f) return;
        _remainingTime -= Time.deltaTime;
    }
}
