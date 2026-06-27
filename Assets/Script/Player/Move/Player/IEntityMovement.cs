using UnityEngine;

public interface IEntityMovement
{
    public void Move(Vector2 MoveDir , bool isSprint);
    public void Dash(float fDashSpeed, float fDashDistance);
    public void Gravity();
}
