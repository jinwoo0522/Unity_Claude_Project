using UnityEngine;

public class CC_Knockback : ICrowdControl
{
    public bool isFlag {get; private set;}

    CharacterController _cct;

    private Vector3                 vKnockback;
    private float                   fKnockbackDecay;

    public CC_Knockback(CharacterController cct)
    {
        _cct = cct;
    }

    public void Apply(ICrowdControl.CCData data)
    {
        isFlag = true;
        vKnockback = data._vDir * data._fValue;
        fKnockbackDecay = data._fDecay;
    }

    public void Tick(float fTimeDelta)
    {
        Vector3 vKnockbackDir = Vector3.zero;

        vKnockbackDir.x += vKnockback.x; 
        vKnockbackDir.z += vKnockback.z;

        _cct.Move(vKnockbackDir * fTimeDelta);
        
        // 지수 감쇠: 초기에 큰 힘을 주고 급격히 줄어드는 방식 — 미끄러지듯 멈추는 현상 방지
        vKnockback *= Mathf.Exp(-fKnockbackDecay * fTimeDelta);
    }

    public bool IsExpired()
    {
        return vKnockback.sqrMagnitude < 0.01f;
    }

    public void Restore()
    {
        vKnockback = Vector3.zero;
        isFlag = false;
    }
}
