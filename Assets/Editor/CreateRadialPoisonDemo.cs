#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public static class CreateRadialPoisonDemo
{
    [MenuItem("Tools/Create Radial Poison Demo and Attach to Player")]
    public static void CreateDemo()
    {
        // Ensure paths
        string matPath = "Assets/Materials/M_RadialPoison.mat";
        string mainTexPath = "Assets/Materials/T_MainRadial.png";
        string blurTexPath = "Assets/Materials/T_BlurRadial.png";
        string prefabPath = "Assets/Prefabs/RadialPoisonDemo.prefab";

        // create folder structure
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // create simple textures if not exist
        Texture2D mainTex = AssetDatabase.LoadAssetAtPath<Texture2D>(mainTexPath);
        if (mainTex == null)
        {
            int size = 256;
            mainTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            // radial gradient light center -> slightly darker edges
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = Mathf.Sqrt(2f) * (size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    float t = d / maxDist;
                    float v = Mathf.Lerp(0.95f, 0.7f, t); // center bright, edges darker
                    mainTex.SetPixel(x, y, new Color(v, v, v, 1f));
                }
            }
            mainTex.Apply();
            byte[] bytes = mainTex.EncodeToPNG();
            System.IO.File.WriteAllBytes(mainTexPath, bytes);
            AssetDatabase.ImportAsset(mainTexPath);
            mainTex = AssetDatabase.LoadAssetAtPath<Texture2D>(mainTexPath);
        }

        Texture2D blurTex = AssetDatabase.LoadAssetAtPath<Texture2D>(blurTexPath);
        if (blurTex == null)
        {
            // create a blurred copy of mainTex using a simple separable box blur
            int w = mainTex.width;
            int h = mainTex.height;
            Color[] src = mainTex.GetPixels();
            Color[] temp = new Color[src.Length];
            Color[] dst = new Color[src.Length];

            int radius = Mathf.Max(2, w / 32); // adaptive radius

            // horizontal pass
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color sum = Color.clear;
                    int count = 0;
                    int x0 = Mathf.Max(0, x - radius);
                    int x1 = Mathf.Min(w - 1, x + radius);
                    for (int xi = x0; xi <= x1; xi++)
                    {
                        sum += src[y * w + xi];
                        count++;
                    }
                    temp[y * w + x] = sum / (float)count;
                }
            }

            // vertical pass
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Color sum = Color.clear;
                    int count = 0;
                    int y0 = Mathf.Max(0, y - radius);
                    int y1 = Mathf.Min(h - 1, y + radius);
                    for (int yi = y0; yi <= y1; yi++)
                    {
                        sum += temp[yi * w + x];
                        count++;
                    }
                    dst[y * w + x] = sum / (float)count;
                }
            }

            blurTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            blurTex.SetPixels(dst);
            blurTex.Apply();
            byte[] bytes = blurTex.EncodeToPNG();
            System.IO.File.WriteAllBytes(blurTexPath, bytes);
            AssetDatabase.ImportAsset(blurTexPath);
            blurTex = AssetDatabase.LoadAssetAtPath<Texture2D>(blurTexPath);
        }

        // create material
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            Shader sh = Shader.Find("UI/RadialRevealBlur");
            if (sh == null)
            {
                Debug.LogError("Shader 'UI/RadialRevealBlur' not found. Ensure the shader file is in the project.");
                return;
            }
            mat = new Material(sh);
            AssetDatabase.CreateAsset(mat, matPath);
        }

        // assign textures into material
        mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        mat.SetTexture("_MainTex", mainTex);
        mat.SetTexture("_BlurTex", blurTex);
        mat.SetFloat("_Softness", 0.08f);
        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();

        // create prefab (Canvas + RawImage)
        GameObject prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabGO == null)
        {
            GameObject canvasGO = new GameObject("RadialCanvas_Demo");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject rawGO = new GameObject("RadialRawImage_Demo");
            rawGO.transform.SetParent(canvasGO.transform, false);
            var raw = rawGO.AddComponent<RawImage>();
            raw.material = mat;
            raw.texture = mainTex;

            // make fullscreen by default
            var rt = raw.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Save as prefab
            prefabGO = PrefabUtility.SaveAsPrefabAsset(canvasGO, prefabPath);
            Object.DestroyImmediate(canvasGO);
        }

        // instantiate prefab in scene and attach to first PlayerPoison found
    var playerPoison = Object.FindFirstObjectByType<PlayerPoison>();
        if (playerPoison == null)
        {
            Debug.LogWarning("No PlayerPoison component found in the scene. Prefab created but not attached.");
            AssetDatabase.Refresh();
            return;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefabGO) as GameObject;
        if (instance != null)
        {
            instance.name = "RadialCanvas_Runtime_Instant";
            instance.transform.SetParent(playerPoison.transform, false);
            // find RawImage component
            var rawImg = instance.GetComponentInChildren<RawImage>();
            if (rawImg != null)
            {
                playerPoison.radialRawImage = rawImg;
                playerPoison.radialMaterial = mat;
                // ensure the RawImage texture is set so the UI doesn't override material _MainTex
                rawImg.texture = mainTex;
                EditorUtility.SetDirty(playerPoison);
                Debug.Log("Radial demo instantiated and attached to PlayerPoison.");
            }
            else
            {
                Debug.LogWarning("Prefab instantiated but no RawImage found inside it.");
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        AssetDatabase.Refresh();
        Debug.Log("Created radial demo assets at: " + matPath + " and " + prefabPath);
    }
}
#endif
