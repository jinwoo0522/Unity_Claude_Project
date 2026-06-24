using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Pool;
using UnityEngine;
using UnityEngine.SceneManagement;

// SkillType을 키로 Skill을 풀링 관리하는 싱글턴
public class SkillPool
{
    private List<SkillData> SkillDatas = new List<SkillData>();
    private Dictionary<SkillType , PooledHandler> SkillPools
     = new Dictionary<SkillType , PooledHandler>(); 
    private int max_Size;
    private int min_Size;

    public SkillPool(int min , int max)
    {
        min_Size = min;
        max_Size = max;

        Init();
    }

    private void ResourceLoad()
    {

        //0
        SkillData skill = Resources.Load<SkillData>("Data/SkillData/ElectricSkillData");
        if(skill == null)
        {
            GameManager.Instance.DebugMessage<SkillPool>("ElectricSkill 스킬 NULL");
            return;
        }
        SkillDatas.Add(skill); 

        //1
        SkillData Magician_Q_Skill = Resources.Load<SkillData>("Data/SkillData/Magician_Q_Skill");
        if(Magician_Q_Skill == null)
        {
            GameManager.Instance.DebugMessage<SkillPool>("Magician_Q_Skill 스킬 NULL");
            return;
        }
        SkillDatas.Add(Magician_Q_Skill); 

        //2
        SkillData Golem_Mouse_Skill = Resources.Load<SkillData>("Data/SkillData/Golem_Mouse_Skill");
        if(Golem_Mouse_Skill == null)
        {
            GameManager.Instance.DebugMessage<SkillPool>("Golem_Mouse_Skill 스킬 NULL");
            return;
        }
        SkillDatas.Add(Golem_Mouse_Skill); 

        //3
        SkillData Magician_Mouse_Skill = Resources.Load<SkillData>("Data/SkillData/Magician_Mouse_Skill");
        if(Magician_Mouse_Skill == null)
        {
            GameManager.Instance.DebugMessage<SkillPool>("Magician_Mouse_Skill 스킬 NULL");
            return;
        }
        SkillDatas.Add(Magician_Mouse_Skill); 

    }

    private void Init()
    {
        ResourceLoad();

        int enumLength = System.Enum.GetValues(typeof(SkillType)).Length;

        for(int i = 0 ; i < enumLength ; i++)
        {
            PooledHandler handle = new PooledHandler(SkillDatas[i].prefab ,min_Size , max_Size);
            SkillPools.Add((SkillType)i , handle);

            NetworkManager.Singleton.PrefabHandler.
            AddHandler(SkillDatas[i].prefab, handle);
        }

        NetworkManager.Singleton.OnClientStarted += ClientStart;
         
    }
    
    public void UseSkill(SkillType type, Vector3 pos, Vector3 dir, ulong clinetID , GameObject owner)
    {
        Debug.Log("스킬 사용!");
        SkillData skilldata = SkillDatas[(int)type];

        if(skilldata == null)
        {
            GameManager.Instance.DebugMessage<SkillPool>("스킬 데이터 NULL");
            return;
        }

        // NGO가 스폰 → 핸들러 Instantiate(풀에서 Get)를 가로채서 호출함
        var netObj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
            SkillDatas[(int)type].prefab.GetComponent<NetworkObject>(),
            ownerClientId: clinetID,
            position: pos,
            rotation: Quaternion.LookRotation(dir)
        );

        netObj.GetComponent<Skill>().Init(type, skilldata, pos, dir, clinetID);
    }

    public void PreCreate(int count)
    {
        foreach(var skillpool in SkillPools)
        {
            skillpool.Value.Prewarm(count);
        }
    }

    public SkillData GetSkillData(SkillType type)
    {
        return SkillDatas[(int)type];
    }

    void OnSceneLoaded(string sceneName, LoadSceneMode mode,
                   List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        PreCreate(30);
    }

    void ClientStart()
    {
       NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded; 
    }


}
