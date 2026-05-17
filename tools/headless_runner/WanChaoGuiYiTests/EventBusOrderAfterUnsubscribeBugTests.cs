using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: EventBus uses
    ///     existing -= listener;
    /// to unsubscribe. MulticastDelegate.Remove removes the LAST
    /// matching invocation, and the resulting delegate's invocation
    /// list is *recreated*. Listener registration order is therefore
    /// not preserved across an unsubscribe in the middle.
    ///
    /// In game flow this matters because UI listeners (which build
    /// transient overlay state from a payload) often need to run
    /// AFTER all Domain listeners have mutated state.  The
    /// Subscribe/Unsubscribe order is the only contract — there is
    /// no priority field on Subscribe.
    ///
    /// Pinned invariant: after subscribing A, B, C and then
    /// unsubscribing B, publishing the event must still call A
    /// before C.
    /// </summary>
    public sealed class EventBusOrderAfterUnsubscribeBugTests
    {
        private readonly ITestOutputHelper output;

        public EventBusOrderAfterUnsubscribeBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Subscribe_Order_Must_Be_Preserved_After_MidList_Unsubscribe()
        {
            EventBus bus = new EventBus();
            List<string> calls = new List<string>();

            Action<GameEvent> a = _ => calls.Add("A");
            Action<GameEvent> b = _ => calls.Add("B");
            Action<GameEvent> c = _ => calls.Add("C");

            bus.Subscribe(GameEventType.TurnStarted, a);
            bus.Subscribe(GameEventType.TurnStarted, b);
            bus.Subscribe(GameEventType.TurnStarted, c);

            bus.Unsubscribe(GameEventType.TurnStarted, b);

            bus.Publish(new GameEvent(GameEventType.TurnStarted, "t1", null));

            output.WriteLine("call order: " + string.Join(",", calls));

            Assert.Equal(new[] { "A", "C" }, calls.ToArray());
        }
    }
}
