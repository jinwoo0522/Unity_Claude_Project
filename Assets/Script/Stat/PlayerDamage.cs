using UnityEngine;

// 플레이어 피격 반응 — 상체만 경직되고 하체는 계속 움직인다
public class PlayerDamage : EntityDamage
{
    protected override void OnHit(IDamagable.DamageInfo damageInfo)
    {
    }
}
