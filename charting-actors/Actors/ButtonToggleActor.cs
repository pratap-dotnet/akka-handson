using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Akka.Actor;

namespace charting_actors.Actors
{
    public class ButtonToggleActor : UntypedActor
    {
        #region Message Types
        public class Toggle { }
        #endregion

        private readonly CounterType _myCounterType;
        private bool _isOn;
        private readonly Button _myButton;
        private readonly IActorRef _coordinatorActor;

        public ButtonToggleActor(IActorRef coordinatorActor, Button myButton,
            CounterType myCounterType, bool isOn = false)
        {
            _myButton = myButton;
            _isOn = isOn;
            _coordinatorActor = coordinatorActor;
            _myCounterType = myCounterType;
        }

        protected override void OnReceive(object message)
        {
            if(message is Toggle && _isOn)
            {
                _coordinatorActor.Tell(new PerformanceCounterCoordinatorActor.Unwatch(_myCounterType));
                FlipToggle();
            }
            else if(message is Toggle && !_isOn)
            {
                _coordinatorActor.Tell(new PerformanceCounterCoordinatorActor.Watch(_myCounterType));
                FlipToggle();
            }
            else
            {
                Unhandled(message);
            }
        }

        private void FlipToggle()
        {
            _isOn = !_isOn;
            _myButton.Text = string.Format("{0} ({1})",
                _myCounterType.ToString().ToUpperInvariant(),
                _isOn ? "ON" : "OFF");
        }
    }
}
