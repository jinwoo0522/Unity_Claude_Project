using UnityEngine;
using UnityEngine.AI;

// 몬스터 공통 베이스 — 모든 몬스터가 쓰는 타격 판정기와 타겟 보관소를 여기서 만든다
public abstract class Monster : Entity
{
    public IHitter _hitter {get; protected set;}
    public MonsterTargeter _targeter {get; protected set;}
    public NavMeshAgent _agent {get; protected set;}
    public EnemyData _enemyData => (EnemyData)_stat._data;
    [SerializeField] protected Transform _spawnPoint;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _hitter = GetComponent<IHitter>();
        _agent = GetComponent<NavMeshAgent>();
        // 타겟 보관소 — 탐색은 IDLE 상태가 주기적으로 돌린다
        _targeter = new MonsterTargeter(transform);
    }
}
