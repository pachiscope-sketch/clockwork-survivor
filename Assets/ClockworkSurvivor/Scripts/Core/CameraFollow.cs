using UnityEngine;

namespace ClockworkSurvivor
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 20f, -16f);
        public float smoothTime = 0.12f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
            transform.rotation = Quaternion.Euler(58f, 0f, 0f);
        }
    }
}
