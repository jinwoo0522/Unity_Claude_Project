using UnityEngine;
using UnityEngine.InputSystem;

public class WalkToRun_Player : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.RUN;
    IEntityInputState _inputState;
    public WalkToRun_Player(IEntityInputState input)
    {
        _inputState = input;
    }
    public bool CheckRule(float fTimeDelta)
    {
        if(((_inputState.inputState & (ushort)ENTITY.InputFlagType.SPRINT) != 0) && 
        ((_inputState.inputState & (ushort)ENTITY.InputFlagType.MOVE) != 0))
            return true;
        
        return false;

    }
    public void OnTransition()
    {
    }
}
