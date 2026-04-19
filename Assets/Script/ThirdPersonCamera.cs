using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("타겟")]
    public Transform target; // 카메라가 따라갈 대상 (플레이어)

    [Header("거리/높이")]
    public float fDistance = 5f; // 타겟으로부터의 거리
    public float fHeight   = 2f; // 타겟 기준 카메라 높이 오프셋

    [Header("마우스 감도")]
    public float fSensitivityX = 3f; // 좌우 회전 감도
    public float fSensitivityY = 2f; // 상하 회전 감도

    [Header("수직 각도 제한")]
    public float fMinPitch = -20f; // 카메라 최대 올림 각도
    public float fMaxPitch =  60f; // 카메라 최대 내림 각도

    float _fYaw   = 0f;  // 좌우 회전값 (Y축)
    float _fPitch = 15f; // 상하 회전값 (X축)

    InputSystem_Actions               _input;
    InputSystem_Actions.PlayerActions _player;

    void Awake()
    {
        // Input 액션 인스턴스 생성 및 Player 액션맵 참조
        _input  = new InputSystem_Actions();
        _player = _input.Player;
    }

    void OnEnable()  => _player.Enable();
    void OnDisable() => _player.Disable();
    void OnDestroy() => _input.Dispose();

    void Start()
    {
        // 마우스 커서 숨기고 화면 중앙에 고정

        _fYaw = target.eulerAngles.y;
    }

    // LateUpdate : 플레이어 이동 후 카메라 위치를 갱신 (Update보다 늦게 실행)
    void LateUpdate()
    {
        if (target == null) return;

        // 마우스 델타값 읽기 (x=좌우, y=상하)
        Vector2 vLook = _player.Look.ReadValue<Vector2>();
        _fYaw   += vLook.x * fSensitivityX;
        _fPitch -= vLook.y * fSensitivityY; // 마우스 위로 올리면 카메라도 올라가도록 반전
        _fPitch  = Mathf.Clamp(_fPitch, fMinPitch, fMaxPitch);

        // 회전값으로 쿼터니언 생성
        Quaternion qRot = Quaternion.Euler(_fPitch, _fYaw, 0f);

        // 플레이어 Y축 회전을 카메라 Yaw에 동기화
        target.rotation = Quaternion.Euler(0f, _fYaw, 0f);

        // 타겟 뒤쪽으로 거리만큼 떨어진 위치에 카메라 배치
        transform.position = target.position + Vector3.up * fHeight - qRot * Vector3.forward * fDistance;
        transform.rotation = qRot;
    }
}
