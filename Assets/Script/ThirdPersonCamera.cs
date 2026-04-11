using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("타겟")]
    public Transform target;

    [Header("거리/높이")]
    public float fDistance = 5f;
    public float fHeight   = 2f;

    [Header("마우스 감도")]
    public float fSensitivityX = 3f;
    public float fSensitivityY = 2f;

    [Header("수직 각도 제한")]
    public float fMinPitch = -20f;
    public float fMaxPitch =  60f;

    float _fYaw;
    float _fPitch = 15f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        _fYaw = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        _fYaw   += Input.GetAxis("Mouse X") * fSensitivityX;
        _fPitch -= Input.GetAxis("Mouse Y") * fSensitivityY;
        _fPitch  = Mathf.Clamp(_fPitch, fMinPitch, fMaxPitch);

        Quaternion qRot = Quaternion.Euler(_fPitch, _fYaw, 0f);
        transform.position = target.position + Vector3.up * fHeight - qRot * Vector3.forward * fDistance;
        transform.rotation = qRot;
    }
}
