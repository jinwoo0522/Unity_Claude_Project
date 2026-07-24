using Unity.Netcode.Components;
using UnityEditorInternal;
using UnityEngine;

public class IdleToAttackStart_Elf : ITransition
{
    public ushort NextState => (ushort)ELF.UpperStateType.ATTACK_START;

    IEntityInputState _inputState;

    public IdleToAttackStart_Elf(IEntityInputState input)
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
