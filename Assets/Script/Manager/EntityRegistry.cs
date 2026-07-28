using System;
using System.Collections.Generic;
using UnityEngine;

// 서버 전용 엔티티 등록소 — 진영별로 나눠 보관하고 최근접 대상을 찾아준다
// 물리 쿼리(OverlapSphere 등) 대신 등록 목록을 직접 훑는다
// 후보 집합이 이미 확정돼 있어 브로드페이즈가 불필요하고, 서버에서만 도는 코드라 판정이 갈리지 않는다
public class EntityRegistry
{
    private readonly List<Entity>[] _entities;

    public EntityRegistry()
    {
        int iFactionCount = Enum.GetValues(typeof(ENTITY.Faction)).Length;

        _entities = new List<Entity>[iFactionCount];
        for(int i = 0; i < iFactionCount; ++i)
            _entities[i] = new List<Entity>();
    }

    public void Add(Entity entity) => _entities[(int)entity.Faction].Add(entity);
    public void Remove(Entity entity) => _entities[(int)entity.Faction].Remove(entity);

    // 사거리 안 최근접 대상 반환 — 대상이 없으면 null (호출부가 '타겟 없음'으로 처리)
    // Transform이 아닌 Entity를 넘겨 호출부가 스탯 등 필요한 참조를 함께 캐싱할 수 있게 한다
    public Entity FindNearest(Vector3 vPos, float fRange, ENTITY.Faction faction)
    {
        List<Entity> targetList = _entities[(int)faction];

        Entity nearest = null;
        float fNearestSqr = fRange * fRange;   // 사거리 밖은 애초에 후보가 되지 않는다

        foreach(Entity entity in targetList)
        {
            if(entity._stat._isDead == true) continue;   // 시체를 쫓지 않는다

            float fSqr = (entity.transform.position - vPos).sqrMagnitude;
            if(fSqr > fNearestSqr) continue;

            fNearestSqr = fSqr;
            nearest = entity;
        }

        return nearest;
    }
}
