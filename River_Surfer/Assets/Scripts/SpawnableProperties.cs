using UnityEngine;

public enum LevelTheme { Summer, Rocky, Pirates }
public enum PlacementLocation { WaterOnly, SurfaceOnly, Both }
public enum ObjectRole { TrackOnly, BackgroundOnly, Both }

public class SpawnableProperties : MonoBehaviour
{
    [Header("Themes")]
    public LevelTheme[] allowedThemes; 
    
    [Header("Placement & Role")]
    public ObjectRole objectRole = ObjectRole.TrackOnly;
    public PlacementLocation placementLocation = PlacementLocation.WaterOnly;
    public bool isSurface; 
    
    [Header("Background Density")]
    public int backgroundDensityMultiplier = 1;

    [Header("Spawn Chance")]
    public int spawnWeight = 1;

    [Header("Placement Nudge & Anchors")]
    public float manualYOffset = 0f;
    public Transform customSurfaceAnchor;

    [Header("Grid Blocking")]
    public bool autoCalculateSpans = true; 
    public int rowSpan = 1;
    public int laneSpan = 1;
    
    [Header("Scale Options")]
    public bool disableRandomScale = false;
    public float baseScaleMultiplier = 1f;
    public float randomScaleJitter = 0.15f;

    [Header("Rotation Options")]
    public bool disableRandomRotation = false;
    public Vector3[] allowedRotations;

    void Awake()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            col.isTrigger = true;
            if (!col.gameObject.CompareTag("Collectables"))
            {
                col.gameObject.tag = "Obstacle";
            }
        }
    }
}