using UnityEngine;

// 타게터가 잡아둔 대상 방향으로 몸통 yaw만 보간 회전 — 상하 기울기는 반영하지 않는다
// 컴포넌트가 아닌 순수 C# 객체 — 상태머신이 서버에서만 돌므로 서버 권위는 호출 시점이 보장한다
public class MonsterRotate : IEntityRotate
{
    private Transform _transform;
    private MonsterTargeter _targeter;
    private float _fRotateSpeed;   // 회전 보간 속도 (클수록 빠르게 따라붙는다)

    public MonsterRotate(Transform transform, MonsterTargeter targeter, float fRotateSpeed)
    {
        _transform = transform;
        _targeter = targeter;
        _fRotateSpeed = fRotateSpeed;
    }

    // 타겟이 없으면 현재 회전을 유지하고, 있으면 수평 성분만 뽑아 목표 회전으로 보간한다
    public void Rotate()
    {
        if(_targeter.Target == null) return;

        Vector3 vFlat = _targeter.Target.position - _transform.position;
        vFlat.y = 0f;

        if(vFlat.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(vFlat);

        _transform.rotation = Quaternion.Slerp(
            _transform.rotation, target, _fRotateSpeed * Time.deltaTime);
    }
}
