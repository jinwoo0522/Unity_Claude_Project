using UnityEngine;
public class MonsterTargeter
{
    private Transform _transform;

    private Transform _spawnPoint;   // 몬스터와 함께 움직이면 안 되므로 외부의 고정 오브젝트를 받는다

    private IDamagable _targetDamagable;   // 매 프레임 GetComponent를 피하려 탐색 시점에 캐싱

    public Transform Target {get; private set;}   // 사거리 안에 대상이 없으면 null

    private const float _fArriveDistance = 0.1f;   // 스폰 지점 도착 판정 거리

    public MonsterTargeter(Transform transform, Transform spawnPoint)
    {
        _transform = transform;
        _spawnPoint = spawnPoint;
    }

    // 사거리 안 최근접 플레이어를 타겟으로 잡는다 — 없으면 Target이 null로 비워진다
    public void Search(float fRange)
    {
        Entity entity = GameManager.Instance.entityRegistry.FindNearest(
            _transform.position, fRange, ENTITY.Faction.PLAYER);

        if(entity == null)
        {
            Target = null;
            _targetDamagable = null;
            return;
        }

        Target = entity.transform;
        _targetDamagable = entity._stat;
    }

    // MOVE 상태에서 매 프레임 호출 — 스폰 지점에서 너무 멀어지거나 타겟이 죽으면 복귀로 전환한다
    public void ReturnPoint(float fMaxDistance)
    {
        // 이동이 수평으로만 일어나므로 높이 차는 빼고 잰다 (스폰 지점 높이가 달라도 도착 판정이 성립)
        Vector3 vFlat = _transform.position - _spawnPoint.position;
        vFlat.y = 0f;

        float fSqr = vFlat.sqrMagnitude;

        // 복귀 중에는 도착만 본다 — 도착하면 타겟을 비워 대기 상태로 돌려보낸다
        if(Target == _spawnPoint)
        {
            if(fSqr > _fArriveDistance * _fArriveDistance) return;

            Target = null;
            return;
        }

        bool isTooFar = fSqr > fMaxDistance * fMaxDistance;
        if(isTooFar == false && _targetDamagable._isDead == false) return;

        Target = _spawnPoint;
        _targetDamagable = null;
    }
}
