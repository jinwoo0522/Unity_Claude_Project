using Unity.Netcode;
using UnityEngine;

public class Golem_TrailEmitting : NetworkBehaviour
{
    [SerializeField]
    private TrailRenderer trailRenderer_1;
    [SerializeField]
    private TrailRenderer trailRenderer_2;
    [SerializeField]
    private TrailRenderer trailRenderer_3;



    override public void OnNetworkSpawn()
    {
        if (IsClient)
        {
            trailRenderer_1.emitting = false;
            trailRenderer_2.emitting = false;
            trailRenderer_3.emitting = false;
        }
    }

    [ClientRpc]
    public void Start_EmitTrail_ClientRpc()
    {
        trailRenderer_1.emitting = true;
        trailRenderer_2.emitting = true;
        trailRenderer_3.emitting = true;

        Debug.Log("Stop_EmitTrail_ClientRpc 히트 시작");
    }

    [ClientRpc]
    public void Stop_EmitTrail_ClientRpc()
    {
        trailRenderer_1.emitting = false;
        trailRenderer_2.emitting = false;
        trailRenderer_3.emitting = false;

        Debug.Log("Stop_EmitTrail_ClientRpc 히트 끝");
    }
}
