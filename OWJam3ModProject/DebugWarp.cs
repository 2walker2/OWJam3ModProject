using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OWJam3ModProject
{
    internal class DebugWarp : MonoBehaviour
    {
        [Tooltip("The control path for the key you must be holding to activate this warp")]
        [SerializeField] Key holdKey = Key.W;
        [Tooltip("The control path for the key you press to activate this wap")]
        [SerializeField] Key pressKey = Key.Digit1;

        void Update()
        {
            //Warp the player to this transform when a key combination is pressed
            if (Keyboard.current[holdKey].IsPressed() && Keyboard.current[pressKey].wasPressedThisFrame)
            {
                OWRigidbody playerBody = Locator.GetPlayerBody();
                if (playerBody != null)
                {
                    playerBody.SetPosition(transform.position);
                    playerBody.SetVelocity(Vector3.zero);
                    playerBody.SetAngularVelocity(Vector3.zero);
                }
            }
        }
    }
}
