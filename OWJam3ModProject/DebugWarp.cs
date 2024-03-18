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

            PlayerBody playerBody = (PlayerBody)Locator.GetPlayerBody();
            if (playerBody != null)
            {
                playerBody.WarpToPositionRotation(transform.position, transform.rotation);
                playerBody.SetVelocity(Vector3.zero);
                playerBody.SetAngularVelocity(Vector3.zero);
            }
        }
    }
}
