using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using NewHorizons;
using System;
using System.Collections;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;

namespace OWJam3ModProject
{
    internal class ProjectionPoolWarp : MonoBehaviour
    {
        const string ExitedProjectionPoolEvent = "ExitNomaiRemoteCamera";
        const string EnteredProjectionPoolEvent = "EnterNomaiRemoteCamera";

        [Tooltip("The ID of the projection pool you warp to")]
        [SerializeField] string warpPoolId;
        [Tooltip("Where to place any item the player tries to bring it with them")]
        [SerializeField] Transform itemTransform;
        [Tooltip("Whether this projection pool is bringing the player inside the simulation")]
        [SerializeField] bool enteringSimulation;

        [Tooltip("The projection pool to make warp you")]
        NomaiRemoteCameraPlatform projectionPool;

        void Start()
        {
            projectionPool = GetComponentInChildren<NomaiRemoteCameraPlatform>();
            GlobalMessenger.AddListener(ExitedProjectionPoolEvent, OnExitedProjectionPool);
            GlobalMessenger.AddListener(EnteredProjectionPoolEvent, OnEnteredProjectionPool);
        }

        private void OnEnteredProjectionPool()
        {
            //Main.Instance.ModHelper.Console.WriteLine("ENTERING PROJECTION POOL");
            if (projectionPool.GetPlatformState() == NomaiRemoteCameraPlatform.State.Connecting_FadeIn)
            {
                //Main.Instance.ModHelper.Console.WriteLine("ENTERING THIS PROJECTION POOL");
                if (projectionPool._slavePlatform._id.ToString() == warpPoolId)
                {
                    //Main.Instance.ModHelper.Console.WriteLine("ENTERING PROJECTION POOL WITH WARP");
                    ProjectionSimulation.instance.StartEnteringSimulation();
                }
            }
        }

        private void OnExitedProjectionPool()
        {
            //Make sure the teleporting projection pool is the one that fired the event
            if (projectionPool.GetPlatformState() == NomaiRemoteCameraPlatform.State.Disconnecting_FadeIn)
            {
                //Make sure the pool is connected to the one it should warp you to
                if (projectionPool._slavePlatform._id.ToString() == warpPoolId)
                {

                    //Teleport player
                    PlayerBody playerBody = (PlayerBody)Locator.GetPlayerBody();
                    Transform target = projectionPool._playerHologram;
                    if (playerBody != null)
                    {
                        playerBody.WarpToPositionRotation(target.position, target.rotation);
                        playerBody.SetVelocity(Vector3.zero);
                        playerBody.SetAngularVelocity(Vector3.zero);
                    }

                    //Turn off flashlight to make transition more seamless
                    Locator.GetFlashlight().TurnOff();

                    //Prevent the player from taking items with them
                    ForceDropItem();

                    //If entering simulation, make player invincible
                    if (enteringSimulation)
                        ProjectionSimulation.instance.EnterSimulation();
                    else
                        ProjectionSimulation.instance.ExitSimulation();

                    //Force recall probe
                    Locator.GetProbe().ExternalRetrieve();
                }
            }
        }

        private void ForceDropItem()
        {
            ItemTool itemTool = Locator.GetPlayerCamera().GetComponentInChildren<ItemTool>();
            if (itemTool.GetHeldItem() != null && itemTool.GetHeldItemType() == ItemType.SharedStone)
            {
                itemTool._waitForUnsocketAnimation = false;
                itemTool.DropItemInstantly(projectionPool._socket._sector, itemTransform);
            }
        }
    }
}
