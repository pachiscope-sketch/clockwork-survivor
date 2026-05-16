using UnityEngine;

namespace ClockworkSurvivor
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Health))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyConfig config;

        private Rigidbody body;
        private Health health;
        private Transform target;
        private float contactTimer;
        private float dashTimer;
        private float dashCooldownTimer;
        private Vector3 dashDirection;
        private float shootTimer;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            health = GetComponent<Health>();
            health.Died += HandleDeath;
        }

        private void Start()
        {
            if (target == null && GameManager.Instance != null && GameManager.Instance.Player != null)
            {
                target = GameManager.Instance.Player.transform;
            }

            ApplyConfig();
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsRunActive)
            {
                return;
            }

            contactTimer -= Time.deltaTime;
            dashCooldownTimer -= Time.deltaTime;
            shootTimer -= Time.deltaTime;

            if (config != null && config.behavior == EnemyBehavior.Shooter)
            {
                TryShoot();
            }

            TryContactDamage();
        }

        private void FixedUpdate()
        {
            if (target == null || config == null || (GameManager.Instance != null && !GameManager.Instance.IsRunActive))
            {
                return;
            }

            switch (config.behavior)
            {
                case EnemyBehavior.Dasher:
                    MoveDasher();
                    break;
                case EnemyBehavior.Shooter:
                    MoveShooter();
                    break;
                default:
                    MoveToward(target.position, config.moveSpeed);
                    break;
            }
        }

        public void Initialize(EnemyConfig enemyConfig, Transform playerTarget)
        {
            config = enemyConfig;
            target = playerTarget;
            ApplyConfig();
        }

        private void ApplyConfig()
        {
            if (config == null || health == null)
            {
                return;
            }

            health.Configure(config.maxHealth);
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = config.tint;
            }
        }

        private void MoveToward(Vector3 destination, float speed)
        {
            Vector3 direction = destination - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            direction.Normalize();
            body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);
            body.MoveRotation(Quaternion.LookRotation(direction, Vector3.up));
        }

        private void MoveDasher()
        {
            if (dashTimer > 0f)
            {
                dashTimer -= Time.fixedDeltaTime;
                body.MovePosition(body.position + dashDirection * config.dashSpeed * Time.fixedDeltaTime);
                return;
            }

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;

            if (dashCooldownTimer <= 0f && toTarget.magnitude <= config.dashRange)
            {
                dashDirection = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : transform.forward;
                dashTimer = config.dashDuration;
                dashCooldownTimer = config.dashCooldown;
                return;
            }

            MoveToward(target.position, config.moveSpeed);
        }

        private void MoveShooter()
        {
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            Vector3 moveDirection = Vector3.zero;

            if (distance > config.preferredDistance + 1f)
            {
                moveDirection = toTarget.normalized;
            }
            else if (distance < config.preferredDistance - 1f)
            {
                moveDirection = -toTarget.normalized;
            }

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                body.MovePosition(body.position + moveDirection * config.moveSpeed * Time.fixedDeltaTime);
            }

            if (toTarget.sqrMagnitude > 0.001f)
            {
                body.MoveRotation(Quaternion.LookRotation(toTarget.normalized, Vector3.up));
            }
        }

        private void TryShoot()
        {
            if (config.projectilePrefab == null || target == null || shootTimer > 0f)
            {
                return;
            }

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            direction.Normalize();
            Vector3 spawnPosition = transform.position + direction * 0.8f + Vector3.up * 0.6f;
            GameObject projectileObject = Instantiate(config.projectilePrefab, spawnPosition, Quaternion.LookRotation(direction, Vector3.up));
            Projectile projectile = projectileObject.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(direction, config.projectileSpeed, config.projectileDamage, 2.5f, ProjectileTeam.Enemy, gameObject);
            }

            shootTimer = config.shootCooldown;
        }

        private void TryContactDamage()
        {
            if (target == null || config == null || contactTimer > 0f)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > 1.25f)
            {
                return;
            }

            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(config.contactDamage, gameObject);
                contactTimer = config.contactCooldown;
            }
        }

        private void HandleDeath(GameObject source)
        {
            if (config != null && config.experiencePrefab != null)
            {
                ExperienceGem gem = Instantiate(config.experiencePrefab, transform.position + Vector3.up * 0.25f, Quaternion.identity);
                gem.Configure(config.experienceValue);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterKill();
            }

            PlayerCombat playerCombat = source != null ? source.GetComponent<PlayerCombat>() : null;
            if (playerCombat != null)
            {
                playerCombat.NotifyEnemyKilled(transform.position);
            }

            Destroy(gameObject);
        }
    }
}
