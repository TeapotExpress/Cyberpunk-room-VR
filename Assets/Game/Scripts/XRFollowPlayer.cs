using UnityEngine;

public class XRFollowPlayer : MonoBehaviour
{
    public Transform playerHead;  // XR camera
    public Transform playerRoot;  // XR Origin or Rig root

    [Tooltip("Smooth follow movement speed")]
    public float followSpeed = 10f;

    [Tooltip("Smooth rotation speed")]
    public float rotationSpeed = 5f;

    private Vector3 worldOffset; // The original offset from the player

    private void Start()
    {
        if (playerRoot == null || playerHead == null)
        {
            Debug.LogError("XRFollowPlayer: Assign both Player Head and Player Root.");
            return;
        }

        // Calculate initial world offset from player center
        worldOffset = transform.position - GetPlayerCenter();
    }

    private void LateUpdate()
    {
        if (playerRoot == null || playerHead == null)
            return;

        // Target position is player's current center + original offset
        Vector3 targetPosition = GetPlayerCenter() + worldOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Only rotate on Y-axis to match player's heading
        Vector3 playerForward = playerHead.forward;
        playerForward.y = 0;
        if (playerForward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(playerForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Helper: gets player's "center" position (XZ only, keeps object's Y)
    private Vector3 GetPlayerCenter()
    {
        Vector3 rootPos = playerRoot.position;
        Vector3 headPos = playerHead.position;

        return new Vector3(headPos.x, transform.position.y, headPos.z); // Follow player's horizontal movement
    }
}
