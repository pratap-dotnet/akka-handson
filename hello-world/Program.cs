using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hello_world
{
    public class Greet
    {
        public Greet(string who)
        {
            Who = who;
        }

        public string Who { get; private set; }
    }

    public class GreetingActor: TypedActor, IHandle<Greet>
    {
        private int count = 0;
        
        public void Handle(Greet message)
        {
            Console.WriteLine($"Hello {message} {count}");
            count++;
        }
    }
    

    class Program
    {
        static void Main(string[] args)
        {
            var system = ActorSystem.Create("HelloWorldSystem");
            var greeter = system.ActorOf<GreetingActor>("greeter");
            greeter.Tell(new Greet("World"));

            Console.WriteLine();
        }
    }
}
