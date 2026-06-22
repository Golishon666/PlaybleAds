using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class WorldNavigation : MonoBehaviour
    {
        public NavigationNode[] nodes;

        public IReadOnlyList<Vector3> BuildRoute(Vector3 from, Vector3 desiredDestination)
        {
            NavigationNode start = FindClosest(from);
            NavigationNode finish = FindClosest(desiredDestination);
            if (start == null || finish == null)
            {
                return new[] { desiredDestination };
            }

            if (start == finish)
            {
                return new[] { finish.transform.position };
            }

            var open = new List<NavigationNode> { start };
            var cameFrom = new Dictionary<NavigationNode, NavigationNode>();
            var gScore = nodes.ToDictionary(node => node, _ => float.PositiveInfinity);
            gScore[start] = 0f;

            while (open.Count > 0)
            {
                NavigationNode current = open.OrderBy(node => gScore[node] + Heuristic(node, finish)).First();
                if (current == finish)
                {
                    return Reconstruct(cameFrom, current);
                }

                open.Remove(current);
                foreach (NavigationNode neighbour in current.neighbours ?? new NavigationNode[0])
                {
                    if (neighbour == null)
                    {
                        continue;
                    }

                    if (!gScore.ContainsKey(neighbour))
                    {
                        gScore[neighbour] = float.PositiveInfinity;
                    }

                    float tentative = gScore[current] + Vector3.Distance(current.transform.position, neighbour.transform.position);
                    if (tentative >= gScore[neighbour])
                    {
                        continue;
                    }

                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentative;
                    if (!open.Contains(neighbour))
                    {
                        open.Add(neighbour);
                    }
                }
            }

            return new[] { finish.transform.position };
        }

        public Vector3 ProjectToWalkable(Vector3 position)
        {
            NavigationNode node = FindClosest(position);
            return node != null ? node.transform.position : position;
        }

        private NavigationNode FindClosest(Vector3 position)
        {
            if (nodes == null)
            {
                return null;
            }

            return nodes
                .Where(node => node != null)
                .OrderBy(node => (node.transform.position - position).sqrMagnitude)
                .FirstOrDefault();
        }

        private static float Heuristic(NavigationNode from, NavigationNode to)
        {
            return Vector3.Distance(from.transform.position, to.transform.position);
        }

        private static IReadOnlyList<Vector3> Reconstruct(
            IReadOnlyDictionary<NavigationNode, NavigationNode> cameFrom,
            NavigationNode current)
        {
            var route = new List<Vector3> { current.transform.position };
            while (cameFrom.TryGetValue(current, out NavigationNode previous))
            {
                current = previous;
                route.Add(current.transform.position);
            }

            route.Reverse();
            return route;
        }
    }
}
