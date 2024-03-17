using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using NewHorizons;
using UnityEngine.InputSystem;
using System.Collections;
using NewHorizons.Components.SizeControllers;

namespace OWJam3ModProject
{
    internal class VesselWarpIn : MonoBehaviour
    {
        [Tooltip("The root GameObject of the vessel. Activated when the warp completes")]
        [SerializeField] GameObject vessel;
        [Tooltip("The root Transform of the singularity. Scaled as the warp occurs")]
        [SerializeField] Transform singularityRoot;
        [Tooltip("The maximum scale the singularity should have")]
        [SerializeField] float maxSingularityScale = 150;
        [Tooltip("How fast the singularity should scale up")]
        [SerializeField] float singularityScaleUpSpeed = 50f;
        [Tooltip("How long the singularity should stay at max scale before scaling down")]
        [SerializeField] float singularityMaxScaleDuration = 2f;
        [Tooltip("How fast the singularity should scale down")]
        [SerializeField] float singularityScaleDownSpeed = 100f;

        [Tooltip("Whether the vessel has started to warp in or not")]
        bool warpStarted = false;
        [Tooltip("The SingularitySizeController on the singularity")]
        SingularitySizeController singularitySizeController;
        [Tooltip("The keyframe array to use to control the singularity's size")]
        Keyframe[] singularitySizeKeyframes;

        void Start()
        {
            //Initialize
            vessel.SetActive(false);
            singularitySizeController = singularityRoot.GetComponentInChildren<SingularitySizeController>();

            singularitySizeKeyframes = new Keyframe[1];
            singularitySizeKeyframes[0] = new Keyframe(0, 0);
            singularitySizeController.scaleCurve.keys = singularitySizeKeyframes;
        }

        void Update()
        {
#if DEBUG
            //Debug key to warp in the vessel
            if (Keyboard.current.wKey.IsPressed() && Keyboard.current.vKey.wasPressedThisFrame)
            {
                StartWarpVessel();
            }
#endif
        }

        [ContextMenu("Warp In Vessel")]
        void StartWarpVessel()
        {
            //if (!warpStarted)
                StartCoroutine(WarpVessel());
        }

        IEnumerator WarpVessel()
        {
            Debug.Log("Starting warp in");
            warpStarted = true;

            //Scale up the singularity to its maximum radius
            float singularityScale = 0;
            while (singularityScale < maxSingularityScale)
            {
                singularityScale += singularityScaleUpSpeed * Time.deltaTime;
                singularitySizeKeyframes[0].value = singularityScale;
                singularitySizeController.scaleCurve.keys = singularitySizeKeyframes;
                yield return new WaitForEndOfFrame();
            }

            //Enable the vessel
            vessel.SetActive(true);

            //Wait at max scale before scaling back down
            yield return new WaitForSeconds(singularityMaxScaleDuration);

            //Scale down the singularity to 0
            while (singularityScale > 0)
            {
                singularityScale -= singularityScaleDownSpeed * Time.deltaTime;
                singularitySizeKeyframes[0].value = singularityScale;
                singularitySizeController.scaleCurve.keys = singularitySizeKeyframes;
                yield return new WaitForEndOfFrame();
            }

            yield return null;
        }
    }
}
