using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour , IJumpMovement
{   
    [SerializeField]
    private Player_Data playerData;
    private CharacterController               _cct;
    private   Stat                            _stat;


    private float                   verticalVelocity = 0f;
    private Vector3                 vKnockback;
    [SerializeField] private float  fKnockbackDecay = 12f;

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

    public void PlayerMove(Vector2 MoveDir , bool isSprint)
    {
        // 스킬 중 또는 빙결 중에는 수평 입력 이동 차단 — 중력·넉백은 유지
        Vector3 vMoveDir = Vector3.zero;
        
        vMoveDir = transform.right * MoveDir.x + transform.forward * MoveDir.y;

        float fSpeed = isSprint ? playerData.fRunSpeed : playerData.fWalkSpeed;
        // 넉백 적용 (수평)

        _cct.Move(vMoveDir * Time.deltaTime * fSpeed);
    
    }

    public void KnockBack()
    {
        Vector3 vKnockbackDir = Vector3.zero;

        vKnockbackDir.x += vKnockback.x; 
        vKnockbackDir.z += vKnockback.z;

        _cct.Move(vKnockbackDir * Time.deltaTime);
        
        // 지수 감쇠: 초기에 큰 힘을 주고 급격히 줄어드는 방식 — 미끄러지듯 멈추는 현상 방지
        vKnockback *= Mathf.Exp(-fKnockbackDecay * Time.deltaTime);
        if (vKnockback.sqrMagnitude < 0.01f) vKnockback = Vector3.zero;
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
}
