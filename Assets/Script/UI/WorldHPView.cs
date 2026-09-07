using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldHPView : MonoBehaviour
{
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private TextMeshProUGUI _nameText;
    
    public void SetHp(float fHp, float fMaxHp)
    {
        _hpSlider.value = fHp / fMaxHp;
    }

    public void SetName(string strName)
    {
        _nameText.text = strName;
    }
}
