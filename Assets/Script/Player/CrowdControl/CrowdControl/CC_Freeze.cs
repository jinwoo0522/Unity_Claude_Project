using UnityEngine;

public class CC_Freeze : ICrowdControl
{
    public bool isFlag {get; private set;}

    MaterialChanger _matChanger;
    EntityEffector _effector;
    IDamagable _damagable;
    float fTime;

    public CC_Freeze(MaterialChanger matChanger, EntityEffector effector, IDamagable damagable)
    {
        _matChanger = matChanger;
        _effector = effector;
        _damagable = damagable;
    }

    public void Apply(ICrowdControl.CCData data)
    {
        isFlag = true;
        fTime = data._fValue;
        _damagable._isHit = false;

        _matChanger.Change(MaterialChanger.MAT_TAG.FROZEN);
        _effector.PlayPoolEffect(PoolObjectType.FROZEN_SMOKE_EFFECT);
    }

    public void Tick(float fTimeDelta)
    {
        fTime -= fTimeDelta;
    }

    public bool IsExpired()
    {
        return fTime <= 0f;
    }

    public void Restore()
    {
        isFlag = false;
        _matChanger.Restore();
    }
}
