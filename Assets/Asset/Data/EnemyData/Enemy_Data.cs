using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : Entity_Data
{
    [Header("탐지")]
    public float fDetectRange = 10f;      // 타겟을 잡는 사거리
    public float fReturnDistance = 15f;   // 추격을 포기하는 거리 — 탐지 사거리보다 넓어야 경계에서 상태가 떨리지 않는다
}
