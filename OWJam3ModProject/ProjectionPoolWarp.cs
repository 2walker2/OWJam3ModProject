using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using NewHorizons;
using System;
using System.Collections;
using System.Net.Sockets;

namespace OWJam3ModProject
{
    internal class ProjectionPoolWarp : MonoBehaviour
    {
        const string ExitedProjectionPoolEvent = "ExitNomaiRemoteCamera";

        [Tooltip("The ID of the projection pool you warp to")]
        [SerializeField] string warpPoolId;
        [Tooltip("Where to place any item the player tries to bring it with them")]
        [SerializeField] Transform itemTransform;

        [Tooltip("The projection pool to make warp you")]
        NomaiRemoteCameraPlatform projectionPool;

        void Start()
        {
            projectionPool = GetComponentInChildren<NomaiRemoteCameraPlatform>();
            GlobalMessenger.AddListener(ExitedProjectionPoolEvent, OnExitedProjectionPool);
        }

        private void OnExitedProjectionPool()
        {
            //Make sure the teleporting projection pool is the one that fired the event
            if (projectionPool.GetPlatformState() == NomaiRemoteCameraPlatform.State.Disconnecting_FadeIn)
            {
                //Make sure the pool is connected to the one it should warp you to
                if (projectionPool._slavePlatform._id.ToString() == warpPoolId)
                {
                    //Teleport player to simulation
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
