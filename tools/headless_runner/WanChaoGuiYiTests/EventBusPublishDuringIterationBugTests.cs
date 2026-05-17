using System;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: EventBus is now backed by List<Action>.
    /// If a listener calls Subscribe or Unsubscribe during a Publish
    /// (which is realistic — a battle event handler may register a
    /// follow-up handler for occupation aftercare), the iterating
    /// list mutates mid-loop. Without snapshotting, this throws
    /// InvalidOperationException("Collection was modified") and
    /// kills the publish chain.
    ///
    /// Pinned invariant: a listener that subscribes a new handler
    /// during publish must not crash the publish, and the new
    /// handler should NOT receive the in-flight event (it must take
    /// effect on the NEXT publish).
    /// </summary>
    public sealed class EventBusPublishDuringIterationBugTests
    {
        private readonly ITestOutputHelper output;

        public EventBusPublishDuringIterationBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Subscribing_During_Publish_Must_Not_Crash_Or_Reach_New_Handler()
        {
            EventBus bus = new EventBus();
            int newHandlerHits = 0;

            Action<GameEvent> firstHandler = _ =>
            {
                bus.Subscribe(GameEventType.TurnStarted, __ => newHandlerHits++);
            };
            bus.Subscribe(GameEventType.TurnStarted, firstHandler);

            // First publish: firstHandler fires once, registers a new
            // handler. The new handler must NOT also fire in this
            // same publish.
            Exception thrown = null;
            try { bus.Publish(new GameEvent(GameEventType.TurnStarted, "t1", null)); }
            catch (Exception ex) { thrown = ex; }

            output.WriteLine("first publish threw? " + (thrown != null));
            output.WriteLine("first publish new-handler hits: " + newHandlerHits);

            Assert.Null(thrown);
            Assert.Equal(0, newHandlerHits);

            // Second publish: now both handlers fire. firstHandler
            // registers ANOTHER new handler. After this publish,
            // newHandlerHits should be 1.
            try { bus.Publish(new GameEvent(GameEventType.TurnStarted, "t2", null)); }
            catch (Exception ex) { thrown = ex; }

            output.WriteLine("second publish threw? " + (thrown != null));
            output.WriteLine("second publish new-handler hits total: " + newHandlerHits);

            Assert.Null(thrown);
            Assert.Equal(1, newHandlerHits);
        }
    }
}
