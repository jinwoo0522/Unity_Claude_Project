using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class SkillHitCast : NetworkBehaviour {
    
    [Header("판정 대상")]
    [SerializeField] private LayerMask _targetMask;

    [Header("판정 정보")]
    [SerializeField] private float _fHitDelayTime;  

    [Header("충돌 이벤트")]
    [SerializeReference, SubclassSelector] private List<ICollsionEventModule> _collisions = new List<ICollsionEventModule>();
    private NetworkObject _networkObject;   // 탄환 자신의 NetworkObject — 시전자를 스스로 역추적 (Skill 비의존)
    public event Action<Collider> HitEnterEvent;
    public event Action<Collider> HitStayEvent;

    private void Awake()
    {
        _networkObject = GetComponentInParent<NetworkObject>();

        // 부착된 충돌 모듈을 시전 스킬에 바인드 후, 모듈이 지정한 타이밍(Enter/Stay) 이벤트에 구독
        Skill skill = GetComponentInParent<Skill>();
        for (int i = 0; i < _collisions.Count; ++i)
        {
            _collisions[i].Bind(skill);

            if (_collisions[i].Timing == CollisionTiming.ENTER)
                HitEnterEvent += _collisions[i].Collsion;
            else
                HitStayEvent += _collisions[i].Collsion;
        }
    }

    // 대상 레이어 콜라이더가 범위에 들어오면 집합에 추가 — 시전자는 제외
    private void OnTriggerEnter(Collider other)
    {
        // [디버그] 트리거에 실제로 들어온 콜라이더 전부 기록 — "안 닿았다"를 검증
        Debug.Log($"[HitCast:{name}] OnTriggerEnter other={other.name} root={other.transform.root.name} layer={other.gameObject.layer} IsServer={IsServer}");

        if(CheckLayer(other) == false) return;

        HitEnterEvent?.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if(CheckLayer(other) == false) return;

        HitStayEvent?.Invoke(other);
    }

    private bool CheckLayer(Collider other)
    {
        if(IsServer == false) return false; 
        
        if (((1 << other.gameObject.layer) & _targetMask) == 0)
        {
            Debug.Log($"[HitCast:{name}] 레이어 미포함 → 제외 other={other.name} layer={other.gameObject.layer} mask={(int)_targetMask}");
            return false;
        }

        // 시전자(OwnerClientId)의 PlayerObject를 조회 — 프리웜/미스폰 등으로 주인이 없으면 판정 제외
        NetworkObject owner = NetworkManager.Singleton.SpawnManager
            .GetPlayerNetworkObject(_networkObject.OwnerClientId);

        // [디버그] 시전자 역추적 결과와 제외 비교 — 여기서 자기피격 원인이 드러남
        Debug.Log($"[HitCast:{name}] 판정 후보 other={other.name} otherRoot={other.transform.root.name} ownerId={_networkObject.OwnerClientId} owner={(owner == null ? "null" : owner.name)} 제외일치={(owner != null && other.transform.root == owner.transform)}");

        if (owner == null) return false;

        if (other.transform.root == owner.transform) return false; // 시전자는 제외

        return true;
    }
    
    private void OnDestroy()
    {
        HitEnterEvent = null;
        HitStayEvent  = null;
    }
}
