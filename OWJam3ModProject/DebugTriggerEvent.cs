using UnityEngine;
using UnityEngine.Events;

namespace OWJam3ModProject
{
    internal class DebugTriggerEvent : DebugKeybind
    {
        [Tooltip("The UnityEvent to trigger")]
        [SerializeField] UnityEvent triggeredEvent;

        protected override void TriggerDebug()
        {
            base.TriggerDebug();

            triggeredEvent.Invoke();
        }

    }
}
