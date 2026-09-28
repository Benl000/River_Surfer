using UnityEngine;
using UnityEditor;

public class ObstacleSetupTool
{
    [MenuItem("Tools/Setup Selected Obstacles")]
    public static void SetupObstacles()
    {
        // מוודא שתיקיית Prefabs קיימת, ואם לא - יוצר אותה
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        int count = 0;

        foreach (GameObject obj in Selection.gameObjects)
        {
            // הגדרת התגית
            obj.tag = "Obstacle";

            // הוספת Mesh Collider במידה ואין
            MeshCollider mc = obj.GetComponent<MeshCollider>();
            if (mc == null)
            {
                mc = obj.AddComponent<MeshCollider>();
            }
            
            // הגדרות הקוליידר
            mc.convex = true;
            mc.isTrigger = true;

            // שמירה כ-Prefab
            string localPath = "Assets/Prefabs/" + obj.name + ".prefab";
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            PrefabUtility.SaveAsPrefabAssetAndConnect(obj, localPath, InteractionMode.UserAction);
            
            count++;
        }

        Debug.Log($"Successfully setup and created {count} Obstacle Prefabs!");
    }
}