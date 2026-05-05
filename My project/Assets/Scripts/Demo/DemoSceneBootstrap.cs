using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class DemoSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private bool buildOnAwake = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureBootstrapObject()
        {
            if (FindObjectOfType<DemoSceneBootstrap>() != null) return;

            GameObject bootstrapObject = new GameObject("DemoSceneBootstrap");
            bootstrapObject.AddComponent<DemoSceneBootstrap>();
        }

        private void Awake()
        {
            if (!buildOnAwake) return;

            DisableEditorPreviewLayer();
            GameManager gameManager = EnsureGameManager();
            MapRenderer mapRenderer = EnsureMapRenderer(gameManager);
            EnsureMapSetup(gameManager, mapRenderer);
            EnsureEntityVisuals(gameManager);
            EnsureCamera();
            EnsureUI(gameManager);
        }

        private static void DisableEditorPreviewLayer()
        {
            GameObject preview = GameObject.Find("EditorVisibleEntityPreview");
            if (preview != null)
            {
                preview.SetActive(false);
            }
        }

        private GameManager EnsureGameManager()
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null) return gameManager;

            GameObject gameObject = new GameObject("GameManager");
            gameManager = gameObject.AddComponent<GameManager>();
            return gameManager;
        }

        private MapRenderer EnsureMapRenderer(GameManager gameManager)
        {
            MapRenderer mapRenderer = FindObjectOfType<MapRenderer>();
            if (mapRenderer != null) return mapRenderer;

            GameObject gameObject = new GameObject("MapRenderer");
            mapRenderer = gameObject.AddComponent<MapRenderer>();
            mapRenderer.Bind(gameManager);
            return mapRenderer;
        }

        private void EnsureMapSetup(GameManager gameManager, MapRenderer mapRenderer)
        {
            MapSetup mapSetup = FindObjectOfType<MapSetup>();
            if (mapSetup == null)
            {
                GameObject gameObject = new GameObject("MapRoot");
                mapSetup = gameObject.AddComponent<MapSetup>();
            }

            mapSetup.Bind(gameManager, mapRenderer);
        }

        private static void EnsureEntityVisuals(GameManager gameManager)
        {
            DemoEntityVisualSpawner spawner = FindObjectOfType<DemoEntityVisualSpawner>();
            if (spawner == null)
            {
                GameObject gameObject = new GameObject("EntityVisuals");
                spawner = gameObject.AddComponent<DemoEntityVisualSpawner>();
            }

            spawner.Bind(gameManager);
        }

        private static void EnsureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            camera.orthographic = true;
            camera.orthographicSize = 6.45f;
            camera.transform.position = new Vector3(0.75f, -0.05f, -10f);

            if (camera.GetComponent<CameraController>() == null)
            {
                camera.gameObject.AddComponent<CameraController>();
            }
        }

        private static void EnsureUI(GameManager gameManager)
        {
            UISetup uiSetup = FindObjectOfType<UISetup>();
            if (uiSetup == null)
            {
                GameObject uiObject = new GameObject("UISetup");
                uiSetup = uiObject.AddComponent<UISetup>();
            }

            uiSetup.Bind(gameManager);
        }
    }
}
