// Copyright (c) Meta Platforms, Inc. and affiliates.

using Fusion;

namespace Discover.DroneRage.Player
{
    public class PlayerStats : NetworkBehaviour
    {
        [Networked] public uint Score { get; set; }
        [Networked] public uint WavesSurvived { get; set; }
        [Networked] public uint ShotsFired { get; set; }
        [Networked] public uint ShotsHit { get; set; }
        [Networked] public uint EnemiesKilled { get; set; }
        [Networked] public float DamageDealt { get; set; }
        [Networked] public float DamageTaken { get; set; }
        [Networked] public float HealingReceived { get; set; }
        [Networked] public ulong TicksSurvived { get; set; }

        [Networked] public uint PvpKills { get; set; }
        [Networked] public uint PvpDeaths { get; set; }
        [Networked] public uint PvpScore { get; set; }
        [Networked] public float DamageDealtToPlayers { get; set; }
        [Networked] public float DamageTakenFromPlayers { get; set; }

        public double CalculateAccuracy()
        {
            return ShotsFired == 0 ? 0 : ShotsHit / (double)ShotsFired;
        }
    }
}
