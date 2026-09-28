using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("האובייקט שאחריו המצלמה תעקוב (השחקן)")]
    public Transform target; 
    
    [Tooltip("המרחק של המצלמה מהשחקן (גובה ואחורה)")]
    public Vector3 offset = new Vector3(0, 5f, -7f); 

    void LateUpdate()
    {
        if (target != null)
        {
            // מעדכן את מיקום המצלמה לפי מיקום השחקן פלוס ההיסט
            transform.position = target.position + offset;
        }
    }
}