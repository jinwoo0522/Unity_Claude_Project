using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System;

public class Player_Input : NetworkBehaviour , IEntityMoveInput , IEntityInputState
{
    PlayerInput _inputAction;
    private InputCommand _input = new();
    public Vector2 MoveInput => _input.MoveDir;
    public bool isSprint => (_input.InputFlag & ENTITY.InputFlagType.SPRINT) == ENTITY.InputFlagType.SPRINT;
    public ushort inputState => (ushort)_input.InputFlag;

    public override void OnNetworkSpawn()
    {
        if(IsOwner == false) return;

        _inputAction = GetComponent<PlayerInput>();

        _inputAction.actions["Player/Move"].performed += OnMovePerformed;
        _inputAction.actions["Player/Move"].canceled += OnMoveCanceled;

        _inputAction.actions["Player/Sprint"].performed += OnSprintPerformed;
        _inputAction.actions["Player/Sprint"].canceled += OnSprintCanceled;

        _inputAction.actions["Player/Jump"].performed += OnJumpPerformed;
        _inputAction.actions["Player/Jump"].canceled += OnJumpCanceled;
    }
    // Inpu처리는 클라에서 행하는 것이기 때문에 서버가 모름
    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if(IsOwner != true) return;
        _input.MoveDir = ctx.ReadValue<Vector2>();

        AddFlag(ENTITY.InputFlagType.MOVE);
        SyncInputData_ServerRpc(_input);
    }
    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        if(IsOwner != true) return;

        _input.MoveDir = Vector2.zero;
        SubFlag(ENTITY.InputFlagType.MOVE);
        SyncInputData_ServerRpc(_input);
    }

    void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if(IsOwner != true) return;

        AddFlag(ENTITY.InputFlagType.JUMP);
        SyncInputData_ServerRpc(_input);
    }
    void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        if(IsOwner != true) return;

        SubFlag(ENTITY.InputFlagType.JUMP);
        SyncInputData_ServerRpc(_input);
    }  

    void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        if(IsOwner != true) return;

        if(ctx.ReadValue<float>() > 0.5f)
            AddFlag(ENTITY.InputFlagType.SPRINT);

        SyncInputData_ServerRpc(_input);
    }

    void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        if(IsOwner != true) return;

        SubFlag(ENTITY.InputFlagType.SPRINT);
        SyncInputData_ServerRpc(_input);

        Debug.Log("Sprint 해제");
    }

    private void AddFlag(ENTITY.InputFlagType flag) =>_input.InputFlag |= flag;
    private void ToggleFlag(ENTITY.InputFlagType flag) =>_input.InputFlag ^= flag;
    private void SubFlag(ENTITY.InputFlagType flag) =>_input.InputFlag &= ~flag;
    public  void Reset() => _input.InputFlag = ENTITY.InputFlagType.NONE; 

    [ServerRpc]
    void SyncInputData_ServerRpc(InputCommand command)
    {
        _input = command;
    }


    // 인풋 네트워크 구조체
    public struct InputCommand : INetworkSerializable
    {
        public Vector2 MoveDir;
        public ENTITY.InputFlagType InputFlag;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref MoveDir);
            
            // enum은 캐스팅을 해서 직렬화 하는게 안전하다
            ushort raw = (ushort)InputFlag; 
            serializer.SerializeValue(ref raw);
            InputFlag = (ENTITY.InputFlagType)raw; 
            //다시 캐스팅 하는 이유는 SerializeValue함수가 지역 변수인 raw 값에 써버리기 때문이다.
        }
    }
}
