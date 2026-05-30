using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UserScrollbar : NetworkBehaviour
{
    [SerializeField]
    private Button StartBtn;
    [SerializeField]
    private GameObject UserCountText;
    [SerializeField]
    string SceneName;

    public GameObject TextPrefeb;
    private TextMeshProUGUI UserCountUI;
    private ScrollRect scrollRect;

    private NetworkVariable<int> net_uiCount = new NetworkVariable<int>(
        0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    private void Awake()
    {
       scrollRect = gameObject.GetComponent<ScrollRect>();

       if(scrollRect == null)
            Debug.Log("scrollRect Null 발생!");

       UserCountUI = UserCountText. GetComponent<TextMeshProUGUI>();

       if(UserCountUI == null)
            Debug.Log("UserCountText TextMeshProUGUI Null 발생!");

    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton == null)
            Debug.Log("네트워크 매니저 NULL , UserScrollbar.cs");

        NetworkManager.Singleton.OnConnectionEvent += AddUser;

        StartBtn.onClick.AddListener(()
            =>
        {
            NetworkManager.Singleton.SceneManager.LoadScene(SceneName , 
            UnityEngine.SceneManagement.LoadSceneMode.Single);
            
            NetworkManager.Singleton.OnConnectionEvent -= AddUser;
        });

        net_uiCount.OnValueChanged += (int pre , int next) =>
        {
             UserCountUI.text = $"{net_uiCount.Value} / 10";
        };

        if(IsServer)
            net_uiCount.Value = NetworkManager.Singleton.ConnectedClientsList.Count;

    }
    public void AddUser(NetworkManager nm, ConnectionEventData data)
    {
        if(IsHost == true && net_uiCount.Value >= 2)
        {
            StartBtn.gameObject.SetActive(true);
        }

        if(IsServer == true)
        {
            net_uiCount.Value = nm.ConnectedClientsList.Count;
            CreateTexts_ClientRpc();
        }
    }

    [ClientRpc]
    void CreateTexts_ClientRpc()
    {
        foreach (Transform child in scrollRect.content)
        {
            Destroy(child.gameObject);
        }

        int index = 0;
        
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject userText = Instantiate(TextPrefeb, scrollRect.content, false);
    
            RectTransform rt = userText.GetComponent<RectTransform>();
            TextMeshProUGUI text = userText.GetComponent<TextMeshProUGUI>();
    
            text.text = $"client : {clientId}";
    
            rt.anchoredPosition = new Vector2(0, index++ * -110);
    
            Debug.Log("텍스트 생성");
        }
        
        
    }

}
