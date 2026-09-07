using System;
using UnityEngine;

public interface IHitter 
{
    public struct HitInfo
    {
        public IDamagable Target;
        public Vector3    Point;    // 피격 VFX 띄울 위치
        public Collider   Collider;
    }
    public void DoHitCheck(Vector3 center, Vector3 halfExtents, float duration, Action<HitInfo> HitInfo);
}
