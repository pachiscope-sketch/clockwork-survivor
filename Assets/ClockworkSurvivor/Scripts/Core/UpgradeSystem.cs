using System.Collections.Generic;
using UnityEngine;

namespace ClockworkSurvivor
{
    public class UpgradeSystem : MonoBehaviour
    {
        public PlayerController player;
        public PlayerCombat playerCombat;
        public Health playerHealth;
        public PickupCollector pickupCollector;
        public List<UpgradeDefinition> upgradePool = new List<UpgradeDefinition>();

        private readonly Dictionary<UpgradeDefinition, int> stacks = new Dictionary<UpgradeDefinition, int>();

        public List<UpgradeDefinition> RollChoices(int count)
        {
            List<UpgradeDefinition> eligible = new List<UpgradeDefinition>();
            for (int i = 0; i < upgradePool.Count; i++)
            {
                UpgradeDefinition upgrade = upgradePool[i];
                if (upgrade == null)
                {
                    continue;
                }

                int currentStacks = stacks.ContainsKey(upgrade) ? stacks[upgrade] : 0;
                if (currentStacks < upgrade.maxStacks)
                {
                    eligible.Add(upgrade);
                }
            }

            List<UpgradeDefinition> choices = new List<UpgradeDefinition>();
            while (choices.Count < count && eligible.Count > 0)
            {
                int index = Random.Range(0, eligible.Count);
                choices.Add(eligible[index]);
                eligible.RemoveAt(index);
            }

            return choices;
        }

        public void ApplyUpgrade(UpgradeDefinition upgrade)
        {
            if (upgrade == null)
            {
                return;
            }

            if (!stacks.ContainsKey(upgrade))
            {
                stacks.Add(upgrade, 0);
            }

            stacks[upgrade]++;

            switch (upgrade.kind)
            {
                case UpgradeKind.DamageMultiplier:
                    if (playerCombat != null)
                    {
                        playerCombat.AddDamageMultiplier(upgrade.amount);
                    }
                    break;
                case UpgradeKind.FireRateMultiplier:
                    if (playerCombat != null)
                    {
                        playerCombat.AddFireRateMultiplier(upgrade.amount);
                    }
                    break;
                case UpgradeKind.ProjectileCount:
                    if (playerCombat != null)
                    {
                        playerCombat.AddProjectileCount(Mathf.RoundToInt(upgrade.amount));
                    }
                    break;
                case UpgradeKind.MoveSpeed:
                    if (player != null)
                    {
                        player.AddMoveSpeed(upgrade.amount);
                    }
                    break;
                case UpgradeKind.MaxHealth:
                    if (playerHealth != null)
                    {
                        playerHealth.AddMaxHealth(upgrade.amount, true);
                    }
                    break;
                case UpgradeKind.PickupRadius:
                    if (pickupCollector != null)
                    {
                        pickupCollector.AddRadius(upgrade.amount);
                    }
                    break;
                case UpgradeKind.ExplosionOnKill:
                    if (playerCombat != null)
                    {
                        playerCombat.AddExplosionOnKill(upgrade.amount, 2.8f);
                    }
                    break;
            }
        }
    }
}
