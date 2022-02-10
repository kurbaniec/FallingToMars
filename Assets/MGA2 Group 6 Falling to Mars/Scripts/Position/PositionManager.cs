using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace Falling_to_Mars.Scripts.Position
{
    public class PositionManager : MonoBehaviour
    {
        [SerializeField] public List<GameObject> players = new List<GameObject>();
        [SerializeField] public List<Text> playerHudElements;
        [SerializeField] protected PositionWaypoint waypoint;

        private readonly SortedDictionary<float, GameObject> positions = new SortedDictionary<float, GameObject>();

        private void Start()
        {
            if (players.Count > playerHudElements.Count)
            {
                throw new ArgumentException("Player / Hud Element Size Mismatch");
            }
        }

        private void Update()
        {
            // Calculate distance between target waypoint & players
            positions.Clear();
            foreach (var player in players)
            {
                positions.Add(Vector3.Distance(waypoint.transform.position, player.transform.position), player);
            }

            // Display positions on the hud
            var position = 1;
            foreach (
                var hud in positions.Select(
                    item => item.Value.GetComponent<GalaxyMinifigController>().PlayerIndex
                ).Select(playerIndex => playerHudElements[playerIndex]))
            {
                hud.text = $"{position}";
                ++position;
            }
        }

        public void SetWaypoint(PositionWaypoint newWaypoint)
        {
            if (newWaypoint.Id > waypoint.Id)
                waypoint = newWaypoint;
        }

        public void OrderPlayers()
        {
            players = players
                .OrderBy(p => p.GetComponent<GalaxyMinifigController>().PlayerIndex).ToList();
        }
    }
}