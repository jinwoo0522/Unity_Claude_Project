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

    private CrowdController _crowdController;

    // 스탯 변경 통지 — UI 등 외부가 구독 (Stat은 구독자를 모름, 단방향)
    public event Action<STAT_TAG> StatChanged;

    void Awake()
    {
        _crowdController = GetComponent<CrowdController>();
    }

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
        Add_Stat(STAT_TAG.HP, -fDamage);

        if(StatList[(int)STAT_TAG.HP] < 0)
            _isDead = true;

        // 에어본·빙결 중에는 데미지만 넣고 경직 반응은 만들지 않는다
        // 여기서 막지 않으면 CC가 풀린 뒤 밀린 플래그가 살아나 뒤늦게 HIT이 재생된다
        if(_crowdController.IsApply(CrowdController.CC_TAG.AIRBORNE) == true) return;
        if(_crowdController.IsApply(CrowdController.CC_TAG.FREEZE) == true) return;

        _isHit = true;
    }
}
