using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player_Move : MonoBehaviour
{
    [Header("이동")]
    public float fWalkSpeed = 3f;
    public float fRunSpeed  = 6f;
    [Header("점프")]
    public float fJumpAmount = 5f;
    public float fGravity = -9.8f;
    public float fJumpDelay = 0.1f;
    [Header("애니메이션")]
    public float fAnimSpeed = 3f;
    private bool isSprint = false;
    private Vector2 MoveDir;
    private float verticalVelocity = 0f;

    private Vector2 vAnimLerp;
    private bool isJumpPending = false;

    CharacterController               cct;
    Animator                          anim;
    Player_UpperBody                  playerUpper;

    void Awake()
    {

    }

    void Start()
    {
        cct      = GetComponent<CharacterController>();
        anim     = GetComponentInChildren<Animator>();
        playerUpper = GetComponent<Player_UpperBody>();
    }

    void Update()
    {
        PlayerMove();
        Anim_Manage();
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
    void PlayerMove()
    {
        Vector3 vMoveDir = new Vector3(MoveDir.x, 0, MoveDir.y);

        float fSpeed = isSprint ? fRunSpeed : fWalkSpeed;
        vMoveDir *= fSpeed;

        // 중력 처리
        if (cct.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // 바닥 감지를 위한 최소 하강값
        else if (!cct.isGrounded)
            verticalVelocity += fGravity * Time.deltaTime;

        vMoveDir.y = verticalVelocity;
        cct.Move(vMoveDir * Time.deltaTime);
    }

    void Anim_Manage()
    {
        // 이동 중이면 해당 방향/속도로, 멈추면 0으로 보간
        float fTargetScale = isSprint ? 1f : 0.5f;
        Vector2 vTarget = MoveDir.magnitude > 0.1f ? MoveDir.normalized * fTargetScale
        : Vector2.zero;
        vAnimLerp = Vector2.Lerp(vAnimLerp, vTarget, Time.deltaTime * fAnimSpeed);

        anim.SetFloat("MoveX", vAnimLerp.x);
        anim.SetFloat("MoveZ", vAnimLerp.y);
        anim.SetBool("IsGrounded", cct.isGrounded && !isJumpPending);
    }

    void PlayerJump()
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
        yield return new WaitForSeconds(fJumpDelay);
        verticalVelocity = fJumpAmount;
        isJumpPending = false;
    }


}
