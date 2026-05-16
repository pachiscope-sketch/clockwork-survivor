using UnityEngine;

namespace ClockworkSurvivor
{
    public class Projectile : MonoBehaviour
    {
        private Vector3 direction = Vector3.forward;
        private float speed = 12f;
        private float damage = 5f;
        private float lifetime = 1f;
        private float age;
        private ProjectileTeam team;
        private GameObject owner;

        public void Initialize(Vector3 newDirection, float newSpeed, float newDamage, float newLifetime, ProjectileTeam newTeam, GameObject newOwner)
        {
            direction = newDirection.sqrMagnitude > 0.001f ? newDirection.normalized : Vector3.forward;
            speed = newSpeed;
            damage = newDamage;
            lifetime = newLifetime;
            team = newTeam;
            owner = newOwner;
            age = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsRunActive)
            {
                return;
            }

            transform.position += direction * speed * Time.deltaTime;
            age += Time.deltaTime;

            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (owner != null && other.transform.root == owner.transform.root)
            {
                return;
            }

            if (team == ProjectileTeam.Player)
            {
                EnemyController enemy = other.GetComponentInParent<EnemyController>();
                if (enemy == null)
                {
                    return;
                }

                Health health = enemy.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage, owner);
                    Destroy(gameObject);
                }
            }
            else
            {
                PlayerController player = other.GetComponentInParent<PlayerController>();
                if (player == null)
                {
                    return;
                }

                Health health = player.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage, owner);
                    Destroy(gameObject);
                }
            }
        }
    }
}
