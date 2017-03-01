using System;
using Akka.Actor;

namespace file_actors
{
    public class ConsoleReaderActor : UntypedActor
    {
        private readonly string ExitCommand = "Exit";
        public const string StartCommand = "Start";
        
        protected override void OnReceive(object message)
        {
            if (message.Equals(StartCommand))
            {
                DoPrintInstructions();
            }
            GetAndValidateInput();
        }

        private void GetAndValidateInput()
        {
            var message = Console.ReadLine();
            if (!string.IsNullOrEmpty(message) && string.Equals(message, ExitCommand, StringComparison.OrdinalIgnoreCase))
            {
                Context.System.Terminate();
                return;
            }
            Context.ActorSelection("akka://MyActorSystem/user/validationActor").Tell(message);
        }

        private void DoPrintInstructions()
        {
            Console.WriteLine("Please provide the URI of a log file on disk.\n");
        }
    }
}
