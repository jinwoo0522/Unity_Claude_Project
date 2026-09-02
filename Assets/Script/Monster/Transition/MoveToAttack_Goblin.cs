using UnityEngine;

// NavMesh가 멈추는 지점까지 붙으면 공격 시작
// 타겟을 놓친 프레임에는 거리 계산이 돌면 안 되므로 MoveToIdle 뒤에 등록해야 한다
public class MoveToAttack_Goblin : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.ATTACK;

    private MonsterTargeter _targeter;

    private float _fStopDistance;

    public MoveToAttack_Goblin(MonsterTargeter targeter, float fStopDistance)
    {
        _targeter = targeter;
        _fStopDistance = fStopDistance;
    }

    public bool CheckRule(float fTimeDelta)
    {
        return _targeter.IsInRange(_fStopDistance);
    }

    public void OnTransition()
    {
    }
}
