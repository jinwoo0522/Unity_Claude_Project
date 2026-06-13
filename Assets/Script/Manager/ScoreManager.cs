using System;
using System.Collections.Generic;

// 서버 권위 점수 집계 — 모든 호출부에서 IsServer 가드 필요
// GameManager.scoreManager를 통해 접근
public class ScoreManager
{
    // 항목 변경 시 해당 clientId 발행 — Scoreboard가 구독해 NetworkList upsert
    public event Action<ulong> Changed;

    private readonly Dictionary<ulong, ScoreData> _scores = new();

    // 스폰 시 1회 호출 — 이름은 내부에서 생성하므로 호출부는 이름 불필요
    public void AddPlayer(ulong clientId)
    {
        if (_scores.ContainsKey(clientId)) return;
        _scores[clientId] = new ScoreData
        {
            clientId = clientId,
            name     = $"client : {clientId}"
        };
        Changed?.Invoke(clientId);
    }

    public void AddDamage(ulong attackerId, float amount)
    {
        if (!_scores.TryGetValue(attackerId, out ScoreData data)) return;
        data.damageDealt += amount;
        Changed?.Invoke(attackerId);
    }

    // killerId == victimId(자해) 시 킬 미집계, 데스는 정상 집계
    public void RegisterKill(ulong killerId, ulong victimId)
    {
        if (_scores.TryGetValue(killerId, out ScoreData killer) && killerId != victimId)
        {
            killer.kills++;
            Changed?.Invoke(killerId);
        }
        if (_scores.TryGetValue(victimId, out ScoreData victim))
        {
            victim.deaths++;
            Changed?.Invoke(victimId);
        }
    }

    public ScoreData GetScore(ulong clientId)
    {
        _scores.TryGetValue(clientId, out ScoreData data);
        return data;
    }

    public IReadOnlyDictionary<ulong, ScoreData> GetAllScores() => _scores;
}
