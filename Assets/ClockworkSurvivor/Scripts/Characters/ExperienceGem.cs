using UnityEngine;

namespace ClockworkSurvivor
{
    public class ExperienceGem : MonoBehaviour
    {
        [SerializeField] private int experienceValue = 1;
        [SerializeField] private float pullSpeed = 9f;
        [SerializeField] private float collectDistance = 0.55f;

        private bool collecting;

        private void Update()
        {
            transform.Rotate(0f, 120f * Time.deltaTime, 0f, Space.World);
        }

        public void Configure(int value)
        {
            experienceValue = Mathf.Max(1, value);
        }

        public void PullToward(Transform target)
        {
            if (target == null || collecting)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, target.position + Vector3.up * 0.4f, pullSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) <= collectDistance)
            {
                Collect();
            }
        }

        private void Collect()
        {
            collecting = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddExperience(experienceValue);
            }

            Destroy(gameObject);
        }
    }
}
