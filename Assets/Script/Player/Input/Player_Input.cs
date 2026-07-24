using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System;

public class Player_Input : NetworkBehaviour , IEntityMoveInput , IEntityInputState
{
    PlayerInput _inputAction;
    private InputCommand _input = new();
    private Camera _mainCam;

    private readonly NetworkVariable<Vector3> _aimDir =
        new(Vector3.forward, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public Vector3 AimDir => _aimDir.Value;
    public Vector2 MoveInput => _input.MoveDir;
    public bool isSprint => (_input.InputFlag & ENTITY.InputFlagType.SPRINT) == ENTITY.InputFlagType.SPRINT;
    public ushort inputState => (ushort)_input.InputFlag;

    public override void OnNetworkSpawn()
    {
        if(IsOwner == false) return;

        _mainCam = GameManager.Instance.cameraManager.Get_Camera(CameraManager.CameraTag.MAIN);

        _inputAction = GetComponent<PlayerInput>();

        _inputAction.actions["Player/Move"].performed += OnMovePerformed;
        _inputAction.actions["Player/Move"].canceled += OnMoveCanceled;

        _inputAction.actions["Player/Sprint"].performed += OnSprintPerformed;
        _inputAction.actions["Player/Sprint"].canceled += OnSprintCanceled;

        _inputAction.actions["Player/Jump"].performed += OnJumpPerformed;
        _inputAction.actions["Player/Jump"].canceled += OnJumpCanceled;

        _inputAction.actions["Player/Attack"].performed += OnAttackPerformed;
        _inputAction.actions["Player/Attack"].canceled += OnAttackCanceled;

        _inputAction.actions["Player/Attack_Skill"].performed += OnAttack_SkillPerformed;
        _inputAction.actions["Player/Attack_Skill"].canceled += OnAttack_SkillCanceled;

        _inputAction.actions["Player/Q"].performed += OnQSkillPerformed;
        _inputAction.actions["Player/Q"].canceled += OnQSkillCanceled;
    }
    // 조준 방향은 로컬 카메라(시네머신)에만 존재 → Owner가 서버로 동기화
    private void Update()
    {
        if(IsOwner == false) return;

        _aimDir.Value = _mainCam.transform.forward;
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

    void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if(IsOwner != true) return;

        AddFlag(ENTITY.InputFlagType.MOUSE_LEFT);
        SyncInputData_ServerRpc(_input);
    }
    void OnAttackCanceled(InputAction.CallbackContext ctx)
    {
        if(IsOwner != true) return;

        SubFlag(ENTITY.InputFlagType.MOUSE_LEFT);
        SyncInputData_ServerRpc(_input);
    }  

    void OnAttack_SkillPerformed(InputAction.CallbackContext ctx)
    {
        if(IsOwner != true) return;

        AddFlag(ENTITY.InputFlagType.MOUSE_RIGHT);
        SyncInputData_ServerRpc(_input);
    }
    void OnAttack_SkillCanceled(InputAction.CallbackContext ctx)
    {
        if(IsOwner != true) return;

        SubFlag(ENTITY.InputFlagType.MOUSE_RIGHT);
        SyncInputData_ServerRpc(_input);
    }  

    void OnQSkillPerformed(InputAction.CallbackContext ctx)
    {
        if(IsOwner != true) return;

        AddFlag(ENTITY.InputFlagType.Q);
        SyncInputData_ServerRpc(_input);
    }
    void OnQSkillCanceled(InputAction.CallbackContext ctx)
    {
        if(IsOwner != true) return;

        SubFlag(ENTITY.InputFlagType.Q);
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
