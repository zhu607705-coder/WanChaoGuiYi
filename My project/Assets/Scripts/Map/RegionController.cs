using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WanChaoGuiYi
{
    public sealed class RegionController : MonoBehaviour
    {
        [SerializeField] private string regionId;

        private GameManager gameManager;
        private GameObject terrainShadowObject;
        private GameObject selectedElevationObject;
        private Coroutine focusPulseRoutine;
        private bool selected;
        private Vector3 selectedElevationBaseScale = Vector3.one;
        private Color selectedElevationBaseColor = new Color(1f, 0.82f, 0.32f, 0.26f);

        public string RegionId { get { return regionId; } }

        public void Bind(string id, GameManager manager)
        {
            regionId = id;
            gameManager = manager;
        }

        public void BindTerrainVisuals(GameObject terrainShadow, GameObject selectedElevation)
        {
            terrainShadowObject = terrainShadow;
            selectedElevationObject = selectedElevation;
            if (selectedElevationObject != null)
            {
                selectedElevationBaseScale = selectedElevationObject.transform.localScale;
                MeshRenderer meshRenderer = selectedElevationObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null && meshRenderer.material != null)
                {
                    selectedElevationBaseColor = meshRenderer.material.color;
                }
            }
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            this.selected = selected;
            if (selectedElevationObject != null)
            {
                if (!selected && focusPulseRoutine != null)
                {
                    StopCoroutine(focusPulseRoutine);
                    focusPulseRoutine = null;
                    ResetSelectedElevationVisual();
                }

                selectedElevationObject.SetActive(selected);
            }
        }

        public void PlayFocusPulse()
        {
            if (selectedElevationObject == null) return;

            if (focusPulseRoutine != null)
            {
                StopCoroutine(focusPulseRoutine);
                focusPulseRoutine = null;
            }

            selectedElevationObject.SetActive(true);
            ApplyFocusPulseVisual(1f);
            focusPulseRoutine = StartCoroutine(FocusPulseRoutine());
        }

        public bool HasTerrainShadowVisual { get { return terrainShadowObject != null; } }
        public bool HasSelectedElevationVisual { get { return selectedElevationObject != null; } }

        private IEnumerator FocusPulseRoutine()
        {
            const float duration = 1.1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float normalized = Mathf.Clamp01(elapsed / duration);
                float wave = Mathf.Sin((1f - normalized) * Mathf.PI * 0.5f);
                ApplyFocusPulseVisual(wave);
                elapsed += Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }

            focusPulseRoutine = null;
            ResetSelectedElevationVisual();
            if (selectedElevationObject != null)
            {
                selectedElevationObject.SetActive(selected);
            }
        }

        private void ApplyFocusPulseVisual(float intensity)
        {
            if (selectedElevationObject == null) return;

            selectedElevationObject.transform.localScale = selectedElevationBaseScale * Mathf.Lerp(1f, 1.055f, intensity);
            MeshRenderer meshRenderer = selectedElevationObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null || meshRenderer.material == null) return;

            Color focusColor = new Color(1f, 0.92f, 0.42f, 0.58f);
            meshRenderer.material.color = Color.Lerp(selectedElevationBaseColor, focusColor, intensity);
        }

        private void ResetSelectedElevationVisual()
        {
            if (selectedElevationObject == null) return;

            selectedElevationObject.transform.localScale = selectedElevationBaseScale;
            MeshRenderer meshRenderer = selectedElevationObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = selectedElevationBaseColor;
            }
        }

        private void OnMouseDown()
        {
            if (gameManager == null || string.IsNullOrEmpty(regionId)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            gameManager.Events.Publish(new GameEvent(GameEventType.RegionSelected, regionId, this));
        }
    }
}
