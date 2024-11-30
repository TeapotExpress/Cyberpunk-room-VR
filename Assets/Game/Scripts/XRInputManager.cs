using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class XRInputManager : MonoBehaviour
{
    [Serializable]
    public class AxisInput
    {
        public InputActionProperty axisInputProperty;
        public string animatorFloatName;
    }

    [Serializable]
    public class ButtonInput
    {
        public InputActionProperty buttonInputProperty;
        public UnityEvent onButtonPressed;
    }

    public AxisInput[] axisInputs;
    public ButtonInput[] buttonInputs;

    public Animator animator;

    private void Start()
    {
        // Subscribe to button input events and enable actions
        foreach (var button in buttonInputs)
        {
            if (button.buttonInputProperty != null && button.buttonInputProperty.action != null)
            {
                button.buttonInputProperty.action.performed += ctx => button.onButtonPressed?.Invoke();
                button.buttonInputProperty.action.Enable();
            }
        }

        // Enable axis input actions
        foreach (var axis in axisInputs)
        {
            if (axis.axisInputProperty != null && axis.axisInputProperty.action != null)
            {
                axis.axisInputProperty.action.Enable();
            }
        }
    }

    private void Update()
    {
        // Update animator float values based on axis inputs
        foreach (var axis in axisInputs)
        {
            if (axis.axisInputProperty != null && axis.axisInputProperty.action != null)
            {
                float value = axis.axisInputProperty.action.ReadValue<float>();
                animator.SetFloat(axis.animatorFloatName, value);
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from button input events and disable actions
        foreach (var button in buttonInputs)
        {
            if (button.buttonInputProperty != null && button.buttonInputProperty.action != null)
            {
                button.buttonInputProperty.action.performed -= ctx => button.onButtonPressed?.Invoke();
                button.buttonInputProperty.action.Disable();
            }
        }

        // Disable axis input actions
        foreach (var axis in axisInputs)
        {
            if (axis.axisInputProperty != null && axis.axisInputProperty.action != null)
            {
                axis.axisInputProperty.action.Disable();
            }
        }
    }
}
