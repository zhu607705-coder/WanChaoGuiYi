using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WanChaoGuiYi.EditorTools
{
    public static class DemoSceneEntitySetup
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.scene";
        private const string PreviewRootName = "EditorVisibleEntityPreview";
        private const string MapSpritePath = "Assets/Art/Map/jiuzhou_generated_map.png";
        private const string MapShapesPath = "Assets/Data/map_region_shapes.json";

        private static readonly string[] EmperorIds =
        {
            "qin_shi_huang",
            "liu_bang",
            "han_wu_di",
            "cao_cao",
            "li_shi_min",
            "zhao_kuang_yin",
            "zhu_yuan_zhang",
            "kang_xi"
        };

        private static readonly string[] EmperorLabels =
        {
            "秦始皇",
            "刘邦",
            "汉武帝",
            "曹操",
            "李世民",
            "赵匡胤",
            "朱元璋",
            "康熙"
        };

        private static readonly Vector3 MapPanelCenter = new Vector3(-4.55f, 0.7f, 0.18f);
        private static readonly Vector3 MapPanelScale = new Vector3(11.95f, 8.95f, 1f);
        private static readonly Vector3 EmperorPanelCenter = new Vector3(5.7f, 0.7f, 0.12f);
        private static readonly Vector3 EmperorPanelScale = new Vector3(5.15f, 8.95f, 1f);
        private static readonly Vector2 RegionToMapScale = new Vector2(0.46f, 0.46f);

        [MenuItem("WanChaoGuiYi/Build Editor Visible Entity Preview")]
        public static void BuildEditorVisibleEntityPreview()
        {
            EditorSceneManager.OpenScene(ScenePath);
            ConfigureSpriteImporter(MapSpritePath, 100f);
            ConfigurePortraitImporters();
            ConfigureUnitImporters();
            AssetDatabase.Refresh();

            GameObject oldRoot = GameObject.Find(PreviewRootName);
            if (oldRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(oldRoot);
            }

            GameObject root = new GameObject(PreviewRootName);
            CreateMapBackground(root.transform);
            CreateCourtPortraitPanel(root.transform);
            CreateArmyMarkers(root.transform);
            CreateSceneCamera();

            EditorSceneManager.MarkSceneDirty(root.scene);
            EditorSceneManager.SaveScene(root.scene);
            AssetDatabase.SaveAssets();
        }

        [InitializeOnLoadMethod]
        private static void BuildPreviewAfterScriptReload()
        {
            if (Application.isBatchMode || SessionState.GetBool("WanChaoGuiYi.EntityPreviewBuilt", false)) return;
            SessionState.SetBool("WanChaoGuiYi.EntityPreviewBuilt", true);

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
                BuildEditorVisibleEntityPreview();
            };
        }

        private static void CreateMapBackground(Transform root)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MapSpritePath);
            if (sprite == null) return;

            CreatePanel(root, "Map_Frame_Shadow", MapPanelCenter + new Vector3(0.14f, -0.14f, 0.08f), MapPanelScale + new Vector3(0.28f, 0.28f, 0f), new Color(0.015f, 0.012f, 0.01f, 0.82f), -42);
            CreatePanel(root, "Map_Frame_Backplate", MapPanelCenter, MapPanelScale + new Vector3(0.16f, 0.16f, 0f), new Color(0.38f, 0.23f, 0.08f, 0.96f), -41);
            CreatePanel(root, "Map_Inner_Backplate", MapPanelCenter + new Vector3(0f, -0.04f, -0.01f), MapPanelScale, new Color(0.09f, 0.065f, 0.04f, 0.96f), -40);

            GameObject map = CreateSpriteObject("Generated_Jiuzhou_Map_Background", sprite, MapPanelCenter + new Vector3(0f, -0.04f, -0.08f), -30, 0.58f, root);
            SpriteRenderer renderer = map.GetComponent<SpriteRenderer>();
            renderer.color = new Color(0.98f, 0.95f, 0.86f, 1f);

            CreateLabel(root, "万朝归一 · 九州战略图", new Vector3(MapPanelCenter.x, 5.95f, -0.2f), 80, 0.25f, new Color(1f, 0.88f, 0.55f, 1f));
            CreateLabel(root, "底图 / 区域 / 军队实体分层显示", new Vector3(MapPanelCenter.x, -4.72f, -0.2f), 79, 0.12f, new Color(0.9f, 0.78f, 0.5f, 1f));
            CreateStatusStrip(root);
        }

        private static void CreateStatusStrip(Transform root)
        {
            CreatePanel(root, "Entity_Layer_Status_Strip", new Vector3(0.45f, -5.42f, 0.04f), new Vector3(16.55f, 0.58f, 1f), new Color(0.06f, 0.045f, 0.032f, 0.93f), 65);
            CreatePanel(root, "Entity_Layer_Status_Trim", new Vector3(0.45f, -5.11f, 0.02f), new Vector3(16.55f, 0.035f, 1f), new Color(0.82f, 0.56f, 0.2f, 0.9f), 66);
            CreateLabel(root, "实体预览层：生成地图作为主舞台，军队落在区域中心，帝皇立绘独立成右侧军帐卡片。", new Vector3(0.45f, -5.42f, -0.08f), 82, 0.095f, new Color(0.94f, 0.86f, 0.68f, 1f));
        }

        private static void CreateCourtPortraitPanel(Transform root)
        {
            CreatePanel(root, "Emperor_Command_Panel_Shadow", EmperorPanelCenter + new Vector3(0.12f, -0.12f, 0.05f), EmperorPanelScale + new Vector3(0.22f, 0.22f, 0f), new Color(0.012f, 0.009f, 0.006f, 0.85f), 8);
            CreatePanel(root, "Emperor_Command_Panel", EmperorPanelCenter, EmperorPanelScale, new Color(0.075f, 0.048f, 0.03f, 0.95f), 10);
            CreatePanel(root, "Emperor_Command_Panel_Header", new Vector3(EmperorPanelCenter.x, 4.72f, 0f), new Vector3(4.58f, 0.72f, 1f), new Color(0.23f, 0.13f, 0.045f, 0.96f), 18);
            CreateLabel(root, "帝皇军帐", new Vector3(EmperorPanelCenter.x, 4.84f, -0.12f), 85, 0.17f, new Color(1f, 0.83f, 0.45f, 1f));
            CreateLabel(root, "核心八帝 · 开局势力", new Vector3(EmperorPanelCenter.x, 4.53f, -0.12f), 84, 0.09f, new Color(0.92f, 0.76f, 0.5f, 1f));

            for (int i = 0; i < EmperorIds.Length; i++)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Portraits/" + EmperorIds[i] + ".png");
                if (sprite == null) continue;

                int column = i % 2;
                int row = i / 2;
                Vector3 cardCenter = new Vector3(4.52f + column * 2.36f, 3.48f - row * 1.55f, -0.02f);
                CreatePortraitCard(root, EmperorIds[i], EmperorLabels[i], sprite, cardCenter);
            }
        }

        private static void CreatePortraitCard(Transform root, string emperorId, string label, Sprite sprite, Vector3 cardCenter)
        {
            CreatePanel(root, "Card_Back_" + emperorId, cardCenter, new Vector3(2.04f, 1.22f, 1f), new Color(0.14f, 0.085f, 0.045f, 0.96f), 20);
            CreatePanel(root, "Card_Trim_" + emperorId, cardCenter + new Vector3(0f, 0.41f, -0.01f), new Vector3(1.92f, 0.035f, 1f), new Color(0.82f, 0.56f, 0.2f, 0.92f), 22);
            CreatePanel(root, "Card_Portrait_Field_" + emperorId, cardCenter + new Vector3(-0.52f, 0.04f, -0.02f), new Vector3(0.7f, 0.76f, 1f), new Color(0.46f, 0.28f, 0.1f, 0.86f), 23);
            GameObject portrait = CreateSpriteObject("Preview_Emperor_" + emperorId, sprite, cardCenter + new Vector3(-0.52f, 0.04f, -0.08f), 32, 0.16f, root);
            portrait.transform.localScale = new Vector3(0.16f, 0.16f, 1f);

            CreateLabel(root, label, cardCenter + new Vector3(0.42f, 0.14f, -0.1f), 42, 0.095f, Color.white);
            CreateLabel(root, "独立立绘", cardCenter + new Vector3(0.42f, -0.18f, -0.1f), 41, 0.065f, new Color(0.86f, 0.72f, 0.52f, 1f));
        }

        private static void CreateArmyMarkers(Transform root)
        {
            MapShapeCollection shapes = LoadMapShapes();
            CreateArmyMarker(root, shapes, "guanzhong", "infantry", "秦军 3000", true, 0);
            CreateArmyMarker(root, shapes, "hedong", "infantry", "敌军 2600", false, 0);
            CreateArmyMarker(root, shapes, "zhongyuan", "cavalry", "骑兵 1200", true, 0);
            CreateArmyMarker(root, shapes, "jingxiang", "river_navy", "舟师 900", false, 0);
            CreateArmyMarker(root, shapes, "youyan", "frontier_cavalry", "边骑 1500", false, 0);
        }

        private static void CreateArmyMarker(Transform root, MapShapeCollection shapes, string regionId, string unitId, string label, bool player, int stackIndex)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Icons/Units/" + unitId + ".png");
            if (sprite == null) return;

            Vector2 center = ResolveRegionCenter(shapes, regionId);
            Vector2 mapPosition = ProjectRegionCenterToMap(center);
            Vector3 position = new Vector3(mapPosition.x + stackIndex * 0.18f, mapPosition.y + 0.28f, -0.2f);
            GameObject halo = CreatePanel(root, "Army_Halo_" + regionId, new Vector3(position.x, position.y, -0.15f), new Vector3(0.62f, 0.38f, 1f), player ? new Color(0.1f, 0.55f, 0.24f, 0.72f) : new Color(0.72f, 0.18f, 0.12f, 0.72f), 38);
            GameObject icon = CreateSpriteObject("Preview_Army_" + regionId, sprite, position, 45, 0.23f, root);
            icon.transform.SetParent(halo.transform, true);
            CreateLabel(root, label, new Vector3(position.x, position.y - 0.3f, -0.24f), 48, 0.065f, Color.white);
        }

        private static Vector2 ProjectRegionCenterToMap(Vector2 regionCenter)
        {
            return new Vector2(
                MapPanelCenter.x + regionCenter.x * RegionToMapScale.x,
                MapPanelCenter.y + regionCenter.y * RegionToMapScale.y);
        }

        private static GameObject CreateSpriteObject(string objectName, Sprite sprite, Vector3 position, int sortingOrder, float scale, Transform parent)
        {
            GameObject gameObject = new GameObject(objectName);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = position;
            gameObject.transform.localScale = Vector3.one * scale;

            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return gameObject;
        }

        private static GameObject CreatePanel(Transform root, string objectName, Vector3 position, Vector3 scale, Color color, int sortingOrder)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            gameObject.name = objectName;
            gameObject.transform.SetParent(root, false);
            gameObject.transform.localPosition = position;
            gameObject.transform.localScale = scale;
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            renderer.sharedMaterial.color = color;
            renderer.sortingOrder = sortingOrder;
            return gameObject;
        }

        private static void CreateLabel(Transform parent, string label, Vector3 localPosition, int sortingOrder, float size, Color color)
        {
            GameObject labelObject = new GameObject("Label_" + label);
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = localPosition;

            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = label;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = size;
            textMesh.fontSize = 32;
            textMesh.color = color;

            MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrder;
            }
        }

        private static void CreateSceneCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            camera.orthographic = true;
            camera.orthographicSize = 6.45f;
            camera.transform.position = new Vector3(0.75f, -0.05f, -18f);
            camera.backgroundColor = new Color(0.045f, 0.04f, 0.035f, 1f);
        }

        private static void ConfigurePortraitImporters()
        {
            foreach (string emperorId in EmperorIds)
            {
                ConfigureSpriteImporter("Assets/Art/Portraits/" + emperorId + ".png", 512f);
            }
        }

        private static void ConfigureUnitImporters()
        {
            ConfigureSpriteImporter("Assets/Art/Icons/Units/infantry.png", 128f);
            ConfigureSpriteImporter("Assets/Art/Icons/Units/cavalry.png", 128f);
            ConfigureSpriteImporter("Assets/Art/Icons/Units/river_navy.png", 128f);
            ConfigureSpriteImporter("Assets/Art/Icons/Units/frontier_cavalry.png", 128f);
        }

        private static void ConfigureSpriteImporter(string path, float pixelsPerUnit)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        private static MapShapeCollection LoadMapShapes()
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(MapShapesPath);
            return asset == null ? new MapShapeCollection() : JsonUtility.FromJson<MapShapeCollection>(asset.text);
        }

        private static Vector2 ResolveRegionCenter(MapShapeCollection shapes, string regionId)
        {
            if (shapes != null && shapes.items != null)
            {
                foreach (MapRegionShape shape in shapes.items)
                {
                    if (shape != null && shape.regionId == regionId && shape.center != null)
                    {
                        return new Vector2(shape.center.x, shape.center.y);
                    }
                }
            }

            return Vector2.zero;
        }

        [Serializable]
        private sealed class MapShapeCollection
        {
            public MapRegionShape[] items;
        }

        [Serializable]
        private sealed class MapRegionShape
        {
            public string regionId;
            public MapPoint center;
        }

        [Serializable]
        private sealed class MapPoint
        {
            public float x;
            public float y;
        }
    }
}
