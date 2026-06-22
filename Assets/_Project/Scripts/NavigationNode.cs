using UnityEngine;

namespace PlayableAdsShort
{
    public sealed class NavigationNode : MonoBehaviour
    {
        public NavigationNode[] neighbours;
        public Color gizmoColor = new Color(1f, 0.82f, 0.12f, 0.9f);
        public float gizmoRadius = 0.06f;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, gizmoRadius);
            if (neighbours == null)
            {
                return;
            }

            foreach (NavigationNode neighbour in neighbours)
            {
                if (neighbour != null)
                {
                    Gizmos.DrawLine(transform.position, neighbour.transform.position);
                }
            }
        }
    }
}
