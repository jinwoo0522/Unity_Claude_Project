using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawner : NetworkBehaviour
{
    enum PLAYER_INDEX
    {
        GOLEM,
        MAGICIAN
        
    }
    [SerializeField]
    GameObject[] PlayerPrefebs;
    
    [SerializeField]
    Transform[] SpawnPoints;
    [SerializeField]
    Button Magician_Btn;
    [SerializeField]
    Button Golem_Btn;
    [SerializeField]
    CinemachineCamera PlayerCamera;


    public override void OnNetworkSpawn()
    {
        Debug.Log($" : 버튼 연결 전");

        if (!IsClient) return;

        GameObject parent =Golem_Btn.GetComponent<Transform>().parent.gameObject;

        Magician_Btn.onClick.AddListener(() =>
        {
             RequestSpawnPlayerServerRpc(PLAYER_INDEX.MAGICIAN);
             parent.SetActive(false);
        });

        Golem_Btn.onClick.AddListener(() =>
        {
             RequestSpawnPlayerServerRpc(PLAYER_INDEX.GOLEM);
             parent.SetActive(false);
        });

        Debug.Log($" : 버튼 연결 후"); 
    }

    void ChoicePlayer(ulong clientID , PLAYER_INDEX index)
    {
        if(IsServer == false)
            return;

        if (NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject != null)
        {
            Debug.Log($"{clientID} : 이미 플레이어가 있음");
            return;
        }
        
        GameObject Player = Instantiate(
        PlayerPrefebs[(int)index], 
        SpawnPoints[(int)index].position,
        SpawnPoints[(int)index].rotation);

        if(Player == null)
        {
            Debug.Log($"{clientID} : 플레이어 프리펩 생성 실패");
            return;
        }

        Debug.Log($"{clientID} : 플레이어 프리펩 생성 성공");

        //해당 객체의 주인을 받아온 클라 id로 바꾸는 것
        Player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnPlayerServerRpc(PLAYER_INDEX characterIndex, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"{NetworkManager.Singleton.LocalClientId} : 플레이어 생성 버튼 호출!");
    // 이 RPC를 호출한 클라이언트의 ID
        ulong clientId = rpcParams.Receive.SenderClientId;
        ChoicePlayer(clientId, characterIndex);
    }

}
