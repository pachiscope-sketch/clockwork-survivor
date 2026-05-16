using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClockworkSurvivor
{
    public class UIController : MonoBehaviour
    {
        public GameObject titlePanel;
        public GameObject hudPanel;
        public GameObject upgradePanel;
        public GameObject resultPanel;

        public Text timerText;
        public Text healthText;
        public Text levelText;
        public Text killsText;
        public Slider experienceSlider;

        public Button startButton;
        public Button restartButton;
        public Button[] upgradeButtons;
        public Text[] upgradeTexts;
        public Text resultTitleText;
        public Text resultStatsText;

        private void Awake()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(delegate
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.BeginRun();
                    }
                });
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(delegate
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.RestartRun();
                    }
                });
            }

            if (upgradeButtons != null)
            {
                for (int i = 0; i < upgradeButtons.Length; i++)
                {
                    int capturedIndex = i;
                    if (upgradeButtons[i] != null)
                    {
                        upgradeButtons[i].onClick.AddListener(delegate
                        {
                            if (GameManager.Instance != null)
                            {
                                GameManager.Instance.SelectUpgrade(capturedIndex);
                            }
                        });
                    }
                }
            }
        }

        public void ShowTitle()
        {
            SetActive(titlePanel, true);
            SetActive(hudPanel, false);
            SetActive(upgradePanel, false);
            SetActive(resultPanel, false);
        }

        public void ShowRun()
        {
            SetActive(titlePanel, false);
            SetActive(hudPanel, true);
            SetActive(upgradePanel, false);
            SetActive(resultPanel, false);
        }

        public void ShowUpgradeSelection(List<UpgradeDefinition> choices)
        {
            SetActive(upgradePanel, true);

            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                bool hasChoice = choices != null && i < choices.Count && choices[i] != null;
                upgradeButtons[i].gameObject.SetActive(hasChoice);

                if (hasChoice && upgradeTexts != null && i < upgradeTexts.Length && upgradeTexts[i] != null)
                {
                    upgradeTexts[i].text = choices[i].upgradeName + "\n<size=15>" + choices[i].description + "</size>";
                }
            }
        }

        public void HideUpgradeSelection()
        {
            SetActive(upgradePanel, false);
        }

        public void ShowResult(bool victory, GameManager manager)
        {
            SetActive(titlePanel, false);
            SetActive(hudPanel, false);
            SetActive(upgradePanel, false);
            SetActive(resultPanel, true);

            if (resultTitleText != null)
            {
                resultTitleText.text = victory ? "SURVIVED" : "SYSTEM DOWN";
            }

            if (resultStatsText != null && manager != null)
            {
                int minutes = Mathf.FloorToInt(manager.Elapsed / 60f);
                int seconds = Mathf.FloorToInt(manager.Elapsed % 60f);
                resultStatsText.text = "Time  " + minutes.ToString("00") + ":" + seconds.ToString("00") +
                    "\nLevel  " + manager.Level +
                    "\nDefeated  " + manager.Kills;
            }
        }

        public void UpdateHud(GameManager manager)
        {
            if (manager == null)
            {
                return;
            }

            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(manager.Remaining / 60f);
                int seconds = Mathf.FloorToInt(manager.Remaining % 60f);
                timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
            }

            if (healthText != null && manager.playerHealth != null)
            {
                healthText.text = "HP " + Mathf.CeilToInt(manager.playerHealth.CurrentHealth) + " / " + Mathf.CeilToInt(manager.playerHealth.MaxHealth);
            }

            if (levelText != null)
            {
                levelText.text = "LV " + manager.Level;
            }

            if (killsText != null)
            {
                killsText.text = "DEFEATED " + manager.Kills;
            }

            if (experienceSlider != null)
            {
                experienceSlider.maxValue = Mathf.Max(1, manager.ExperienceToNextLevel);
                experienceSlider.value = manager.Experience;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
