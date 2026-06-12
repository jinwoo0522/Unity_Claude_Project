using Unity.Netcode;
using UnityEngine;

public class Player_TrailEmitting : NetworkBehaviour
{

    [SerializeField]
    private TrailRenderer[] trailRenderers;

    override public void OnNetworkSpawn()
    {
        if (IsClient)
        {
            for (int i = 0; i < trailRenderers.Length; i++)
            {
                trailRenderers[i].emitting = false;
            }
        }
    }

    [ClientRpc]
    public void Start_EmitTrail_ClientRpc()
    {
        for (int i = 0; i < trailRenderers.Length; i++)
        {
            trailRenderers[i].emitting = true;
        }
    }

    [ClientRpc]
    public void Stop_EmitTrail_ClientRpc()
    {
        for (int i = 0; i < trailRenderers.Length; i++)
        {
            trailRenderers[i].emitting = false;
        }
    }
}
