using Unity.Netcode.Components;
using UnityEngine;

public class LandToIdle_Player  : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.IDLE;

    float fLandDuration;
    float fAccTime;
    public LandToIdle_Player(float fDuration = 1f)
    {
        fLandDuration = fDuration;
    }
    public bool CheckRule(float fTimeDelta)
    {
        fAccTime += fTimeDelta;
        // Land To Idle 애니메이션이 종료되면 idle로
        if (fAccTime >= fLandDuration)
        {
            return true;
        }

        return false;
    }

    public void OnTransition()
    {
        fAccTime = 0f;
    }
      
}
