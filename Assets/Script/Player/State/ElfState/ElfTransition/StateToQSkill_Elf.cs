using UnityEngine;

public class StateToQSkill_Elf : ITransition
{
    public ushort NextState => (ushort)ELF.StateType.Q_SKILL;

    IEntityInputState _inputState;


    public StateToQSkill_Elf(IEntityInputState input)
    {
        _inputState = input;
        
    }
    public bool CheckRule(float fTimeDelta)
    {
        if((_inputState.inputState & (ushort)ENTITY.InputFlagType.Q) != 0)
            return true;

        return false;
    }

    public void OnTransition()
    {
    }
}
