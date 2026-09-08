using System.Collections.Generic;
using UnityEngine;

public class CrowdController : MonoBehaviour
{
    public enum CC_TAG
    {
        KNOCKBACK,
        FREEZE,
        AIRBORNE,
    }

    Dictionary<CC_TAG , ICrowdControl> CrowdControls = new();
    IDamagable _damagable;
    void Awake()
    {
        _damagable = GetComponent<IDamagable>();

        CharacterController cct = GetComponent<CharacterController>();
        MaterialChanger matChanger = GetComponent<MaterialChanger>();
        EntityEffector effector = GetComponent<EntityEffector>();

        CrowdControls.Add(CC_TAG.KNOCKBACK , new CC_Knockback(cct));
        CrowdControls.Add(CC_TAG.AIRBORNE , new CC_AirBorne(cct , _damagable));
        CrowdControls.Add(CC_TAG.FREEZE , new CC_Freeze(matChanger, effector, _damagable));
    }

    public void CrowdController_Update(float fTimeDelta)
    {
        foreach(var CC in CrowdControls)
        {
            if(CC.Value.isFlag == false) continue;

            // 종료 판정은 Tick 이전에 수행 — 에어본은 자신의 상승 Move 직후 isGrounded가 항상 false가 되어 착지 판정이 성립하지 않는다
            if(CC.Value.IsExpired() == true)
            {
                CC.Value.Restore();
                continue;
            }

            CC.Value.Tick(fTimeDelta);
        }
    }

    public bool IsApply(CC_TAG tag)
    {
        
        return CrowdControls[tag].isFlag;
    }

    public void Apply(CC_TAG tag , ICrowdControl.CCData data)
    {
        if(_damagable._isDead == true) return;
        CrowdControls[tag].Apply(data);
    }
    
    public void Restore(CC_TAG tag)
    {
        CrowdControls[tag].Restore();
    }

    public void RestoreAll()
    {
        foreach(var CC in CrowdControls)
        {
            CC.Value.Restore();
        }
    }

    private void OnDisable() {

        RestoreAll(); 
    }

}
