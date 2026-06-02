using UnityEngine;
using Unity.Netcode;

public class Golem_UpperBody : Player_UpperBody
{
    [SerializeField] private Golem_AttackHitbox hitbox;
    [SerializeField] private float fKnockbackStrength = 8f;

    public override void OnAttackHitboxOn() => hitbox?.EnableHitbox();
    public override void OnAttackHitboxOff() => hitbox?.DisableHitbox();

    public override void NormalAttack()
    {
        // 골렘 공격 추후 구현
    }

    [ServerRpc]
    public void ReportHit_ServerRpc(ulong targetNetId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetNetId, out NetworkObject tgt))
            return;
        if (tgt.NetworkObjectId == NetworkObjectId) return;

        Player_UpperBody targetUpper = tgt.GetComponent<Player_UpperBody>();
        if (targetUpper == null) return;
        targetUpper.TakeHit(GetComponent<Stat>().pDamage);

        Vector3 dir = tgt.transform.position - transform.position;
        dir.y = 0f;
        tgt.GetComponent<Player_Move>()?.ApplyKnockback(dir, fKnockbackStrength);
    }
}
