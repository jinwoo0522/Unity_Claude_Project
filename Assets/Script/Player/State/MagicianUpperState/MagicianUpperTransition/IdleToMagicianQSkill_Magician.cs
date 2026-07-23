using UnityEngine;

// 상체 IDLE → 마법사 Q스킬(QSKILL) : Q 입력 시 전환
public class IdleToMagicianQSkill_Magician : ITransition
{
    public ushort NextState => (ushort)MAGICIAN.UpperStateType.QSKILL;

    IEntityInputState _inputState;

    public IdleToMagicianQSkill_Magician(IEntityInputState input)
    {
        _inputState = input;
    }

    public bool CheckRule(float fTimeDelta)
    {
        if((_inputState.inputState & (ushort)ENTITY.InputFlagType.Q) == 0)
            return false;

        return true;
    }

    public void OnTransition() { }
}
