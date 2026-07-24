using UnityEngine;

// 상체 IDLE → 마법사 공격(ATTACK) : 마우스 우클릭 입력 시 전환
public class IdleToMagicianAttack_Magician : ITransition
{
    public ushort NextState => (ushort)MAGICIAN.UpperStateType.ATTACK;

    IEntityInputState _inputState;

    public IdleToMagicianAttack_Magician(IEntityInputState input)
    {
        _inputState = input;
    }

    public bool CheckRule(float fTimeDelta)
    {
        if((_inputState.inputState & (ushort)ENTITY.InputFlagType.MOUSE_LEFT) == 0)
            return false;

        return true;
    }

    public void OnTransition() { }
}
