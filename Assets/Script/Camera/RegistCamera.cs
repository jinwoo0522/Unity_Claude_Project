using System;
using UnityEngine;

public class RegistCamera : MonoBehaviour
{
    [SerializeField]
    private CameraManager.CameraTag cameraTag;
    private Camera _camera;
    private void Awake()
    {
        
        _camera = gameObject.GetComponent<Camera>();
        
        if(_camera == null)
        {
            Debug.Log("Camera 없음 !");
            return;
        }
           
        GameManager.Instance.cameraManager.Push_Camera(cameraTag , _camera);
    }

    private void OnDestroy()
    {
        if(GameManager.Instance.cameraManager == null)
        { 
            GameManager.Instance.DebugMessage<RegistCamera>("카메라 매니저 NULL");
            return;
        }
            
        GameManager.Instance.cameraManager.Remove_Camera(cameraTag);
    }
}
