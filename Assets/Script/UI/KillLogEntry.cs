using TMPro;
using UnityEngine;

// 킬로그 한 줄을 담당하는 뷰 컴포넌트 — KillFeed 풀에서 재사용됨
public class KillLogEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    public void SetData(string killer, string victim)
    {
        _text.text = $"{killer} Kill {victim}";
    }
}
