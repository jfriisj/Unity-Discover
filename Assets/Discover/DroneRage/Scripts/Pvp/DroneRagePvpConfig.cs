// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace Discover.DroneRage.Pvp
{
    /// <summary>
    /// Configuration settings for DroneRage PvP mode.
    /// </summary>
    [CreateAssetMenu(menuName = "Discover/DroneRage/PvP Config")]
    public class DroneRagePvpConfig : ScriptableObject
    {
        [Header("Combat Settings")]
        [Tooltip("Enable or disable damage between players.")]
        public bool pvpDamageEnabled = true;

        [Tooltip("Damage multiplier for hits on teammates or self.")]
        public float friendlyFireMultiplier = 1.0f;

        [Header("Match Settings")]
        [Tooltip("Time in seconds before a player respawns.")]
        public float respawnDelaySeconds = 5.0f;

        [Tooltip("Health points a player respawns with.")]
        public int respawnHealth = 100;

        [Header("Victory Conditions")]
        [Tooltip("Total kills required to win the match.")]
        public int scoreLimit = 20;

        [Tooltip("Duration of the match in seconds.")]
        public int timeLimitSeconds = 300;
    }
}
