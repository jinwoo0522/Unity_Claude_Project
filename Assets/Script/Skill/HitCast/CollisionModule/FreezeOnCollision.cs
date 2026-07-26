using UnityEngine;

// 충돌한 대상을 지정 시간 동안 빙결시키는 충돌 이벤트 모듈.
// 빙결은 지속시간만 사용하므로 감쇠/방향은 넘기지 않는다.
// 서버 권위 — CC 판정/적용은 서버에서만 수행한다.
[System.Serializable]
public class FreezeOnCollision : ICollsionEventModule
{
    [SerializeField] private CollisionTiming _timing = CollisionTiming.ENTER; // 빙결 판정 시점(진입/겹침)
    [SerializeField] private float _fFreezeTime;   // 빙결 지속시간(초)

    private Skill _skill;

    public CollisionTiming Timing => _timing;

    // 시전 스킬 주입 (프리팹 생성 시 1회)
    public void Bind(Skill skill) => _skill = skill;

    // 충돌 대상에 빙결 적용 (서버 전용)
    public void Collsion(Collider col)
    {
        if (col.TryGetComponent(out CrowdController crowdController) == false) return;

        crowdController.Apply(CrowdController.CC_TAG.FREEZE,
            new ICrowdControl.CCData(_fFreezeTime));
    }
}
