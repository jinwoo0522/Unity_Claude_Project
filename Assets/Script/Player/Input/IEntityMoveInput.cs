using UnityEngine;

public interface IEntityMoveInput
{
    public Vector2 MoveInput{get;}
    public Vector3 AimDir {get;}
    public bool isSprint{get;}
}
