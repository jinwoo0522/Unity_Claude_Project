using UnityEngine;

public class AnyToAirborne_Entity : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.AIRBORNE;


    CrowdController _crowdController;
    public AnyToAirborne_Entity(CrowdController crowdController)
    {
        _crowdController = crowdController;
    }

    public bool CheckRule(float fTimeDelta)
    {
        if(_crowdController.IsApply(CrowdController.CC_TAG.AIRBORNE) == true)
            return true;

        return false;
    }

    public void OnTransition()
    {
    }
}
