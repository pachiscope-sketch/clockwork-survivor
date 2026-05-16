using System;
using UnityEngine;

namespace ClockworkSurvivor
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 30f;

        private float currentHealth;
        private bool dead;

        public event Action<float, float> HealthChanged;
        public event Action<GameObject> Died;

        public float CurrentHealth
        {
            get { return currentHealth; }
        }

        public float MaxHealth
        {
            get { return maxHealth; }
        }

        public bool IsDead
        {
            get { return dead; }
        }

        private void Awake()
        {
            ResetHealth();
        }

        public void Configure(float newMaxHealth)
        {
            maxHealth = Mathf.Max(1f, newMaxHealth);
            ResetHealth();
        }

        public void ResetHealth()
        {
            dead = false;
            currentHealth = maxHealth;
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void AddMaxHealth(float amount, bool fillAddedHealth)
        {
            float previousMax = maxHealth;
            maxHealth = Mathf.Max(1f, maxHealth + amount);
            if (fillAddedHealth)
            {
                currentHealth += maxHealth - previousMax;
            }

            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(float amount, GameObject source)
        {
            if (dead || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            HealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f)
            {
                dead = true;
                Died?.Invoke(source);
            }
        }
    }
}
