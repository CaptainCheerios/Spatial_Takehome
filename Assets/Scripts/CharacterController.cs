using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CharacterController : MonoBehaviour
{

    public float PitchMax = 45;
    public float PitchMin = -45;

    public float PitchSensistivity = 0.1f;
    public float YawSensitivity = 0.1f;

    private float _currentPitch = 0;
    private float _currentYaw = 0;
    
    public Camera camera;

    public void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnQuit(InputAction.CallbackContext context)
    {
#if UNITY_EDITOR
        // Stop playing in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Close the application in a standalone build
        Application.Quit();
#endif
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
  
    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 mouseDelta = context.ReadValue<Vector2>();

        _currentPitch -= mouseDelta.y * PitchSensistivity;
        //Clamp the pitch so we don't flip it 
        _currentPitch = Mathf.Clamp(_currentPitch, PitchMin, PitchMax);
        
        _currentYaw += mouseDelta.x * YawSensitivity;
        //Keep the yaw within 0 and 360.  It is possible to go over.
        _currentYaw = Mathf.Repeat(_currentYaw, 360f);
        
        transform.localRotation = Quaternion.Euler(0, _currentYaw, 0);
        camera.transform.localRotation = Quaternion.Euler(_currentPitch, 0, 0);
    }
}
