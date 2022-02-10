using System;
using System.Numerics;
using Player;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace PlayerCamera
{
    /// <summary>
    /// Deprecated: Use the Cinemachine Virtual Camera.
    /// </summary>
    // Based on: http://answers.unity.com/comments/508715/view.html
    [RequireComponent(typeof(Camera))]
    public class GalaxyCamera : MonoBehaviour
    {
        public GameObject player;
        public float upDistance = 7.0f;
        public float backDistance = 10.0f;
        public float trackingSpeed = 25.0f;
        public float rotationSpeed = 25.0f;

        private GalaxyMinifigController controller;
        private Vector3 v3To;
        private Quaternion qTo;

        private void Awake()
        {
            controller = player.GetComponent<GalaxyMinifigController>();
            // Position camera in beginning
            var target = player.transform;
            // transform.position = target.position - target.forward * backDistance + target.up * upDistance;
            // transform.position = target.position - target.forward * backDistance*1.5f + target.up/2 * upDistance;
            transform.position = target.position - transform.forward * backDistance * 1.5f +
                                 transform.up * upDistance / 2;
            transform.rotation = Quaternion.LookRotation(target.position - transform.position, target.up);
            // Magic: Parent camera to player
            // No problems with rotations anymore!
            transform.parent = target;
        }

        void LateUpdate()
        {
            var target = player.transform;

            if (controller.Speed == 0)
            {
                // Idle camera movement
                // Allow free looking
                v3To = target.position - transform.forward * backDistance * 1.5f + transform.up * upDistance / 2;
                transform.position = Vector3.Lerp(transform.position, v3To, trackingSpeed * 2 * Time.deltaTime);

                // var x = Input.GetAxis("Mouse X") * 100 * Time.deltaTime;
                // var y = Input.GetAxis("Mouse Y") * 100 * Time.deltaTime;
                var x = 0;
                var y = 0;

                transform.RotateAround(target.position, target.up, x);
                transform.RotateAround(target.position, transform.right, y);

                // Rotation is now handled through parenting
                // === Deprecated
                // var targetView = Quaternion.LookRotation(target.forward, target.up);
                // var transformView = Quaternion.LookRotation(transform.forward, target.up);
                // var targetView = Quaternion.FromToRotation(Vector3.zero, target.forward);
                // var transformView = Quaternion.FromToRotation(Vector3.zero, transform.forward);
                // var rotation = Quaternion.Slerp(transformView, targetView, 1);

                // var rot = transform.localRotation.eulerAngles;
                // rot.z = target.localRotation.eulerAngles.z;
                // // var rotation = Quaternion.FromToRotation(transform.forward, target.forward);
                // // Debug.Log("rot" + rotation.eulerAngles);
                // Debug.Log("rot" + rot);
                // transform.localRotation = Quaternion.Euler(rot);

                //
                // transform.rotation =  rotation * transform.rotation;

                // qTo = Quaternion.LookRotation( transform.position-target.position, transform.up);
                // transform.rotation = qTo;

                // var targetView = Quaternion.FromToRotation(Vector3.forward, target.forward);
                // var transformView = Quaternion.FromToRotation(Vector3.forward, transform.forward);
                // var rotation = Quaternion.Slerp(transformView, targetView, 1);
                // transform.rotation =  rotation * transform.rotation;

                // Rotation is now handled through parenting
                // === Deprecated
                //  Attempt to auto rotate camera with player
                // Does not work :(
                // ===
                // var hypotenuse = transform.position - target.position;
                // var hLength = hypotenuse.magnitude;
                //
                // var a = (target.position + target.up) - target.position;
                // var b = transform.position - target.position;
                // var aAngle = Vector3.Angle(a, b);
                //
                // var aLength = Mathf.Cos(Mathf.Deg2Rad * aAngle) * hLength;
                // var expectedPoint = target.position + target.up * aLength;
                //
                // var bAngle = 180 - 90 - aAngle;
                // var errorRotation = Quaternion.AngleAxis(-(90-bAngle), transform.right);
                // var transformUp = errorRotation * transform.up;
                // //transformUp = new Vector3(0, 1, 0);
                //
                // var currentPoint = target.position + transformUp * aLength;
                //
                // // var errorAngle = Quaternion.FromToRotation(transform.up, target.up);
                // //
                // // var currentPoint = errorAngle * expectedPoint;
                // var rotation = Quaternion.FromToRotation(currentPoint, expectedPoint);
                // transform.rotation = rotation * transform.rotation;
            }
            else
            {
                // Player movement camera
                // Force camera behind player back
                // v3To = target.position - target.forward * backDistance*1.5f + target.up * upDistance;
                v3To = target.position - target.forward * backDistance * 1.5f + target.up * upDistance;
                transform.position = Vector3.Lerp(transform.position, v3To, trackingSpeed * 2 * Time.deltaTime);

                var forward = Quaternion.AngleAxis(-10, target.right) * (target.position - transform.position);

                qTo = Quaternion.LookRotation(forward, target.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, qTo, rotationSpeed * 2 * Time.deltaTime);
            }
        }

        // Source: http://answers.unity.com/answers/532318/view.html
        private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            var dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }
    }
}