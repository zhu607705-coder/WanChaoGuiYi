using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class MapSetup : MonoBehaviour
    {
        private const string GeneratedMapResourcePath = "Map/jiuzhou_generated_map";
        private const string MapRenderMetadataResourcePath = "Data/map_render_metadata";

        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MapRenderer mapRenderer;
        [SerializeField] private GameObject regionNodePrefab;
        [SerializeField] private Material regionSurfaceMaterial;
        [SerializeField] private bool allowNodeFallback;
        [SerializeField] private bool useGeneratedMapBackground = true;
        [SerializeField] private bool hideRegionSurfaceVisualsWhenBackgroundPresent = true;

        [Header("Layout")]
        [SerializeField] private float nodeSpacing = 2.5f;
        [SerializeField] private Vector2 mapOffset = Vector2.zero;
        [SerializeField] private float labelDepthOffset = -0.05f;
        [SerializeField] private int labelSortingOrderOffset = 1000;
        [SerializeField] private Vector2 generatedMapCenter = new Vector2(-4.55f, 0.7f);
        [SerializeField] private float generatedMapScale = 0.58f;
        [SerializeField] private Vector2 generatedMapShapeCenter = new Vector2(-3.65f, 0.05f);
        [SerializeField] private float regionProjectionScale = 0.4988f;
        [SerializeField] private float generatedMapSpritePixelsPerUnit = 100f;

        private Transform regionLayerRoot;
        private bool generatedMapBackgroundLoaded;

        public bool UsedGeneratedMapResource { get; private set; }
        public bool UsedMapRenderMetadataResource { get; private set; }
        public bool AllowNodeFallback { get { return allowNodeFallback; } }
        public int RegionSurfaceCount { get; private set; }
        public int LegacyNodeFallbackCount { get; private set; }
        public int MissingUsableShapeCount { get; private set; }
        public int MeshBuildFailureCount { get; private set; }

        public void Bind(GameManager manager, MapRenderer renderer)
        {
            gameManager = manager;
            mapRenderer = renderer;
        }

        private void Start()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }

            if (mapRenderer == null)
            {
                mapRenderer = FindObjectOfType<MapRenderer>();
            }

            if (gameManager == null || !gameManager.Data.IsLoaded) return;

            BuildMainMap();
        }

        public void BuildMapNodes()
        {
            BuildMainMap();
        }

        public void BuildMainMap()
        {
            if (gameManager == null || gameManager.Data == null) return;

            ClearExistingNodes();
            if (mapRenderer != null)
            {
                mapRenderer.ClearRegions();
            }

            UsedGeneratedMapResource = false;
            UsedMapRenderMetadataResource = false;
            RegionSurfaceCount = 0;
            LegacyNodeFallbackCount = 0;
            MissingUsableShapeCount = 0;
            MeshBuildFailureCount = 0;
            ApplyMapRenderMetadata();
            generatedMapBackgroundLoaded = CreateGeneratedMapBackground();
            regionLayerRoot = CreateRegionLayerRoot(generatedMapBackgroundLoaded);
            if (mapRenderer != null)
            {
                mapRenderer.SetHideMeshSurfacesInGovernanceLens(generatedMapBackgroundLoaded && hideRegionSurfaceVisualsWhenBackgroundPresent);
            }
            Dictionary<string, Vector2> positions = allowNodeFallback ? CalculatePositions() : null;

            foreach (RegionDefinition region in gameManager.Data.Regions.Values)
            {
                MapRegionShapeDefinition shape;
                if (gameManager.Data.TryGetMapRegionShapeByRegionId(region.id, out shape) && IsUsableShape(shape))
                {
                    CreateRegionSurface(region, shape);
                    continue;
                }

                MissingUsableShapeCount++;
                if (!allowNodeFallback)
                {
                    Debug.LogError("Missing usable map region shape for " + region.id + ". Enable node fallback only for debug scenes.");
                    continue;
                }

                Vector2 pos = Vector2.zero;
                if (positions != null)
                {
                    positions.TryGetValue(region.id, out pos);
                }

                Debug.LogWarning("Using legacy node fallback for region " + region.id + ".");
                LegacyNodeFallbackCount++;
                CreateRegionNode(region, pos);
            }

            if (mapRenderer != null)
            {
                mapRenderer.Refresh();
            }
        }

        private static bool IsUsableShape(MapRegionShapeDefinition shape)
        {
            return shape != null && shape.center != null && shape.boundary != null && shape.boundary.Length >= 3;
        }

        private Dictionary<string, Vector2> CalculatePositions()
        {
            Dictionary<string, Vector2> positions = new Dictionary<string, Vector2>();

            // 基于九州历史地理的近似坐标布局
            // 参照中国地图大致方位：x 为东西，y 为南北
            positions["guanzhong"] = new Vector2(-2f, 1f);
            positions["longxi"] = new Vector2(-4f, 2f);
            positions["hedong"] = new Vector2(0f, 2f);
            positions["hanzhong"] = new Vector2(-3f, 0f);
            positions["bashu"] = new Vector2(-4f, -1f);
            positions["jingxiang"] = new Vector2(-1f, -1f);
            positions["zhongyuan"] = new Vector2(1f, 1f);
            positions["qilu"] = new Vector2(3f, 2f);
            positions["youyan"] = new Vector2(2f, 4f);
            positions["bingzhou"] = new Vector2(-1f, 3.5f);
            positions["liangzhou"] = new Vector2(-5f, 3f);
            positions["huainan"] = new Vector2(2f, 0f);
            positions["jiangdong"] = new Vector2(3f, -1f);
            positions["minyue"] = new Vector2(4f, -2f);
            positions["lingnan"] = new Vector2(1f, -3f);
            positions["yun_gui"] = new Vector2(-2f, -3f);
            positions["liaodong"] = new Vector2(4f, 4f);

            // 应用缩放和偏移
            Dictionary<string, Vector2> scaled = new Dictionary<string, Vector2>();
            foreach (var kvp in positions)
            {
                scaled[kvp.Key] = kvp.Value * nodeSpacing + mapOffset;
            }

            return scaled;
        }

        private void CreateRegionNode(RegionDefinition region, Vector2 position)
        {
            GameObject node;
            if (regionNodePrefab != null)
            {
                node = Instantiate(regionNodePrefab, transform, false);
            }
            else
            {
                node = new GameObject("Region_" + region.id);
                node.transform.SetParent(transform, false);

                // 添加 SpriteRenderer 作为默认可视化
                SpriteRenderer sr = node.AddComponent<SpriteRenderer>();
                sr.sprite = CreateCircleSprite();
                sr.color = Color.white;
            }

            node.layer = gameObject.layer;
            node.transform.localPosition = new Vector3(position.x, position.y, 0f);

            // 设置 RegionController
            RegionController controller = node.GetComponent<RegionController>();
            if (controller == null)
            {
                controller = node.AddComponent<RegionController>();
            }

            controller.Bind(region.id, gameManager);

            // 注册到 MapRenderer
            if (mapRenderer != null)
            {
                mapRenderer.RegisterRegion(controller);
            }

            // 添加区域名称标签
            CreateRegionLabel(node, region.name, new Vector2(0f, -0.6f));
        }

        private void CreateRegionSurface(RegionDefinition region, MapRegionShapeDefinition shape)
        {
            GameObject surface = new GameObject("RegionSurface_" + region.id);
            surface.layer = gameObject.layer;
            surface.transform.SetParent(regionLayerRoot != null ? regionLayerRoot : transform, false);
            surface.transform.localPosition = Vector3.zero;
            surface.transform.localRotation = Quaternion.identity;
            surface.transform.localScale = Vector3.one;

            Mesh mesh = RegionMeshBuilder.Build(shape);
            if (mesh == null)
            {
                MeshBuildFailureCount++;
                if (allowNodeFallback)
                {
                    Vector2 fallbackPosition = shape.center != null ? ToVector2(shape.center) : Vector2.zero;
                    Debug.LogWarning("Using legacy node fallback because mesh build failed for region " + region.id + ".");
                    LegacyNodeFallbackCount++;
                    CreateRegionNode(region, fallbackPosition);
                    return;
                }

                Debug.LogError("Failed to build region mesh for " + region.id + ".");
                return;
            }

            MeshFilter meshFilter = surface.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            MeshRenderer meshRenderer = surface.AddComponent<MeshRenderer>();
            meshRenderer.material = ResolveRegionSurfaceMaterial();
            meshRenderer.sortingOrder = shape.renderOrder;
            if (generatedMapBackgroundLoaded && hideRegionSurfaceVisualsWhenBackgroundPresent)
            {
                meshRenderer.enabled = false;
            }

            MeshCollider meshCollider = surface.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            RegionController controller = surface.AddComponent<RegionController>();
            controller.Bind(region.id, gameManager);

            if (mapRenderer != null)
            {
                mapRenderer.RegisterRegion(controller);
            }

            RegionSurfaceCount++;

            Vector2 labelPosition = shape.center != null ? ToVector2(shape.center) : Vector2.zero;
            if (shape.labelOffset != null)
            {
                labelPosition += ToVector2(shape.labelOffset);
            }

            if (!generatedMapBackgroundLoaded)
            {
                CreateRegionLabel(surface, region.name, labelPosition, shape.renderOrder + labelSortingOrderOffset);
            }
        }

        private void CreateRegionLabel(GameObject parent, string name, Vector2 localPosition)
        {
            CreateRegionLabel(parent, name, localPosition, labelSortingOrderOffset);
        }

        private void CreateRegionLabel(GameObject parent, string name, Vector2 localPosition, int sortingOrder)
        {
            GameObject labelObj = new GameObject("Label_" + name);
            labelObj.layer = parent.layer;
            labelObj.transform.SetParent(parent.transform);
            labelObj.transform.localPosition = new Vector3(localPosition.x, localPosition.y, labelDepthOffset);

            TextMesh textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = name;
            textMesh.characterSize = 0.15f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 24;
            textMesh.color = Color.white;

            MeshRenderer labelRenderer = labelObj.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
            {
                labelRenderer.sortingOrder = sortingOrder;
            }
        }

        private Material ResolveRegionSurfaceMaterial()
        {
            if (regionSurfaceMaterial != null)
            {
                return new Material(regionSurfaceMaterial);
            }

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            return new Material(shader);
        }

        private bool CreateGeneratedMapBackground()
        {
            if (!useGeneratedMapBackground) return false;

            Sprite sprite = LoadGeneratedMapSprite();
            if (sprite == null) return false;

            GameObject background = new GameObject("Generated_Jiuzhou_Map_Background_Runtime");
            background.layer = gameObject.layer;
            background.transform.SetParent(transform, false);
            background.transform.localPosition = new Vector3(generatedMapCenter.x, generatedMapCenter.y, 0.2f);
            background.transform.localScale = Vector3.one * generatedMapScale;

            SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -30;
            renderer.color = Color.white;
            return true;
        }

        private Transform CreateRegionLayerRoot(bool projectToGeneratedMap)
        {
            GameObject layer = new GameObject("RegionInteractionLayer");
            layer.layer = gameObject.layer;
            layer.transform.SetParent(transform, false);
            if (projectToGeneratedMap)
            {
                Vector2 projectedOrigin = generatedMapCenter - generatedMapShapeCenter * regionProjectionScale;
                layer.transform.localPosition = new Vector3(projectedOrigin.x, projectedOrigin.y, 0f);
                layer.transform.localScale = Vector3.one * regionProjectionScale;
            }
            else
            {
                layer.transform.localPosition = Vector3.zero;
                layer.transform.localScale = Vector3.one;
            }

            return layer.transform;
        }

        private Sprite LoadGeneratedMapSprite()
        {
            Sprite resourceSprite = Resources.Load<Sprite>(GeneratedMapResourcePath);
            if (resourceSprite != null)
            {
                UsedGeneratedMapResource = true;
                return resourceSprite;
            }

            Texture2D resourceTexture = Resources.Load<Texture2D>(GeneratedMapResourcePath);
            if (resourceTexture != null)
            {
                UsedGeneratedMapResource = true;
                return CreateGeneratedMapSprite(resourceTexture);
            }

            string path = Path.Combine(Application.dataPath, "Art", "Map", "jiuzhou_generated_map.png");
            if (!File.Exists(path)) return null;

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.name = "jiuzhou_generated_map";
            if (!texture.LoadImage(bytes)) return null;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            return CreateGeneratedMapSprite(texture);
        }

        private Sprite CreateGeneratedMapSprite(Texture2D texture)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), generatedMapSpritePixelsPerUnit);
            sprite.name = texture.name;
            return sprite;
        }

        private void ApplyMapRenderMetadata()
        {
            MapRenderMetadata metadata = LoadMapRenderMetadata();
            if (metadata == null) return;

            if (metadata.shapeCenter != null)
            {
                generatedMapShapeCenter = ToVector2(metadata.shapeCenter);
            }

            if (metadata.spritePixelsPerUnit > 0f)
            {
                generatedMapSpritePixelsPerUnit = metadata.spritePixelsPerUnit;
            }

            if (metadata.pixelsPerShapeUnit > 0f && generatedMapSpritePixelsPerUnit > 0f)
            {
                regionProjectionScale = generatedMapScale * metadata.pixelsPerShapeUnit / generatedMapSpritePixelsPerUnit;
            }
        }

        private MapRenderMetadata LoadMapRenderMetadata()
        {
            TextAsset resource = Resources.Load<TextAsset>(MapRenderMetadataResourcePath);
            if (resource != null)
            {
                UsedMapRenderMetadataResource = true;
                return ParseMapRenderMetadata(resource.text, "Resources/" + MapRenderMetadataResourcePath);
            }

            string path = Path.Combine(Application.dataPath, "Data", "map_render_metadata.json");
            if (!File.Exists(path)) return null;

            return ParseMapRenderMetadata(File.ReadAllText(path), path);
        }

        private static MapRenderMetadata ParseMapRenderMetadata(string json, string source)
        {
            try
            {
                return JsonUtility.FromJson<MapRenderMetadata>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to load map render metadata from " + source + ": " + ex.Message);
                return null;
            }
        }

        private static Vector2 ToVector2(MapPoint point)
        {
            return new Vector2(point.x, point.y);
        }

        private void ClearExistingNodes()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private static Sprite CreateCircleSprite()
        {
            // 创建一个简单的圆形纹理
            int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        [Serializable]
        private sealed class MapRenderMetadata
        {
            public MapPoint shapeCenter;
            public float pixelsPerShapeUnit;
            public float spritePixelsPerUnit;
        }
    }
}
