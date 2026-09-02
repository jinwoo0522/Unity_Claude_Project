using Unity.Netcode.Components;
using UnityEngine;

public class IdleToAttackStart_Golem : ITransition
{
    public ushort NextState => (ushort)GOLEM.UpperStateType.ATTACK_START;

    IEntityInputState _inputState;

    public IdleToAttackStart_Golem(IEntityInputState input)
    {
        _inputState = input;
    }
    public bool CheckRule(float fTimeDelta)
    {
        if((_inputState.inputState & (ushort)ENTITY.InputFlagType.MOUSE_LEFT) == 0)
            return false;

        return true;
    }

    public void OnTransition()
    {
        
    }
}
