using UnityEngine;

public class OceanFollow : MonoBehaviour
{
    [Tooltip("האובייקט שאחריו הים עוקב (המצלמה)")]
    public Transform targetTransform;
    
    [Tooltip("השחקן (כדי לקרוא את המהירות שלו)")]
    public PlayerController player;
    
    [Tooltip("שליטה בקצב הזרימה. שנה את זה ב-Inspector למספר קטן כמו 0.0005!")]
    public float visualFlowMultiplier = 0.0005f; 

    private Material waterMat;
    private float currentOffset = 0f;

    void Start()
    {
        if (GetComponent<Renderer>() != null)
        {
            waterMat = GetComponent<Renderer>().material;
        }
    }

    // פשוט להחליף את המילה Update ב-LateUpdate
    void LateUpdate()
    {
        // 1. הים עוקב אחרי המצלמה רק קדימה
        if (targetTransform != null)
        {
            transform.position = new Vector3(0f, transform.position.y, targetTransform.position.z);
        }

        // 2. קידום הטקסטורה לאחור
        if (player != null && waterMat != null)
        {
            if (player.forwardSpeed > 0f) 
            {
                currentOffset -= player.forwardSpeed * visualFlowMultiplier * Time.deltaTime;
                waterMat.SetFloat("_WaterSpeed", currentOffset);
            }
        }
    }
}