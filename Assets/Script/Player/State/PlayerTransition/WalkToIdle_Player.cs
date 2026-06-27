using UnityEngine;

public class WalkToIdle_Player : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.IDLE;

    IEntityInputState _inputState;

    public WalkToIdle_Player(IEntityInputState inputState)
    {
        _inputState = inputState;
    }

    public bool CheckRule(float fTimeDelta)
    {
        if((_inputState.inputState & (ushort)ENTITY.InputFlagType.MOVE) != 0)
            return false;
        
        return true;
    }
    public void OnTransition()
    {
    }
}
