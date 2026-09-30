using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("סמן ב-V כדי שהסירה לא תיפסל ממכשולים (לצורך בדיקות)")]
    public bool godMode = false;

    void OnTriggerEnter(Collider other)
    {
        // מוודאים שהמנהל קיים והמשחק לא נגמר
        if (GameManager.instance == null || GameManager.instance.isGameOver) return;

        // התנגשות במכשול
        if (other.CompareTag("Obstacle"))
        {
            // אם אנחנו בגאד-מוד, תתעלם ותצא מהפונקציה מיד
            if (godMode) return;

            Debug.Log("Collision Detected with Obstacle: " + other.gameObject.name);
            
            // עוצר את המהירות של השחקן כדי שהים והאנימציות יעצרו
            PlayerController player = GetComponent<PlayerController>();
            if (player != null)
            {
                player.forwardSpeed = 0f;
                player.isDead = true;
            }

            // פוסל את המשחק
            GameManager.instance.TriggerGameOver(); 
        }
        // איסוף מטבע 
        else if (other.CompareTag("Collectables"))
        {
            GameManager.instance.AddToken(); 
            
            // משמידים אך ורק את המטבע הספציפי שפגענו בו
            Destroy(other.gameObject);
        }
    }
}