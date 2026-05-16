using UnityEngine;

namespace ClockworkSurvivor
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float baseMoveSpeed = 6f;

        private Rigidbody body;
        private Vector3 moveInput;
        private float moveSpeedBonus;

        public float CurrentMoveSpeed
        {
            get { return baseMoveSpeed + moveSpeedBonus; }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsRunActive)
            {
                moveInput = Vector3.zero;
                return;
            }

            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                horizontal -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                horizontal += 1f;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                vertical -= 1f;
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                vertical += 1f;
            }

            moveInput = new Vector3(horizontal, 0f, vertical);
            moveInput = Vector3.ClampMagnitude(moveInput, 1f);
            RotateTowardMouse();
        }

        private void FixedUpdate()
        {
            Vector3 nextPosition = body.position + moveInput * CurrentMoveSpeed * Time.fixedDeltaTime;
            body.MovePosition(nextPosition);
        }

        public void ResetCharacter()
        {
            transform.position = new Vector3(0f, 1f, 0f);
            transform.rotation = Quaternion.identity;
            moveInput = Vector3.zero;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        public void AddMoveSpeed(float amount)
        {
            moveSpeedBonus += amount;
        }

        private void RotateTowardMouse()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, transform.position);
            float distance;

            if (!plane.Raycast(ray, out distance))
            {
                return;
            }

            Vector3 target = ray.GetPoint(distance);
            Vector3 direction = target - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.05f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }
    }
}
