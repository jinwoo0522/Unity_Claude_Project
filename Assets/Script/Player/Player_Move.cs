using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Netcode.Components;

public class Player_Move : NetworkBehaviour
{
    // 인스펙터에서 할당된 객체들은 캐싱해서 들어가서 서버가 알고있음
    [SerializeField]
    protected Player_Data playerData;

    // 플레이어 인풋을 통해 들어오는 값은 클라가 행하기 때문에 서버는 모름  
    protected bool isSprint = false;
    protected Vector2 MoveDir;

    // 아래 값은 서버에서는 ServerRpc를 통해 값을 바꾸지만 클라는 값이 안바뀌어있음
    protected float verticalVelocity = 0f;
    protected Vector2 vAnimLerp;
    protected bool isJumpPending = false;
    protected float fCamYaw;


    protected CharacterController               cct;
    protected Animator                          anim;

    protected NetworkAnimator                   net_anim;
    protected Player_UpperBody                  playerUpper;

    NetworkVariable<int> test = new NetworkVariable<int>(0 
    , NetworkVariableReadPermission.Everyone, 
    NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        // 이 객체들은 서버에서도 갱신 되어야 하기 때문에 실행해야함
        cct      = GetComponent<CharacterController>();
        anim     = GetComponentInChildren<Animator>();
        net_anim = GetComponent<NetworkAnimator>();
        playerUpper = GetComponent<Player_UpperBody>();

        if(IsOwner == false)
            return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        if(IsServer == true)
        {
            AnimManage();
        }

        if(IsOwner == true)
        {
           SubmitCamYaw_ServerRpc(Camera.main.transform.eulerAngles.y);
        }
    }

    void FixedUpdate()
    {
        if(IsServer == true)
        {
            PlayerMove();
            RotateWithCamera();
        }
    }

    protected virtual void RotateWithCamera()
    {
        transform.rotation = Quaternion.Euler(0f, fCamYaw, 0f);
    }

// Inpu처리는 클라에서 행하는 것이기 때문에 서버가 모름
    void OnMove(InputValue value)
    {
        if(IsOwner == false) return;

        MoveDir = value.Get<Vector2>();
        SubmitMoveInput_ServerRpc(MoveDir);
    }
    void OnJump(){

        if(IsOwner == false) return;
        PlayerJump();
    } 
    void OnSprint(InputValue value)
    {
        if(IsOwner == false) return;
        isSprint = value.Get<float>() > 0.5f;
        SubmitSprint_ServerRpc(isSprint);
    }

    //서버가 클라에서 수행한 인풋값을 모르기때문에 인자로 넘겨줘야함
    protected virtual void PlayerMove()
    {
        
        Vector3 vMoveDir = transform.right * MoveDir.x + transform.forward * MoveDir.y;

        float fSpeed = isSprint ? playerData.fRunSpeed : playerData.fWalkSpeed;
        vMoveDir *= fSpeed;

        // 중력 처리
        if (cct.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // 바닥 감지를 위한 최소 하강값
        else if (!cct.isGrounded)
            verticalVelocity += playerData.fGravity * Time.deltaTime;

        vMoveDir.y = verticalVelocity;
        cct.Move(vMoveDir * Time.deltaTime);
    }

    protected virtual void AnimManage()
    {
        // 이동 중이면 해당 방향/속도로, 멈추면 0으로 보간
        float fTargetScale = isSprint ? 1f : 0.5f;
        Vector2 vTarget = MoveDir.magnitude > 0.1f ? MoveDir.normalized * fTargetScale
        : Vector2.zero;
        vAnimLerp = Vector2.Lerp(vAnimLerp, vTarget, Time.deltaTime * playerData.fAnimSpeed);

        anim.SetFloat("MoveX", vAnimLerp.x);
        anim.SetFloat("MoveZ", vAnimLerp.y);
        anim.SetBool("IsMove", vTarget == Vector2.zero ? false : true);
        anim.SetBool("IsGrounded", cct.isGrounded && !isJumpPending);
    }

    protected virtual void PlayerJump()
    {
        if (playerUpper != null && playerUpper.IsHit) return;

        Debug.Log($"점프 1단 예외처리 isGrounded{ cct.isGrounded } isJump {isJumpPending}" );

        if (!isJumpPending)
        {
            Debug.Log("점프 2단 예외처리");

            net_anim.SetTrigger("Jump");
            isJumpPending = true;

            SubmitJumpPending_ServerRpc(isJumpPending);
            StartCoroutine(JumpDelay());
        }
    }

    IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(playerData.fJumpDelay);
        SubmitVelocity_ServerRpc(playerData.fJumpAmount);
        SubmitJumpPending_ServerRpc(false);
        isJumpPending = false;
    }

    [ServerRpc]
    void SubmitSprint_ServerRpc(bool _isSprint)
    {
        isSprint = _isSprint;
    }

    [ServerRpc]
    void SubmitVelocity_ServerRpc(float Input)
    {
        verticalVelocity = Input;
    }

    [ServerRpc]
    void SubmitMoveInput_ServerRpc(Vector2 _Input)
    {
        MoveDir = _Input;
    }

    [ServerRpc]
    void SubmitJumpPending_ServerRpc(bool _Input)
    {
        isJumpPending = _Input;
    }

    [ServerRpc]
    void SubmitCamYaw_ServerRpc(float _Input)
    {
        fCamYaw = _Input;
    }

    
}
