using UnityEngine;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
    public static TrackManager instance;

    [Header("Prefabs Lists")]
    public GameObject[] basePrefabs; 
    public GameObject[] propPrefabs; 
    
    [Header("Collectibles (Coin Trails)")]
    [Tooltip("גרור לכאן את ה-Prefab של coin-gold")]
    public GameObject coinPrefab;
    [Range(0f, 1f)]
    [Tooltip("הסיכוי להתחיל שביל מטבעות חדש במים פתוחים")]
    public float coinTrailChance = 0.2f;
    [Tooltip("כמות מינימלית של מטבעות ברצף")]
    public int minCoinTrailLength = 3;
    [Tooltip("כמות מקסימלית של מטבעות ברצף")]
    public int maxCoinTrailLength = 6;
    [Tooltip("גובה הצבת המטבע מעל פני המים")]
    public float coinHeightOffset = 0.7f;
    
    [Header("Theme Settings")]
    public LevelTheme currentTheme = LevelTheme.Summer; 

    [Header("Grid & Spacing Settings")]
    [Tooltip("מרחק פיזי ביוניטי בין שורה לשורה")]
    public float zSpacing = 12f;  
    public float[] laneXPositions = { -3.5f, 0f, 3.5f }; 

    [Header("Pacing & Obstacle Frequency")]
    [Tooltip("כמה שורות ריקות לחלוטין מובטחות אחרי כל שורת מכשולים")]
    public int emptyRowsBetweenObstacles = 3;
    [Range(0f, 1f)]
    [Tooltip("הסיכוי שתיווצר שורת מכשול כשתקופת הצינון מסתיימת")]
    public float obstacleRowChance = 0.45f;
    [Range(1, 2)]
    [Tooltip("מקסימום מכשולים בשורה אחת (כדי לא לסגור את הנהר)")]
    public int maxObstaclesPerRow = 1;

    [Header("Infinite Generation Settings")]
    public Transform playerTransform; 
    [Tooltip("כמה מטרים קדימה מהשחקן המסלול תמיד יישאר בנוי")]
    public float forwardViewDistance = 120f; 
    [Tooltip("מרחק מאחורי השחקן שבו שורה שעברנו תימחק מהזיכרון")]
    public float destroyDistanceBehind = 25f; 

    private float nextSpawnZ = 10f; 
    private Queue<GameObject> activeRows = new Queue<GameObject>();

    private int currentRowIndex = 0;
    private int[] laneBlockedUntilRow;
    private int rowsSinceLastObstacle = 0;

    // משתני ניהול שבילי מטבעות
    private int activeCoinTrailLane = -1;
    private int remainingCoinsInTrail = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        laneBlockedUntilRow = new int[laneXPositions.Length];
        rowsSinceLastObstacle = emptyRowsBetweenObstacles;
        activeCoinTrailLane = -1;
        remainingCoinsInTrail = 0;
    }

    void Start()
    {
        if (playerTransform == null)
        {
            PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
            if (pc != null) playerTransform = pc.transform;
        }

        while (nextSpawnZ < forwardViewDistance)
        {
            SpawnRow();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        while (nextSpawnZ - playerTransform.position.z < forwardViewDistance)
        {
            SpawnRow();
        }

        while (activeRows.Count > 0 && 
               playerTransform.position.z - activeRows.Peek().transform.position.z > destroyDistanceBehind)
        {
            GameObject oldestRow = activeRows.Dequeue();
            Destroy(oldestRow);
        }
    }

    void SpawnRow()
    {
        List<GameObject> validBases = new List<GameObject>();
        foreach (GameObject baseObj in basePrefabs)
        {
            SpawnableProperties props = baseObj.GetComponent<SpawnableProperties>();
            if (props != null && props.allowedThemes != null)
            {
                foreach (LevelTheme theme in props.allowedThemes)
                {
                    if (theme == currentTheme)
                    {
                        validBases.Add(baseObj);
                        break;
                    }
                }
            }
        }

        if (validBases.Count == 0) return;

        GameObject rowParent = new GameObject("Row_" + nextSpawnZ);
        rowParent.transform.position = new Vector3(0, 0, nextSpawnZ);
        rowParent.transform.SetParent(this.transform);

        bool canSpawnObstacle = rowsSinceLastObstacle >= emptyRowsBetweenObstacles;
        bool shouldSpawnObstacle = canSpawnObstacle && (Random.value < obstacleRowChance);

        List<int> freeLanes = new List<int>();
        for (int i = 0; i < laneXPositions.Length; i++)
        {
            if (currentRowIndex >= laneBlockedUntilRow[i])
            {
                freeLanes.Add(i);
            }
        }

        HashSet<int> lanesToSpawn = new HashSet<int>();
        if (shouldSpawnObstacle && freeLanes.Count > 1)
        {
            int chosenLane = freeLanes[Random.Range(0, freeLanes.Count)];
            lanesToSpawn.Add(chosenLane);
        }

        // --- ניהול שבילי מטבעות ---
        // 1. קטיעת שביל קיים אם הנתיב פתאום נחסם על ידי מכשול
        if (remainingCoinsInTrail > 0)
        {
            if (lanesToSpawn.Contains(activeCoinTrailLane) || currentRowIndex < laneBlockedUntilRow[activeCoinTrailLane])
            {
                remainingCoinsInTrail = 0;
                activeCoinTrailLane = -1;
            }
        }

        // 2. התחלת שביל חדש אם אנחנו לא באמצע אחד
        if (remainingCoinsInTrail <= 0 && coinPrefab != null)
        {
            if (Random.value < coinTrailChance)
            {
                List<int> safeLanes = new List<int>();
                for (int i = 0; i < laneXPositions.Length; i++)
                {
                    if (!lanesToSpawn.Contains(i) && currentRowIndex >= laneBlockedUntilRow[i])
                    {
                        safeLanes.Add(i);
                    }
                }

                if (safeLanes.Count > 0)
                {
                    activeCoinTrailLane = safeLanes[Random.Range(0, safeLanes.Count)];
                    remainingCoinsInTrail = Random.Range(minCoinTrailLength, maxCoinTrailLength + 1);
                }
            }
        }

        // --- יצירת האובייקטים בנתיבים ---
        for (int i = 0; i < laneXPositions.Length; i++)
        {
            // נתיב מים פתוח - בודקים אם צריך לשים בו מטבע מתוך שביל פעיל
            if (!lanesToSpawn.Contains(i))
            {
                if (i == activeCoinTrailLane && remainingCoinsInTrail > 0)
                {
                    Vector3 coinPos = new Vector3(laneXPositions[i], coinHeightOffset, nextSpawnZ);
                    Instantiate(coinPrefab, coinPos, Quaternion.identity, rowParent.transform);
                    
                    remainingCoinsInTrail--;
                    if (remainingCoinsInTrail <= 0)
                    {
                        activeCoinTrailLane = -1;
                    }
                }
                continue;
            }

            // נתיב חסום - יצירת מכשול
            float xPos = laneXPositions[i];
            Vector3 spawnPos = new Vector3(xPos, 0, nextSpawnZ);
            
            GameObject selectedBase = validBases[Random.Range(0, validBases.Count)];
            GameObject newBase = Instantiate(selectedBase, spawnPos, Quaternion.identity, rowParent.transform);

            ApplyScaleAndRotation(newBase);

            SpawnableProperties baseProps = newBase.GetComponent<SpawnableProperties>();
            if (baseProps != null)
            {
                if (baseProps.rowSpan > 1)
                {
                    laneBlockedUntilRow[i] = currentRowIndex + baseProps.rowSpan;
                }

                if (baseProps.isSurface)
                {
                    GameObject spawnedProp = SpawnPropOnSurface(baseProps);
                    if (spawnedProp != null)
                    {
                        SpawnableProperties propProps = spawnedProp.GetComponent<SpawnableProperties>();
                        if (propProps != null && propProps.rowSpan > 1)
                        {
                            laneBlockedUntilRow[i] = Mathf.Max(laneBlockedUntilRow[i], currentRowIndex + propProps.rowSpan);
                        }
                    }
                }
            }
        }

        if (lanesToSpawn.Count > 0)
        {
            rowsSinceLastObstacle = 0;
        }
        else
        {
            rowsSinceLastObstacle++;
        }
        
        activeRows.Enqueue(rowParent);
        nextSpawnZ += zSpacing;
        currentRowIndex++;
    }

    GameObject SpawnPropOnSurface(SpawnableProperties baseProps)
    {
        List<GameObject> validProps = new List<GameObject>();
        
        foreach (GameObject prop in propPrefabs)
        {
            SpawnableProperties propProps = prop.GetComponent<SpawnableProperties>();
            if (propProps != null && propProps.allowedThemes != null)
            {
                foreach (LevelTheme theme in propProps.allowedThemes)
                {
                    if (theme == currentTheme)
                    {
                        validProps.Add(prop);
                        break; 
                    }
                }
            }
        }

        if (validProps.Count > 0)
        {
            GameObject selectedProp = validProps[Random.Range(0, validProps.Count)];
            Vector3 spawnPoint = baseProps.transform.position;
            
            Renderer[] baseRenderers = baseProps.GetComponentsInChildren<Renderer>();
            if (baseRenderers.Length > 0)
            {
                float maxY = baseRenderers[0].bounds.max.y;
                foreach (Renderer r in baseRenderers)
                {
                    if (r.bounds.max.y > maxY) maxY = r.bounds.max.y;
                }
                spawnPoint.y = maxY;
            }
            
            GameObject newProp = Instantiate(selectedProp, spawnPoint, Quaternion.identity, baseProps.transform);
            ApplyScaleAndRotation(newProp);

            Renderer[] propRenderers = newProp.GetComponentsInChildren<Renderer>();
            if (propRenderers.Length > 0)
            {
                float minY = propRenderers[0].bounds.min.y;
                foreach (Renderer r in propRenderers)
                {
                    if (r.bounds.min.y < minY) minY = r.bounds.min.y;
                }
                
                float currentY = newProp.transform.position.y;
                float offset = currentY - minY;
                
                Vector3 correctedPos = newProp.transform.position;
                correctedPos.y += offset;
                newProp.transform.position = correctedPos;
            }

            return newProp;
        }

        return null;
    }

    void ApplyScaleAndRotation(GameObject obj)
    {
        SpawnableProperties props = obj.GetComponent<SpawnableProperties>();
        
        if (props != null)
        {
            float scaleFactor = props.baseScaleMultiplier;
            if (!props.disableRandomScale)
            {
                scaleFactor *= Random.Range(1f - props.randomScaleJitter, 1f + props.randomScaleJitter);
            }

            Vector3 finalScale = obj.transform.localScale * scaleFactor;

            if (obj.transform.parent != null && obj.transform.parent.GetComponent<SpawnableProperties>() != null)
            {
                Vector3 parentScale = obj.transform.parent.localScale;
                finalScale = new Vector3(
                    finalScale.x / parentScale.x,
                    finalScale.y / parentScale.y,
                    finalScale.z / parentScale.z
                );
            }

            obj.transform.localScale = finalScale;

            if (props.allowedRotations != null && props.allowedRotations.Length > 0)
            {
                Vector3 randomRot = props.allowedRotations[Random.Range(0, props.allowedRotations.Length)];
                obj.transform.localEulerAngles = randomRot;
            }
            else
            {
                float randomY = Random.Range(0f, 360f);
                obj.transform.localEulerAngles = new Vector3(obj.transform.localEulerAngles.x, randomY, obj.transform.localEulerAngles.z);
            }
        }
        else
        {
            float randomY = Random.Range(0f, 360f);
            obj.transform.localEulerAngles = new Vector3(obj.transform.localEulerAngles.x, randomY, obj.transform.localEulerAngles.z);
        }
    }
}