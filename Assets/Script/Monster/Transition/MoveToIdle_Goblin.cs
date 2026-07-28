using UnityEngine;

// 쫓을 대상이 사라지면 대기로 복귀 — 다음 탐색은 IDLE이 다시 돌린다
public class MoveToIdle_Goblin : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.IDLE;

    private MonsterTargeter _targeter;

    public MoveToIdle_Goblin(MonsterTargeter targeter)
    {
        _targeter = targeter;
    }

    public bool CheckRule(float fTimeDelta)
    {
        return _targeter.Target == null;
    }

    public void OnTransition()
    {
    }
}
