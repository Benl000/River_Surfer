using UnityEngine;

public class Token : MonoBehaviour
{
    public float rotateSpeed = 90f;
    private bool isCollected = false; 

    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return; 

        if (other.CompareTag("Player"))
        {
            isCollected = true; 
            
            if (AudioManager.Instance != null) 
                AudioManager.Instance.PlaySFX(AudioManager.Instance.coinSound);
                
            GameManager.instance.AddToken();
            Destroy(gameObject);
        }
    }
}