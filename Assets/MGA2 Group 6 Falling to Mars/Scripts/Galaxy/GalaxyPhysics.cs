using Player;
using UnityEngine;

namespace Galaxy
{
    /// <summary>
    /// GalaxyPhysics object that changes gravity in given collider!
    /// Based on https://www.youtube.com/watch?v=aZOyZJhreSU
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GalaxyPhysics : MonoBehaviour
    {
        public GalaxyType type; // Type, supports fixed or orbit gravitation
        public float gravity; // Gravity force down
        public float jumpModifier = 1.0f; // One can modify the player jump power with this
        public Collider collider { get; private set; }

        private void Start()
        {
        }

        private void Awake()
        {
            collider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            // Add GalaxyPhysics to player
            var controller = other.GetComponent<GalaxyMinifigController>();
            if (controller)
            {
                //Debug.Log("GalaxyOrbit enter " + gameObject.name);
                //Debug.Log("sphere center" + transform.position);
                controller.AddGalaxyPhysics(this);
            }
            else
            {
                // If it is not a player object ignore collisions with other
                // Prevent OnTriggerStay and so on..
                // See: https://docs.unity3d.com/ScriptReference/Physics.IgnoreCollision.html
                Physics.IgnoreCollision(this.collider, other);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            // Add GalaxyPhysics to player (sometimes it is need to call it multiple times)
            var controller = other.GetComponent<GalaxyMinifigController>();
            if (controller)
            {
                //Debug.Log("Stay " + gameObject.name);
                controller.AddGalaxyPhysics(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Add GalaxyPhysics from player
            var controller = other.GetComponent<GalaxyMinifigController>();
            if (controller)
            {
                //Debug.Log("GalaxyOrbit exit " + gameObject.name);
                controller.RemoveGalaxyPhysics(this);
            }
        }
    }
}