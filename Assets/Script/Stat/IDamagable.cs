using UnityEngine;

public interface IDamagable 
{
    public bool _isHit {get;}
    public bool _isDead {get;}
    public void Hit(float fDamage);
}
