using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class DemoRuntimeHud : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
        }

        private void OnGUI()
        {
            if (gameManager == null || gameManager.State == null) return;

            GUILayout.BeginArea(new Rect(12f, 12f, 360f, 120f), GUI.skin.box);
            GUILayout.Label($"Turn {gameManager.State.turn} / {gameManager.State.season}");
            FactionState playerFaction = gameManager.State.FindFaction(gameManager.State.playerFactionId);
            if (playerFaction != null)
            {
                GUILayout.Label($"Legitimacy {playerFaction.legitimacy}  Money {playerFaction.money}");
            }
            if (GUILayout.Button("Next Turn"))
            {
                gameManager.NextTurn();
            }
            GUILayout.EndArea();
        }
    }
}
