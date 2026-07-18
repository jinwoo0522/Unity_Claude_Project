using UnityEngine;

public interface IMovementCollsionModule
{
    void Bind(SkillMovement skillMove);
    void Collsion(IHitter.HitInfo hitInfo);
}
