using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; 
    public Vector3 offset = new Vector3(0f, 6.5f, -10f); 

    [Header("Speed Sense (FOV Effect)")]
    public float baseFOV = 60f;
    public float maxFOV = 95f;
    public float speedForMaxFOV = 120f;

    private Camera cam;
    private PlayerController player;

    void Awake()
    {
        cam = GetComponent<Camera>();
        
        if (target != null)
        {
            player = target.GetComponent<PlayerController>();
            
            if (cam != null)
            {
                cam.fieldOfView = baseFOV;
            }

            transform.position = new Vector3(
                offset.x, 
                target.position.y + offset.y, 
                target.position.z + offset.z
            );
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = new Vector3(
                offset.x, 
                target.position.y + offset.y, 
                target.position.z + offset.z
            );
        }

        if (cam != null && player != null)
        {
            float speedPercent = Mathf.Clamp01(player.forwardSpeed / speedForMaxFOV);
            float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedPercent);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.unscaledDeltaTime * 3f);
        }
    }
}