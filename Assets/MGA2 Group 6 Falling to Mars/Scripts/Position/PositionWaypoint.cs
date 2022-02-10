using System;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Falling_to_Mars.Scripts.Position
{
    public class PositionWaypoint : MonoBehaviour
    {
        [SerializeField] protected bool first = false;
        [SerializeField] protected PositionManager manager;
        [SerializeField] protected PositionWaypoint nextWaypoint;
        private bool active = true;

        public int Id { get; private set; } = -1;

        private void Awake()
        {
            if (!first) return;
            Id = 0;
            nextWaypoint.UpdateId(++Id);
        }

        private void UpdateId(int id)
        {
            Id = id;
            if (nextWaypoint && nextWaypoint != this)
            {
                nextWaypoint.UpdateId(++id);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!active || !other.gameObject.GetComponent<GalaxyMinifigController>()) return;
            // Update manager with new target waypoint
            manager.SetWaypoint(nextWaypoint);
            active = false;
            // Remove trigger
            // See: https://forum.unity.com/threads/how-to-disable-collider-trigger-on-collision-enter.427891/
            GetComponent<Collider>().enabled = false;
        }
    }
}