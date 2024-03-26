using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace OWJam3ModProject
{
    internal class EventOnProject : MonoBehaviour
    {
        const string EnteredProjectionPoolEvent = "EnterNomaiRemoteCamera";

        [Tooltip("The event to invoke when the player starts projecting in this projection pool")]
        [SerializeField] UnityEvent eventToTrigger;

        [Tooltip("The projection pool to make warp you")]
        NomaiRemoteCameraPlatform projectionPool;

        void Start()
        {
            projectionPool = GetComponentInChildren<NomaiRemoteCameraPlatform>();
            GlobalMessenger.AddListener(EnteredProjectionPoolEvent, OnEnteredProjectionPool);
        }

        void OnEnteredProjectionPool()
        {
            //Make sure the teleporting projection pool is the one that fired the event
            if (projectionPool.GetPlatformState() == NomaiRemoteCameraPlatform.State.Connecting_FadeIn)
            {
                eventToTrigger.Invoke();
            }
        }
    }
}
