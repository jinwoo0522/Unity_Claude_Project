using UnityEngine;
using UnityEngine.UI;

// 스킬 쿨타임 HUD 컨트롤러 (owner 로컬 전용, NetworkBehaviour 아님).
// 오버레이 Image의 fillAmount로 쿨타임 진행을 표시한다.
// 슬롯 0=Q(Buff), 슬롯 1=우클릭(Attack_Skill).
public class SkillCooldownUI : MonoBehaviour
{
    // Inspector에서 QSlot/Overlay, MouseSlot/Overlay를 각각 연결
    [SerializeField] private Image[] _overlays;

    // 슬롯별 쿨타임 총 길이(초)
    private readonly float[] _lengths  = new float[2];
    // 슬롯별 경과 시간 (-1 = 비활성)
    private readonly float[] _elapsed  = { -1f, -1f };

    // Player_Skill.OnNetworkSpawn에서 캐릭터별 fCooldown 값으로 등록
    public void SetCooldownLength(int slot, float seconds)
    {
        if (!IsValidSlot(slot)) return;
        _lengths[slot] = seconds;
    }

    // 서버가 스킬 발동을 확정한 순간 owner에게 호출됨
    public void StartCooldown(int slot)
    {
        if (!IsValidSlot(slot)) return;
        _elapsed[slot] = 0f;

        // 오버레이 활성화 및 전체 덮음
        _overlays[slot].enabled    = true;
        _overlays[slot].fillAmount = 1f;
    }

    private void Update()
    {
        CheckCooltime();
    }

    private bool IsValidSlot(int slot) => slot >= 0 && slot < 2;
    private void CheckCooltime()
    {
        for (int i = 0; i < 2; i++)
        {
            if (_elapsed[i] < 0f) continue;

            _elapsed[i] += Time.deltaTime;

            float ratio = _elapsed[i] / _lengths[i];
            if (ratio >= 1f)
            {
                // 쿨타임 완료 — 오버레이 비활성
                _elapsed[i]            = -1f;
                _overlays[i].enabled   = false;
                _overlays[i].fillAmount = 0f;
            }
            else
            {
                // fillOrigin=Bottom이므로 fillAmount 감소 시 위에서부터 걷힘
                _overlays[i].fillAmount = 1f - ratio;
            }
        }
    }
}
