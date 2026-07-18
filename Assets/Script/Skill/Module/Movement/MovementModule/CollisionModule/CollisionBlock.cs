using UnityEngine;

[System.Serializable]
public class CollisionBlock : IMovementCollsionModule
{
    SkillMovement _skillMove;
    public void Bind(SkillMovement skillMove)
    {
        _skillMove = skillMove;
    }
    public void Collsion(IHitter.HitInfo hitInfo)
    {
        _skillMove.Move = false;
    }
}
