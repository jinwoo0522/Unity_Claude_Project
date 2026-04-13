using System;
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
    [Header("애니메이션")]
    public float fAnimSpeed = 3f;
    private bool isSprint = false;
    private Vector2 MoveDir;
    private float verticalVelocity = 0f;
    private float fAccTime = 0f;
    private Vector2 vAnimLerp;

    CharacterController               cct;
    Animator                          anim;

    void Awake()
    {

    }

    void Start()
    {
        cct   = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
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
        isSprint = value.isPressed;
    }
    void PlayerMove()
    {
        Vector2 vMoveInput = MoveDir;
        
        Vector3 vMoveDir = new Vector3(vMoveInput.x , 0, vMoveInput.y);

        float fSpeed = isSprint ? fRunSpeed : fWalkSpeed;

        vMoveDir *= fSpeed; // 이동 속도!

        if (cct.isGrounded == false)
        {
             fAccTime += Time.deltaTime;
             verticalVelocity += fGravity * fAccTime * fAccTime;  // 중력 가속
             vMoveDir.y = verticalVelocity; // 중력 적용
        }

        cct.Move(vMoveDir * Time.deltaTime);
    }

    void Anim_Manage()
    {
        vAnimLerp += MoveDir * Time.deltaTime * fAnimSpeed;
        float fClampValue = 0.5f;
        fClampValue = isSprint ? fClampValue + 0.5f : fClampValue;

        vAnimLerp.x = Mathf.Clamp(vAnimLerp.x ,-fClampValue , fClampValue);
        vAnimLerp.y = Mathf.Clamp(vAnimLerp.y , -fClampValue , fClampValue);

        anim.SetFloat("MoveX",vAnimLerp.x);
        anim.SetFloat("MoveZ",vAnimLerp.y);
    }

    void PlayerJump()
    {
        if (cct.isGrounded)
        { 
            verticalVelocity = fJumpAmount;
            fAccTime = 0f;
        }
    }


}
