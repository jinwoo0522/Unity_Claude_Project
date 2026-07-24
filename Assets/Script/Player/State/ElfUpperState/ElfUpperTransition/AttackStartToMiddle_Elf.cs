using UnityEngine;

public class AttackStartToMiddle_Elf : ITransition
{
    public ushort NextState => (ushort)ELF.UpperStateType.ATTACK_MIDDLE;

    EntityAnimator _upperAniController;
    IEntityInputState _inputState;

    public AttackStartToMiddle_Elf(IEntityInputState input, EntityAnimator upperAniController)
    {
        _upperAniController = upperAniController;
        _inputState = input;
    }
    public bool CheckRule(float fTimeDelta)
    {
        if(_upperAniController.IsCurrentStateFinished() == false)
            return false;

        if((_inputState.inputState & (ushort)ENTITY.InputFlagType.MOUSE_LEFT) == 0)
            return false;

        return true;
    }

    public void OnTransition()
    {
        Debug.Log("Start -> Middle");
    }
}
