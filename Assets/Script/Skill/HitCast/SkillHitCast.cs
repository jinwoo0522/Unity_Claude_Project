using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// 트리거 기반 스킬 판정 컴포넌트.
// 같은 GameObject에 붙은 isTrigger SphereCollider가 판정 범위를 정의하고,
// OnTriggerEnter/Exit로 현재 겹친 대상 집합을 유지한다.
// HitOn~HitOff 구간 동안 Update에서 집합을 순회하며 최초 진입은 HitEnterEvent,
// 이후 겹쳐 있는 동안은 HitStayEvent를 매 프레임 발생시킨다.
public class SkillHitCast : MonoBehaviour
{
    [Header("판정 대상")]
    [SerializeField] private LayerMask _targetMask;

    [Header("판정 정보")]
    [SerializeField] private float _fHitTime;  

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

    // 스폰 시 지정된 주인(OwnerClientId)으로 시전자 플레이어 루트를 되찾음 — 자해 방지 비교용
    private Transform GetOwnerRoot()
    {
        return NetworkManager.Singleton.SpawnManager
            .GetPlayerNetworkObject(_networkObject.OwnerClientId).transform;
    }
    // 대상 레이어 콜라이더가 범위에 들어오면 집합에 추가 — 시전자는 제외
    private void OnTriggerEnter(Collider other)
    {
        if(CheckLayer(other) == false) return;

        HitEnterEvent?.Invoke(other);
    }
    
    private void OnTriggerStay(Collider other) 
    {
        if(CheckLayer(other) == false) return;
    }

    private bool CheckLayer(Collider other)
    {
        if (((1 << other.gameObject.layer) & _targetMask) == 0) return false;
        if (other.transform.root == GetOwnerRoot()) return false;

        return true;
    }
    private void OnDestroy()
    {
        HitEnterEvent = null;
        HitStayEvent  = null;
    }
}
