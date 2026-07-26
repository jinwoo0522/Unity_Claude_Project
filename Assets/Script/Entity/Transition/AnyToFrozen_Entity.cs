using UnityEngine;

public class AnyToFrozen_Entity : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.FROZEN;


    CrowdController _crowdController;
    public AnyToFrozen_Entity(CrowdController crowdController)
    {
        _crowdController = crowdController;
    }

    public bool CheckRule(float fTimeDelta)
    {
        if(_crowdController.IsApply(CrowdController.CC_TAG.FREEZE) == true)
            return true;

        return false;
    }

    public void OnTransition()
    {
    }
}
