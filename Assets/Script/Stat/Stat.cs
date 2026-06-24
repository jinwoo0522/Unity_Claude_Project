using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Components;
using System.Collections.Generic;
using System;

public class Stat : NetworkBehaviour , IDamagable
{
    public enum STAT_TAG
    {
        HP,
        MAX_HP,
        MP,
        MAX_MP,
        DAMAGE,
        WALK_SPEED,
        RUN_SPEED,
        END,
    }
    [SerializeField] private Entity_Data Stat_Data;
    NetworkList<float> StatList = new();
    public bool _isHit {get; set;}
    public bool _isDead {get; set;}

    public override void OnNetworkSpawn()
    {
        for(int i = 0 ; i < (int)STAT_TAG.END ; i++)
            StatList.Add(0f);

        StatList[(int)STAT_TAG.HP] = Stat_Data.fMaxHp;
        StatList[(int)STAT_TAG.MAX_HP] = Stat_Data.fMaxHp;
        StatList[(int)STAT_TAG.MP] = Stat_Data.fMaxMana;
        StatList[(int)STAT_TAG.MAX_MP] = Stat_Data.fMaxMana;
        StatList[(int)STAT_TAG.DAMAGE] = Stat_Data.fAttackDamage;
        StatList[(int)STAT_TAG.WALK_SPEED] = Stat_Data.fWalkSpeed;
        StatList[(int)STAT_TAG.RUN_SPEED] = Stat_Data.fRunSpeed;
    }

    public float Get_Stat(STAT_TAG tag)
    {
        return StatList[(int)tag];
    }
    public void Set_Stat(STAT_TAG tag , float fValue)
    {
        StatList[(int)tag] = fValue;
    }
    public void Add_Stat(STAT_TAG tag , float fValue)
    {
        StatList[(int)tag] += fValue;
    }


    public void Hit(float fDamage)
    {
        if(_isHit == true) return;

        _isHit = true;
        Add_Stat(STAT_TAG.HP, fDamage);
        
        if(StatList[(int)STAT_TAG.HP] < 0)
            _isDead = true;
    }
}
