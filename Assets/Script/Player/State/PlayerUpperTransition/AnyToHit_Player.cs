using UnityEngine;

public class AnyToHit_Player : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.JUMP;



    EntityAnimator _upperAniController;

    public AnyToHit_Player(EntityAnimator upperAniController)
    {
        _upperAniController = upperAniController;
    }
    public bool CheckRule(float fTimeDelta)
    {
        return false;
    }

    public void OnTransition()
    {
        _upperAniController._state.Value = (ushort)ELF.UpperStateType.HIT;
    }
}
