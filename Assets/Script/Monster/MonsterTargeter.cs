using UnityEngine;
public class MonsterTargeter
{
    private Transform _transform;
    private IDamagable _targetDamagable;   // 매 프레임 GetComponent를 피하려 탐색 시점에 캐싱
    public Transform Target {get; private set;}   // 사거리 안에 대상이 없으면 null

    public MonsterTargeter(Transform transform)
    {
        _transform = transform;
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
        _targetDamagable = entity._damagable;
    }

    // 추격 중 매 프레임 호출 — 잡아둔 타겟이 죽거나 이탈 거리를 벗어나면 비운다
    // 재탐색은 하지 않는다. 비워두면 상태가 대기로 돌아가고 거기서 다시 탐색한다
    public void KeepTarget(float fLeaveRange)
    {
        if(IsInRange(fLeaveRange) == true && _targetDamagable._isDead == false) return;

        Target = null;
        _targetDamagable = null;
    }

    // 타겟이 주어진 거리 안에 있는지 — 추격 유지·공격 진입 판정이 함께 쓴다
    public bool IsInRange(float fRange)
    {
        float fSqr = (Target.position - _transform.position).sqrMagnitude;

        return fSqr <= fRange * fRange;
    }
}
