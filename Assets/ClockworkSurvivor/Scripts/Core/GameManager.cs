using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClockworkSurvivor
{
    public enum GameState
    {
        Title,
        Playing,
        Upgrade,
        Victory,
        Defeat
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public PlayerController player;
        public Health playerHealth;
        public EnemySpawner enemySpawner;
        public UpgradeSystem upgradeSystem;
        public UIController uiController;

        public float runDuration = 180f;
        public int baseExperienceToLevel = 10;
        public int experienceGrowthPerLevel = 6;

        private readonly List<UpgradeDefinition> pendingChoices = new List<UpgradeDefinition>();
        private GameState state = GameState.Title;
        private float elapsed;
        private int level = 1;
        private int experience;
        private int experienceToNextLevel;
        private int kills;

        public PlayerController Player
        {
            get { return player; }
        }

        public bool IsRunActive
        {
            get { return state == GameState.Playing; }
        }

        public float Elapsed
        {
            get { return elapsed; }
        }

        public float Remaining
        {
            get { return Mathf.Max(0f, runDuration - elapsed); }
        }

        public int Level
        {
            get { return level; }
        }

        public int Experience
        {
            get { return experience; }
        }

        public int ExperienceToNextLevel
        {
            get { return experienceToNextLevel; }
        }

        public int Kills
        {
            get { return kills; }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            experienceToNextLevel = baseExperienceToLevel;

            if (playerHealth != null)
            {
                playerHealth.Died += HandlePlayerDied;
            }
        }

        private void Start()
        {
            Time.timeScale = 0f;
            state = GameState.Title;
            if (uiController != null)
            {
                uiController.ShowTitle();
                uiController.UpdateHud(this);
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= HandlePlayerDied;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (state != GameState.Playing)
            {
                return;
            }

            elapsed += Time.deltaTime;

            if (uiController != null)
            {
                uiController.UpdateHud(this);
            }

            if (elapsed >= runDuration)
            {
                EndRun(true);
            }
        }

        public void BeginRun()
        {
            elapsed = 0f;
            level = 1;
            experience = 0;
            kills = 0;
            experienceToNextLevel = baseExperienceToLevel;
            pendingChoices.Clear();

            if (player != null)
            {
                player.ResetCharacter();
            }

            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
            }

            if (enemySpawner != null)
            {
                enemySpawner.BeginSpawning();
            }

            state = GameState.Playing;
            Time.timeScale = 1f;

            if (uiController != null)
            {
                uiController.ShowRun();
                uiController.UpdateHud(this);
            }
        }

        public void AddExperience(int amount)
        {
            if (state != GameState.Playing)
            {
                return;
            }

            experience += Mathf.Max(0, amount);

            while (experience >= experienceToNextLevel)
            {
                experience -= experienceToNextLevel;
                level++;
                experienceToNextLevel = baseExperienceToLevel + (level - 1) * experienceGrowthPerLevel;
                OpenUpgradeSelection();
                break;
            }

            if (uiController != null)
            {
                uiController.UpdateHud(this);
            }
        }

        public void RegisterKill()
        {
            kills++;
        }

        public void SelectUpgrade(int index)
        {
            if (state != GameState.Upgrade || index < 0 || index >= pendingChoices.Count)
            {
                return;
            }

            if (upgradeSystem != null)
            {
                upgradeSystem.ApplyUpgrade(pendingChoices[index]);
            }

            pendingChoices.Clear();
            state = GameState.Playing;
            Time.timeScale = 1f;

            if (uiController != null)
            {
                uiController.HideUpgradeSelection();
                uiController.UpdateHud(this);
            }
        }

        public void RestartRun()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OpenUpgradeSelection()
        {
            if (upgradeSystem == null)
            {
                return;
            }

            pendingChoices.Clear();
            pendingChoices.AddRange(upgradeSystem.RollChoices(3));

            if (pendingChoices.Count == 0)
            {
                return;
            }

            state = GameState.Upgrade;
            Time.timeScale = 0f;

            if (uiController != null)
            {
                uiController.ShowUpgradeSelection(pendingChoices);
            }
        }

        private void HandlePlayerDied(GameObject source)
        {
            EndRun(false);
        }

        private void EndRun(bool victory)
        {
            if (state == GameState.Victory || state == GameState.Defeat)
            {
                return;
            }

            state = victory ? GameState.Victory : GameState.Defeat;
            Time.timeScale = 0f;

            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
            }

            if (uiController != null)
            {
                uiController.ShowResult(victory, this);
            }
        }
    }
}
