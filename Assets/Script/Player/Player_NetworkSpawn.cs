using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
public class Player_NetworkSpawn : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public override void OnNetworkSpawn()
    {
        if(IsOwner == false)
            return;

        CinemachineCamera PlayerCamera = FindObjectOfType<CinemachineCamera>();

        if(PlayerCamera == null)
        {
            Debug.Log($"{NetworkManager.Singleton.LocalClientId} : 카메라 객체 연결 실패!");
            return;
        }

        PlayerCamera.Target.TrackingTarget = GetComponent<Transform>();

        Debug.Log($"{NetworkManager.Singleton.LocalClientId} : 카메라 객체 연결 성공!");
    }
    // Update is called once per frame
}
