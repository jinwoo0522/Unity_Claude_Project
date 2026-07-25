using Unity.Netcode;
using UnityEngine;

public class MotionTrailer : NetworkBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _smr;
    [SerializeField] private Color _rimColor;
    [SerializeField] private float _fTrailTime = 0.5f;   // 잔상 1개의 유지 시간(초)
    [SerializeField] private float _fDistInterval = 0.5f; // 잔상 생성 간격(이동 거리, m)

    private float _fRunTimer;      // 남은 실행 시간
    private float _fDistAcc;       // 다음 생성까지 누적 이동 거리
    private Vector3 _vPrevPos;     // 직전 프레임 위치
    private bool _bRunning;


    // 서버에서 호출 — 지정한 시간(초) 동안 이동 거리마다 잔상을 생성
    public void Play_Trail(float fDuration)
    {
        _fRunTimer = fDuration;
        _fDistAcc = 0f;
        _vPrevPos = transform.position;
        _bRunning = true;
    }

    private void Update()
    {
        if (!IsServer || !_bRunning) return;

        // 실행 시간이 끝나면 정지
        _fRunTimer -= Time.deltaTime;
        if (_fRunTimer <= 0f)
        {
            _bRunning = false;
            return;
        }

        // 이동 거리 누적
        Vector3 vCur = transform.position;
        _fDistAcc += Vector3.Distance(vCur, _vPrevPos);
        _vPrevPos = vCur;

        // 일정 거리마다 모든 클라이언트에 잔상 생성을 전파
        if (_fDistAcc >= _fDistInterval)
        {
            _fDistAcc -= _fDistInterval;
            Start_MotionTrail_ClientRpc();
        }
    }

    // 모든 클라이언트가 각자 풀에서 잔상을 꺼내 재생
    [ClientRpc]
    public void Start_MotionTrail_ClientRpc()
    {
        MotionTrail trail = GameManager.Instance.objectPoolManager
            .Get<MotionTrail>(PoolObjectType.MOTION_TRAIL_OBJECT);

        trail.Play(_smr.transform.position, _smr.transform.rotation, _smr, _rimColor, _fTrailTime);
    }
}
