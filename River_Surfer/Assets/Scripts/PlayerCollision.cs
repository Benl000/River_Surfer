using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // רדאר: מדפיס לקונסול כל אובייקט בעולם שהקוליידר שלנו חותך
        Debug.Log("Collision Detected with: " + other.gameObject.name + " | Tag: " + other.gameObject.tag);

        // נוודא שהמנהל קיים כדי למנוע שגיאות אדומות
        if (GameManager.instance == null) 
        {
            Debug.LogError("GameManager is missing from the scene!");
            return;
        }

        if (GameManager.instance.isGameOver) return;

        // חיפוש חכם: בודק אם התגית נמצאת על הקוליידר עצמו או על אובייקט האב שלו
        if (other.CompareTag("Obstacle") || other.transform.root.CompareTag("Obstacle"))
        {
            GameManager.instance.GameOver(); //[cite: 6]
        }
        else if (other.CompareTag("Collectible") || other.transform.root.CompareTag("Collectible"))
        {
            GameManager.instance.AddScore(1); //[cite: 6]
            
            // השמדת אובייקט האב כדי שכל המטבע ייעלם, גם אם פגענו רק בילד
            Destroy(other.transform.root.gameObject);
        }
    }
}