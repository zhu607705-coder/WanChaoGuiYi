using System;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: EventBus.Publish iterates listeners via
    /// MulticastDelegate.Invoke, which surfaces the first thrown
    /// exception and aborts subsequent listener calls in the same
    /// publish.  In a real game, a faulty UI listener would silently
    /// drop downstream economy/war/governance updates.  This test pins
    /// down the desired invariant: a buggy listener must not poison
    /// other listeners.
    /// </summary>
    public sealed class EventBusErrorIsolationBugTests
    {
        private readonly ITestOutputHelper output;

        public EventBusErrorIsolationBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void A_Throwing_Listener_Must_Not_Block_Other_Listeners()
        {
            EventBus bus = new EventBus();
            int secondListenerInvocations = 0;

            bus.Subscribe(GameEventType.TurnStarted, _ =>
            {
                throw new InvalidOperationException("simulated UI bug");
            });
            bus.Subscribe(GameEventType.TurnStarted, _ =>
            {
                secondListenerInvocations++;
            });

            // The current implementation will throw out of Publish.
            // Capture the throw so that the test fails on the assertion
            // about the second listener instead of on the unhandled
            // exception (which would be confusing).
            try
            {
                bus.Publish(new GameEvent(GameEventType.TurnStarted, "turn_1", null));
            }
            catch (Exception ex)
            {
                output.WriteLine("Publish surfaced exception: " + ex.GetType().Name + " - " + ex.Message);
            }

            output.WriteLine("secondListenerInvocations = " + secondListenerInvocations);
            Assert.Equal(1, secondListenerInvocations);
        }
    }
}
