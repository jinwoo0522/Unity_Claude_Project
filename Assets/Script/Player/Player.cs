using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Netcode.Components;
using Unity.VisualScripting;
public abstract class Player : NetworkBehaviour
{
    [SerializeField]
    AnimData animData;

    public Player_Input _input {get; protected set;}
    public PlayerMovement _move {get; protected set;}
    public StateMachine _stateMachine {get; protected set;}
    public Animator _animator {get; protected set;}
    public NetworkAnimator _netAnimator {get; protected set;}
    public EntityAnimator _aniController {get; protected set;}

    public PlayerCameraRotate _CamRotater {get; protected set;}

    public override void OnNetworkSpawn()
    {
       _input = GetComponent<Player_Input>(); 
       _move = GetComponent<PlayerMovement>();
       _animator = GetComponent<Animator>();
       _netAnimator = GetComponent<NetworkAnimator>();
       _stateMachine = new StateMachine();
       _CamRotater = GetComponent<PlayerCameraRotate>();

       _aniController = new EntityAnimator(_animator , _netAnimator , animData,
        new NetworkVariable<ushort>(0, NetworkVariableReadPermission.Everyone , NetworkVariableWritePermission.Server));
    }

    // Update is called once per frame
    void Update()
    {
        ServerUpdate();
        ClinetUpdate();
    }

    void ServerUpdate()
    {
        if(IsServer == false) return;
        _stateMachine.State_Update(Time.deltaTime);
    }

    void ClinetUpdate()
    {
        if(IsOwner == false) return;
    }
}
