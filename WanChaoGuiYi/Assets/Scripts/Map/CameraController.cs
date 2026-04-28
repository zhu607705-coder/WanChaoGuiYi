using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class CameraController : MonoBehaviour
    {
        [Header("Pan")]
        [SerializeField] private float panSpeed = 10f;
        [SerializeField] private float edgePanThreshold = 20f;
        [SerializeField] private bool edgePanEnabled = true;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 15f;

        [Header("Bounds")]
        [SerializeField] private bool useBounds;
        [SerializeField] private float minX = -15f;
        [SerializeField] private float maxX = 15f;
        [SerializeField] private float minY = -10f;
        [SerializeField] private float maxY = 15f;

        private Camera cam;
        private Vector3 dragOrigin;
        private bool isDragging;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
        }

        private void Update()
        {
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
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) < 0.01f) return;

            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }

        private void ClampPosition()
        {
            if (!useBounds) return;

            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }

        public void CenterOnRegion(Vector2 worldPosition)
        {
            Vector3 target = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
            transform.position = target;
        }
    }
}
