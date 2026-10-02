using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 100f;

    [Header("Player")]
    [SerializeField] private Transform playerBody;

    private float verticalRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Muis input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        verticalRotation -= mouseY;

        verticalRotation = Mathf.Clamp(
            verticalRotation,
            -90f,
            90f
        );

        transform.localRotation = Quaternion.Euler(
            verticalRotation,
            0f,
            0f
        );

        playerBody.Rotate(Vector3.up * mouseX);
    }
}