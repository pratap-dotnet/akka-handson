using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;

namespace charting_actors.Actors
{
    public class PerformanceCounterActor : ReceiveActor
    {
        private readonly string _seriesName;
        private readonly Func<PerformanceCounter> _performanceCounterGenerator;
        private PerformanceCounter _counter;

        private readonly HashSet<IActorRef> _subscriptions;
        private readonly ICancelable _cancelPublishing;

        public PerformanceCounterActor(string seriesName, 
            Func<PerformanceCounter> performanceCounterGenerator)
        {
            _seriesName = seriesName;
            _performanceCounterGenerator = performanceCounterGenerator;

            _subscriptions = new HashSet<IActorRef>();
            _cancelPublishing = new Cancelable(Context.System.Scheduler);

            Receive<GatherMetrics>((m) =>
            {
                var metric = new Metric(_seriesName, _counter.NextValue());
                foreach (var sub in _subscriptions)
                {
                    sub.Tell(metric);
                }
            });

            Receive<SubscribeCounter>(sc =>
            {
                _subscriptions.Add(sc.Subscriber);
            });

            Receive<UnsubscribeCounter>(uc =>
            {
                _subscriptions.Remove(uc.Subscriber);
            });
        }

        protected override void PreStart()
        {
            _counter = _performanceCounterGenerator();
            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.FromMilliseconds(250),
                TimeSpan.FromMilliseconds(250),
                Self,
                new GatherMetrics(),
                Self, _cancelPublishing);
        }

        protected override void PostStop()
        {
            try
            {
                _cancelPublishing.Cancel(false);
                _counter.Dispose();
            }
            catch
            {
            }
            finally
            {
                base.PostStop();
            }
        }
        
    }
}
