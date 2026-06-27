using UnityEngine;

public class AirborneToLand_Entity : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.LAND;


    CrowdController _crowdController;
    public AirborneToLand_Entity(CrowdController crowdController)
    {
        _crowdController = crowdController;
    }

    public bool CheckRule(float fTimeDelta)
    {
        if(_crowdController.IsApply(CrowdController.CC_TAG.AIRBORNE) == false)
            return true;

        return false;
    }

    public void OnTransition()
    {
    }
}
