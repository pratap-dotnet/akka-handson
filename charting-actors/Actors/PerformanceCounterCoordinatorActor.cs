using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;

namespace charting_actors.Actors
{
    public class PerformanceCounterCoordinatorActor : ReceiveActor
    {
        #region Message Types
        public class Watch
        {
            public CounterType Counter { get; private set; }
            public Watch(CounterType counter)
            {
                Counter = counter;
            }
        }

        public class Unwatch
        {
            public CounterType Counter { get; private set; }
            public Unwatch(CounterType counter)
            {
                Counter = counter;
            }
        }
        #endregion 

        private static readonly Dictionary<CounterType, Func<PerformanceCounter>> CounterGenerators
            = new Dictionary<CounterType, Func<PerformanceCounter>>
            {
                { CounterType.Cpu, () => new PerformanceCounter("Processor", "% Processor Time", "_Total", true)},
                { CounterType.Disk, () => new PerformanceCounter("LogicalDisk", "% Disk Time", "_Total", true)},
                { CounterType.Memory, () => new PerformanceCounter("Memory", "% Committed Bytes In Use", true)}
            };

        private static readonly Dictionary<CounterType, Func<Series>> CounterSeries =
            new Dictionary<CounterType, Func<Series>>()
        {
            {CounterType.Cpu, () =>
            new Series(CounterType.Cpu.ToString()){
                 ChartType = SeriesChartType.SplineArea,
                 Color = Color.DarkGreen}},
            {CounterType.Memory, () =>
            new Series(CounterType.Memory.ToString()){
                ChartType = SeriesChartType.FastLine,
                Color = Color.MediumBlue}},
            {CounterType.Disk, () =>
            new Series(CounterType.Disk.ToString()){
                ChartType = SeriesChartType.SplineArea,
                Color = Color.DarkRed}},
        };

        private Dictionary<CounterType, IActorRef> _counterActors;
        private IActorRef _chartingActor;
        
        public PerformanceCounterCoordinatorActor(IActorRef chartingActor)
            : this(chartingActor, new Dictionary<CounterType,IActorRef>())
        {

        }

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor, Dictionary<CounterType, IActorRef> dictionary)
        {
            _counterActors = dictionary;
            _chartingActor = chartingActor;

            Receive<Unwatch>(uw => {
                if (!_counterActors.ContainsKey(uw.Counter))
                {
                    return;
                }

                _counterActors[uw.Counter].Tell(new UnsubscribeCounter(uw.Counter, _chartingActor));
                _chartingActor.Tell(new ChartingActor.RemoveSeries(uw.Counter.ToString()));
            });

            Receive<Watch>(arg => {
                if (!_counterActors.ContainsKey(arg.Counter))
                {
                    var counterActor = Context.ActorOf(Props.Create(() =>
                        new PerformanceCounterActor(arg.Counter.ToString(), CounterGenerators[arg.Counter]))
                        .WithDispatcher("akka.actor.synchronized-dispatcher"));
                    _counterActors.Add(arg.Counter, counterActor);
                }
                _chartingActor.Tell(new ChartingActor.AddSeries(CounterSeries[arg.Counter]()));
                _counterActors[arg.Counter].Tell(new SubscribeCounter(arg.Counter, _chartingActor));
            });
        }
    }
}
