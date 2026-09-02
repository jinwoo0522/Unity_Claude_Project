using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Netcode.Components;
using Unity.VisualScripting;
public abstract class Player : Entity
{

    [SerializeField]
    AnimData upperAnimData;


    NetworkVariable<ushort> _upperState = new(0);

    public Player_Input         _input {get; protected set;}
    public StateMachine         _upperStateMachine {get; protected set;}
    public EntityAnimator       _upperAniController {get; protected set;}
    public IJumpMovement        _jump {get; protected set;}
    public IEntityRotate        _rotate {get; protected set;}   // 회전 소스가 엔티티마다 달라 생성은 파생 클래스가 담당
    public override ENTITY.Faction Faction => ENTITY.Faction.PLAYER;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
       //인풋 컴포넌트
       _input = GetComponent<Player_Input>();
       // 점프 컴포넌트
       _jump = GetComponent<IJumpMovement>();
       // 회전 객체 생성 — 플레이어는 조준 입력을 회전 소스로 쓴다
       _rotate = new PlayerCameraRotate(transform, _input);

       //상체 상태머신 생성
        _upperStateMachine = new StateMachine();
       // 애니메이션 컨트롤러 생성
       Animator _animator = GetComponent<Animator>();
       NetworkAnimator _netAnimator = GetComponent<NetworkAnimator>();
 
       _aniController = new EntityAnimator(_animator , _netAnimator , animData, _State);
       _upperAniController = new EntityAnimator(_animator , _netAnimator , upperAnimData, _upperState);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        ServerUpdate();
        ClinetUpdate();
        Simulate();
    }

    void ServerUpdate()
    {
        if(IsServer == false) return;
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
