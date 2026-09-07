using UnityEngine;

public class WorldHPPresenter : MonoBehaviour
{
    [SerializeField] private WorldHPView _view;
    [SerializeField] private Stat _stat;
    [SerializeField] private Entity_Data _data;

    void Bind()
    {
        _stat.StatChanged += HPStatEvent;
    }

    void Start()
    {
        Bind();
        _view.SetName(_data.strName);
        _view.SetHp(_stat.Get_Stat(Stat.STAT_TAG.HP), _stat.Get_Stat(Stat.STAT_TAG.MAX_HP));
    }

    void HPStatEvent(Stat.STAT_TAG tag)
    {
        if (tag == Stat.STAT_TAG.HP)
        {
            _view.SetHp(_stat.Get_Stat(Stat.STAT_TAG.HP), _stat.Get_Stat(Stat.STAT_TAG.MAX_HP));
        }
    }

    void OnDestroy()
    {
        _stat.StatChanged -= HPStatEvent;
    }

}
