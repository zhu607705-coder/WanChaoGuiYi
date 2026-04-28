using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class MapRenderer : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private readonly Dictionary<string, RegionController> controllers = new Dictionary<string, RegionController>();

        public void RegisterRegion(RegionController controller)
        {
            if (controller == null || string.IsNullOrEmpty(controller.RegionId)) return;
            controllers[controller.RegionId] = controller;
        }

        public void Refresh()
        {
            if (gameManager == null || gameManager.State == null) return;

            foreach (RegionState region in gameManager.State.regions)
            {
                RegionController controller;
                if (!controllers.TryGetValue(region.id, out controller)) continue;
                ApplyOwnerColor(controller, region.ownerFactionId);
            }
        }

        private static void ApplyOwnerColor(RegionController controller, string ownerFactionId)
        {
            SpriteRenderer spriteRenderer = controller.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            int hash = ownerFactionId != null ? ownerFactionId.GetHashCode() : 0;
            float hue = Mathf.Abs(hash % 360) / 360f;
            spriteRenderer.color = Color.HSVToRGB(hue, 0.45f, 0.85f);
        }
    }
}
