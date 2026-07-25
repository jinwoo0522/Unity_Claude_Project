using UnityEngine;

// 충돌한 대상을 공중에 띄우는(에어본) CC를 적용하는 충돌 이벤트 모듈.
// 에어본은 수직 방향 판정이므로 방향은 넘기지 않는다(세기/감쇠만 사용).
// 서버 권위 — CC 판정/적용은 서버에서만 수행한다.
[System.Serializable]
public class AirborneOnCollision : ICollsionEventModule
{
    [SerializeField] private CollisionTiming _timing = CollisionTiming.ENTER; // 에어본 판정 시점(진입/겹침)
    [SerializeField] private float _fAirbornePower;   // 에어본 초기 세기
    [SerializeField] private float _fAirborneDecay;   // 에어본 감쇠율

    private Skill _skill;

    public CollisionTiming Timing => _timing;

    // 시전 스킬 주입 (프리팹 생성 시 1회)
    public void Bind(Skill skill) => _skill = skill;

    // 충돌 대상에 에어본 적용 (서버 전용)
    public void Collsion(Collider col)
    { 
        if (col.TryGetComponent(out CrowdController crowdController) == false) return;

        crowdController.Apply(CrowdController.CC_TAG.AIRBORNE,
            new ICrowdControl.CCData(_fAirbornePower, _fAirborneDecay));
    }
}
