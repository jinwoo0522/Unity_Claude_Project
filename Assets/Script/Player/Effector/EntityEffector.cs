using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using UnityEngine.Networking;


public class EntityEffector : NetworkBehaviour , IEffector
{
    [System.Serializable]
    public struct TrailRenderers
    {
        public TrailRenderer[] trailRenderers;
    }

    [SerializeField]
    private ParticleSystem[] Effects;
    
    [SerializeField]
    private TrailRenderers[] Trails;

    [ClientRpc]
    void PlayEffect_ClientRpc(int iEffectNumber, Vector3 vPos = default)
    {
        if(vPos != Vector3.zero)
            Effects[iEffectNumber].gameObject.transform.position = vPos;

        Effects[iEffectNumber].gameObject.SetActive(true);
        Effects[iEffectNumber].Clear();
    }
    [ClientRpc]
    void PlayTrail_ClientRpc(int iTrailtNumber)
    {
        foreach(var trail in Trails[iTrailtNumber].trailRenderers)
        {
            trail.emitting = true;
        }
    }
    [ClientRpc]
    public void StopEffect_ClientRpc(int iEffectNumber) // 파티클 이펙트에 StopAction -> Disable 필수
    {
        Effects[iEffectNumber].Stop();
    }
    [ClientRpc]
    public void StopTrail_ClientRpc(int iTrailtNumber)
    {
        foreach(var trail in Trails[iTrailtNumber].trailRenderers)
        {
            trail.emitting = false;
        }
    }

    public void PlayEffect(int iEffectNumber, Vector3 vPos = default) => PlayEffect_ClientRpc(iEffectNumber, vPos);

    public void PlayTrail(int iTrailtNumber) => PlayTrail_ClientRpc(iTrailtNumber);

    public void StopEffect(int iEffectNumber) => StopEffect_ClientRpc(iEffectNumber);

    public void StopTrail(int iTrailtNumber) => StopTrail_ClientRpc(iTrailtNumber);

}
