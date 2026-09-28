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
    [Tooltip("אילו נושאים (Themes) מתאימים לאובייקט הזה?")]
    public LevelTheme[] allowedThemes; 
    public PlacementType placementType;
    public bool isSurface; 

    [Header("Scale & Size Customization")]
    [Tooltip("סמן כאן כדי לבטל את שינוי הגודל האקראי (האובייקט ייווצר תמיד בגודל קבוע)")]
    public bool disableRandomScale = false;
    
    [Tooltip("תיקון גודל בסיסי (למשל להגדיל עץ קטן או להקטין אובייקט ענק)")]
    public float baseScaleMultiplier = 1f;
    [Tooltip("כמה אחוז אקראיות להוסיף לגודל כדי לשבור אחידות (לא יעבוד אם סימנת V למעלה)")]
    public float randomScaleJitter = 0.15f;

    [Header("Containment Rules")]
    public bool isContainer; 
    public Transform contentAnchor; 
    public bool canBeContained; 

    [Header("Rotation Options")]
    [Tooltip("אם רשימה זו ריקה, האובייקט יקבל סיבוב אקראי חופשי לחלוטין על ציר ה-Y")]
    public Vector3[] allowedRotations;
}