using Akka.Actor;
using System;

namespace console_actors
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
            Console.WriteLine("Write whatever you want into the console!");
            Console.WriteLine("Some entries will pass validation, and some won't...\n\n");
            Console.WriteLine("Type 'exit' to quit this application at any time.\n");
        }
    }

    public class ConsoleWriterActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if(message is InputError)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(((InputError)message).Reason);
            }
            else if(message is InputSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(((InputSuccess)message).Reason);
            }
            else
            {
                Console.WriteLine(message);
            }
            Console.ResetColor();
        }
    }

    public class ValidationActor : UntypedActor
    {
        private readonly IActorRef consoleWriterActor;
        public ValidationActor(IActorRef consoleWriterActor)
        {
            this.consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;

            if (string.IsNullOrEmpty(msg))
            {
                consoleWriterActor.Tell(new InputError("No input received"));
            }
            else
            {
                if (IsValid(msg))
                {
                    consoleWriterActor.Tell(new InputSuccess("Thank you! message was valid"));
                }
                else
                {
                    consoleWriterActor.Tell(new InputError("message was invalid"));
                }
            }
            Sender.Tell(new ContinueProcessing());
        }

        private bool IsValid(string message)
        {
            return message.Length % 2 == 0;
        }
    }
}
