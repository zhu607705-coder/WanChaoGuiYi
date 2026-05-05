using UnityEngine;

namespace WanChaoGuiYi
{
    public enum MapZoomBand
    {
        Detail,
        Operation,
        Overview
    }

    public sealed class CameraController : MonoBehaviour
    {
        private const string GeneratedMapBackgroundName = "Generated_Jiuzhou_Map_Background_Runtime";

        [Header("Pan")]
        [SerializeField] private float panSpeed = 10f;
        [SerializeField] private float edgePanThreshold = 20f;
        [SerializeField] private bool edgePanEnabled = true;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 15f;
        [SerializeField] private float detailZoomMax = 6f;
        [SerializeField] private float operationZoomMax = 10f;

        [Header("Bounds")]
        [SerializeField] private bool useBounds;
        [SerializeField] private bool autoUseGeneratedMapBounds = true;
        [SerializeField] private float minX = -15f;
        [SerializeField] private float maxX = 15f;
        [SerializeField] private float minY = -10f;
        [SerializeField] private float maxY = 15f;

        private Camera cam;
        private Vector3 dragOrigin;
        private bool isDragging;
        private bool autoBoundsConfigured;
        private MapZoomBand currentZoomBand = MapZoomBand.Operation;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
        }

        private void Start()
        {
            TryConfigureGeneratedMapBounds();
            RefreshZoomBand();
            ClampPosition();
        }

        private void Update()
        {
            TryConfigureGeneratedMapBounds();
            HandleKeyboardPan();
            HandleEdgePan();
            HandleMouseDrag();
            HandleZoom();
            ClampPosition();
        }

        private void HandleKeyboardPan()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if (Mathf.Abs(horizontal) < 0.01f && Mathf.Abs(vertical) < 0.01f) return;

            Vector3 move = new Vector3(horizontal, vertical, 0f).normalized;
            transform.position += move * (panSpeed * Time.deltaTime);
        }

        private void HandleEdgePan()
        {
            if (!edgePanEnabled) return;

            Vector3 mousePos = Input.mousePosition;
            Vector3 move = Vector3.zero;

            if (mousePos.x < edgePanThreshold) move.x = -1f;
            else if (mousePos.x > Screen.width - edgePanThreshold) move.x = 1f;

            if (mousePos.y < edgePanThreshold) move.y = -1f;
            else if (mousePos.y > Screen.height - edgePanThreshold) move.y = 1f;

            if (move.sqrMagnitude > 0.01f)
            {
                transform.position += move.normalized * (panSpeed * Time.deltaTime);
            }
        }

        private void HandleMouseDrag()
        {
            ResolveCamera();
            if (cam == null) return;

            if (Input.GetMouseButtonDown(2))
            {
                dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
                isDragging = true;
            }

            if (Input.GetMouseButton(2) && isDragging)
            {
                Vector3 current = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3 diff = dragOrigin - current;
                transform.position += diff;
            }

            if (Input.GetMouseButtonUp(2))
            {
                isDragging = false;
            }
        }

        private void HandleZoom()
        {
            ResolveCamera();
            if (cam == null) return;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) < 0.01f) return;

            SetZoom(cam.orthographicSize - scroll * zoomSpeed);
        }

        private void ClampPosition()
        {
            if (!useBounds) return;

            Vector3 pos = transform.position;
            ResolveCamera();
            if (cam != null && cam.orthographic)
            {
                float halfHeight = Mathf.Max(0.01f, cam.orthographicSize);
                float halfWidth = halfHeight * Mathf.Max(0.01f, cam.aspect);
                pos.x = ClampAxisToViewport(pos.x, minX, maxX, halfWidth);
                pos.y = ClampAxisToViewport(pos.y, minY, maxY, halfHeight);
            }
            else
            {
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
            }

            transform.position = pos;
        }

        private static float ClampAxisToViewport(float value, float minimum, float maximum, float viewportHalfSize)
        {
            float lower = Mathf.Min(minimum, maximum);
            float upper = Mathf.Max(minimum, maximum);
            if (upper - lower <= viewportHalfSize * 2f)
            {
                return (lower + upper) * 0.5f;
            }

            return Mathf.Clamp(value, lower + viewportHalfSize, upper - viewportHalfSize);
        }

        private void TryConfigureGeneratedMapBounds()
        {
            if (!autoUseGeneratedMapBounds || useBounds || autoBoundsConfigured) return;

            GameObject background = GameObject.Find(GeneratedMapBackgroundName);
            if (background == null) return;

            SpriteRenderer spriteRenderer = background.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            Bounds bounds = spriteRenderer.bounds;
            if (bounds.size.x <= Mathf.Epsilon || bounds.size.y <= Mathf.Epsilon) return;

            ConfigureBounds(Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y));
            autoBoundsConfigured = true;
        }

        private void ResolveCamera()
        {
            if (cam != null) return;
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
        }

        public bool UseBounds { get { return useBounds; } }
        public float CurrentZoom { get { ResolveCamera(); return cam != null ? cam.orthographicSize : 0f; } }
        public MapZoomBand CurrentZoomBand { get { ResolveCamera(); RefreshZoomBand(); return currentZoomBand; } }
        public Rect WorldBounds { get { return Rect.MinMaxRect(minX, minY, maxX, maxY); } }

        public void ConfigureBounds(Rect worldBounds)
        {
            if (worldBounds.width <= Mathf.Epsilon || worldBounds.height <= Mathf.Epsilon) return;

            minX = Mathf.Min(worldBounds.xMin, worldBounds.xMax);
            maxX = Mathf.Max(worldBounds.xMin, worldBounds.xMax);
            minY = Mathf.Min(worldBounds.yMin, worldBounds.yMax);
            maxY = Mathf.Max(worldBounds.yMin, worldBounds.yMax);
            useBounds = true;
            autoBoundsConfigured = true;
            ClampPosition();
        }

        public void ConfigureZoomLimits(float minimumZoom, float maximumZoom)
        {
            float safeMinimum = Mathf.Max(0.01f, Mathf.Min(minimumZoom, maximumZoom));
            float safeMaximum = Mathf.Max(safeMinimum, Mathf.Max(minimumZoom, maximumZoom));
            minZoom = safeMinimum;
            maxZoom = safeMaximum;

            ResolveCamera();
            if (cam != null)
            {
                SetZoom(cam.orthographicSize);
            }
        }

        public void SetZoom(float zoom)
        {
            ResolveCamera();
            if (cam == null) return;

            cam.orthographicSize = Mathf.Clamp(zoom, minZoom, maxZoom);
            RefreshZoomBand();
            ClampPosition();
        }

        public void ConfigureZoomBands(float detailMax, float operationMax)
        {
            detailZoomMax = Mathf.Max(0.01f, Mathf.Min(detailMax, operationMax));
            operationZoomMax = Mathf.Max(detailZoomMax, Mathf.Max(detailMax, operationMax));
            RefreshZoomBand();
        }

        private void RefreshZoomBand()
        {
            if (cam == null) return;

            float zoom = cam.orthographicSize;
            if (zoom <= detailZoomMax)
            {
                currentZoomBand = MapZoomBand.Detail;
            }
            else if (zoom <= operationZoomMax)
            {
                currentZoomBand = MapZoomBand.Operation;
            }
            else
            {
                currentZoomBand = MapZoomBand.Overview;
            }
        }

        public void ClampToBounds()
        {
            ClampPosition();
        }

        public void CenterOnRegion(Vector2 worldPosition)
        {
            Vector3 target = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
            transform.position = target;
            ClampPosition();
        }
    }
}
