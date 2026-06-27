using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour ,IJumpMovement, IEntityMovement
{   
    [SerializeField]
    private Player_Data playerData;
    private CharacterController               _cct;
    private Stat                              _stat;


    private float                   verticalVelocity = 0f;
    public bool isGrounded => _cct.isGrounded;

    public override void OnNetworkSpawn()
    {
        // 이 객체들은 서버에서도 갱신 되어야 하기 때문에 실행해야함
        _cct         = GetComponent<CharacterController>();
        _stat       = GetComponent<Stat>();

        if(IsOwner == false)
            return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    public void Move(Vector2 MoveDir , bool isSprint)
    {
        // 스킬 중 또는 빙결 중에는 수평 입력 이동 차단 — 중력·넉백은 유지
        Vector3 vMoveDir = Vector3.zero;
        
        vMoveDir = transform.right * MoveDir.x + transform.forward * MoveDir.y;

        float fSpeed = isSprint ? playerData.fRunSpeed : playerData.fWalkSpeed;
        // 넉백 적용 (수평)

        _cct.Move(vMoveDir * Time.deltaTime * fSpeed);
    
    }

    public void Gravity()
    {
        if (_cct.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // 바닥 감지를 위한 최소 하강값
        else if (!_cct.isGrounded)
            verticalVelocity += playerData.fGravity * Time.deltaTime;

        Vector3 vGravity= Vector3.zero;

        vGravity.y = verticalVelocity;
        _cct.Move(vGravity * Time.deltaTime);
    }

    public void Jumping()
    {
        if (_cct.isGrounded)
            verticalVelocity = Mathf.Sqrt(playerData.fJumpAmount * -2f * playerData.fGravity);
    }

    // 앞방향으로 순간 전진 — fDashSpeed 속도로 fDashDistance 거리만큼 이동
    public void Dash(float fDashSpeed, float fDashDistance)
    {
        StartCoroutine(DashRoutine(fDashSpeed, fDashDistance));
    }

    private IEnumerator DashRoutine(float fDashSpeed, float fDashDistance)
    {
        float fMoved = 0f;

        while (fMoved < fDashDistance)
        {
            float fStep = fDashSpeed * Time.deltaTime;
            // 이번 프레임 이동량이 남은 거리를 넘지 않도록 보정
            fStep = Mathf.Min(fStep, fDashDistance - fMoved);

            _cct.Move(transform.forward * fStep);
            fMoved += fStep;

            yield return null;
        }
    }
}
