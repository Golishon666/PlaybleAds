using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayableAdsShort
{
    public sealed class WorldGameInput : MonoBehaviour, IGameInput
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private LayerMask selectionMask = ~0;
        [SerializeField] private float rayDistance = 100f;

        private bool _enabled = true;

        public event Action<string> TargetSelected;

        public void SetCamera(Camera camera)
        {
            worldCamera = camera;
        }

        public void Publish(string targetId)
        {
            if (_enabled)
            {
                TargetSelected?.Invoke(targetId);
            }
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        private void Update()
        {
            if (!_enabled || worldCamera == null || !TryGetPointerDown(out Vector2 screenPosition))
            {
                return;
            }

            Ray ray = worldCamera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, selectionMask, QueryTriggerInteraction.Collide))
            {
                return;
            }

            SelectableWorldView target = hit.collider.GetComponentInParent<SelectableWorldView>();
            if (target != null)
            {
                Publish(target.Id);
            }
        }

        private static bool TryGetPointerDown(out Vector2 screenPosition)
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }
    }
}
