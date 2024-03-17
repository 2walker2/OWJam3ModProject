using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OWJam3ModProject
{
    internal class DebugWarp : DebugKeybind
    {
        protected override void TriggerDebug()
        {
            base.TriggerDebug();

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
