using UnityEngine;

// 충돌 모듈이 반응할 판정 시점 — 진입 1회(ENTER) / 겹친 동안 매 프레임(STAY)
public enum CollisionTiming { ENTER, STAY }

public interface ICollsionEventModule
{
    CollisionTiming Timing { get; }   // 이 모듈을 Enter/Stay 중 어느 이벤트에 구독할지 선택
    void Bind(Skill skill);
    void Collsion(Collider col);
}
