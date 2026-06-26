using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Netcode.Components;
using Unity.VisualScripting;
public abstract class Player : NetworkBehaviour
{
    [SerializeField]
    AnimData animData;

    [SerializeField]
    AnimData upperAnimData;

    // NetworkVariable은 NetworkBehaviour의 필드로 선언해야 스폰 시 Netcode가 등록한다
    NetworkVariable<ushort> _State = new(0);
    NetworkVariable<ushort> _upperState = new(0);

    public Player_Input _input {get; protected set;}
    public PlayerMovement _move {get; protected set;}
    public StateMachine _stateMachine {get; protected set;}

    public StateMachine _upperStateMachine {get; protected set;}
    public EntityAnimator _aniController {get; protected set;}
    public PlayerCameraRotate _camRotater {get; protected set;}
    public EntityEffector _effector {get; protected set;}
    public EntityAnimator _upperAniController {get; protected set;}
    public Stat           _stat {get; protected set;}

    public override void OnNetworkSpawn()
    {
       //인풋 컴포넌트
       _input = GetComponent<Player_Input>();
       //이동 컴포넌트 
       _move = GetComponent<PlayerMovement>();
       //스탯 컴포넌트
       _stat = GetComponent<Stat>();
       //이펙터 컴포넌트
       _effector = GetComponent<EntityEffector>();

       //상태 머신 생성
       _stateMachine = new StateMachine();
       _upperStateMachine = new StateMachine();

       // 카메라 로테이터
       _camRotater = GetComponent<PlayerCameraRotate>();

       // 애니메이션 컨트롤러 생성
       Animator _animator = GetComponent<Animator>();
       NetworkAnimator _netAnimator = GetComponent<NetworkAnimator>();
 
       _aniController = new EntityAnimator(_animator , _netAnimator , animData, _State);
       _upperAniController = new EntityAnimator(_animator , _netAnimator , upperAnimData, _upperState);
    }

    // Update is called once per frame
     protected virtual void Update()
    {
        ServerUpdate();
        ClinetUpdate();
        Simulate();
    }

    void ServerUpdate()
    {
        if(IsServer == false) return;
        _stateMachine.State_Update(Time.deltaTime);
        _upperStateMachine.State_Update(Time.deltaTime);
        
    }

    void ClinetUpdate()
    {
        if(IsOwner == false) return;
    }

    void Simulate()
    {
        _upperAniController.AnimUpdate(Time.deltaTime);   
    }
}
