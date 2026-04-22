using UnityEngine;
public class Entity_Data : ScriptableObject
{
    public string strName;
    [TextArea]
    public string strDesc;

    [Header("공격")]
    public float fAttackDamage;
    
    [Header("방어")]
    
    public float fHp;
    public float fMaxHp;
    public float fArmor;
    public float fResistance;
    public float fMaxResistance;
    
    [Header("이동")]
    public float fWalkSpeed;
    public float fRunSpeed;

    [Header("애니메이션")]
    public float fAnimSpeed;
}
