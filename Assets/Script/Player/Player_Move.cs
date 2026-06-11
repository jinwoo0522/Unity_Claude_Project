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
    // 착지 기반 점프 게이트 — 서버가 착지 시점에만 false로 리셋
    protected NetworkVariable<bool> net_isJumpPending = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    protected float fCamYaw;

    // 지수 감쇠 계수 — 값이 클수록 넉백이 빠르게 소멸 (Inspector에서 튜닝)
    [SerializeField] private float fKnockbackDecay = 12f;
    protected Vector3 vKnockback;

    // isGrounded 플리커 대응 — 서버 전용, fLastGroundedTime 기준으로 유예 판정
    private float fLastGroundedTime;
    private const float GroundedGraceTime = 0.15f;


    protected CharacterController               cct;
    protected Animator                          anim;

    protected NetworkAnimator                   net_anim;
    protected Player_UpperBody                  playerUpper;
    private   Player_Skill                      skill;
    private   StatusEffect_Airborne             _airborne;
    private   StatusEffect_Slow                 _slow;

    public override void OnNetworkSpawn()
    {
        // 이 객체들은 서버에서도 갱신 되어야 하기 때문에 실행해야함
        cct         = GetComponent<CharacterController>();
        // Animator가 루트로 이동됐으므로 GetComponent로 직접 참조
        anim        = GetComponent<Animator>();
        net_anim    = GetComponent<NetworkAnimator>();
        playerUpper = GetComponent<Player_UpperBody>();
        skill       = GetComponent<Player_Skill>();
        _airborne   = GetComponent<StatusEffect_Airborne>();
        _slow       = GetComponent<StatusEffect_Slow>();

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

        // 스킬 중에는 수평 입력 이동 차단 — 루트모션과의 간섭 방지(중력·넉백은 유지)
        Vector3 vMoveDir = Vector3.zero;
        if (skill == null || !skill.IsSkilling)
        {
            vMoveDir = transform.right * MoveDir.x + transform.forward * MoveDir.y;

            float fSpeed = isSprint ? playerData.fRunSpeed : playerData.fWalkSpeed;
            // 슬로우 배율 적용 — 서버에서만 읽히므로 로컬 float으로 충분
            vMoveDir *= fSpeed * _slow.SpeedMultiplier;
        }

        // 공중 띄움 펜딩 상승속도 1회 소비 — 점프와 동일 경로로 verticalVelocity에 주입
        if (_airborne.ConsumePendingLaunch(out float launchForce))
            verticalVelocity = launchForce;

        // 중력 처리
        if (cct.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // 바닥 감지를 위한 최소 하강값
        else if (!cct.isGrounded)
            verticalVelocity += playerData.fGravity * Time.deltaTime;

        // 중력 적용
        vMoveDir.y = verticalVelocity;
        // 넉백 적용 (수평)
        vMoveDir.x += vKnockback.x; 
        vMoveDir.z += vKnockback.z;
        
        cct.Move(vMoveDir * Time.deltaTime);
        if (cct.isGrounded) fLastGroundedTime = Time.time; // 접지 시각 갱신 (유예 판정용)
        
        // 지수 감쇠: 초기에 큰 힘을 주고 급격히 줄어드는 방식 — 미끄러지듯 멈추는 현상 방지
        vKnockback *= Mathf.Exp(-fKnockbackDecay * Time.deltaTime);
        if (vKnockback.sqrMagnitude < 0.01f) vKnockback = Vector3.zero;
    }

    public void ApplyKnockback(Vector3 dir, float strength)
    {
        dir.y = 0f;
        vKnockback = dir.normalized * strength;
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
        // 공중 상태 포함 — 띄움 직후 IsGrounded 플리커 차단, 점프와 동일 애니 판정 보장
        anim.SetBool("IsGrounded", cct.isGrounded && !net_isJumpPending.Value && !_airborne.IsAirborne);
    }

    protected virtual void PlayerJump()
    {
        if (playerUpper != null && playerUpper.IsHit) return;
        if (skill != null && skill.IsSkilling) return;  // 스킬 중 점프 차단
        if (net_isJumpPending.Value) return;            // 착지 전 재점프 차단
        if (_airborne.IsAirborne) return;               // 공중 띄움 중 점프 차단
        Jump_ServerRpc();                               // 서버 검증·적용 요청 (애니 트리거는 서버 승인 후)
    }

    [ServerRpc]
    void Jump_ServerRpc()
    {
        if (net_isJumpPending.Value) return;
        if (_airborne.IsAirborne) return;               // 서버 재검증: RPC 위조 클라의 공중 점프 차단
        // 서버 검증: isGrounded 플리커 대응 — 유예 시간(0.15s) 내 접지 이력이 있으면 통과
        if (Time.time - fLastGroundedTime > GroundedGraceTime) return;
        net_isJumpPending.Value = true;
        net_anim.SetTrigger("Jump"); // 서버 승인 후 트리거 — 기각 시 애니 깜빡임 제거
        StartCoroutine(JumpRoutine());
    }

    // 착지 시점에만 게이트를 해제 — 공중 연속 점프 방지
    IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(playerData.fJumpDelay);
        verticalVelocity = playerData.fJumpAmount;          // 서버에서 직접 적용
        yield return new WaitUntil(() => !cct.isGrounded);  // 이륙 대기
        yield return new WaitUntil(() => cct.isGrounded);   // 착지 대기
        net_isJumpPending.Value = false;                    // 착지 시점에만 해제
    }

    [ServerRpc]
    void SubmitSprint_ServerRpc(bool _isSprint)
    {
        isSprint = _isSprint;
    }

    [ServerRpc]
    void SubmitMoveInput_ServerRpc(Vector2 _Input)
    {
        MoveDir = _Input;
    }

    [ServerRpc]
    void SubmitCamYaw_ServerRpc(float _Input)
    {
        fCamYaw = _Input;
    }

    
}
