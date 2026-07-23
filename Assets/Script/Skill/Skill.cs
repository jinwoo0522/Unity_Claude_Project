using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// 모든 스킬의 추상 기반 — 공통 데이터, 초기화, 데미지 처리 담당
public class Skill : NetworkBehaviour , IPoolable
{
    public NetworkObjectType Type { get; private set; }
    public IPoolReturner Handler { get; set; }
    public GameObject Owner {get; private set;}
    public SkillHitCast Hitcast => _hitter;
    
    [SerializeField] private SkillEffector _effector;
    [SerializeField] private SkillHitCast _hitter;
    private ulong  ClinetID;
    private ISkillModule _effectorModule;   // 이펙터를 모듈로 취급 — DIM 기본 구현을 쓰려면 인터페이스 경유 호출 필요
    // 인스펙터에서 SubclassSelector 드롭다운으로 모듈 조합 — 프리팹 인스턴스마다 리스트가 복제돼 상태 독립
    [SerializeReference, SubclassSelector] private List<ISkillModule> _skillModules = new List<ISkillModule>();

    // 1회성 — 직렬화 모듈 + 이펙터를 Bind하고 충돌 이벤트에 구독 (프리팹 생성 시 1회)
    private void Awake()
    {
        _effectorModule = _effector;

        for (int i = 0; i < _skillModules.Count; ++i)
        {
            _skillModules[i].Bind(this);

            _hitter.HitEnterEvent += _skillModules[i].CollisionEnter;
            _hitter.HitStayEvent += _skillModules[i].CollisionStay;
        }

        _effectorModule.Bind(this);
        _hitter.HitEnterEvent += _effectorModule.CollisionEnter;
        _hitter.HitStayEvent  += _effectorModule.CollisionStay;
    }

    // 풀에서 생성될 때 자신의 핸들러를 주입받음

    // 공통 초기화 — SetActive는 ObjectPoolManager의 풀 Get()에서 호출해 OnEnable 타이밍을 제어
    public void Init(NetworkObjectType type, Vector3 position, Vector3 direction, ulong clinetID , GameObject owner)
    {
        Type               = type;
        ClinetID           = clinetID;
        transform.position = position;
        transform.forward  = direction.normalized;
        Owner = owner;
    }

    public void Active()
    {
        gameObject.SetActive(true);
    }

    public void Enter()
    {
        for (int i = 0; i < _skillModules.Count; ++i)
            _skillModules[i].Enter();

        _effectorModule.Enter();
    }

    private void Update()
    {
        float fTimeDelta = Time.deltaTime;

        if (IsServer)
        {
            for (int i = 0; i < _skillModules.Count; ++i)
                _skillModules[i].ServerTick(fTimeDelta);
            _effectorModule.ServerTick(fTimeDelta);
        }

        if (IsClient)
        {
            for (int i = 0; i < _skillModules.Count; ++i)
                _skillModules[i].ClientTick(fTimeDelta);
            _effectorModule.ClientTick(fTimeDelta);
        }
    }

    public virtual void Release()
    {
        for (int i = 0; i < _skillModules.Count; ++i)
            _skillModules[i].Exit();
        _effectorModule.Exit();

        gameObject.SetActive(false);
    }

    public virtual void Destroy()
    {
        GameObject.Destroy(gameObject);
    }

    [ServerRpc]
    protected virtual void DespawnSkill_ServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
    }
}
