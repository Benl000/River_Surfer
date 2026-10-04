using UnityEngine;

public class OceanFollow : MonoBehaviour
{
    public Transform targetTransform;
    public PlayerController player;
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

    void LateUpdate()
    {
        if (targetTransform != null)
        {
            transform.position = new Vector3(0f, transform.position.y, targetTransform.position.z);
        }

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