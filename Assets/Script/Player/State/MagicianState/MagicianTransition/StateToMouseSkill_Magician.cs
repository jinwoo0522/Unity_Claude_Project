using UnityEngine;

// 우클릭 입력 + 카메라 포워드 그라운드 레이(5m 이내) 조건을 모두 만족하면 마우스 스킬 상태로 전환
// (프로젝트 입력 매핑상 물리 우클릭 = MOUSE_LEFT 플래그)
public class StateToMouseSkill_Magician : ITransition
{
    public ushort NextState => (ushort)MAGICIAN.StateType.MOUSE_SKILL;

    private Player_Input _input;
    private Transform _transform;
    private LayerMask _groundMask;
    private Transform _headPos;

    private const float fMaxRayDistance = 15f;   // 이 거리 안에서 그라운드에 맞아야 전환

    public StateToMouseSkill_Magician(Magician_Player player, LayerMask groundMask , Transform headPos)
    {
        _input = player._input;
        _transform = player.transform;
        _groundMask = groundMask;
        _headPos = headPos;
    }

    public bool CheckRule(float fTimeDelta)
    {
        if ((_input.inputState & (ushort)ENTITY.InputFlagType.MOUSE_RIGHT) == 0)
        {
            return false;
        }

        bool isHit = Physics.Raycast(_headPos.position, _input.AimDir, out RaycastHit hit, fMaxRayDistance, _groundMask);

            // [디버그] Scene 뷰 + Gizmos 켠 상태에서 Host 실행 시 조준선이 보임 (노란선=전체, 빨간선=히트)
        Debug.DrawRay(_headPos.position, _input.AimDir * fMaxRayDistance, Color.yellow);
        if (isHit)
            Debug.DrawLine(_headPos.position, hit.point, Color.red);


        // 5m 이내 그라운드에 맞지 않거나 아무것도 못 맞으면 전환 안 함
        return isHit;
    }

    public void OnTransition()
    {
    }
}
