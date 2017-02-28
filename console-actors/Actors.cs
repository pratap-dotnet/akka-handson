using Akka.Actor;
using System;

namespace console_actors
{
    public class ConsoleReaderActor : UntypedActor
    {
        private readonly IActorRef consoleWriterActor;
        private readonly string ExitCommand = "Exit";
        public const string StartCommand = "Start";

        public ConsoleReaderActor(IActorRef consoleWriterActor)
        {
            this.consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            if (message.Equals(StartCommand))
            {
                DoPrintInstructions();
            }
            else if (message is InputError)
            {
                consoleWriterActor.Tell(message as InputError);
            }
            GetAndValidateInput();
        }

        private void GetAndValidateInput()
        {
            var message = Console.ReadLine();
            if (string.IsNullOrEmpty(message))
            {
                Self.Tell(new NullInputError("No input received"));
            }
            else if (string.Equals(message, ExitCommand, StringComparison.OrdinalIgnoreCase))
            {
                Context.System.Terminate();
            }
            else
            {
                var valid = IsValid(message);
                if (valid)
                {
                    consoleWriterActor.Tell(new InputSuccess("Thank you! Message was valid."));
                    Self.Tell(new ContinueProcessing());
                }
                else
                {
                    Self.Tell(new ValidationError("Invalid: input had odd number of characters."));
                }
            }
        }

        private bool IsValid(string message)
        {
            return message.Length % 2 == 0;
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
}
