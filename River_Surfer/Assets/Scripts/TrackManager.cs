using UnityEngine;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
    public static TrackManager instance;

    [Header("All Prefabs")]
    public GameObject[] allPrefabs; 
    
    [Header("Collectibles")]
    public GameObject coinPrefab;
    [Range(0f, 1f)] public float coinTrailChance = 0.2f;
    public int minCoinTrailLength = 3;
    public int maxCoinTrailLength = 6;
    public float coinHeightOffset = 0.7f;
    
    [Header("Theme Settings")]
    public LevelTheme currentTheme = LevelTheme.Summer; 
    
    [Header("Theme Progression")]
    public bool autoSwitchThemes = true;
    public int rowsPerTheme = 80;

    [Header("Grid Settings (Track)")]
    public float zSpacing = 12f;  
    public float[] laneXPositions = { -3.5f, 0f, 3.5f }; 
    
    [Header("Background Scenery (Framing)")]
    public float outerWallDistanceX = 35f;
    public float outerWallScaleMultiplier = 4.5f;
    public float outerWallYOffset = -2f;
    public int minPropsOnOuterWall = 3;
    public int maxPropsOnOuterWall = 8;

    [Header("Far Backdrop (Horizon Blocker)")]
    public bool spawnFarBackdrop = true;
    public float farBackdropDistanceX = 85f;
    public float farBackdropScaleMultiplier = 12f;
    public float farBackdropYOffset = -10f;

    [Header("Spawning Rules")]
    public int initialSafeRows = 8; 
    public int emptyRowsBetweenObstacles = 5; 
    [Range(0f, 1f)] public float obstacleRowChance = 0.35f; 
    [Range(0f, 1f)] public float propOnSurfaceChance = 0.5f; 
    
    [Range(0.1f, 1f)]
    public float surfaceSpreadRadius = 0.6f;

    [Header("Generation Settings")]
    public Transform playerTransform; 
    public float forwardViewDistance = 120f; 
    public float destroyDistanceBehind = 25f; 

    private float nextSpawnZ = 10f; 
    private Queue<GameObject> activeRows = new Queue<GameObject>();
    private int currentRowIndex = 0;
    private int[] laneBlockedUntilRow;
    private int rowsSinceLastObstacle = 0;
    
    private int activeCoinTrailLane = -1;
    private int remainingCoinsInTrail = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        laneBlockedUntilRow = new int[laneXPositions.Length];
        rowsSinceLastObstacle = emptyRowsBetweenObstacles;
    }

    void Start()
    {
        if (playerTransform == null)
        {
            PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
            if (pc != null) playerTransform = pc.transform;
        }
        while (nextSpawnZ < forwardViewDistance) SpawnRow();
    }

    void Update()
    {
        if (playerTransform == null) return;
        while (nextSpawnZ - playerTransform.position.z < forwardViewDistance) SpawnRow();
        while (activeRows.Count > 0 && playerTransform.position.z - activeRows.Peek().transform.position.z > destroyDistanceBehind)
        {
            Destroy(activeRows.Dequeue());
        }
    }

    void SpawnRow()
    {
        if (autoSwitchThemes && currentRowIndex > 0 && currentRowIndex % rowsPerTheme == 0)
        {
            int totalThemes = System.Enum.GetValues(typeof(LevelTheme)).Length;
            int nextThemeIndex = ((int)currentTheme + 1) % totalThemes;
            currentTheme = (LevelTheme)nextThemeIndex;
            Debug.Log("Switched to new Theme: " + currentTheme);
        }

        GameObject rowParent = new GameObject("Row_" + nextSpawnZ);
        rowParent.transform.position = new Vector3(0, 0, nextSpawnZ);
        rowParent.transform.SetParent(this.transform);

        List<GameObject> validScenery = GetValidPrefabs(PlacementLocation.WaterOnly, ObjectRole.BackgroundOnly, true);
        if (validScenery.Count > 0)
        {
            foreach (float dir in new float[] { -1f, 1f })
            {
                GameObject wallObj = validScenery[Random.Range(0, validScenery.Count)];
                Vector3 wallPos = new Vector3(outerWallDistanceX * dir, 0, nextSpawnZ);
                GameObject newWall = Instantiate(wallObj, wallPos, Quaternion.identity, rowParent.transform);
                
                ApplyTransformRules(newWall);
                newWall.transform.localScale *= outerWallScaleMultiplier;
                
                SpawnableProperties bgProps = newWall.GetComponent<SpawnableProperties>();
                float customHeightNudge = (bgProps != null) ? bgProps.manualYOffset : 0f;

                Renderer[] renderers = newWall.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    float minY = renderers[0].bounds.min.y;
                    foreach (Renderer r in renderers) if (r.bounds.min.y < minY) minY = r.bounds.min.y;
                    newWall.transform.position += new Vector3(0, outerWallYOffset - minY + customHeightNudge, 0);
                }

                if (bgProps != null && bgProps.isSurface)
                {
                    int propsToSpawn = Random.Range(minPropsOnOuterWall, maxPropsOnOuterWall + 1);
                    SpawnPropsOnSurface(bgProps, propsToSpawn);
                }
            }

            if (spawnFarBackdrop)
            {
                foreach (float dir in new float[] { -1f, 1f })
                {
                    GameObject backdropObj = validScenery[Random.Range(0, validScenery.Count)];
                    Vector3 backdropPos = new Vector3(farBackdropDistanceX * dir, 0, nextSpawnZ);
                    GameObject newBackdrop = Instantiate(backdropObj, backdropPos, Quaternion.identity, rowParent.transform);
                    
                    ApplyTransformRules(newBackdrop);
                    newBackdrop.transform.localScale *= farBackdropScaleMultiplier;
                    
                    SpawnableProperties bgProps = newBackdrop.GetComponent<SpawnableProperties>();
                    float customHeightNudge = (bgProps != null) ? bgProps.manualYOffset * 1.5f : 0f; 

                    Renderer[] renderers = newBackdrop.GetComponentsInChildren<Renderer>();
                    if (renderers.Length > 0)
                    {
                        float minY = renderers[0].bounds.min.y;
                        foreach (Renderer r in renderers) if (r.bounds.min.y < minY) minY = r.bounds.min.y;
                        newBackdrop.transform.position += new Vector3(0, farBackdropYOffset - minY + customHeightNudge, 0);
                    }
                }
            }
        }

        bool canSpawnObstacle = (currentRowIndex >= initialSafeRows) && (rowsSinceLastObstacle >= emptyRowsBetweenObstacles);
        bool shouldSpawnObstacle = canSpawnObstacle && (Random.value < obstacleRowChance);

        List<int> freeLanes = new List<int>();
        for (int i = 0; i < laneXPositions.Length; i++)
        {
            if (currentRowIndex >= laneBlockedUntilRow[i]) freeLanes.Add(i);
        }

        HashSet<int> lanesToSpawn = new HashSet<int>();
        if (shouldSpawnObstacle && freeLanes.Count > 0)
        {
            List<GameObject> validWaterBases = GetValidPrefabs(PlacementLocation.WaterOnly, ObjectRole.TrackOnly, false);
            if (validWaterBases.Count > 0)
            {
                int chosenLaneIndex = freeLanes[Random.Range(0, freeLanes.Count)];
                lanesToSpawn.Add(chosenLaneIndex);
                
                GameObject selectedBase = validWaterBases[Random.Range(0, validWaterBases.Count)];
                GameObject newBase = Instantiate(selectedBase, new Vector3(laneXPositions[chosenLaneIndex], 0, nextSpawnZ), Quaternion.identity, rowParent.transform);
                
                ApplyTransformRules(newBase);

                SpawnableProperties baseProps = newBase.GetComponent<SpawnableProperties>();
                if (baseProps != null)
                {
                    if (baseProps.manualYOffset != 0) 
                    {
                        newBase.transform.position += new Vector3(0, baseProps.manualYOffset, 0);
                    }

                    if (baseProps.laneSpan >= 2 && chosenLaneIndex == 1)
                    {
                        chosenLaneIndex = Random.value > 0.5f ? 0 : 2; 
                        newBase.transform.position = new Vector3(laneXPositions[chosenLaneIndex], newBase.transform.position.y, nextSpawnZ);
                        lanesToSpawn.Clear();
                        lanesToSpawn.Add(chosenLaneIndex);
                    }

                    if (baseProps.rowSpan > 1) laneBlockedUntilRow[chosenLaneIndex] = currentRowIndex + baseProps.rowSpan;
                    
                    if (baseProps.laneSpan > 1)
                    {
                        if (chosenLaneIndex == 0 || chosenLaneIndex == 2) 
                        {
                            laneBlockedUntilRow[1] = currentRowIndex + baseProps.rowSpan;
                            lanesToSpawn.Add(1);
                        }
                    }

                    if (baseProps.isSurface && Random.value <= propOnSurfaceChance)
                    {
                        SpawnPropsOnSurface(baseProps, 1);
                    }
                }
            }
        }

        if (remainingCoinsInTrail > 0)
        {
            if (lanesToSpawn.Contains(activeCoinTrailLane) || currentRowIndex < laneBlockedUntilRow[activeCoinTrailLane])
            {
                remainingCoinsInTrail = 0;
                activeCoinTrailLane = -1;
            }
        }

        if (remainingCoinsInTrail <= 0 && coinPrefab != null && Random.value < coinTrailChance && currentRowIndex >= initialSafeRows)
        {
            List<int> safeLanesForCoins = new List<int>();
            for (int i = 0; i < laneXPositions.Length; i++)
            {
                if (!lanesToSpawn.Contains(i) && currentRowIndex >= laneBlockedUntilRow[i]) safeLanesForCoins.Add(i);
            }

            if (safeLanesForCoins.Count > 0)
            {
                activeCoinTrailLane = safeLanesForCoins[Random.Range(0, safeLanesForCoins.Count)];
                remainingCoinsInTrail = Random.Range(minCoinTrailLength, maxCoinTrailLength + 1);
            }
        }

        for (int i = 0; i < laneXPositions.Length; i++)
        {
            if (!lanesToSpawn.Contains(i) && i == activeCoinTrailLane && remainingCoinsInTrail > 0)
            {
                Instantiate(coinPrefab, new Vector3(laneXPositions[i], coinHeightOffset, nextSpawnZ), Quaternion.identity, rowParent.transform);
                remainingCoinsInTrail--;
                if (remainingCoinsInTrail <= 0) activeCoinTrailLane = -1;
            }
        }

        if (lanesToSpawn.Count > 0) rowsSinceLastObstacle = 0;
        else rowsSinceLastObstacle++;
        
        activeRows.Enqueue(rowParent);
        nextSpawnZ += zSpacing;
        currentRowIndex++;
    }

    void SpawnPropsOnSurface(SpawnableProperties baseProps, int basePropCount)
    {
        ObjectRole requiredRole = baseProps.objectRole == ObjectRole.BackgroundOnly ? ObjectRole.BackgroundOnly : ObjectRole.TrackOnly;
        
        List<GameObject> validProps = GetValidPrefabs(PlacementLocation.SurfaceOnly, requiredRole, false);
        if (validProps.Count == 0) return;

        Bounds baseBounds = new Bounds(baseProps.transform.position, Vector3.zero);
        
        Collider mainCollider = baseProps.GetComponent<Collider>();
        if (mainCollider != null)
        {
            baseBounds = mainCollider.bounds;
        }
        else
        {
            Renderer[] baseRenderers = baseProps.GetComponentsInChildren<Renderer>();
            if (baseRenderers.Length > 0)
            {
                baseBounds = baseRenderers[0].bounds;
                foreach (Renderer r in baseRenderers) baseBounds.Encapsulate(r.bounds);
            }
        }

        Vector3 surfaceCenterTop;

        // --- התיקון: בדיקת עוגן ידני ---
        if (baseProps.customSurfaceAnchor != null)
        {
            surfaceCenterTop = baseProps.customSurfaceAnchor.position;
        }
        else
        {
            surfaceCenterTop = new Vector3(baseBounds.center.x, baseBounds.max.y, baseBounds.center.z);
        }

        float safeRadius = Mathf.Min(baseBounds.extents.x, baseBounds.extents.z) * surfaceSpreadRadius;

        for (int i = 0; i < basePropCount; i++)
        {
            GameObject selectedPropPrefab = validProps[Random.Range(0, validProps.Count)];
            SpawnableProperties propSettings = selectedPropPrefab.GetComponent<SpawnableProperties>();
            
            int spawnAmount = 1;
            if (requiredRole == ObjectRole.BackgroundOnly && propSettings != null)
            {
                spawnAmount = Mathf.Max(1, propSettings.backgroundDensityMultiplier);
            }

            for (int j = 0; j < spawnAmount; j++)
            {
                GameObject newProp = Instantiate(selectedPropPrefab, surfaceCenterTop, Quaternion.identity, baseProps.transform);
                ApplyTransformRules(newProp);

                Bounds propBounds = new Bounds(newProp.transform.position, Vector3.zero);
                Renderer[] propRenderers = newProp.GetComponentsInChildren<Renderer>();
                if (propRenderers.Length > 0)
                {
                    propBounds = propRenderers[0].bounds;
                    foreach (Renderer r in propRenderers) propBounds.Encapsulate(r.bounds);
                    
                    float maxAllowedSize = Mathf.Min(baseBounds.size.x, baseBounds.size.z) * 0.85f;
                    float currentPropSize = Mathf.Max(propBounds.size.x, propBounds.size.z);
                    float fitScale = maxAllowedSize / Mathf.Max(0.01f, currentPropSize); 

                    if (fitScale < 0.45f)
                    {
                        Destroy(newProp);
                        continue; 
                    }

                    if (fitScale < 1f)
                    {
                        newProp.transform.localScale *= fitScale;
                        propBounds = propRenderers[0].bounds;
                        foreach (Renderer r in propRenderers) propBounds.Encapsulate(r.bounds);
                    }

                    // משתמשים במרכז של העוגן כבסיס לפיזור
                    float finalX = surfaceCenterTop.x;
                    float finalZ = surfaceCenterTop.z;

                    if (basePropCount > 1 || spawnAmount > 1)
                    {
                        Vector2 randomCircle = Random.insideUnitCircle * safeRadius;
                        finalX += randomCircle.x;
                        finalZ += randomCircle.y;
                    }

                    float finalY = newProp.transform.position.y + (surfaceCenterTop.y - propBounds.min.y);
                    newProp.transform.position = new Vector3(finalX, finalY, finalZ);
                }
            }
        }
    }

    List<GameObject> GetValidPrefabs(PlacementLocation targetLocation, ObjectRole targetRole, bool includeBoth)
    {
        List<GameObject> valid = new List<GameObject>();
        foreach (GameObject prefab in allPrefabs)
        {
            SpawnableProperties props = prefab.GetComponent<SpawnableProperties>();
            if (props == null) continue;

            bool themeMatches = false;
            foreach (LevelTheme theme in props.allowedThemes)
            {
                if (theme == currentTheme) { themeMatches = true; break; }
            }
            if (!themeMatches) continue;

            bool locationMatches = (props.placementLocation == targetLocation) || (props.placementLocation == PlacementLocation.Both);
            bool roleMatches = (props.objectRole == targetRole) || (props.objectRole == ObjectRole.Both);

            if (locationMatches && roleMatches) 
            {
                int weight = Mathf.Max(1, props.spawnWeight);
                for (int i = 0; i < weight; i++)
                {
                    valid.Add(prefab);
                }
            }
        }
        return valid;
    }

    void ApplyTransformRules(GameObject obj)
    {
        SpawnableProperties props = obj.GetComponent<SpawnableProperties>();
        if (props == null) return;

        float scaleFactor = props.baseScaleMultiplier;
        if (!props.disableRandomScale) scaleFactor *= Random.Range(1f - props.randomScaleJitter, 1f + props.randomScaleJitter);
        
        Vector3 finalScale = obj.transform.localScale * scaleFactor;
        if (obj.transform.parent != null && obj.transform.parent.GetComponent<SpawnableProperties>() != null)
        {
            Vector3 parentScale = obj.transform.parent.localScale;
            finalScale = new Vector3(finalScale.x / parentScale.x, finalScale.y / parentScale.y, finalScale.z / parentScale.z);
        }
        obj.transform.localScale = finalScale;

        if (!props.disableRandomRotation)
        {
            if (props.allowedRotations != null && props.allowedRotations.Length > 0)
                obj.transform.localEulerAngles = props.allowedRotations[Random.Range(0, props.allowedRotations.Length)];
            else 
                obj.transform.localEulerAngles = new Vector3(obj.transform.localEulerAngles.x, Random.Range(0f, 360f), obj.transform.localEulerAngles.z);
        }

        if (props.autoCalculateSpans)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                Bounds totalBounds = colliders[0].bounds;
                foreach (Collider col in colliders) totalBounds.Encapsulate(col.bounds);

                props.rowSpan = Mathf.Max(1, Mathf.CeilToInt(totalBounds.size.z / zSpacing));
                float laneWidth = Mathf.Abs(laneXPositions[1] - laneXPositions[0]);
                props.laneSpan = Mathf.Max(1, Mathf.CeilToInt(totalBounds.size.x / laneWidth));
            }
        }
    }
}