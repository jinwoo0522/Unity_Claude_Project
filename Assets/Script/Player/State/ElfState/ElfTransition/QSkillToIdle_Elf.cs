using UnityEngine;

public class QSkillToIdle_Elf : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.IDLE;

    IEntityInputState _inputState;


    public QSkillToIdle_Elf(IEntityInputState input)
    {
        _inputState = input;
        
    }
    public bool CheckRule(float fTimeDelta)
    {
        if((_inputState.inputState & (ushort)ENTITY.InputFlagType.Q) == 0)
            return true;

        return false;
    }

    public void OnTransition()
    {
    }
}
