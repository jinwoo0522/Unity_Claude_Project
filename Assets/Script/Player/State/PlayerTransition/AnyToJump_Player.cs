using Unity.Netcode.Components;
using UnityEditorInternal;
using UnityEngine;

public class AnyToJump_Player : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.JUMP;

    IEntityInputState _inputState;
    IJumpMovement _jump;

    EntityAnimator _aniController;
    CrowdController _crowdController;

    public AnyToJump_Player(IEntityInputState input, EntityAnimator Animator , IJumpMovement jump,
     CrowdController crowdController)
    {
        _inputState = input;
        _jump = jump;
        _aniController = Animator;
        _crowdController = crowdController;
    }
    public bool CheckRule(float fTimeDelta)
    {
        // 빙결 중에는 애니 상태값이 IDLE/WALK/RUN 그대로 남아 있으므로 별도로 막아야 한다
        if(_crowdController.IsApply(CrowdController.CC_TAG.FREEZE) == true)
            return false;

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
