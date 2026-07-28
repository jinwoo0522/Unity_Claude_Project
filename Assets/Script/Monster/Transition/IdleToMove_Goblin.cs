using UnityEngine;

// 타게터가 대상을 잡으면 추격 시작
public class IdleToMove_Goblin : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.WALK;

    private MonsterTargeter _targeter;

    public IdleToMove_Goblin(MonsterTargeter targeter)
    {
        _targeter = targeter;
    }

    public bool CheckRule(float fTimeDelta)
    {
        return _targeter.Target != null;
    }

    public void OnTransition()
    {
    }
}
