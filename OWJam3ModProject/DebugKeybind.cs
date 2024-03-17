using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace OWJam3ModProject
{
    internal class DebugKeybind : MonoBehaviour
    {
        [Tooltip("The control path for the key you must be holding to activate this warp")]
        [SerializeField] Key holdKey = Key.T;
        [Tooltip("The control path for the key you press to activate this wap")]
        [SerializeField] Key pressKey = Key.Digit1;

        void Update()
        {
#if DEBUG
            //Warp the player to this transform when a key combination is pressed
            if (Keyboard.current[holdKey].IsPressed() && Keyboard.current[pressKey].wasPressedThisFrame)
                TriggerDebug();
#endif
        }

        protected virtual void TriggerDebug()
        {
            return;
        }
    }
}
