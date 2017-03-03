using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;
using Akka.Util.Internal;
using charting_actors.Actors;

namespace charting_actors
{
    public partial class Main : Form
    {
        private IActorRef _chartActor;
        private readonly AtomicCounter _seriesCounter = new AtomicCounter(1);
        private IActorRef _coordinatorActor;
        private Dictionary<CounterType, IActorRef> _toggleActors = new Dictionary<CounterType,
            IActorRef>();

        public Main()
        {
            InitializeComponent();
        }

        #region Initialization


        private void Main_Load(object sender, EventArgs e)
        {
            _chartActor = Program.ChartActors.ActorOf
                (Props.Create(() => new ChartingActor(sysChart, btnPauseResume)), "charting");

            _chartActor.Tell(new ChartingActor.InitializeChart(null));

            _coordinatorActor = Program.ChartActors.ActorOf(Props.Create(() =>
                new PerformanceCounterCoordinatorActor(_chartActor))
                .WithDispatcher("akka.actor.synchronized-dispatcher"), "counters");
            
            _toggleActors[CounterType.Cpu] = Program.ChartActors.ActorOf(
                Props.Create(() => new ButtonToggleActor(_coordinatorActor, this.button1,
                CounterType.Cpu, false))
                .WithDispatcher("akka.actor.synchronized-dispatcher"));

            _toggleActors[CounterType.Disk] = Program.ChartActors.ActorOf(
                Props.Create(() => new ButtonToggleActor(_coordinatorActor, this.button3,
                CounterType.Disk, false))
                .WithDispatcher("akka.actor.synchronized-dispatcher"));

            _toggleActors[CounterType.Memory] = Program.ChartActors.ActorOf(
                            Props.Create(() => new ButtonToggleActor(_coordinatorActor, this.button2,
                            CounterType.Memory, false))
                            .WithDispatcher("akka.actor.synchronized-dispatcher"));

            
            _toggleActors[CounterType.Cpu].Tell(new ButtonToggleActor.Toggle());
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //shut down the charting actor
            _chartActor.Tell(PoisonPill.Instance);

            //shut down the ActorSystem
            Program.ChartActors.Terminate();
        }

        #endregion
        

        private void OnCpuClick(object sender, EventArgs e)
        {
            _toggleActors[CounterType.Cpu].Tell(new ButtonToggleActor.Toggle());
        }

        private void OnMemoryClick(object sender, EventArgs e)
        {
            _toggleActors[CounterType.Memory].Tell(new ButtonToggleActor.Toggle());
        }

        private void OnDiskClick(object sender, EventArgs e)
        {
            _toggleActors[CounterType.Disk].Tell(new ButtonToggleActor.Toggle());
        }

        private void btnPauseResume_Click(object sender, EventArgs e)
        {
            _chartActor.Tell(new ChartingActor.TogglePause());
        }
    }
}
