using TMPro;
using UnityEngine;

// 항목 프리팹용 — Scoreboard.OnListChanged에서 SetData 호출로 값 갱신
public class ScoreboardEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _killsText;
    [SerializeField] private TextMeshProUGUI _deathsText;
    [SerializeField] private TextMeshProUGUI _damageText;

    // 정렬 기준값 — SortAndRearrange에서 비교
    public int Kills { get; private set; }

    public void SetData(string playerName, int kills, int deaths, float damageDealt)
    {
        Kills            = kills;
        _nameText.text   = playerName;
        _killsText.text  = kills.ToString();
        _deathsText.text = deaths.ToString();
        _damageText.text = ((int)damageDealt).ToString();
    }
}
