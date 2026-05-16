using System.Collections.Generic;
using UnityEngine;

namespace ClockworkSurvivor
{
    public class EnemySpawner : MonoBehaviour
    {
        public Transform player;
        public List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();
        public float spawnRadius = 17f;
        public float initialSpawnInterval = 1.3f;
        public float minimumSpawnInterval = 0.35f;
        public int maxConcurrentEnemies = 55;

        private readonly List<EnemyController> activeEnemies = new List<EnemyController>();
        private float spawnTimer;
        private bool spawning;

        private void Update()
        {
            if (!spawning || GameManager.Instance == null || !GameManager.Instance.IsRunActive)
            {
                return;
            }

            activeEnemies.RemoveAll(enemy => enemy == null);

            if (activeEnemies.Count >= maxConcurrentEnemies)
            {
                return;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f)
            {
                return;
            }

            SpawnEnemy();
            float progress = Mathf.Clamp01(GameManager.Instance.Elapsed / Mathf.Max(1f, GameManager.Instance.runDuration));
            spawnTimer = Mathf.Lerp(initialSpawnInterval, minimumSpawnInterval, progress);
        }

        public void BeginSpawning()
        {
            activeEnemies.Clear();
            spawnTimer = 0.4f;
            spawning = true;
        }

        public void StopSpawning()
        {
            spawning = false;
        }

        private void SpawnEnemy()
        {
            EnemyConfig config = ChooseEnemyConfig();
            if (config == null || config.prefab == null)
            {
                return;
            }

            Transform target = player;
            if (target == null && GameManager.Instance != null && GameManager.Instance.Player != null)
            {
                target = GameManager.Instance.Player.transform;
            }

            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            if (randomCircle.sqrMagnitude < 0.001f)
            {
                randomCircle = Vector2.right;
            }

            Vector3 center = target != null ? target.position : Vector3.zero;
            Vector3 position = center + new Vector3(randomCircle.x, 0f, randomCircle.y) * spawnRadius;
            GameObject enemyObject = Instantiate(config.prefab, position, Quaternion.identity);
            EnemyController controller = enemyObject.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.Initialize(config, target);
                activeEnemies.Add(controller);
            }
        }

        private EnemyConfig ChooseEnemyConfig()
        {
            float elapsed = GameManager.Instance != null ? GameManager.Instance.Elapsed : 0f;
            float totalWeight = 0f;

            for (int i = 0; i < enemyConfigs.Count; i++)
            {
                EnemyConfig config = enemyConfigs[i];
                if (config != null && config.prefab != null && elapsed >= config.unlockAfterSeconds)
                {
                    totalWeight += Mathf.Max(0f, config.spawnWeight);
                }
            }

            if (totalWeight <= 0f)
            {
                return enemyConfigs.Count > 0 ? enemyConfigs[0] : null;
            }

            float roll = Random.Range(0f, totalWeight);
            for (int i = 0; i < enemyConfigs.Count; i++)
            {
                EnemyConfig config = enemyConfigs[i];
                if (config == null || config.prefab == null || elapsed < config.unlockAfterSeconds)
                {
                    continue;
                }

                roll -= Mathf.Max(0f, config.spawnWeight);
                if (roll <= 0f)
                {
                    return config;
                }
            }

            return enemyConfigs[0];
        }
    }
}
