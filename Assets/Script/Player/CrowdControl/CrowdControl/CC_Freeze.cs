using UnityEngine;

public class CC_Freeze : ICrowdControl
{
    public bool isFlag {get; private set;}

    float fTime;

    public void Apply(ICrowdControl.CCData data)
    {
        isFlag = true;
        fTime = data._fValue;
    }

    public void Tick(float fTimeDelta)
    {
        if(fTime <= 0f)
        {
            isFlag = false;
            return;
        }

        fTime -= fTimeDelta;
    }
}
