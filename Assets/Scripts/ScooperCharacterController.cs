using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ScooperCharacterController : MonoBehaviour
{

    public CharacterController characterController;
    public float PitchMax = 45;
    public float PitchMin = -45;

    public float PitchSensistivity = 0.1f;
    public float YawSensitivity = 0.1f;
    public float movementSpeed = 1f;
    public float sprintSpeed = 2f;

    private float _currentPitch = 0;
    private float _currentYaw = 0;
    
    // Store input states instead of accumulating a vector directly.
    private float _forwardInput;
    private float _leftInput;
    private bool _sprint;
    
    public Camera camera;

    public void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Initialize rotation based on the transform so the camera doesn't snap to 0,0,0
        _currentYaw = transform.localRotation.eulerAngles.y;
        if (camera != null)
        {
            _currentPitch = camera.transform.localRotation.eulerAngles.x;
            if (_currentPitch > 180f)
            {
                _currentPitch -= 360f;
            }
        }
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

    #region Movement Functionality
    public void OnMoveFoward(InputAction.CallbackContext context)
    {
        // Store the value so it persists across frames while the key is held.
        _forwardInput = context.ReadValue<float>();
    }
    
    public void OnMoveLeft(InputAction.CallbackContext context)
    {
        _leftInput = context.ReadValue<float>();
    }
  
    public void OnLook(InputAction.CallbackContext context)
    {
        // Ignore inputs right after start to prevent huge camera jumps from cursor locking
        if (Time.timeSinceLevelLoad < 0.1f) return;

        Vector2 mouseDelta = context.ReadValue<Vector2>();

        _currentPitch -= mouseDelta.y * PitchSensistivity;
        //Clamp the pitch so we don't flip it 
        _currentPitch = Mathf.Clamp(_currentPitch, PitchMin, PitchMax);
        
        _currentYaw += mouseDelta.x * YawSensitivity;
        //Keep the yaw within 0 and 360.  It is possible to go over.
        _currentYaw = Mathf.Repeat(_currentYaw, 360f);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        _sprint = context.ReadValue<bool>();
    }
    #endregion
    
    public void Update()
    {
        MovementUpdate();
    }

    private void MovementUpdate()
    {
        Vector3 moveDirection = (transform.forward * _forwardInput) + (-transform.right * _leftInput);
        
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        characterController.Move(moveDirection * (Time.deltaTime * (_sprint? sprintSpeed : movementSpeed )));
        
        transform.localRotation = Quaternion.Euler(0, _currentYaw, 0);
        if (camera != null)
        {
            camera.transform.localRotation = Quaternion.Euler(_currentPitch, 0, 0);
        }
    }
}