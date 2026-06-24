using UnityEngine;

// 근접 공격 판정 전용 컴포넌트.
// 상태머신/애니메이션이 히트 시점에 DoHitCheck(center, halfExtents)를 호출하면,
// OverlapBox로 대상을 수집해 IDamagable.Hit()으로 데미지를 적용한다.
// - 서버 권위: 이 컴포넌트는 MonoBehaviour이며, 서버에서만 도는 상태머신이 호출하는 것을 전제로 한다.
// - 판정 범위는 공격마다 다르므로 호출 시점에 인자로 받는다.
public class AttackHitbox : MonoBehaviour , IHitter
{
    [Header("판정 대상")]
    [SerializeField] private LayerMask _targetMask;   // 피격 대상 레이어

#if UNITY_EDITOR
    [Header("디버그 (Gizmo 미리보기)")]
    [SerializeField] private bool    _drawGizmo = true;
    [SerializeField] private Vector3 _previewCenter      = new Vector3(0f, 0f, 1f);     // 미판정 시 표시할 기본 범위
    [SerializeField] private Vector3 _previewHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color   _idleColor = new Color(0f, 1f, 0f, 0.25f);         // 평상시
    [SerializeField] private Color   _hitColor  = new Color(1f, 0f, 0f, 0.45f);         // 판정 발생 순간
    [SerializeField] private float   _hitFlashTime = 0.12f;
    // 판정이 막 발생했음을 잠시 빨갛게 보여주기 위한 상태 + 실제 질의한 박스 캐시
    private float   _hitFlash;
    private Vector3 _debugCenter;
    private Vector3 _debugExtents;
#endif

    private Stat _ownerStat;

    void Awake()
    {
        _ownerStat = GetComponentInParent<Stat>();
    }

    // 히트 프레임에 호출 — 범위 질의 + 데미지 적용
    // center      : 이 Transform 로컬 기준 박스 중심 오프셋 (공격별 리치)
    // halfExtents : 박스 절반 크기 (공격별 크기)

    public void DoHitCheck(Vector3 center, Vector3 halfExtents)
    {
        MarkHitFlash(center, halfExtents);   // 디버그 표시 — 빌드에선 호출 자체가 제거됨

        GetBoxWorld(center, out Vector3 worldCenter, out Quaternion rotation);
        Collider[] cols = Physics.OverlapBox(worldCenter, halfExtents, rotation, _targetMask);

        float damage = _ownerStat.Get_Stat(Stat.STAT_TAG.DAMAGE);

        foreach (Collider col in cols)
        {
            if (!col.TryGetComponent(out IDamagable target)) continue;
            if (ReferenceEquals(target, _ownerStat))         continue;   // 자기 자신 제외

            // Stat.Hit() 내부에서 데미지만큼 HP를 차감하므로 양수 그대로 전달
            target.Hit(damage);
        }
    }

    // 로컬 중심 오프셋 → 월드 중심·회전 계산 (Transform 회전을 따라감)
    void GetBoxWorld(Vector3 localCenter, out Vector3 center, out Quaternion rotation)
    {
        rotation = transform.rotation;
        center   = transform.position + rotation * localCenter;
    }

    // 빌드(UNITY_EDITOR 미정의)에선 컴파일러가 이 메서드 호출을 통째로 제거
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    void MarkHitFlash(Vector3 center, Vector3 halfExtents)
    {
#if UNITY_EDITOR
        _hitFlash     = _hitFlashTime;
        _debugCenter  = center;       // 실제 질의한 박스를 Gizmo로 표시
        _debugExtents = halfExtents;
#endif
    }

#if UNITY_EDITOR
    void Update()
    {
        if (_hitFlash > 0f) _hitFlash -= Time.deltaTime;
    }

    // 어택 판정 중에만 표시 — 실제로 질의한 박스를 빨갛게
    void OnDrawGizmos()
    {
        if (!_drawGizmo || _hitFlash <= 0f) return;
        DrawBox(_debugCenter, _debugExtents, _hitColor);
    }

    // 오브젝트 선택 시에만 표시 — 범위 튜닝용 미리보기(평상시 씬은 깔끔)
    void OnDrawGizmosSelected()
    {
        if (!_drawGizmo || _hitFlash > 0f) return;   // 판정 중이면 위 OnDrawGizmos가 그림
        DrawBox(_previewCenter, _previewHalfExtents, _idleColor);
    }

    void DrawBox(Vector3 localCenter, Vector3 halfExtents, Color color)
    {
        GetBoxWorld(localCenter, out Vector3 center, out Quaternion rotation);

        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);

        Vector3 size = halfExtents * 2f;   // OverlapBox는 halfExtents, Gizmos는 전체 크기
        Gizmos.color = color;
        Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.color = new Color(color.r, color.g, color.b, 1f);
        Gizmos.DrawWireCube(Vector3.zero, size);

        Gizmos.matrix = old;
    }
#endif
}
