using Unity.Netcode.Components;
using UnityEngine;

public class JumpToLand_Player : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.LAND;

    IJumpMovement _jump;
    float fJumpDelay;
    float fAccTime;
    public JumpToLand_Player(IJumpMovement jump, float fDelay = 1f)
    {
        _jump = jump;
        fJumpDelay = fDelay;
    }
    public bool CheckRule(float fTimeDelta)
    {
        fAccTime += fTimeDelta; 
         if(_jump.isGrounded == true && fAccTime > fJumpDelay) // 땅에 붙어 있
            return true;
        
        return false;
    }

    public void OnTransition()
    {
        fAccTime = 0f;
    }
}
