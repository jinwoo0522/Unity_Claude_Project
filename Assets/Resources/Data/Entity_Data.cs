using UnityEngine;
public class Entity_Data : ScriptableObject
{
    [Header("이름")]
    public string strName;
    [TextArea]
    public string strDesc;

    [Header("공격")]
    public float fAttackDamage = 3f;
    
    [Header("방어")]
    public float fMaxHp = 100f;    
    [Header("이동")]
    public float fWalkSpeed = 3f;
    public float fRunSpeed = 6f;

    [Header("마나")]
    public float fMaxMana   = 100f;
    public float fManaRegen = 2f;
}
