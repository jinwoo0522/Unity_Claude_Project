using UnityEngine;

// 조준 방향(입력이 소유)을 소비해 몸통 yaw만 회전 — 상하(pitch)는 몸통에 반영하지 않는다
// 컴포넌트가 아닌 순수 C# 객체 — 상태머신이 서버에서만 돌므로 서버 권위는 호출 시점이 보장한다
public class PlayerCameraRotate : IEntityRotate
{
    private Transform _transform;
    private Player_Input _input;

    public PlayerCameraRotate(Transform transform, Player_Input input)
    {
        _transform = transform;
        _input = input;
    }

    // 조준 방향에서 수평 성분만 뽑아 몸통을 회전 (수직 조준 시 기존 회전 유지)
    public void Rotate()
    {
        Vector3 vFlat = _input.AimDir;
        vFlat.y = 0f;

        if(vFlat.sqrMagnitude < 0.0001f) return;

        _transform.rotation = Quaternion.LookRotation(vFlat);
    }
}
