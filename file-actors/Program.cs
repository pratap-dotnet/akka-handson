using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace file_actors
{
    class Program
    {
        static void Main(string[] args)
        {
            ActorSystem actorSystem = ActorSystem.Create("MyActorSystem");

            var tailCoordinatorProps = Props.Create(() => new TailCoordinatorActor());
            var tailCoordinatorActor = actorSystem.ActorOf(tailCoordinatorProps, "tailCoordinatorActor");

            var consoleWriterProps = Props.Create(() =>
                new ConsoleWriterActor());
            var consoleWriterActor = actorSystem.ActorOf(consoleWriterProps, "consoleWriterActor");

            var validationActorProps = Props.Create(() =>
                new FileValidatorActor(consoleWriterActor, tailCoordinatorActor));
            var validationActor = actorSystem.ActorOf(validationActorProps, "validationActor");

            Props consoleReaderProps = Props.Create<ConsoleReaderActor>(validationActor);
            var consoleReaderActor = actorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");

            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            actorSystem.WhenTerminated.Wait();
        }
    }
}
