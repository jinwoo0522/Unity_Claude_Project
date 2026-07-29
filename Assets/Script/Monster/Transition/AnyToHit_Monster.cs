using UnityEngine;

// 어떤 상태에서든 피격되면 즉시 경직으로 전환
public class AnyToHit_Monster : ITransition
{
    public ushort NextState => (ushort)MONSTER.StateType.HIT;

    private IDamagable _damagable;
    private CrowdController _crowdController;

    public AnyToHit_Monster(CrowdController crowdController, IDamagable damagable)
    {
        _crowdController = crowdController;
        _damagable = damagable;
    }

    public bool CheckRule(float fTimeDelta)
    {
        // 에어본·빙결 중에는 경직으로 끊지 않는다 (CC가 끝난 뒤 자기 흐름대로 복귀해야 함)
        if(_crowdController.IsApply(CrowdController.CC_TAG.AIRBORNE) == true)
            return false;

        if(_crowdController.IsApply(CrowdController.CC_TAG.FREEZE) == true)
            return false;

        return _damagable._isHit;
    }

    public void OnTransition()
    {
    }
}
