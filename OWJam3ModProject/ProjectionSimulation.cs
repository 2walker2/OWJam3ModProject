using NewHorizons;
using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using UnityEngine;

namespace OWJam3ModProject
{
    internal class ProjectionSimulation : MonoBehaviour
    {
        const string MEDITATION_CONDITION = "KNOWS_MEDITATION";

        public static ProjectionSimulation instance;

        [Tooltip("Whether the player is currently in the simulation")]
        bool playerInSimulation = false;
        [Tooltip("Whether the player knew how to meditate when entering the simulation")]
        bool playerKnewMeditationWhenEntering = false;
        [Tooltip("The number of seconds remaining when the player entered the simulation")]
        float secondsRemainingWhenEntering;

        [Tooltip("The PauseMenuManager in the scene")]
        PauseMenuManager pauseMenuManager;

        void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                OWJam3ModProject.instance.ModHelper.Console.WriteLine("Duplicate ProjectionSimulation component: "+gameObject.name, MessageType.Error);
                Destroy(gameObject);
            }
        }

        void Start()
        {
            pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        }

        public void EnterSimulation()
        {
            playerInSimulation = true;
            PlayerState._insideTheEye = true;

            playerKnewMeditationWhenEntering = PlayerData.GetPersistentCondition(MEDITATION_CONDITION);
            pauseMenuManager._skipToNextLoopButton.SetActive(false);

            secondsRemainingWhenEntering = TimeLoop.GetSecondsRemaining();
        }

        public void ExitSimulation()
        {
            playerInSimulation = false;
            PlayerState._insideTheEye = false;

            pauseMenuManager._skipToNextLoopButton.SetActive(playerKnewMeditationWhenEntering);
            TimeLoop.SetSecondsRemaining(secondsRemainingWhenEntering);
        }

        void Update()
        {
            if (playerInSimulation)
            {
                TimeLoop.SetSecondsRemaining(secondsRemainingWhenEntering);
            }
        }
    }
}
