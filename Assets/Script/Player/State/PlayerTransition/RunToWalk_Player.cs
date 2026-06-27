using UnityEngine;

public class RunToWalk_Player : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.WALK;


    IEntityInputState _inputState;

    public RunToWalk_Player(IEntityInputState Input)
    {
        _inputState = Input;
    }

    public bool CheckRule(float fTimeDelta)
    {
        if(((_inputState.inputState & (ushort)ENTITY.InputFlagType.SPRINT) == 0)
        && ((_inputState.inputState & (ushort)ENTITY.InputFlagType.MOVE) != 0))
            return true;

        return false;
    }
    public void OnTransition()
    {
    }
}
