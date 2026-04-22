using System;
using System.Collections.Generic;
using UnityEngine;

// SkillType을 키로 Skill을 풀링 관리하는 싱글턴
public class SkillPool : MonoBehaviour
{
    public static SkillPool Instance { get; private set; }

    // Inspector에서 SkillType ↔ SkillData 매핑 설정
    [Serializable]
    public class SkillEntry
    {
        public SkillType type;
        public SkillData data;
    }

    [SerializeField] private List<SkillEntry> skillEntries = new();

    // type → 데이터 조회용
    private readonly Dictionary<SkillType, SkillData>        dataMap = new();
    // type → 풀 큐
    private readonly Dictionary<SkillType, Queue<Skill>>     pools   = new();

    void Awake()
    {
        Instance = this;
        foreach (var entry in skillEntries)
            if (entry.data != null) dataMap[entry.type] = entry.data;
    }

    // 스킬 발사 — 풀에 여유가 없으면 새 인스턴스 생성
    public void Get(SkillType type, Vector3 position, Vector3 direction, GameObject owner)
    {
        if (!dataMap.TryGetValue(type, out var data) || data.prefab == null) return;

        // 해당 타입 풀이 없으면 초기 수량만큼 미리 생성
        if (!pools.TryGetValue(type, out var queue))
        {
            queue = new Queue<Skill>();
            pools[type] = queue;
            for (int i = 0; i < data.nPoolSize; i++)
                queue.Enqueue(CreateNew(data));
        }

        Skill skill = queue.Count > 0 ? queue.Dequeue() : CreateNew(data);
        skill.Init(type, data, position, direction, owner);
        // Init 완료 후 활성화 — 자식 Init 전부 끝난 뒤 OnEnable 발동 보장
        skill.gameObject.SetActive(true);
    }

    // 스킬이 수명을 다하면 비활성화 후 큐에 반환
    public void Return(Skill skill)
    {
        skill.gameObject.SetActive(false);
        if (pools.TryGetValue(skill.Type, out var queue))
            queue.Enqueue(skill);
    }

    // 프리팹을 SkillPool 하위에 비활성 상태로 생성
    Skill CreateNew(SkillData data)
    {
        var obj = Instantiate(data.prefab, transform);
        obj.SetActive(false);
        return obj.GetComponent<Skill>();
    }
}
