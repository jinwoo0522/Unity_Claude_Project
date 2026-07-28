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
    // 네트워크 동기화가 필요 없는 원본 설정값을 외부가 읽도록 노출 (쓰기는 막는다)
    public Entity_Data _data => Stat_Data;

    [SerializeField] private Entity_Data Stat_Data;
    NetworkList<float> StatList = new();
    public bool _isHit {get; set;}
    public bool _isDead {get; set;}

    // 스탯 변경 통지 — UI 등 외부가 구독 (Stat은 구독자를 모름, 단방향)
    public event Action<STAT_TAG> StatChanged;

    public override void OnNetworkSpawn()
    {
        // 모든 클라가 변경을 받도록 구독 (NetworkList는 서버 쓰기 → 전 클라 통지)
        StatList.OnListChanged += HandleListChanged;

        if(IsServer == false) return;

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

    public override void OnNetworkDespawn()
    {
        StatList.OnListChanged -= HandleListChanged;
    }

    // NetworkList 변경을 STAT_TAG 단위 이벤트로 변환해 외부에 전달
    private void HandleListChanged(NetworkListEvent<float> e)
    {
        StatChanged?.Invoke((STAT_TAG)e.Index);
    }

    public float Get_Stat(STAT_TAG tag)
    {
        return StatList[(int)tag];
    }
    public void Set_Stat(STAT_TAG tag , float fValue)
    {
        if(IsServer == false) return;
        
        StatList[(int)tag] = fValue;
    }
    public void Add_Stat(STAT_TAG tag , float fValue)
    {
        if(IsServer == false) return;

        StatList[(int)tag] += fValue;
    }


    public void Hit(float fDamage)
    {
        // [디버그] _isHit이 실제로 켜지는 유일한 지점 — 언제/누구에게 데미지가 들어오는지 기록
        Debug.Log($"[Stat:{name}] Hit 호출 dmg={fDamage} 이전_isHit={_isHit} frame={Time.frameCount}");

        if(_isHit == true) return;

        _isHit = true;
        Add_Stat(STAT_TAG.HP, -fDamage);

        if(StatList[(int)STAT_TAG.HP] < 0)
            _isDead = true;
    }
}
