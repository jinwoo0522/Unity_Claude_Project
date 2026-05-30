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
        
    }

}