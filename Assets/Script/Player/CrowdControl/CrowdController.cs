using System.Collections.Generic;
using UnityEngine;

public class CrowdController : MonoBehaviour
{
    public enum CC_TAG
    {
        KNOCKBACK,
        AIRBORNE,
        FREEZE,
    }

    Dictionary<CC_TAG , ICrowdControl> CrowdControls = new();

    void Awake()
    {
        CharacterController cct = GetComponent<CharacterController>();
        
        CrowdControls.Add(CC_TAG.KNOCKBACK , new CC_Knockback(cct));
        CrowdControls.Add(CC_TAG.AIRBORNE , new CC_AirBorne(cct));
        CrowdControls.Add(CC_TAG.FREEZE , new CC_Freeze());
    }

    public void CrowdController_Update(float fTimeDelat)
    {
        foreach(var CC in CrowdControls)
        {
            CC.Value.Tick(fTimeDelat);
        }
    }

    public bool IsApply(CC_TAG tag)
    {
        return CrowdControls[tag].isFlag;
    }

    public void Apply(CC_TAG tag , ICrowdControl.CCData data)
    {
        CrowdControls[tag].Apply(data);
    }


}
