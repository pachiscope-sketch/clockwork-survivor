using UnityEngine;

namespace ClockworkSurvivor
{
    public class PlayerCombat : MonoBehaviour
    {
        public WeaponConfig baseWeapon;
        public Transform firePoint;
        public LayerMask enemyMask;

        private float cooldown;
        private float damageMultiplier = 1f;
        private float fireRateMultiplier = 1f;
        private int bonusProjectileCount;
        private float explosionDamage;
        private float explosionRadius = 2.6f;

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsRunActive)
            {
                return;
            }

            if (baseWeapon == null || baseWeapon.projectilePrefab == null)
            {
                return;
            }

            cooldown -= Time.deltaTime;
            if (cooldown > 0f)
            {
                return;
            }

            EnemyController target = FindClosestEnemy();
            if (target == null)
            {
                return;
            }

            FireAt(target.transform.position);
            cooldown = 1f / Mathf.Max(0.05f, baseWeapon.fireRate * fireRateMultiplier);
        }

        public void AddDamageMultiplier(float amount)
        {
            damageMultiplier += amount;
        }

        public void AddFireRateMultiplier(float amount)
        {
            fireRateMultiplier += amount;
        }

        public void AddProjectileCount(int amount)
        {
            bonusProjectileCount += amount;
        }

        public void AddExplosionOnKill(float damage, float radius)
        {
            explosionDamage += damage;
            explosionRadius = Mathf.Max(explosionRadius, radius);
        }

        public void NotifyEnemyKilled(Vector3 position)
        {
            if (explosionDamage <= 0f)
            {
                return;
            }

            Collider[] hits = Physics.OverlapSphere(position, explosionRadius, enemyMask.value == 0 ? Physics.AllLayers : enemyMask);
            for (int i = 0; i < hits.Length; i++)
            {
                EnemyController enemy = hits[i].GetComponentInParent<EnemyController>();
                if (enemy == null)
                {
                    continue;
                }

                Health health = enemy.GetComponent<Health>();
                if (health != null && !health.IsDead)
                {
                    health.TakeDamage(explosionDamage, gameObject);
                }
            }
        }

        private EnemyController FindClosestEnemy()
        {
            LayerMask mask = enemyMask.value == 0 ? Physics.AllLayers : enemyMask;
            Collider[] hits = Physics.OverlapSphere(transform.position, baseWeapon.range, mask);
            EnemyController closest = null;
            float bestDistance = baseWeapon.range * baseWeapon.range;

            for (int i = 0; i < hits.Length; i++)
            {
                EnemyController enemy = hits[i].GetComponentInParent<EnemyController>();
                if (enemy == null)
                {
                    continue;
                }

                Health health = enemy.GetComponent<Health>();
                if (health != null && health.IsDead)
                {
                    continue;
                }

                float distance = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    closest = enemy;
                }
            }

            return closest;
        }

        private void FireAt(Vector3 targetPosition)
        {
            Transform origin = firePoint != null ? firePoint : transform;
            Vector3 baseDirection = targetPosition - origin.position;
            baseDirection.y = 0f;

            if (baseDirection.sqrMagnitude < 0.001f)
            {
                baseDirection = transform.forward;
            }

            baseDirection.Normalize();
            int projectileTotal = Mathf.Max(1, baseWeapon.projectileCount + bonusProjectileCount);
            float totalSpread = baseWeapon.spreadAngle * (projectileTotal - 1);

            for (int i = 0; i < projectileTotal; i++)
            {
                float angle = projectileTotal == 1 ? 0f : -totalSpread * 0.5f + baseWeapon.spreadAngle * i;
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;
                GameObject projectileObject = Instantiate(baseWeapon.projectilePrefab, origin.position, Quaternion.LookRotation(direction, Vector3.up));
                Projectile projectile = projectileObject.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Initialize(direction, baseWeapon.projectileSpeed, baseWeapon.damage * damageMultiplier, baseWeapon.projectileLifetime, ProjectileTeam.Player, gameObject);
                }
            }
        }
    }
}
