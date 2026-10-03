using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("האובייקט שאחריו המצלמה תעקוב (השחקן)")]
    public Transform target; 
    
    [Tooltip("היסט המצלמה: ה-X קובע את הנתיב הקבוע של המצלמה (0 = אמצע)")]
    public Vector3 offset = new Vector3(0f, 6.5f, -10f); 

    [Header("Speed Sense (FOV Effect)")]
    public float baseFOV = 60f;
    public float maxFOV = 95f;
    public float speedForMaxFOV = 120f;

    private Camera cam;
    private PlayerController player;

    // עברנו מ-Start ל-Awake כדי לחתוך את המצלמה למקום לפני שהסצנה מתחילה להתרנדר
    void Awake()
    {
        cam = GetComponent<Camera>();
        
        if (target != null)
        {
            player = target.GetComponent<PlayerController>();
            
            // איפוס הזום באופן מיידי
            if (cam != null)
            {
                cam.fieldOfView = baseFOV;
            }

            // הצבת המצלמה במיקום המדויק עוד לפני הפריים הראשון
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
            // מעקב רגיל אחרי השחקן
            transform.position = new Vector3(
                offset.x, 
                target.position.y + offset.y, 
                target.position.z + offset.z
            );
        }

        // אפקט ה-FOV
        if (cam != null && player != null)
        {
            float speedPercent = Mathf.Clamp01(player.forwardSpeed / speedForMaxFOV);
            float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedPercent);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.unscaledDeltaTime * 3f);
        }
    }
}