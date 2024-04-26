using Akka.Actor;
using Akka.Event;

namespace Asteroids.Shared.Actors
{
    public class DeadletterMonitor : ReceiveActor
    {

        public DeadletterMonitor()
        {
            Receive<DeadLetter>(HandleDeadletter);
        }

        private static void HandleDeadletter(DeadLetter dl) => Console.WriteLine($"DeadLetter captured: {dl.Message}, sender: {dl.Sender}, recipient: {dl.Recipient}");
    }
}