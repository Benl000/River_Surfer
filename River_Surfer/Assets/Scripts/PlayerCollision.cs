using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // מוודאים שהמנהל קיים והמשחק לא נגמר
        if (GameManager.instance == null || GameManager.instance.isGameOver) return;

        // התנגשות במכשול
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("Collision Detected with Obstacle: " + other.gameObject.name);
            GameManager.instance.TriggerGameOver(); 
        }
        // איסוף מטבע 
        else if (other.CompareTag("Collectables"))
        {
            GameManager.instance.AddToken(); 
            
            // התיקון: משמידים אך ורק את המטבע הספציפי שפגענו בו!
            Destroy(other.gameObject);
        }
    }
}