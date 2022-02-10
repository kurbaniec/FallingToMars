using System.Collections.Generic;
using UnityEngine;

namespace Falling_to_Mars.Scripts.Particles
{
    public class ParticleStarter : MonoBehaviour
    {
        [SerializeField] private List<ParticleSystem> particles;
        
        private bool active = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!active) return;
            // Update manager with new target waypoint
            foreach (var particle in particles)
            {
                particle.Play(true);
            }
            active = false;
            GetComponent<Collider>().enabled = false;
        }
    }
}