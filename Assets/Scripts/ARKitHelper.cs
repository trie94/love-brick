namespace Love.Core
{
    using System.Collections.Generic;
    using UnityEngine;

#if ARCORE_IOS_SUPPORT
    using UnityEngine.XR.iOS;
    using UnityARUserAnchorComponent = UnityEngine.XR.iOS.UnityARUserAnchorComponent;
#else
    using UnityARUserAnchorComponent = UnityEngine.Component;
#endif

    /// <summary>
    /// A helper class to interact with the ARKit plugin.
    /// </summary>
    public class ARKitHelper
    {
#if ARCORE_IOS_SUPPORT
        private List<ARHitTestResult> m_HitResultList = new List<ARHitTestResult>();
#endif
        /// <summary>
        /// Performs a Raycast against a plane.
        /// </summary>
        /// <param name="camera">The AR camera being used.</param>
        /// <param name="x">The x screen position.</param>
        /// <param name="y">The y screen position.</param>
        /// <param name="hitPose">The resulting hit pose if the method returns <c>true</c>.</param>
        /// <returns><c>true</c> if a plane was hit. Otherwise <c>false</c>.</returns>
        public bool RaycastPlane(Camera camera, float x, float y, out Pose hitPose)
        {
            hitPose = new Pose();
#if ARCORE_IOS_SUPPORT
            var session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

            var viewportPoint = camera.ScreenToViewportPoint(new Vector2(x, y));
            ARPoint arPoint = new ARPoint
            {
                x = viewportPoint.x,
                y = viewportPoint.y
            };

            m_HitResultList = session.HitTest(arPoint, ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);
            if (m_HitResultList.Count > 0)
            {
                int minDistanceIndex = 0;
                for (int i = 1; i < m_HitResultList.Count; i++)
                {
                    if (m_HitResultList[i].distance < m_HitResultList[minDistanceIndex].distance)
                    {
                        minDistanceIndex = i;
                    }
                }

                hitPose.position = UnityARMatrixOps.GetPosition(m_HitResultList[minDistanceIndex].worldTransform);

                // Point the hitPose rotation roughly away from the raycast/camera to match ARCore.
                hitPose.rotation.eulerAngles = new Vector3(0.0f, camera.transform.eulerAngles.y, 0.0f);

                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Creates an ARKit user anchor.
        /// </summary>
        /// <param name="pose">The pose for the new anchor.</param>
        /// <returns>A newly created ARKit anchor.</returns>
        public UnityARUserAnchorComponent CreateAnchor(Pose pose)
        {
            var anchorGO = new GameObject("User Anchor");
            var anchor = anchorGO.AddComponent<UnityARUserAnchorComponent>();
            anchorGO.transform.position = pose.position;
            anchorGO.transform.rotation = pose.rotation;
            return anchor;
        }
    }
}
