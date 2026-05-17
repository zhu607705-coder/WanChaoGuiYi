using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: My previous EventBus order test only
    /// covered "subscribe A,B,C; unsubscribe B; expect A,C". That case
    /// happens to pass because Delegate.Remove walks from the end and
    /// rebuilds the invocation list, which preserves order in this
    /// specific shape. But MulticastDelegate is documented as having
    /// no order guarantee across multiple Remove calls AND across
    /// re-additions of the SAME listener instance.
    ///
    /// This more rigorous test:
    ///   subscribe A,B,C,D
    ///   unsubscribe B and D
    ///   subscribe E
    ///   resubscribe B (now at the end?)
    ///   expect: A, C, E, B  (in registration-after-removal order)
    ///
    /// In the current Action<GameEvent>+= / -= implementation, the
    /// resubscribed B usually lands at the end, and order generally
    /// holds. But the fragility is documented as "implementation
    /// detail". A real game cannot rely on it.
    ///
    /// Pinned invariant: after these operations, the call order must
    /// be exactly [A, C, E, B]. If today's MulticastDelegate happens
    /// to produce that, fine. If a future refactor breaks it, this
    /// test red-flags it.
    /// </summary>
    public sealed class EventBusListenerOrderRigorousBugTests
    {
        private readonly ITestOutputHelper output;

        public EventBusListenerOrderRigorousBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Order_Must_Be_Preserved_Across_Multiple_Unsub_And_Resub()
        {
            EventBus bus = new EventBus();
            List<string> calls = new List<string>();

            Action<GameEvent> a = _ => calls.Add("A");
            Action<GameEvent> b = _ => calls.Add("B");
            Action<GameEvent> c = _ => calls.Add("C");
            Action<GameEvent> d = _ => calls.Add("D");
            Action<GameEvent> e = _ => calls.Add("E");

            bus.Subscribe(GameEventType.TurnStarted, a);
            bus.Subscribe(GameEventType.TurnStarted, b);
            bus.Subscribe(GameEventType.TurnStarted, c);
            bus.Subscribe(GameEventType.TurnStarted, d);

            bus.Unsubscribe(GameEventType.TurnStarted, b);
            bus.Unsubscribe(GameEventType.TurnStarted, d);
            bus.Subscribe(GameEventType.TurnStarted, e);
            bus.Subscribe(GameEventType.TurnStarted, b);

            bus.Publish(new GameEvent(GameEventType.TurnStarted, "t1", null));

            output.WriteLine("call order: " + string.Join(",", calls));

            // Registration-after-removal order: A and C remained;
            // E and B were added afterwards.  So [A, C, E, B].
            Assert.Equal(new[] { "A", "C", "E", "B" }, calls.ToArray());
        }

        [Fact]
        public void Same_Listener_Subscribed_Twice_Should_Not_Fire_Twice()
        {
            EventBus bus = new EventBus();
            int hits = 0;
            Action<GameEvent> handler = _ => hits++;

            bus.Subscribe(GameEventType.TurnStarted, handler);
            bus.Subscribe(GameEventType.TurnStarted, handler);

            bus.Publish(new GameEvent(GameEventType.TurnStarted, "t1", null));

            output.WriteLine("hits: " + hits);

            // Today: hits == 2, because Action+= just stacks. A subscriber
            // that registered itself twice (e.g. on two scenes both
            // initialising) will receive duplicate callbacks and double
            // every state effect. Either Subscribe must dedupe or the
            // invariant must be documented (and Unsubscribe must be
            // documented to remove only one). The pin: dedupe.
            Assert.Equal(1, hits);
        }
    }
}
