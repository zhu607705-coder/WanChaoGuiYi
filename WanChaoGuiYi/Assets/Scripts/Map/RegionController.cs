using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class RegionController : MonoBehaviour
    {
        [SerializeField] private string regionId;

        private GameManager gameManager;

        public string RegionId { get { return regionId; } }

        public void Bind(string id, GameManager manager)
        {
            regionId = id;
            gameManager = manager;
        }

        private void OnMouseDown()
        {
            if (gameManager == null || string.IsNullOrEmpty(regionId)) return;
            gameManager.Events.Publish(new GameEvent(GameEventType.RegionSelected, regionId, this));
        }
    }
}
