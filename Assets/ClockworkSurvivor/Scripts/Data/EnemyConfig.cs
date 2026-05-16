using UnityEngine;

namespace ClockworkSurvivor
{
    [CreateAssetMenu(menuName = "Clockwork Survivor/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        public string enemyName = "Walker";
        public EnemyBehavior behavior = EnemyBehavior.Walker;
        public GameObject prefab;
        public GameObject projectilePrefab;
        public ExperienceGem experiencePrefab;
        public Color tint = Color.white;

        [Header("Stats")]
        public float maxHealth = 12f;
        public float moveSpeed = 3f;
        public float contactDamage = 8f;
        public float contactCooldown = 0.8f;
        public int experienceValue = 1;
        public float unlockAfterSeconds;
        public float spawnWeight = 1f;

        [Header("Dasher")]
        public float dashRange = 6f;
        public float dashSpeed = 9f;
        public float dashDuration = 0.35f;
        public float dashCooldown = 2.5f;

        [Header("Shooter")]
        public float preferredDistance = 7f;
        public float projectileDamage = 7f;
        public float projectileSpeed = 8f;
        public float shootCooldown = 1.8f;
    }
}
