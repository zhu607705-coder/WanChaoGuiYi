using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class MapSetup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MapRenderer mapRenderer;
        [SerializeField] private GameObject regionNodePrefab;

        [Header("Layout")]
        [SerializeField] private float nodeSpacing = 2.5f;
        [SerializeField] private Vector2 mapOffset = Vector2.zero;

        private readonly Dictionary<string, Vector2> regionPositions = new Dictionary<string, Vector2>();

        private void Start()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }

            if (gameManager == null || !gameManager.Data.IsLoaded) return;

            BuildMapNodes();
        }

        public void BuildMapNodes()
        {
            if (gameManager == null || gameManager.Data == null) return;

            // 清理已有节点
            ClearExistingNodes();

            // 使用手动布局表（如果存在）或自动布局
            Dictionary<string, Vector2> positions = CalculatePositions();

            foreach (RegionDefinition region in gameManager.Data.Regions.Values)
            {
                Vector2 pos;
                if (!positions.TryGetValue(region.id, out pos))
                {
                    pos = Vector2.zero;
                }

                CreateRegionNode(region, pos);
            }

            // 刷新地图渲染
            if (mapRenderer != null)
            {
                mapRenderer.Refresh();
            }
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
                node = Instantiate(regionNodePrefab, transform);
            }
            else
            {
                node = new GameObject("Region_" + region.id);
                node.transform.SetParent(transform);

                // 添加 SpriteRenderer 作为默认可视化
                SpriteRenderer sr = node.AddComponent<SpriteRenderer>();
                sr.sprite = CreateCircleSprite();
                sr.color = Color.white;
            }

            node.transform.position = new Vector3(position.x, position.y, 0f);

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
            CreateRegionLabel(node, region.name, position);
        }

        private void CreateRegionLabel(GameObject parent, string name, Vector2 position)
        {
            GameObject labelObj = new GameObject("Label_" + name);
            labelObj.transform.SetParent(parent.transform);
            labelObj.transform.localPosition = new Vector3(0f, -0.6f, 0f);

            TextMesh textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = name;
            textMesh.characterSize = 0.15f;
            textMesh.anchor = TextAnchor.UpperCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 24;
            textMesh.color = Color.white;
        }

        private void ClearExistingNodes()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            regionPositions.Clear();
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
    }
}
