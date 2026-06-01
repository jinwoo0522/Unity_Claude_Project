using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Golem_AttackHitbox : MonoBehaviour
{
    [SerializeField] private Collider hitCollider;

    private Golem_UpperBody owner;
    private NetworkObject ownerNetObj;
    private readonly HashSet<ulong> hitSet = new();

    void Start()
    {
        owner = GetComponentInParent<Golem_UpperBody>();
        ownerNetObj = owner != null ? owner.GetComponent<NetworkObject>() : null;
    }

    public void EnableHitbox()
    {
        hitSet.Clear();
        hitCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        hitCollider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (owner == null || owner.IsOwner == false) return;
        if (!other.CompareTag("Player")) return;

        NetworkObject tgt = other.GetComponent<NetworkObject>();
        if (tgt == null) return;
        if (tgt.NetworkObjectId == ownerNetObj.NetworkObjectId) return;
        if (!hitSet.Add(tgt.NetworkObjectId)) return;

        owner.ReportHit_ServerRpc(tgt.NetworkObjectId);
    }
}
