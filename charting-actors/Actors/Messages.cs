using Akka.Actor;

namespace charting_actors.Actors
{
    public class GatherMetrics { }

    public class Metric
    {
        public Metric(string series, float counterValue)
        {
            Series = series; CounterValue = counterValue;
        }
        public float CounterValue { get; private set; }
        public string Series { get; private set; }
    }
    
    public enum CounterType
    {
        Cpu, Memory, Disk
    }

    public class SubscribeCounter
    {
        public CounterType Counter { get; private set; }
        public IActorRef Subscriber { get; private set; }

        public SubscribeCounter(CounterType counter, IActorRef subscriber)
        {
            Subscriber = subscriber; Counter = counter;
        }
    }

    public class UnsubscribeCounter
    {
        public CounterType Counter { get; private set; }
        public IActorRef Subscriber { get; private set; }

        public UnsubscribeCounter(CounterType counter, IActorRef subscriber)
        {
            Subscriber = subscriber; Counter = counter;
        }

    }
}
