using UnityEngine;

namespace WanChaoGuiYi
{
    internal static class UIPanelVisibility
    {
        public static void Show(GameObject root)
        {
            SetVisible(root, true);
        }

        public static void Hide(GameObject root)
        {
            SetVisible(root, false);
        }

        private static void SetVisible(GameObject root, bool visible)
        {
            if (root == null) return;

            if (!root.activeSelf)
            {
                root.SetActive(true);
            }

            CanvasGroup group = root.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = root.AddComponent<CanvasGroup>();
            }

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
