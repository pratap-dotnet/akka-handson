using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace console_actors
{
    class Program
    {
        static void Main(string[] args)
        {
            ActorSystem actorSystem = ActorSystem.Create("ConsoleActorSystem");

            var consoleWriterActor = actorSystem.ActorOf(Props.Create(() =>
                new ConsoleWriterActor()));
            var consoleReaderActor = actorSystem.ActorOf(Props.Create(() =>
                new ConsoleReaderActor(consoleWriterActor)));

            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            actorSystem.WhenTerminated.Wait();
        }
    }
}
