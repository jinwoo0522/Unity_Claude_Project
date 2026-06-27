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

    [System.Serializable]
    public struct EffectPool
    {
        public ParticleSystem effect;   // 원본 파티클
        public int poolSize;            // 풀 크기
    }

    [SerializeField]
    private EffectPool[] Effects;

    [SerializeField]
    private TrailRenderers[] Trails;

    private ParticleSystem[][] _pools;  // 이펙트 인덱스별 인스턴스 풀
    private int[] _nextIndex;           // 전부 재생 중일 때 라운드로빈 포인터

    public override void OnNetworkSpawn()
    {
        BuildPools();
    }

    // 인스펙터 설정대로 이펙트별 풀을 구성
    void BuildPools()
    {
        _pools = new ParticleSystem[Effects.Length][];
        _nextIndex = new int[Effects.Length];

        for (int i = 0; i < Effects.Length; i++)
        {
            ParticleSystem src = Effects[i].effect;
            int size = Mathf.Max(1, Effects[i].poolSize);

            // 원본을 0번으로 두고, 나머지는 같은 부모 아래로 복제 (size 1이면 원본 1개 풀)
            ParticleSystem[] pool = new ParticleSystem[size];
            pool[0] = src;
            for (int n = 1; n < size; n++)
            {
                ParticleSystem copy = Instantiate(src, src.transform.parent);
                copy.transform.localPosition = src.transform.localPosition;
                copy.transform.localRotation = src.transform.localRotation;
                copy.gameObject.SetActive(false);
                pool[n] = copy;
            }
            _pools[i] = pool;
        }
    }

    // 비활성(재생 종료) 인스턴스를 우선 반환, 전부 재생 중이면 라운드로빈 재사용
    ParticleSystem GetAvailable(int iEffectNumber)
    {
        ParticleSystem[] pool = _pools[iEffectNumber];

        for (int n = 0; n < pool.Length; n++)
        {
            if (!pool[n].gameObject.activeSelf)
                return pool[n];
        }

        int idx = _nextIndex[iEffectNumber];
        _nextIndex[iEffectNumber] = (idx + 1) % pool.Length;
        return pool[idx];
    }

    [ClientRpc]
    void PlayEffect_ClientRpc(int iEffectNumber, Vector3 vPos = default)
    {
        ParticleSystem ps = GetAvailable(iEffectNumber);

        if(vPos != Vector3.zero)
            ps.transform.position = vPos;

        ps.gameObject.SetActive(true);
        ps.Clear();
        ps.Play();
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
        foreach(var ps in _pools[iEffectNumber])
        {
            ps.Stop();
        }
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
