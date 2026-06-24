using UnityEngine;

public interface IDamagable 
{
    public bool _isHit {get; set;}
    public bool _isDead {get; set;}
    public void Hit(float fDamage);
}
