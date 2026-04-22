using UnityEngine;

public class Stat : MonoBehaviour
{
   [SerializeField]
    private Entity_Data Stat_Data;

    private float fHp;
    public float pHp {get => fHp; set
        {
            float fCurHp = fHp + value;
            if(fCurHp <= 0)   
                Die();

            fHp = Mathf.Clamp(fCurHp , 0 , pMaxHp);
        }}
    public float pDamage {get; private set;}
    public float pResistance {get; private set;}
    public float pMaxHp{get; private set;}

    void Start()
    {
        pDamage = Stat_Data.fAttackDamage;
        pResistance = Stat_Data.fResistance;
        pMaxHp = Stat_Data.fMaxHp;
        pHp = Stat_Data.fHp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Die()
    {
        
    }
}
