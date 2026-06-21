using UnityEngine;
using UnityEngine.InputSystem;

public class IdleToWalk_Player : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.WALK;
    IEntityInputState _inputState;
    public IdleToWalk_Player(IEntityInputState input)
    {
        _inputState = input;

    }
    public bool CheckRule(float fTimeDelta)
    {
        if((_inputState.inputState & (ushort)ENTITY.InputFlagType.MOVE) != 0)
            return true;
        
        return false;

    }
    public void OnTransition()
    {
    }
}
