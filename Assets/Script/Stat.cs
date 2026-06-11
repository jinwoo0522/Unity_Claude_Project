using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Stat : NetworkBehaviour
{
    [SerializeField]
    private Entity_Data Stat_Data;
    [SerializeField]
    private Slider hpSlider;
   
    public float pHp {get => fHp.Value; set
        {
            float fCurHp = fHp.Value + value;
            if(fCurHp <= 0)   
                Die();

            fHp.Value = Mathf.Clamp(fCurHp , 0 , pMaxHp);
        }}
    public float pDamage {get; private set;}
    public float pResistance {get; private set;}
    public float pMaxHp{get => fMaxHp.Value; set => fMaxHp.Value = value;}

    NetworkVariable<float> fHp = new NetworkVariable<float>(0f 
    , NetworkVariableReadPermission.Everyone, 
    NetworkVariableWritePermission.Server);

    NetworkVariable<float> fMaxHp = new NetworkVariable<float>(0f 
    , NetworkVariableReadPermission.Everyone, 
    NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (hpSlider == null)
        {
            Debug.Log("Slider Null 발생! , Stat.cs");
            return;
        }

        if(IsServer == true)
        {
            pDamage = Stat_Data.fAttackDamage;
            pResistance = Stat_Data.fResistance;
            pMaxHp = Stat_Data.fMaxHp;
            pHp = Stat_Data.fHp;
        }

        hpSlider.maxValue = fMaxHp.Value;
        hpSlider.value = fHp.Value;

        fHp.OnValueChanged += (float pre , float next) =>
        {
            hpSlider.value = next;
        };

        fMaxHp.OnValueChanged += (float pre , float next) =>
        {
            hpSlider.maxValue = next;
        };
    }

    void Die()
    {
        
    }
}
