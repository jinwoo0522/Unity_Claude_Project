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

    void Start()
    {
        Magician_Btn.onClick.AddListener(() =>
         RequestSpawnPlayerServerRpc(PLAYER_INDEX.MAGICIAN));

        Golem_Btn.onClick.AddListener(() =>
         RequestSpawnPlayerServerRpc(PLAYER_INDEX.GOLEM));
    }

    void ChoicePlayer(ulong clientID , PLAYER_INDEX index)
    {
        if(IsClient == false)
            return;
        
        GameObject Player = Instantiate(
        PlayerPrefebs[(int)index], 
        SpawnPoints[(int)index]);

        if(Player == null)
        {
            Debug.Log($"{clientID} : 플레이어 프리펩 생성 실패");
            return;
        }

        Debug.Log($"{clientID} : 플레이어 프리펩 생성 성공");

        PlayerCamera.Target.TrackingTarget = Player.GetComponent<Transform>();

        Player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);

        Golem_Btn.GetComponent<Transform>().parent.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnPlayerServerRpc(PLAYER_INDEX characterIndex, ServerRpcParams rpcParams = default)
    {
    // 이 RPC를 호출한 클라이언트의 ID
        ulong clientId = rpcParams.Receive.SenderClientId;
        ChoicePlayer(clientId, characterIndex);
    }

}
