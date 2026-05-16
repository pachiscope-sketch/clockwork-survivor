using UnityEngine;

namespace ClockworkSurvivor
{
    public class PickupCollector : MonoBehaviour
    {
        [SerializeField] private float pickupRadius = 2.6f;
        [SerializeField] private LayerMask pickupMask;

        private readonly Collider[] hits = new Collider[32];

        public void AddRadius(float amount)
        {
            pickupRadius += amount;
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsRunActive)
            {
                return;
            }

            LayerMask mask = pickupMask.value == 0 ? Physics.AllLayers : pickupMask;
            int count = Physics.OverlapSphereNonAlloc(transform.position, pickupRadius, hits, mask);

            for (int i = 0; i < count; i++)
            {
                ExperienceGem gem = hits[i].GetComponentInParent<ExperienceGem>();
                if (gem != null)
                {
                    gem.PullToward(transform);
                }
            }
        }
    }
}
