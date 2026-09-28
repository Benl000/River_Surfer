using UnityEngine;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
    [Header("Prefabs Lists")]
    public GameObject[] basePrefabs; 
    public GameObject[] propPrefabs; 

    [Header("Theme Settings")]
    public LevelTheme currentTheme = LevelTheme.Summer; 

    [Header("Grid Settings")]
    public int rowsToSpawn = 15; 
    public float zSpacing = 5f;  
    public float[] laneXPositions = { -5f, -2.5f, 0f, 2.5f, 5f }; 

    private float nextSpawnZ = 10f; 

    void Start()
    {
        for (int i = 0; i < rowsToSpawn; i++)
        {
            SpawnRow();
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

        foreach (float xPos in laneXPositions)
        {
            Vector3 spawnPos = new Vector3(xPos, 0, nextSpawnZ);
            
            GameObject selectedBase = validBases[Random.Range(0, validBases.Count)];
            GameObject newBase = Instantiate(selectedBase, spawnPos, Quaternion.identity, transform);

            ApplyScaleAndRotation(newBase);

            SpawnableProperties baseProps = newBase.GetComponent<SpawnableProperties>();
            
            if (baseProps != null && baseProps.isSurface)
            {
                if (Random.value < 0.8f) 
                {
                    SpawnPropOnSurface(baseProps);
                }
            }
        }
        nextSpawnZ += zSpacing;
    }

    void SpawnPropOnSurface(SpawnableProperties baseProps)
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
            
            // 1. חישוב גובה הגג של הפלטפורמה לפי המודל הויזואלי (Renderer) במקום קוליידר
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
            
            // 2. הפעלת גודל וסיבוב
            ApplyScaleAndRotation(newProp);

            // 3. איתור התחתית המדויקת של החפץ המוגרל לפי המודל הויזואלי
            Renderer[] propRenderers = newProp.GetComponentsInChildren<Renderer>();
            if (propRenderers.Length > 0)
            {
                float minY = propRenderers[0].bounds.min.y;
                foreach (Renderer r in propRenderers)
                {
                    if (r.bounds.min.y < minY) minY = r.bounds.min.y;
                }
                
                // חישוב ההפרש ותיקון מיקום החפץ כך שיעמוד בול על הגג
                float currentY = newProp.transform.position.y;
                float offset = currentY - minY;
                
                Vector3 correctedPos = newProp.transform.position;
                correctedPos.y += offset;
                newProp.transform.position = correctedPos;
            }
        }
    }void ApplyScaleAndRotation(GameObject obj)
    {
        SpawnableProperties props = obj.GetComponent<SpawnableProperties>();
        
        if (props != null)
        {
            // 1. קביעת פקטור הגודל הרצוי (עם או בלי אקראיות)
            float scaleFactor = props.baseScaleMultiplier;
            if (!props.disableRandomScale)
            {
                scaleFactor *= Random.Range(1f - props.randomScaleJitter, 1f + props.randomScaleJitter);
            }

            // 2. חישוב הגודל הסופי ביחס למודל המקורי
            Vector3 finalScale = obj.transform.localScale * scaleFactor;

            // 3. תיקון קריטי: ניטרול השפעת הגודל של פלטפורמת האב!
            if (obj.transform.parent != null)
            {
                Vector3 parentScale = obj.transform.parent.localScale;
                finalScale = new Vector3(
                    finalScale.x / parentScale.x,
                    finalScale.y / parentScale.y,
                    finalScale.z / parentScale.z
                );
            }

            obj.transform.localScale = finalScale;

            // 4. טיפול בסיבוב
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
            // רשת ביטחון: רק סיבוב אקראי אם אין סקריפט
            float randomY = Random.Range(0f, 360f);
            obj.transform.localEulerAngles = new Vector3(obj.transform.localEulerAngles.x, randomY, obj.transform.localEulerAngles.z);
        }
    }
}
    