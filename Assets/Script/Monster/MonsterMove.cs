using UnityEngine;
using UnityEngine.AI;


public class MonsterMove : MonoBehaviour, IEntityMovement
{
    [SerializeField] private CharacterController _cct;
    [SerializeField] private Stat _stat;
    [SerializeField] private Monster _goblin;   // 타게터 소유자 — 타겟은 매 프레임 바뀌므로 참조만 캐싱한다
    [SerializeField] private NavMeshAgent _agent;
    private float _fVerticalVelocity = 0f;

    void Awake()
    {
        _agent.updatePosition = false;
    }


    // 이동 방향은 타게터가 정하므로 인자로 받은 방향은 사용하지 않는다 (인터페이스 호환용)
    public void Move(Vector2 vMoveDir, bool isSprint)
    {
        Transform target = _goblin._targeter.Target;

        if(target == null)
        {
            _agent.nextPosition = _cct.transform.position;
            return;
        }

        _agent.SetDestination(target.position);

        Vector3 vFlat = _agent.desiredVelocity;
        vFlat.y = 0f;

        float fSpeed = isSprint ? _stat._data.fRunSpeed : _stat._data.fWalkSpeed;

        _cct.Move(vFlat.normalized * fSpeed * Time.deltaTime);

        _agent.nextPosition = _cct.transform.position;   // CCT 이동 후 NavMeshAgent 위치를 동기화

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
