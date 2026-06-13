using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

// 씬 상주 NetworkBehaviour — NetworkList<ScoreEntry>로 전 클라 점수 동기화, Tab hold로 Panel 토글
public class Scoreboard : NetworkBehaviour
{
    [SerializeField] private GameObject      _panel;
    [SerializeField] private Transform       _entryContainer;
    [SerializeField] private ScoreboardEntry _entryPrefab;

    private NetworkList<ScoreEntry>                  _scoreList;
    private readonly Dictionary<ulong, ScoreboardEntry> _entryInstances = new();

    // Tab hold로 Panel 토글하는 로컬 입력 액션 (액션 에셋 없이 코드 바인딩)
    private InputAction _scoreboardAction;

    private void Awake()
    {
        // NetworkList는 OnNetworkSpawn 이전에 초기화 필요
        _scoreList = new NetworkList<ScoreEntry>();

        _scoreboardAction = new InputAction("Scoreboard", InputActionType.Button, "<Keyboard>/tab");
        _scoreboardAction.performed += _ => _panel.SetActive(true);  // Tab 누름
        _scoreboardAction.canceled  += _ => _panel.SetActive(false); // Tab 뗌
    }

    private void OnEnable()  => _scoreboardAction.Enable();
    private void OnDisable() => _scoreboardAction.Disable();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            GameManager.Instance.scoreManager.Changed += OnScoreChanged;

        _scoreList.OnListChanged += OnListChanged;
        _panel.SetActive(false);

        // 늦게 참가한 클라이언트: 이미 복제된 목록 즉시 반영
        foreach (ScoreEntry entry in _scoreList)
            CreateOrUpdateEntry(entry);

        SortAndRearrange();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
            GameManager.Instance.scoreManager.Changed -= OnScoreChanged;

        _scoreList.OnListChanged -= OnListChanged;
    }

    // 서버 전용 — ScoreManager 변경 이벤트 수신 후 NetworkList upsert
    private void OnScoreChanged(ulong clientId)
    {
        ScoreData data = GameManager.Instance.scoreManager.GetScore(clientId);

        ScoreEntry entry = new ScoreEntry
        {
            clientId    = data.clientId,
            name        = data.name,
            kills       = data.kills,
            deaths      = data.deaths,
            damageDealt = data.damageDealt
        };

        for (int i = 0; i < _scoreList.Count; i++)
        {
            if (_scoreList[i].clientId == clientId)
            {
                _scoreList[i] = entry;
                return;
            }
        }
        _scoreList.Add(entry);
    }

    // 전 클라 — NetworkList 변경 시 항목 생성·갱신 후 킬 기준 정렬·재배치
    private void OnListChanged(NetworkListEvent<ScoreEntry> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<ScoreEntry>.EventType.Add:
            case NetworkListEvent<ScoreEntry>.EventType.Value:
                CreateOrUpdateEntry(changeEvent.Value);
                SortAndRearrange();
                break;
        }
    }

    private void CreateOrUpdateEntry(ScoreEntry entry)
    {
        if (!_entryInstances.TryGetValue(entry.clientId, out ScoreboardEntry ui))
        {
            ui = Instantiate(_entryPrefab, _entryContainer);
            _entryInstances[entry.clientId] = ui;
        }
        ui.SetData(entry.name.ToString(), entry.kills, entry.deaths, entry.damageDealt);
    }

    // 킬 내림차순으로 sibling index만 재배치 — Update 금지, 킬 변동 시에만 호출
    private void SortAndRearrange()
    {
        List<ScoreboardEntry> sorted = new(_entryInstances.Values);
        sorted.Sort((a, b) => b.Kills.CompareTo(a.Kills));
        for (int i = 0; i < sorted.Count; i++)
            sorted[i].transform.SetSiblingIndex(i);
    }

    public override void OnDestroy()
    {
        _scoreboardAction.Dispose();
        base.OnDestroy();
    }
}
