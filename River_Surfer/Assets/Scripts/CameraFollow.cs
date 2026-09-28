using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("האובייקט שאחריו המצלמה תעקוב (השחקן)")]
    public Transform target; 
    
    [Tooltip("היסט המצלמה: ה-X קובע את הנתיב הקבוע של המצלמה (0 = אמצע)")]
    public Vector3 offset = new Vector3(0f, 6.5f, -10f); 

    [Header("Speed Sense (FOV Effect)")]
    [Tooltip("זווית הראייה הרגילה בתחילת המשחק")]
    public float baseFOV = 60f;
    [Tooltip("זווית הראייה המקסימלית במהירויות שיא (יוצר אפקט מהירות מטורף)")]
    public float maxFOV = 95f;
    [Tooltip("המהירות שבה המצלמה תגיע ל-FOV המקסימלי")]
    public float speedForMaxFOV = 120f;

    private Camera cam;
    private PlayerController player;

    void Start()
    {
        cam = GetComponent<Camera>();
        
        if (target != null)
        {
            player = target.GetComponent<PlayerController>();
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // 1. מעקב מיקום רגיל - מצלמה נעולה על X=0 (אמצע הנהר)
            transform.position = new Vector3(
                offset.x, 
                target.position.y + offset.y, 
                target.position.z + offset.z
            );
        }

        // 2. מנגנון תחושת מהירות - שינוי FOV דינמי
        if (cam != null && player != null)
        {
            // מחשבים כמה אנחנו קרובים למהירות השיא (בין 0 ל-1)
            float speedPercent = Mathf.Clamp01(player.forwardSpeed / speedForMaxFOV);
            
            // מחשבים את הזווית הרצויה ויוצרים מעבר חלק
            float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedPercent);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 3f);
        }
    }
}