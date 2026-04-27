using UnityEngine;

[CreateAssetMenu(fileName = "Player_Data", menuName = "Scriptable Objects/Player_Data")]
public class Player_Data : Entity_Data
{
    [Header("점프")]
    public float fJumpAmount = 10f;
    public float fGravity = -9.8f;
    public float fJumpDelay = 0.1f;
}
