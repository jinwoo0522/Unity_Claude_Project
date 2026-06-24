using Unity.Netcode.Components;
using UnityEditorInternal;
using UnityEngine;

public class AnyToJump_Player : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.JUMP;

    IEntityInputState _inputState;
    IJumpMovement _jump;

    EntityAnimator _aniController;

    public AnyToJump_Player(IEntityInputState input, EntityAnimator Animator , IJumpMovement jump)
    {
        _inputState = input;
        _jump = jump;
        _aniController = Animator;
    }
    public bool CheckRule(float fTimeDelta)
    {
        if(
            ((_aniController._state.Value & (ushort)ENTITY.StateType.WALK) == 0) && 
            ((_aniController._state.Value & (ushort)ENTITY.StateType.RUN) == 0 ) &&
            ((_aniController._state.Value & (ushort)ENTITY.StateType.IDLE) == 0)
          ) return false;

        if(((_inputState.inputState & (ushort)ENTITY.InputFlagType.JUMP) != 0) // 땅에 붙어 있고 점프 인풋이 눌린다면
         && (_jump.isGrounded == true))
            return true;
        
        return false;
    }

    public void OnTransition()
    {
        _aniController._state.Value = (ushort)ENTITY.StateType.JUMP;
    }
}
