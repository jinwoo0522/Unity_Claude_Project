using UnityEngine;

[System.Serializable]
public class ProjectileMove : IMovementModule
{
    [Header("투사체 움직임")]
    [SerializeField] private Vector3 _vCustomDir = Vector3.zero; // zero면 스킬 전방, 아니면 이 값(월드 기준)으로 이동
    [SerializeField] private float   _fDelay     = 0f;           // 이동 시작 전 대기 시간(초)
    [SerializeField] private float   _fSpeed;

    private Transform _transform;
    private Vector3   _vDir;
    private float     _fElapsed;

    // 이동 대상 Transform 주입 (프리팹 생성 시 1회)
    public void Bind(Transform transform)
    {
        _transform = transform;
    }

    // 발동 시 경과 시간·방향 초기화 (풀 재사용마다 재설정)
    public void Enter()
    {
        _fElapsed = 0f;
        // _vCustomDir이 zero면 스킬 전방(Init에서 세팅된 dir), 아니면 지정 dir(월드 기준)
        _vDir = (_vCustomDir == Vector3.zero ? _transform.forward : _vCustomDir).normalized;
    }

    public void Move(float fTimeDelta)
    {
        _fElapsed += fTimeDelta;
        if (_fElapsed < _fDelay) return; // 딜레이 동안 정지

        _transform.position += _vDir * (_fSpeed * fTimeDelta);
    }
}
