using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillEffector : NetworkBehaviour, ISkillModule
{
    // 이펙트 시작 시점
    private enum EffectStartTiming { ENTER, COLLISION, EXIT }
    // 이펙트 종료 시점
    private enum EffectEndTiming { SELF, COLLISION, EXIT }

    // 이펙트 하나의 구성 단위 — 인스펙터 리스트에서 항목별로 설정
    [System.Serializable]
    private struct EffectInfo
    {
        [SerializeField] private PoolObjectType    _effectType;   // 재생할 이펙트 종류(풀 키)
        [SerializeField] private EffectStartTiming _startTiming;  // 시작 시점
        [SerializeField] private EffectEndTiming   _endTiming;    // 종료 시점
        [SerializeField] private bool              _bFollow;      // 스킬을 따라 이동시킬지(부모 부착)

        public PoolObjectType    Effect      => _effectType;
        public EffectStartTiming StartTiming => _startTiming;
        public EffectEndTiming   EndTiming   => _endTiming;
        public bool              Follow      => _bFollow;
    }

    [SerializeField] private List<EffectInfo> _effects = new List<EffectInfo>();
    private readonly Dictionary<int, EffectView> _activeEffects = new Dictionary<int, EffectView>();

    // 스킬 발동 — 상태 초기화(전 인스턴스). RPC는 스폰 전이라 여기서 못 쏨 → ServerTick으로 미룸
    public void Enter()
    {
        if(IsServer == false) return;

        _activeEffects.Clear();

        for (int i = 0; i < _effects.Count; ++i)
            if (_effects[i].StartTiming == EffectStartTiming.ENTER)
            {
                Play_ClientRpc(i, transform.position, transform.rotation);
            }
    }

    // 스킬 종료 — 로컬에서 캐시된 이펙트를 전부 정지 (looping 누수 방지)
    public void Exit()
    {
        foreach (var kv in _activeEffects)
            kv.Value.Stop();

        _activeEffects.Clear();
    }

    [ClientRpc]
    private void Play_ClientRpc(int iIndex, Vector3 position, Quaternion rotation)
    {
        bool isFollow   = _effects[iIndex].Follow;
        bool isCanCache = _effects[iIndex].EndTiming != EffectEndTiming.SELF || isFollow;  // follow는 Exit 정리 위해 캐싱
        if (isCanCache && _activeEffects.ContainsKey(iIndex)) return;   // 이미 재생 중 — 1:1

        EffectView effect = GameManager.Instance.objectPoolManager.Get<EffectView>(_effects[iIndex].Effect);
        effect.Play(position, rotation);
        effect.gameObject.SetActive(true);

        if (isFollow)   effect.Attach(transform);       // 스킬을 따라 이동(부모 부착)
        if (isCanCache) _activeEffects[iIndex] = effect; // 종료 제어 위해 캐싱
    }

    // 각 클라에서 캐시된 이펙트를 정지 (StopEmitting 후 콜백으로 반납)
    [ClientRpc]
    private void Stop_ClientRpc(int iIndex)
    {
        if (_activeEffects.TryGetValue(iIndex, out EffectView effect))
        {
            effect.Stop();
            _activeEffects.Remove(iIndex);
            effect.Release();
        }
    }

    public void CollisionEnter(Collider collider)
    {
        if (!IsServer) return; // RPC는 서버에서만

        for (int i = 0; i < _effects.Count; ++i)
        {
            if (_effects[i].StartTiming == EffectStartTiming.COLLISION)
                Play_ClientRpc(i, transform.position, transform.rotation);

            if (_effects[i].EndTiming == EffectEndTiming.COLLISION)
                Stop_ClientRpc(i);
        }
    }
}
