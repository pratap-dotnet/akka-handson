using System.IO;
using Akka.Actor;

namespace file_actors
{
    public class FileValidatorActor : UntypedActor
    {
        private readonly IActorRef consoleWriterActor;
        
        public FileValidatorActor(IActorRef consoleWriterActor)
        {
            this.consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                consoleWriterActor.Tell(new NullInputError("Input was blank, Please try again \n"));
                Sender.Tell(new ContinueProcessing());
            }else
            {
                if (IsFileUri(msg))
                {
                    consoleWriterActor.Tell(new InputSuccess(
                        string.Format("Starting processing for {0}", msg)));
                    
                    // start coordinator
                    Context.ActorSelection("akka://MyActorSystem/user/tailCoordinatorActor")
                        .Tell(new TailCoordinatorActor.StartTail(msg, consoleWriterActor));
                }
                else
                {
                    consoleWriterActor.Tell(new ValidationError(
                        string.Format("{0} is not an existing URI on disk.", msg)));
                }
                Sender.Tell(new ContinueProcessing());
            }
        }

        private bool IsFileUri(string msg)
        {
            return File.Exists(msg);
        }
    }
}
