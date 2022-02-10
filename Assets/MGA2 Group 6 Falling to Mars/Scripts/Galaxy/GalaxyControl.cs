using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Galaxy
{
    /// <summary>
    /// GalaxyControl manages all the GalaxyPhysics object the player interacts with.
    /// Has an internal hierarchy that orders GalaxyPhysics objects when they are nested.
    /// </summary>
    public class GalaxyControl
    {
        // Nested hierarchy
        // Ordered from "smallest" to "biggest", that means that next element in list
        // intersects or fully contains previous one
        private readonly List<GalaxyPhysics> hierarchy;
        private readonly Collider player;
        [CanBeNull] public GalaxyPhysics Galaxy => hierarchy.FirstOrDefault();

        public GalaxyControl(Collider collider)
        {
            hierarchy = new List<GalaxyPhysics>();
            player = collider;
        }

        public void AddPhysics(GalaxyPhysics physics)
        {
            // GalaxyPhysics object already in hierarchy, can be ommited
            if (Galaxy == physics || hierarchy.Contains(physics)) return;
            // Check if player is completely in physics object
            if (!physics.collider.bounds.ContainBounds(player.bounds))
            {
                //if (!physics.collider.bounds.ContainBoundsUpperHalf(player.bounds) && 
                //    !physics.collider.bounds.ContainBoundsLowerHalf(player.bounds)) return;
                return;
            }

            // Order GalaxyPhysics object in the hierarchy
            var update = false;
            var index = 0;
            foreach (var (item, i) in hierarchy.WithIndex())
            {
                // GalaxyPhysics object is bigger, check next element
                if (!physics.collider.bounds.Intersects(item.collider.bounds) &&
                    physics.collider.bounds.ContainBounds(item.collider.bounds)) continue;
                // Not bigger? Update hierarchy
                update = true;
                index = i;
                break;
            }

            if (update) hierarchy.Insert(index, physics);
            else hierarchy.Add(physics);
            //Debug.Log(hierarchy.Count());
        }

        public void RemovePhysics(GalaxyPhysics physics)
        {
            hierarchy.Remove(physics);
            //Debug.Log(hierarchy.Count());
        }
    }


    // Some utility extension methods
    public static class Extension
    {
        // See: https://thomaslevesque.com/2019/11/18/using-foreach-with-index-in-c/
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }

        // See: http://answers.unity.com/answers/661842/view.html
        public static bool ContainBounds(this Bounds bounds, Bounds target)
        {
            // var minPoint = target.min;
            // var maxPoint = target.max;
            // var points = new List<Vector3>
            // {
            //     minPoint,
            //     maxPoint,
            //     new Vector3(minPoint.x, minPoint.y, maxPoint.z),
            //     new Vector3(minPoint.x, maxPoint.y, minPoint.z),
            //     new Vector3(maxPoint.x, minPoint.y, minPoint.z),
            //     new Vector3(minPoint.x, maxPoint.y, maxPoint.z),
            //     new Vector3(maxPoint.x, minPoint.y, maxPoint.z),
            //     new Vector3(maxPoint.x, maxPoint.y, minPoint.z)
            // };
            // return points.All(bounds.Contains);
            return bounds.Contains(target.min) && bounds.Contains(target.max);
        }

        public static bool ContainBoundsLowerHalf(this Bounds bounds, Bounds target)
        {
            return bounds.Contains(target.min) && bounds.Contains(target.center);
        }

        public static bool ContainBoundsUpperHalf(this Bounds bounds, Bounds target)
        {
            return bounds.Contains(target.center) && bounds.Contains(target.max);
        }
    }
}