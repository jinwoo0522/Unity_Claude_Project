using UnityEngine;

public interface IDamagable 
{
    // 피격 1회의 정보 — 데미지와 함께 "누가·어디를" 때렸는지 실어 보낸다.
    // 피격 반응(회전·넉백·VFX)은 데미지만으로 만들 수 없어 공격자와 타격 지점이 함께 필요하다.
    public struct DamageInfo
    {
        public float     Damage;
        public Transform Attacker;   // 때린 주체 (스킬이면 시전자)
        public Vector3   Point;      // 맞은 위치
    }

    public bool _isHit {get; set;}
    public bool _isDead {get; set;}
    // 마지막으로 받은 피격 정보 — 피격 상태가 읽어 반응을 만든다 (쓰기는 Hit이 독점)
    public DamageInfo _damageInfo {get;}
    public void Hit(DamageInfo damageInfo);
}
