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
        Debug.Log("넉백 적용");
    }

    public void Tick(float fTimeDelta)
    {
        if (vKnockback.sqrMagnitude < 0.01f) // 종료구문
        {
            vKnockback = Vector3.zero;
            isFlag = false;
            return;
        } 

        Vector3 vKnockbackDir = Vector3.zero;

        vKnockbackDir.x += vKnockback.x; 
        vKnockbackDir.z += vKnockback.z;

        _cct.Move(vKnockbackDir * Time.deltaTime);
        
        // 지수 감쇠: 초기에 큰 힘을 주고 급격히 줄어드는 방식 — 미끄러지듯 멈추는 현상 방지
        vKnockback *= Mathf.Exp(-fKnockbackDecay * Time.deltaTime);
    }
}
