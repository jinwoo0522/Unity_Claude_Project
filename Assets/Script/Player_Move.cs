using UnityEngine;

public class Player_Move : MonoBehaviour
{
    [Header("이동")]
    public float fWalkSpeed = 3f;
    public float fRunSpeed  = 6f;

    [Header("점프")]
    public float fJumpHeight = 1.5f;
    public float fGravity    = -20f;

    CharacterController _cc;
    Animator            _anim;
    Vector3             _vVelocity;

    void Start()
    {
        _cc   = GetComponent<CharacterController>();
        _anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        float fH = Input.GetAxisRaw("Horizontal");
        float fV = Input.GetAxisRaw("Vertical");

        bool  bRunning = Input.GetKey(KeyCode.LeftShift);
        float fSpeed   = bRunning ? fRunSpeed : fWalkSpeed;
        Vector3 vMove  = new Vector3(fH, 0f, fV).normalized * fSpeed;

        if (vMove.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(new Vector3(vMove.x, 0f, vMove.z));

        if (_cc.isGrounded && _vVelocity.y < 0f)
            _vVelocity.y = -2f;

        if (Input.GetKeyDown(KeyCode.Space) && _cc.isGrounded)
        {
            _vVelocity.y = Mathf.Sqrt(fJumpHeight * -2f * fGravity);
            _anim.SetTrigger("Jump");
        }

        if (Input.GetMouseButtonDown(0))
            _anim.SetTrigger("Attack");

        _vVelocity.y += fGravity * Time.deltaTime;
        _cc.Move((vMove + Vector3.up * _vVelocity.y) * Time.deltaTime);

        float fNormSpeed = vMove.sqrMagnitude > 0.01f ? (bRunning ? 1f : 0.5f) : 0f;
        _anim.SetFloat("MoveX", fH * fNormSpeed, 0.1f, Time.deltaTime);
        _anim.SetFloat("MoveZ", fV * fNormSpeed, 0.1f, Time.deltaTime);
        _anim.SetFloat("Speed", fNormSpeed,       0.1f, Time.deltaTime);
        _anim.SetBool("IsGrounded", _cc.isGrounded);
    }
}
