using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class TurnManager : MonoBehaviour
    {
        private readonly List<IGameSystem> systems = new List<IGameSystem>();
        private GameContext context;

        public void Configure(GameContext gameContext, IEnumerable<IGameSystem> gameSystems)
        {
            context = gameContext;
            systems.Clear();
            systems.AddRange(gameSystems);

            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Initialize(context);
            }
        }

        public void AdvanceTurn()
        {
            if (context == null)
            {
                Debug.LogWarning("TurnManager has no GameContext.");
                return;
            }

            context.Events.Publish(new GameEvent(GameEventType.TurnStarted, string.Empty, context.State.turn));

            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].OnTurnStart(context);
            }

            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].ExecuteTurn(context);
            }

            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].OnTurnEnd(context);
            }

            context.State.AddLog("turn", "第 " + context.State.turn + " 回合结算完成。");
            context.Events.Publish(new GameEvent(GameEventType.TurnEnded, string.Empty, context.State.turn));
            context.State.AdvanceHalfYear();
        }
    }
}
