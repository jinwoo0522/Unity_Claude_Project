using System;
using UnityEngine;

public class RegistCamera : MonoBehaviour
{
    [SerializeField]
    private CameraManager.CameraTag cameraTag;
    private Camera camera;
    private void Start()
    {
        
        camera = gameObject.GetComponent<Camera>();
        
        if(camera == null)
        {
            Debug.Log("Camera 없음 !");
            return;
        }
           
        GameManager.Instance.cameraManager.Push_Camera(cameraTag , camera);
    }

    private void OnDestroy()
    {
        if(GameManager.Instance.cameraManager == null)
            GameManager.Instance.DebugMessage<RegistCamera>("카메라 매니저 NULL");
            
        GameManager.Instance.cameraManager.Remove_Camera(cameraTag);
    }
}
