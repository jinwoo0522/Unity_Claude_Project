using UnityEngine;

public class StateToMouseAttack_Golem : ITransition
{
    public ushort NextState => (ushort)GOLEM.StateType.MOUSE_SKILL;

    IEntityInputState _inputState;
    private StateMachine _upperStateMachine;


    public StateToMouseAttack_Golem(IEntityInputState input, StateMachine upperStateMachine)
    {
        _inputState = input;
        _upperStateMachine = upperStateMachine;
    }
    public bool CheckRule(float fTimeDelta)
    {
        // 상체가 공격·피격 중이면 전신 스킬로 넘어가지 않는다
        if(_upperStateMachine.CurrentState != (ushort)ENTITY.UpperStateType.IDLE)
            return false;

        if((_inputState.inputState & (ushort)ENTITY.InputFlagType.MOUSE_RIGHT) == 0)
            return false;

        return true;
    }

    public void OnTransition()
    {
    }
}
