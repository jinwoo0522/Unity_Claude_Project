using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// 스킬(네트워크)과 이펙트(클라 로컬)를 각각의 enum 키로 풀링 관리하는 매니저.
// 네트워크 풀과 로컬 풀을 컨테이너부터 분리한다.
public class ObjectPoolManager
{
    private List<GameObject> _NetworkPrefabs = new List<GameObject>();
    private List<GameObject> _Prefabs = new List<GameObject>();

    // 네트워크 풀 — NGO 어댑터 보유
    private Dictionary<NetworkObjectType, NetworkPoolHandler> _skillPools = new();
    // 로컬 풀 — 순수 IPoolable 풀
    private Dictionary<PoolObjectType, PooledHandler> _effectPools = new();

    private int _iMaxSize;
    private int _iMinSize;

    // 프리팹 리소스 로드 전담 — 이 매니저가 소유하고 실행
    private PoolResourceLoader _loader = new PoolResourceLoader();

    public ObjectPoolManager(int iMin, int iMax)
    {
        _iMinSize = iMin;
        _iMaxSize = iMax;

        Init();
    }

    // 로컬 풀 — 이펙트 등 IPoolable을 제네릭 타입으로 획득
    public T Get<T>(PoolObjectType type) where T : Component, IPoolable
    {
        return (T)_effectPools[type].Get();
    }

    // 네트워크 풀 — 스킬을 제네릭 타입으로 획득 (직접 Get은 NGO 스폰 우회)
    public T Get<T>(NetworkObjectType type) where T : Component, IPoolable
    {
        return (T)_skillPools[type].Get();
    }

    private void Init()
    {
        _loader.LoadNetworkPrefabs(_NetworkPrefabs);
        _loader.LoadLocalPrefabs(_Prefabs);

        // 네트워크 풀 — 로드된 스킬마다 NGO 어댑터 생성 및 등록
        for (int i = 0; i < _NetworkPrefabs.Count; ++i)
        {
            NetworkPoolHandler handler = new NetworkPoolHandler(_NetworkPrefabs[i], _iMinSize, _iMaxSize);
            _skillPools.Add((NetworkObjectType)i, handler);

            NetworkManager.Singleton.PrefabHandler.
                AddHandler(_NetworkPrefabs[i], handler);
        }

        // 로컬 풀 — 로드된 이펙트마다 순수 풀 생성
        for (int i = 0; i < _Prefabs.Count; ++i)
        {
            PooledHandler handler = new PooledHandler(_Prefabs[i], _iMinSize, _iMaxSize);
            _effectPools.Add((PoolObjectType)i, handler);
        }

        NetworkManager.Singleton.OnClientStarted += ClientStart;
    }


    private void PreCreate(int iCount)
    {
        foreach (var skillpool in _skillPools)
            skillpool.Value.Prewarm(iCount);

        foreach (var effectpool in _effectPools)
            effectpool.Value.Prewarm(iCount);
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode mode,
                   List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        PreCreate(30);
    }

    private void ClientStart()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }
}
