using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class Player_Move : NetworkBehaviour
{
    [SerializeField]
    protected Player_Data playerData;

    protected bool isSprint = false;
    protected Vector2 MoveDir;
    protected float verticalVelocity = 0f;

    protected Vector2 vAnimLerp;
    protected bool isJumpPending = false;

    protected CharacterController               cct;
    protected Animator                          anim;
    protected Player_UpperBody                  playerUpper;

    NetworkVariable<int> test = new NetworkVariable<int>(0 
    , NetworkVariableReadPermission.Everyone, 
    NetworkVariableWritePermission.Server);

    void Awake()
    {
    }

    void Start()
    {
        if(IsOwner == false) return;

        cct      = GetComponent<CharacterController>();
        anim     = GetComponentInChildren<Animator>();
        playerUpper = GetComponent<Player_UpperBody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

    }

    void Update()
    {
        if(IsOwner == false) return;
        RotateWithCamera();
        PlayerMove();
        Anim_Manage();
    }

    protected virtual void RotateWithCamera()
    {
        transform.rotation = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f);
    }

    void OnMove(InputValue value)
    {
        MoveDir = value.Get<Vector2>();
    }
    void OnJump() => PlayerJump();
    void OnSprint(InputValue value)
    {
        isSprint = value.Get<float>() > 0.5f;
    }
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

    protected virtual void Anim_Manage()
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
        if (cct.isGrounded && !isJumpPending)
        {
            anim.SetTrigger("Jump");
            isJumpPending = true;
            StartCoroutine(JumpDelay());
        }
    }

    IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(playerData.fJumpDelay);
        verticalVelocity = playerData.fJumpAmount;
        isJumpPending = false;
    }


}
