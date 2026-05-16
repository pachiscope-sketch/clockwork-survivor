using UnityEngine;

namespace ClockworkSurvivor
{
    [CreateAssetMenu(menuName = "Clockwork Survivor/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        public string upgradeName = "Upgrade";

        [TextArea]
        public string description = "Upgrade description";

        public UpgradeKind kind;
        public float amount = 1f;
        public int maxStacks = 5;
    }
}
