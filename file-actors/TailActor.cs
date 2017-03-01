using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace file_actors
{
    public class TailActor : UntypedActor
    {
        private readonly string _filePath;
        private readonly IActorRef _reporterActor;
        private readonly FileObserver _observer;
        private readonly Stream _fileStream;
        private readonly StreamReader _fileStreamReader;

        public TailActor(IActorRef reporterActor, string filePath)
        {
            _reporterActor = reporterActor;
            _filePath = filePath;

            _observer = new FileObserver(Self, filePath);
            _observer.Start();

            _fileStream = new FileStream(Path.GetFullPath(_filePath),
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        protected override void OnReceive(object message)
        {
            if(message is FileWrite)
            {
                var text = _fileStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(text))
                {
                    _reporterActor.Tell(text);
                }
            }
            else if(message is FileError)
            {
                _reporterActor.Tell($"Tail error: {(message as FileError).Reason}");
            }
            else if(message is InitialRead)
            {
                _reporterActor.Tell((message as InitialRead).Text);
            }
        }

        public class FileWrite
        {
            public FileWrite(string fileName)
            {
                FileName = fileName;
            }
            public string FileName { get; private set; }
        }

        public class FileError
        {
            public FileError(string fileName, string reason)
            {
                FileName = fileName;
                Reason = reason;
            }

            public string FileName { get; private set; }

            public string Reason { get; private set; }
        }

        public class InitialRead
        {
            public InitialRead(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }

            public string FileName { get; private set; }
            public string Text { get; private set; }
        }

    }
}
