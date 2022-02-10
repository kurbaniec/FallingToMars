using System;
using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;

namespace MovingPlatforms
{
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] public Vector3 distance;

        [SerializeField, Range(0, 1000)] public float speed = 1;

        private Rigidbody rb;
        private Vector3 originalPos;
        private Vector3 targetPos;

        private struct MovingPlayer
        {
            public Rigidbody Rb;
            public float MassDiff;
        }

        private List<MovingPlayer> players;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            players = new List<MovingPlayer>();
            originalPos = transform.position;
            targetPos = transform.position + distance;
        }

        private void FixedUpdate()
        {
            if (Vector3.Distance(transform.position, targetPos) <= 0.2)
            {
                (originalPos, targetPos) = (targetPos, originalPos);
                distance = -distance;
                // if (player)
                //     player.velocity *= -1;
                foreach (var p in players)
                {
                    var velocity = p.Rb.velocity;
                    if (distance.x != 0) velocity.x *= -1;
                    if (distance.y != 0) velocity.y *= -1;
                    if (distance.z != 0) velocity.z *= -1;
                    p.Rb.velocity = velocity;
                }
            }

            rb.MovePosition(transform.position + distance * speed * Time.fixedDeltaTime);
            
            // foreach (var p in players)
            // {
            //     p.Rb.MovePosition(p.Rb.position + distance * speed * p.MassDiff * Time.fixedDeltaTime);
            // }
            //player.MovePosition(rb.position + distance * speed * massDiff * Time.fixedDeltaTime);
            
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.transform.root.CompareTag("Player"))
            {
                //other.gameObject.GetComponent<GalaxyMinifigController>().isMoving(true);
                players.Add(new MovingPlayer()
                {
                    Rb = other.rigidbody,
                    MassDiff = other.rigidbody.mass / rb.mass
                });
                Debug.Log("Added");
                Debug.Log(players);
                Debug.Log(players.Count);
            }
        }
        
        private void OnCollisionExit(Collision other)
        {
            if (other.transform.root.CompareTag("Player"))
            {
                //player = null;
                players.RemoveAll(p => p.Rb == other.rigidbody);
                // players.RemoveAll(p =>
                // {
                //     if (p.Rb == other.rigidbody)
                //     {
                //         other.gameObject.GetComponent<GalaxyMinifigController>().isMoving(false);
                //         return true;
                //     }
                //
                //     return false;
                // });
                
                Debug.Log("removed");
                Debug.Log(players);
                Debug.Log(players.Count);
            }
        }
    }
}