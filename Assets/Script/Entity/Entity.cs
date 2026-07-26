using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public abstract class Entity : NetworkBehaviour
{


    public EntityEffector       _effector {get; protected set;}
    public Stat                 _stat {get; protected set;}
    public CrowdController      _crowdController {get; protected set;}
    public StateMachine         _stateMachine {get; protected set;}
    public EntityAnimator       _aniController {get; protected set;}
    public IEntityMovement      _move {get; protected set;}

    [SerializeField]
    protected AnimData animData;

    protected NetworkVariable<ushort> _State = new(0);

    public override void OnNetworkSpawn()
    {
       _stat = GetComponent<Stat>();
       //이펙터 컴포넌트
       _effector = GetComponent<EntityEffector>();
       //군중제어 컨트롤러 컴포넌트
       _crowdController = GetComponent<CrowdController>();
       //상태 머신 생성
       _stateMachine = new StateMachine();
       //움직임 컴포넌트
       _move = GetComponent<IEntityMovement>();

        // 애니메이션 컨트롤러 생성
       Animator _animator = GetComponent<Animator>();
       NetworkAnimator _netAnimator = GetComponent<NetworkAnimator>();
 
       _aniController = new EntityAnimator(_animator , _netAnimator , animData, _State);
    }

    protected virtual void Update()
    {
        ServerUpdate();
        ClinetUpdate();
    }

    // 상태머신은 서버에서만 돌기 때문에 애니메이션 재생 속도는 모든 피어에 따로 전파해야 한다
    public void Set_AnimSpeed(float fSpeed)
    {
        _aniController._animator.speed = fSpeed;   // Dedicated 서버는 ClientRpc를 받지 않으므로 직접 적용
        Set_AnimSpeed_ClientRpc(fSpeed);
    }

    [ClientRpc]
    private void Set_AnimSpeed_ClientRpc(float fSpeed)
    {
        _aniController._animator.speed = fSpeed;
    }

    void ServerUpdate()
    {
        if(IsServer == false) return;
        _crowdController.CrowdController_Update(Time.deltaTime);
        _stateMachine.State_Update(Time.deltaTime);

    }

    void ClinetUpdate()
    {
        if(IsOwner == false) return;
    }
}
