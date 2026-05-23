using System;
using Unity.Cinemachine;
using UnityEngine;

public class RegistChinemachine : MonoBehaviour
{
        [SerializeField]
    private CameraManager.CinemachineTag cinemachineTag;
    private CinemachineCamera cinemachineCamera;
    private void Start()
    {
        
       cinemachineCamera = gameObject.GetComponent<CinemachineCamera>();
        
        if(cinemachineCamera == null)
        {
            Debug.Log("Camera 없음 !");
            return;
        }
           
        GameManager.Instance.cameraManager.Push_Cinemachine(cinemachineTag , cinemachineCamera);
    }

    private void OnDestroy()
    {
        GameManager.Instance.cameraManager.Remove_Cinemachine(cinemachineTag);
    }
}
