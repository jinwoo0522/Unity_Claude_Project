using UnityEngine;

// 어떤 상태에서든 HP가 0 이하이거나 사망 플래그가 서면 즉시 사망으로 전환
public class AnyToDie_Player : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.DIE;

    private Stat _stat;
    private IDamagable _damagable;

    public AnyToDie_Player(Stat stat, IDamagable damagable)
    {
        _stat = stat;
        _damagable = damagable;
    }

    public bool CheckRule(float fTimeDelta)
    {
        if(_damagable._isDead == true)
            return true;

        return _stat.Get_Stat(Stat.STAT_TAG.HP) <= 0f;
    }

    public void OnTransition()
    {
    }
}
