using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkBtn : MonoBehaviour
{
    [SerializeField]
    Button _HostBtn;
    [SerializeField]
    Button _ServerBtn;
    [SerializeField]
    Button _ClientBtn;
    [SerializeField]
    string SceneName;

    void Start()
    {
        _HostBtn?.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
        _ServerBtn?.onClick.AddListener(() => NetworkManager.Singleton.StartServer());
        _ClientBtn?.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
        
        Button[] btns = {_HostBtn ,_ServerBtn, _ClientBtn };
        AddEvent_SceneChange(btns);
    }

    void AddEvent_SceneChange(Button[] btns)
    {
        foreach(var btn in btns)
        {
            btn.onClick.AddListener(()
            => NetworkManager.Singleton.SceneManager.LoadScene(SceneName , 
            UnityEngine.SceneManagement.LoadSceneMode.Single));
            // single -> 전 씬 삭제 , additvie -> 전 씬 유지
        }

    }

}