using UnityEngine;

// 타게터가 잡아둔 대상 방향으로 수평 이동 — 중력 규칙은 플레이어와 동일하다
// 호출 시점은 상태가 정하며, 상태머신이 서버에서만 돌므로 서버 권위는 호출 시점이 보장한다
public class MonsterMove : MonoBehaviour, IEntityMovement
{
    private CharacterController _cct;
    private Stat _stat;
    private Goblin _goblin;   // 타게터 소유자 — 타겟은 매 프레임 바뀌므로 참조만 캐싱한다

    private float _fVerticalVelocity = 0f;

    private void Awake()
    {
        _cct = GetComponent<CharacterController>();
        _stat = GetComponent<Stat>();
        _goblin = GetComponent<Goblin>();
    }

    // 이동 방향은 타게터가 정하므로 인자로 받은 방향은 사용하지 않는다 (인터페이스 호환용)
    public void Move(Vector2 vMoveDir, bool isSprint)
    {
        Transform target = _goblin._targeter.Target;

        if(target == null) return;

        Vector3 vFlat = target.position - transform.position;
        vFlat.y = 0f;

        float fSpeed = isSprint ? _stat._data.fRunSpeed : _stat._data.fWalkSpeed;

        _cct.Move(vFlat.normalized * fSpeed * Time.deltaTime);
    }

    public void Gravity()
    {
        if (_cct.isGrounded && _fVerticalVelocity < 0f)
            _fVerticalVelocity = -2f;   // 바닥 감지를 위한 최소 하강값
        else if (!_cct.isGrounded)
            _fVerticalVelocity += _stat._data.fGravity * Time.deltaTime;

        Vector3 vGravity = Vector3.zero;

        vGravity.y = _fVerticalVelocity;
        _cct.Move(vGravity * Time.deltaTime);
    }

    // 몬스터는 대시를 사용하지 않는다 — 인터페이스 구현만 채운다
    public void Dash(float fDashSpeed, float fDashDistance)
    {
    }
}
