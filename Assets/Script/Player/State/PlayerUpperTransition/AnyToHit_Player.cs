using UnityEngine;

public class AnyToHit_Player : ITransition
{
    public ushort NextState => (ushort)ENTITY.UpperStateType.HIT;

    IDamagable _damagable;

    EntityAnimator _upperAniController;

    public AnyToHit_Player(EntityAnimator upperAniController , IDamagable damagalbe)
    {
        _upperAniController = upperAniController;
        _damagable = damagalbe;
    }
    public bool CheckRule(float fTimeDelta)
    {
        if(_damagable._isHit == false)
            return false;

        return true;
    }

    public void OnTransition()
    {

    }
}
