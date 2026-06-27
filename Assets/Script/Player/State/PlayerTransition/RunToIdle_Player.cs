using Unity.Netcode.Components;
using UnityEngine;

public class RunToIdle_Player : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.IDLE;

    IEntityInputState _inputState;
    public RunToIdle_Player(IEntityInputState inputState )
    {
        _inputState = inputState;
    }

    public bool CheckRule(float fTimeDelta)
    {
        if((_inputState.inputState & (ushort)ENTITY.InputFlagType.MOVE) == 0)
            return true;
        
        return false;
    }
    public void OnTransition()
    {
    }
}
