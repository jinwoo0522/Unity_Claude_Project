using System;
using Unity.VisualScripting;
using UnityEngine;

// 근접 공격 판정 전용 컴포넌트.
// 상태머신/애니메이션이 히트 시점에 DoHitCheck(center, halfExtents, duration)를 호출하면,
// duration 초 동안 매 프레임 OverlapBox로 대상을 수집해 IDamagable.Hit()으로 데미지를 적용한다.
// - 서버 권위: DoHitCheck는 서버 전용 상태머신만 호출하므로 클라에선 _hitTimer가 0 → 판정 안 함.
// - 판정 범위·지속시간은 공격마다 다르므로 호출 시점에 인자로 받는다.
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
    [SerializeField] private Color   _hitColor  = new Color(1f, 0f, 0f, 0.45f);         // 판정 발생 중
#endif
    public event Action<IHitter.HitInfo> _HitInfo;

    private float      _hitTimer;
    private Vector3    _curCenter;
    private Vector3    _curHalfExtents;
    private bool       _isFollowRotation = true;   // false면 호출 시점 회전으로 고정
    private Quaternion _fixedRotation;

    public void DoHitCheck(Vector3 center, Vector3 halfExtents, float duration, Action<IHitter.HitInfo> HitInfo, bool isFollowRotation = true)
    {
        _curCenter        = center;
        _curHalfExtents   = halfExtents;
        _hitTimer         = duration;
        _HitInfo          = HitInfo;
        _isFollowRotation = isFollowRotation;
        _fixedRotation    = transform.rotation;   // 호출 시점 회전 스냅샷
    }

    void Update()
    {
        if (_hitTimer <= 0f) return;

        HitOnce();                       // 윈도우 동안 매 프레임 질의
        _hitTimer -= Time.deltaTime;
    }

    // 박스 질의 1회 + 데미지 적용
    void HitOnce()
    {
        GetBoxWorld(_curCenter, out Vector3 worldCenter, out Quaternion rotation);
        Collider[] cols = Physics.OverlapBox(worldCenter, _curHalfExtents, rotation, _targetMask);

        foreach (Collider col in cols)
        {
            if (!col.TryGetComponent(out IDamagable target)) continue;
            if (ReferenceEquals(col.gameObject, gameObject)) continue;   // 자기 자신 제외
            if (target._isHit == true) continue; // 피격중이라면 피격시키지 않음

            IHitter.HitInfo hitInfo = new IHitter.HitInfo();

            hitInfo.Target = target;
            hitInfo.Collider = col;
            hitInfo.Point = col.ClosestPoint(worldCenter);
            _HitInfo.Invoke(hitInfo);
        }
    }

    // 로컬 중심 오프셋 → 월드 중심·회전 계산
    // 추적 모드면 Transform 회전을 따라가고, 고정 모드면 판정 중 스냅샷 회전을 유지한다 (Gizmo 프리뷰는 항상 현재 회전)
    void GetBoxWorld(Vector3 localCenter, out Vector3 center, out Quaternion rotation)
    {
        rotation = (_isFollowRotation || _hitTimer <= 0f) ? transform.rotation : _fixedRotation;
        center   = transform.position + rotation * localCenter;
    }

    void OnDestroy()
    {
        _HitInfo = null;
    }

#if UNITY_EDITOR
    // 판정 중(_hitTimer>0)엔 실제 질의하는 박스를 빨갛게 — 지속시간만큼 표시됨
    void OnDrawGizmos()
    {
        if (!_drawGizmo || _hitTimer <= 0f) return;
        DrawBox(_curCenter, _curHalfExtents, _hitColor);
    }

    // 오브젝트 선택 시에만 표시 — 범위 튜닝용 미리보기(평상시 씬은 깔끔)
    void OnDrawGizmosSelected()
    {
        if (!_drawGizmo || _hitTimer > 0f) return;   // 판정 중이면 위 OnDrawGizmos가 그림
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
