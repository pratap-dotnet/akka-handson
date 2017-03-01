using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace file_actors
{
    public class ConsoleReaderActor : UntypedActor
    {
        private readonly IActorRef validationActor;
        private readonly string ExitCommand = "Exit";
        public const string StartCommand = "Start";

        public ConsoleReaderActor(IActorRef validationActor)
        {
            this.validationActor = validationActor;
        }

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
            validationActor.Tell(message);
        }

        private void DoPrintInstructions()
        {
            Console.WriteLine("Please provide the URI of a log file on disk.\n");
        }
    }
}
