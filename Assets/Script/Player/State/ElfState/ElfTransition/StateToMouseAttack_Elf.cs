using UnityEngine;

public class StateToMouseAttack_Elf : ITransition
{
    public ushort NextState => (ushort)ELF.StateType.MOUSE_SKILL;

    IEntityInputState _inputState;


    public StateToMouseAttack_Elf(IEntityInputState input)
    {
        _inputState = input;
        
    }
    public bool CheckRule(float fTimeDelta)
    {
        if((_inputState.inputState & (ushort)ENTITY.InputFlagType.MOUSE_RIGHT) == 0)
            return false;

        return true;
    }

    public void OnTransition()
    {
    }
}
