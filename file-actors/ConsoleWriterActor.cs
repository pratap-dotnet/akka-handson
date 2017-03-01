using System;
using Akka.Actor;

namespace file_actors
{

    public class ConsoleWriterActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if (message is InputError)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(((InputError)message).Reason);
            }
            else if (message is InputSuccess)
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
