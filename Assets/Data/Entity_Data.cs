using UnityEngine;
public class Entity_Data : ScriptableObject
{
    public string strName;
    [TextArea]
    public string strDesc;

    [Header("공격")]
    public float fAttackDamage = 3f;
    
    [Header("방어")]
    
    public float fHp = 100f;
    public float fMaxHp = 100f;
    public float fArmor = 15f;
    public float fResistance = 15f;
    public float fMaxResistance = 35f;
    
    [Header("이동")]
    public float fWalkSpeed = 3f;
    public float fRunSpeed = 6f;

    [Header("애니메이션")]
    public float fAnimSpeed = 3f;
}
