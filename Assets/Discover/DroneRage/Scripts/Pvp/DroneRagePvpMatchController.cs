// Copyright (c) Meta Platforms, Inc. and affiliates.

using Discover.DroneRage.Player;
using Discover.Utilities;
using Fusion;
using UnityEngine;

namespace Discover.DroneRage.Pvp
{
    [DefaultExecutionOrder(0)]
    public class DroneRagePvpMatchController : NetworkSingleton<DroneRagePvpMatchController>
    {
        public enum MatchState
        {
            NotStarted,
            Running,
            Ended
        }

        [Networked(OnChanged = nameof(OnMatchStateChanged))]
        public MatchState State { get; set; } = MatchState.NotStarted;

        [Networked]
        public TickTimer MatchTimer { get; set; }

        [SerializeField]
        private DroneRagePvpConfig m_config;

        public DroneRagePvpConfig Config => m_config;

        public float RemainingTime => MatchTimer.RemainingTime(Runner).GetValueOrDefault();

        protected override void InternalAwake()
        {
            if (!DroneRagePvpMode.IsPvpMode())
            {
                gameObject.SetActive(false);
                return;
            }
        }

        public override void Spawned()
        {
            if (HasStateAuthority && State == MatchState.NotStarted)
            {
                StartMatch();
            }
        }

        private void StartMatch()
        {
            State = MatchState.Running;
            var duration = m_config != null ? (float)m_config.timeLimitSeconds : 300f;
            MatchTimer = TickTimer.CreateFromSeconds(Runner, duration);
            Debug.Log($"PvP Match Started with duration: {duration}s");
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || State != MatchState.Running) return;

            if (MatchTimer.Expired(Runner))
            {
                EndMatch();
                return;
            }

            // Check score limit
            var scoreLimit = m_config != null ? (uint)m_config.scoreLimit : 20u;
            foreach (var player in Player.Player.Players)
            {
                if (player.PlayerStats.PvpKills >= scoreLimit)
                {
                    EndMatch();
                    return;
                }
            }
        }

        public void EndMatch()
        {
            if (!HasStateAuthority) return;
            State = MatchState.Ended;
            Debug.Log("PvP Match Ended");
        }

        private static void OnMatchStateChanged(Changed<DroneRagePvpMatchController> changed)
        {
            Debug.Log($"PvP Match State Changed: {changed.Behaviour.State}");
        }
    }
}