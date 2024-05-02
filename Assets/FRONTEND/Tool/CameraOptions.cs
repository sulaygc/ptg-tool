using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOptions : MonoBehaviour
{
    public CameraControls CameraControls_instance;

    public void SetCameraMoveSpeed(float value)
    {
        CameraControls_instance.move_speed = value;
    }

    public void SetCameraRotateSpeed(float value)
    {
        CameraControls_instance.rotate_speed = value;
    }

    public void SetCameraZoomSpeed(float value)
    {
        CameraControls_instance.zoom_speed = value;
    }
}
