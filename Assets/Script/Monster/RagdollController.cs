using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class RagdollController : NetworkBehaviour {

    private Rigidbody[] _ragdollRigidbodies;
    private Collider[] _ragdollColliders;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private CharacterController _cct;

    override public void OnNetworkSpawn() {
        SetRagdollState(false);
    }

    [ClientRpc]
    public void SetRagdollState_ClientRpc(bool isRagdoll) {
        SetRagdollState(isRagdoll);
    }

    private void Awake() {
        CollectRagdollParts();
    }

    // 인스펙터 할당 대신 하위 본에서 래그돌 파츠를 직접 수집한다
    private void CollectRagdollParts() {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _cct = GetComponent<CharacterController>();

        // 본에 붙은 Rigidbody 를 기준으로 수집 — 루트 CharacterController 와 트리거 히트박스는 제외된다
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>(true);

        List<Collider> ragdollColliders = new List<Collider>();
        foreach (var rb in _ragdollRigidbodies) {
            foreach (var col in rb.GetComponents<Collider>()) {
                if (col.isTrigger) {
                    continue;
                }

                ragdollColliders.Add(col);
            }
        }

        _ragdollColliders = ragdollColliders.ToArray();
    }

    // 래그돌 온/오프 실제 처리 — RPC 없이 로컬에서 바로 적용된다
    private void SetRagdollState(bool isRagdoll) {
        foreach (var rb in _ragdollRigidbodies) {
            rb.isKinematic = !isRagdoll;
        }

        foreach (var col in _ragdollColliders) {
            col.enabled = isRagdoll;
        }

        _animator.enabled = !isRagdoll;
        _navMeshAgent.enabled = !isRagdoll;
        _cct.enabled = !isRagdoll;
    }

    public void Explode(Vector3 center, float force, float radius)
    {
        foreach (var rb in _ragdollRigidbodies)
            rb.AddExplosionForce(force, center, radius, 1.0f, ForceMode.Impulse);
    }
}
