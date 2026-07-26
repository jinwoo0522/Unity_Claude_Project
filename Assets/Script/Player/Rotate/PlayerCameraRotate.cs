using Unity.Netcode;
using UnityEngine;

// 조준 방향(입력이 소유)을 소비해 몸통 yaw만 회전 — 상하(pitch)는 몸통에 반영하지 않는다
public class PlayerCameraRotate : NetworkBehaviour
{
    private Player_Input _input;

    private bool _isLock;   // 빙결 등으로 잠기면 조준 입력을 받아도 몸통을 돌리지 않는다

    // 현재 방향을 그대로 둔 채 회전만 정지 (상태에서 제어)
    public void Lock() => _isLock = true;
    public void Unlock() => _isLock = false;

    public override void OnNetworkSpawn()
    {
        _input = GetComponent<Player_Input>();
    }

    private void Update()
    {
        if(IsServer == false) return;
        if(_isLock == true) return;

        RotateBodyYaw();
    }

    // 조준 방향에서 수평 성분만 뽑아 몸통을 회전 (수직 조준 시 기존 회전 유지)
    private void RotateBodyYaw()
    {
        Vector3 vFlat = _input.AimDir;
        vFlat.y = 0f;

        if(vFlat.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(vFlat);
    }
}
