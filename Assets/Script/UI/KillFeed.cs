using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// 서버에서 킬 이벤트 수신 → 전 클라 ClientRpc 브로드캐스트 → 풀/큐로 킬로그 렌더링
// NetworkBehaviour이므로 씬 내 NetworkObject에 부착 필요 (Scoreboard와 동일 방식)
public class KillFeed : NetworkBehaviour
{
    [SerializeField] private RectTransform _container;
    [SerializeField] private KillLogEntry  _entryPrefab;
    [SerializeField] private float         _duration = 5f;
    [SerializeField] private int           _poolSize  = 10;

    private readonly Queue<KillLogEntry>                       _pool   = new();
    private readonly Queue<(KillLogEntry entry, float expire)> _active = new();

    private void Awake()
    {
        // 런타임 중 Instantiate 추가 없음 — 시작 시에만 poolSize개 생성
        for (int i = 0; i < _poolSize; i++)
        {
            KillLogEntry entry = Instantiate(_entryPrefab, _container);
            entry.gameObject.SetActive(false);
            _pool.Enqueue(entry);
        }
    }

    public override void OnNetworkSpawn()
    {
        // 이름 해석·이벤트 구독은 서버에서만 — 클라이언트 입력 불신 규칙
        if (IsServer)
            GameManager.Instance.scoreManager.Killed += OnKilled;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
            GameManager.Instance.scoreManager.Killed -= OnKilled;
    }

    // 서버 전용 — 이름 조회 후 전 클라로 브로드캐스트
    private void OnKilled(ulong killerId, ulong victimId)
    {
        string killerName = GameManager.Instance.scoreManager.GetScore(killerId).name;
        string victimName  = GameManager.Instance.scoreManager.GetScore(victimId).name;
        AddKillLogClientRpc(killerName, victimName);
    }

    [ClientRpc]
    private void AddKillLogClientRpc(FixedString64Bytes killer, FixedString64Bytes victim)
    {
        // 활성 10개 모두 소진 시 가장 오래된 항목 강제 만료
        if (_pool.Count == 0)
            ExpireOldest();

        KillLogEntry entry = _pool.Dequeue();
        entry.SetData(killer.ToString(), victim.ToString());
        entry.gameObject.SetActive(true);
        _active.Enqueue((entry, Time.time + _duration));

        // 삽입 시점에만 위치 보정 — Update에서 매 프레임 보정 금지
        Reposition();
    }

    private void Update()
    {
        // 큐 앞쪽의 만료된 항목만 제거 — 위치 보정은 ExpireOldest 내부에서 수행
        while (_active.Count > 0 && Time.time >= _active.Peek().expire)
            ExpireOldest();
    }

    // 가장 오래된 활성 항목을 비활성화하고 풀로 반환 → 이후 위치 보정
    private void ExpireOldest()
    {
        var (entry, _) = _active.Dequeue();
        entry.gameObject.SetActive(false);
        _pool.Enqueue(entry);

        // 삭제 시점에만 위치 보정
        Reposition();
    }

    // 활성 Queue 순서(FIFO)대로 sibling index 재배치 — 최신이 위(index 0)
    // VerticalLayoutGroup이 비활성 항목을 건너뛰므로 sibling 0 = 화면 최상단
    private void Reposition()
    {
        var arr = _active.ToArray(); // arr[0] = oldest, arr[last] = newest
        for (int i = 0; i < arr.Length; i++)
            arr[i].entry.transform.SetSiblingIndex(arr.Length - 1 - i);
    }
}
