using System;
using UnityEngine;

public class RegistCamera : MonoBehaviour
{
    [SerializeField]
    private CameraManager.CameraTag cameraTag;
    private Camera camera;
    private void Start()
    {
        
        try
        {
            camera = gameObject.GetComponent<Camera>();
        }
        catch(NullReferenceException e)
        {
            Debug.Log("Camera 없음 !" + e.Message);
        }
        finally
        {
            GameManager.Instance.cameraManager.Push_Camera(cameraTag , camera);
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.cameraManager.Remove_Camera(cameraTag);
    }
}
