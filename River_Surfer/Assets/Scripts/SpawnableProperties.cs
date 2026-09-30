using UnityEngine;

public enum PlacementType 
{ 
    WaterOnly, 
    LandOnly, 
    Both 
}

public enum LevelTheme 
{ 
    Summer, 
    Winter, 
    Ruins 
}

public class SpawnableProperties : MonoBehaviour
{
    [Header("Theme & Placement")]
    public LevelTheme[] allowedThemes; 
    public PlacementType placementType;
    public bool isSurface; 
    
    public int rowSpan = 1;
    
    [Header("Scale & Size Customization")]
    public bool disableRandomScale = false;
    public float baseScaleMultiplier = 1f;
    public float randomScaleJitter = 0.15f;

    [Header("Containment Rules")]
    public bool isContainer; 
    public Transform contentAnchor; 
    public bool canBeContained; 

    [Header("Rotation Options")]
    public Vector3[] allowedRotations;

    // --- אוטומציית קוליידרים ---
    void Awake()
    {
        // סורק את האובייקט הראשי ואת כל הילדים שלו כדי למצוא קוליידרים
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        
        foreach (Collider col in colliders)
        {
            // הופך לטריגר בכל מקרה
            col.isTrigger = true;

            // משנה את התגית למכשול *רק* אם זה לא מטבע
            if (!col.gameObject.CompareTag("Collectables"))
            {
                col.gameObject.tag = "Obstacle";
            }
        }
    }
}