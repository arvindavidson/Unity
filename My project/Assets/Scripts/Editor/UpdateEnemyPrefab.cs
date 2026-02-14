using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

public class UpdateEnemyPrefab : MonoBehaviour
{
    [MenuItem("Tools/Update Enemy Prefab (Add UI)")]
    public static void UpdatePrefab()
    {
        string prefabPath = "Assets/Prefabs/EnemyAgent.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        if (prefabRoot == null)
        {
            Debug.LogError("Could not load prefab at " + prefabPath);
            return;
        }

        try
        {
            // 1. Check for existing UI
            EnemyStatusUI statusUI = prefabRoot.GetComponentInChildren<EnemyStatusUI>();
            GameObject canvasObj;

            if (statusUI == null)
            {
                // Create new Canvas Object
                canvasObj = new GameObject("EnemyStatusCanvas");
                canvasObj.transform.SetParent(prefabRoot.transform);
                canvasObj.transform.localPosition = new Vector3(0, 2.2f, 0); // Above head
                canvasObj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f); // World Space scaling

                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvasObj.AddComponent<CanvasScaler>(); // Optional
                
                statusUI = canvasObj.AddComponent<EnemyStatusUI>();
                
                // Add Background/Meter
                GameObject meterObj = new GameObject("Meter");
                meterObj.transform.SetParent(canvasObj.transform);
                meterObj.transform.localPosition = Vector3.zero;
                meterObj.transform.localScale = Vector3.one;
                
                Image meterBg = meterObj.AddComponent<Image>();
                meterBg.color = new Color(0,0,0,0.5f);
                meterBg.rectTransform.sizeDelta = new Vector2(100, 100);

                GameObject meterFillObj = new GameObject("MeterFill");
                meterFillObj.transform.SetParent(meterObj.transform);
                meterFillObj.transform.localPosition = Vector3.zero;
                meterFillObj.transform.localScale = Vector3.one;
                
                Image meterFill = meterFillObj.AddComponent<Image>();
                meterFill.color = Color.white;
                meterFill.type = Image.Type.Filled;
                meterFill.fillMethod = Image.FillMethod.Radial360;
                meterFill.fillOrigin = 2; // Top
                meterFill.rectTransform.sizeDelta = new Vector2(100, 100);
                
                statusUI.meterImage = meterFill;

                // Add Icon
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(canvasObj.transform);
                iconObj.transform.localPosition = Vector3.zero;
                iconObj.transform.localScale = Vector3.one;
                
                Image iconImg = iconObj.AddComponent<Image>();
                iconImg.rectTransform.sizeDelta = new Vector2(64, 64);
                
                statusUI.iconImage = iconImg;
                statusUI.canvas = canvas;
            }
            else
            {
                canvasObj = statusUI.gameObject;
            }

            // Assign References with Debugging
            string[] iconPaths = new string[] {
                "Assets/Textures/UI/Icon_Eye.png",
                "Assets/Textures/UI/Icon_Question.png",
                "Assets/Textures/UI/Icon_Exclamation.png",
                "Assets/Textures/UI/Meter_Circle.png"
            };

            Sprite[] loadedSprites = new Sprite[4];

            for (int i = 0; i < iconPaths.Length; i++)
            {
                string path = iconPaths[i];
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    bool changed = false;
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        changed = true;
                    }
                    if (importer.spriteImportMode != SpriteImportMode.Single)
                    {
                        importer.spriteImportMode = SpriteImportMode.Single;
                        changed = true;
                    }
                    
                    if (changed)
                    {
                        Debug.Log("Fixing Importer for " + path);
                        importer.SaveAndReimport();
                    }
                }
                
                loadedSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (loadedSprites[i] == null)
                {
                    Debug.LogError("Failed to load Sprite at " + path + " even after re-import attempt.");
                }
            }

            Sprite eyeParams = loadedSprites[0];
            Sprite questParams = loadedSprites[1];
            Sprite exclamParams = loadedSprites[2];
            Sprite meterSprite = loadedSprites[3];

            Debug.Log($"Loading Sprites... Eye: {eyeParams}, Question: {questParams}, Exclamation: {exclamParams}, Meter: {meterSprite}");

            if (statusUI != null)
            {
                statusUI.eyeIcon = eyeParams;
                statusUI.questionIcon = questParams;
                statusUI.exclamationIcon = exclamParams;
                
                // Update Meter Images in Hierarchy
                // We know structure is Canvas -> Meter (Image) -> MeterFill (Image)
                
                Transform meterTrans = statusUI.transform.Find("Meter");
                if (meterTrans != null)
                {
                    Image meterBg = meterTrans.GetComponent<Image>();
                    if (meterBg != null) {
                         meterBg.sprite = meterSprite;
                         meterBg.color = new Color(0,0,0,0.5f); // Semi-transparent black background
                    }
                    
                    Transform meterFillTrans = meterTrans.Find("MeterFill");
                    if (meterFillTrans != null)
                    {
                        Image meterFill = meterFillTrans.GetComponent<Image>();
                        if (meterFill != null)
                        {
                            meterFill.sprite = meterSprite;
                            // Ensure radial fill settings
                            meterFill.type = Image.Type.Filled;
                            meterFill.fillMethod = Image.FillMethod.Radial360;
                            meterFill.fillOrigin = 2; // Top
                        }
                    }
                }
                
                EditorUtility.SetDirty(statusUI);
            }
            
            // Assign to EnemyAI component on root
            EnemyAI enemyAI = prefabRoot.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.statusUI = statusUI;
                EditorUtility.SetDirty(enemyAI);
            }
            
            // Save Changes
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            Debug.Log("Updated Enemy Prefab with UI at " + prefabPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error updating prefab: " + e.Message);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}
