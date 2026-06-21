using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
public class Player_Move : NetworkBehaviour
{
    // 인스펙터에서 할당된 객체들은 캐싱해서 들어가서 서버가 알고있음
    [SerializeField]
    protected Player_Data playerData;

    // 플레이어 인풋을 통해 들어오는 값은 클라가 행하기 때문에 서버는 모름  
    
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
    private   Player_Status                     _status;

    private   Stat                              _stat;

    public override void OnNetworkSpawn()
    {
        // 이 객체들은 서버에서도 갱신 되어야 하기 때문에 실행해야함
        cct         = GetComponent<CharacterController>();
        // Animator가 루트로 이동됐으므로 GetComponent로 직접 참조
        anim        = GetComponent<Animator>();
        net_anim    = GetComponent<NetworkAnimator>();
        playerUpper = GetComponent<Player_UpperBody>();
        skill       = GetComponent<Player_Skill>();
        _status     = GetComponent<Player_Status>();
        _stat       = GetComponent<Stat>();

        if(IsOwner == false)
            return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        if(IsServer == true)
        {
            //AnimManage();
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
            GravityManage();

            if (_status.IsFrozen) return; // 빙결 시 멈춤
            RotateWithCamera();
            
        }
    }

    protected virtual void RotateWithCamera()
    {
        if(_stat._isDead == true) return;
        transform.rotation = Quaternion.Euler(0f, fCamYaw, 0f);
    }



    //서버가 클라에서 수행한 인풋값을 모르기때문에 인자로 넘겨줘야함
    public void PlayerMove(Vector2 MoveDir , bool isSprint)
    {

        // 스킬 중 또는 빙결 중에는 수평 입력 이동 차단 — 중력·넉백은 유지
        Vector3 vMoveDir = Vector3.zero;
        
        vMoveDir = transform.right * MoveDir.x + transform.forward * MoveDir.y;

        float fSpeed = isSprint ? playerData.fRunSpeed : playerData.fWalkSpeed;
            // 슬로우 배율 적용 — 서버에서만 읽히므로 로컬 float으로 충분
        vMoveDir *= fSpeed * _status.SpeedMultiplier;
        // 넉백 적용 (수평)
        vMoveDir.x += vKnockback.x; 
        vMoveDir.z += vKnockback.z;
        
        cct.Move(vMoveDir * Time.deltaTime);
        if (cct.isGrounded) fLastGroundedTime = Time.time; // 접지 시각 갱신 (유예 판정용)
        
        // 지수 감쇠: 초기에 큰 힘을 주고 급격히 줄어드는 방식 — 미끄러지듯 멈추는 현상 방지
        vKnockback *= Mathf.Exp(-fKnockbackDecay * Time.deltaTime);
        if (vKnockback.sqrMagnitude < 0.01f) vKnockback = Vector3.zero;
    }

    void GravityManage()
    {
        if (cct.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // 바닥 감지를 위한 최소 하강값
        else if (!cct.isGrounded)
            verticalVelocity += playerData.fGravity * Time.deltaTime;

        // 공중 띄움 펜딩 상승속도 1회 소비 — 점프와 동일 경로로 verticalVelocity에 주입
        if (_status.ConsumePendingLaunch(out float launchForce))
            verticalVelocity = launchForce;

        Vector3 vGravity= Vector3.zero;

        vGravity.y = verticalVelocity;
        cct.Move(vGravity * Time.deltaTime);
    }

    public void ApplyKnockback(Vector3 dir, float strength)
    {
        dir.y = 0f;
        vKnockback = dir.normalized * strength;
    }

    protected virtual void AnimManage()
    {    
        // 공중 상태 포함 — 띄움 직후 IsGrounded 플리커 차단, 점프와 동일 애니 판정 보장
        anim.SetBool("IsGrounded", cct.isGrounded && !net_isJumpPending.Value && !_status.IsAirborne);
    }

    protected virtual void PlayerJump()
    {
        if (playerUpper != null && playerUpper.IsHit) return;
        if (skill != null && skill.IsSkilling) return;  // 스킬 중 점프 차단
        if (net_isJumpPending.Value) return;            // 착지 전 재점프 차단
        if (_status.IsAirborne) return;                 // 공중 띄움 중 점프 차단
        if (_status.IsFrozen) return;                   // 빙결 중 점프 차단
        Jump_ServerRpc();                               // 서버 검증·적용 요청 (애니 트리거는 서버 승인 후)
    }

    [ServerRpc]
    void Jump_ServerRpc()
    {
        if (net_isJumpPending.Value) return;
        if (_status.IsAirborne) return;                 // 서버 재검증: RPC 위조 클라의 공중 점프 차단
        if (_status.IsFrozen) return;                   // 서버 재검증: 빙결 중 점프 차단
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
    void SubmitCamYaw_ServerRpc(float _Input)
    {
        fCamYaw = _Input;
    }

    
}
