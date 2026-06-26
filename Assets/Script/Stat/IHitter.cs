using UnityEngine;

public interface IHitter 
{
    public void DoHitCheck(Vector3 center, Vector3 halfExtents, float duration);
}
