using UnityEngine;

public class AttackStartToLast_Golem : ITransition
{
    public ushort NextState => (ushort)GOLEM.UpperStateType.ATTACK_LAST;

    EntityAnimator _upperAniController;
    IEntityInputState _inputState;

    public AttackStartToLast_Golem(IEntityInputState input, EntityAnimator upperAniController)
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
    }
}
