using System;
using UnityEngine;

public class SkillHitCast : MonoBehaviour
{
    [Header("판정 대상")]
    [SerializeField] private LayerMask _targetMask;

    [Header("캐스팅 정보")]
    [SerializeField] private Vector3 _vCenter;      // 이 Transform 로컬 기준 중심 오프셋
    [SerializeField] private float   _fRange;       // 구 반지름
    [SerializeField] private float   _fDelay;       // 판정 시작 전 대기 시간(초)
    [SerializeField] private float   _fHitTime;     // 판정 지속시간(초), 0이면 HitOff까지 무한

#if UNITY_EDITOR
    [Header("디버그 (Gizmo 미리보기)")]
    [SerializeField] private bool  _isDrawGizmo = true;
    [SerializeField] private Color _idleColor   = new Color(0f, 1f, 0f, 0.25f);
    [SerializeField] private Color _hitColor    = new Color(1f, 0f, 0f, 0.45f);
#endif

    private bool  _isHitting;
    private float _fDelayTimer;
    private float _fHitTimer;
    // 피격 시 호출 — 모듈의 Collision 등을 외부에서 구독 (Awake에서 1회 등록)
    public event Action<IHitter.HitInfo> HitEvent;

    // 판정 시작 — 딜레이 경과 후부터 지속 판정
    public void HitOn()
    {
        _isHitting   = true;
        _fDelayTimer = _fDelay;
        _fHitTimer   = _fHitTime;
    }

    // 판정 중단 — 지속시간이 남아있어도 즉시 종료
    public void HitOff()
    {
        _isHitting = false;
    }

    private void Update()
    {
        if (!_isHitting) return;

        if (_fDelayTimer > 0f)
        {
            _fDelayTimer -= Time.deltaTime;
            return;
        }

        HitOnce();

        if (_fHitTime <= 0f) return;   // 무한 판정 — 지속시간 소진 없음

        _fHitTimer -= Time.deltaTime;
        if (_fHitTimer <= 0f) HitOff();
    }

    // 구 질의 1회 + 피격 정보 전달
    private void HitOnce()
    {
        Vector3 vWorldCenter = GetWorldCenter();
        Collider[] cols = Physics.OverlapSphere(vWorldCenter, _fRange, _targetMask);

        foreach (Collider col in cols)
        {
            if (ReferenceEquals(col.gameObject, gameObject)) continue; // 자기 자신 제외

            // IDamagable 없으면 null로 전달
            col.TryGetComponent(out IDamagable target);

            IHitter.HitInfo hitInfo = new IHitter.HitInfo();

            hitInfo.Target   = target;
            hitInfo.Collider = col;
            hitInfo.Point    = col.ClosestPoint(vWorldCenter);
            HitEvent?.Invoke(hitInfo);
        }
    }

    // 로컬 중심 오프셋 → 월드 중심 (Transform 회전을 따라감)
    private Vector3 GetWorldCenter()
    {
        return transform.position + transform.rotation * _vCenter;
    }

    // 풀 반납 등으로 꺼질 때 판정도 함께 종료
    private void OnDisable()
    {
        HitOff();
    }

    private void OnDestroy()
    {
        HitEvent = null;
    }

#if UNITY_EDITOR
    // 판정 중엔 실제 질의하는 구를 빨갛게 — 딜레이 동안은 표시 안 함
    private void OnDrawGizmos()
    {
        if (!_isDrawGizmo || !_isHitting || _fDelayTimer > 0f) return;
        DrawSphere(_hitColor);
    }

    // 오브젝트 선택 시에만 표시 — 범위 튜닝용 미리보기(평상시 씬은 깔끔)
    private void OnDrawGizmosSelected()
    {
        if (!_isDrawGizmo || (_isHitting && _fDelayTimer <= 0f)) return;   // 판정 중이면 위 OnDrawGizmos가 그림
        DrawSphere(_idleColor);
    }

    private void DrawSphere(Color color)
    {
        Vector3 vCenter = GetWorldCenter();

        Gizmos.color = color;
        Gizmos.DrawSphere(vCenter, _fRange);
        Gizmos.color = new Color(color.r, color.g, color.b, 1f);
        Gizmos.DrawWireSphere(vCenter, _fRange);
    }
#endif
}
