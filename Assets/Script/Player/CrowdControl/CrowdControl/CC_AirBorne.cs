using UnityEngine;

public class CC_AirBorne : ICrowdControl
{
    public bool isFlag {get; private set;}

    CharacterController _cct;
    IDamagable _damagable;

    float fVerticalVelocity;       // 상승 속도 (하강 중력은 외부에서 적용됨)
    float fDecay   = 3f;     // 상승력 지수 감쇠 — 띄울 때 확 올라갔다 점점 약해짐
    float fAcctime = 0f;
    const float fDelay = 0.3f;

    public CC_AirBorne(CharacterController cct , IDamagable damagable)
    {
        _cct = cct;
        _damagable = damagable;
    }

    public void Apply(ICrowdControl.CCData data)
    {
        isFlag           = true;
        fVerticalVelocity = data._fValue;   // 상승 초기속도
        fDecay = data._fDecay;
        fAcctime = 0f;
        _damagable._isHit = false;
    }

    public void Tick(float fTimeDelta)
    {
        fAcctime += fTimeDelta;

        // 상승 이동만 수행 — 하강(중력)은 외부 이동 로직에서 이미 적용 중
        Vector3 vMove = Vector3.zero;
        vMove.y = fVerticalVelocity;
        _cct.Move(vMove * fTimeDelta);

        // 지수 감쇠 — 초기에 강하게 띄우고 점점 약해져 외부 중력에 자연히 밀림
        fVerticalVelocity *= Mathf.Exp(-fDecay * fTimeDelta);
    }

    // 자가 해제: 땅을 떠난 뒤 다시 착지하면 스스로 off
    public bool IsExpired()
    {
        return _cct.isGrounded == true && fAcctime > fDelay;
    }

    public void Restore()
    {
        isFlag = false;
        fAcctime = 0f;
        fVerticalVelocity = 0f;
    }
}
