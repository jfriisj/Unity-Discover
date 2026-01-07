// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discover.DroneRage.Player;
using Discover.DroneRage.Pvp;
using Discover.Networking;
using TMPro;
using UnityEngine;

namespace Discover.DroneRage.UI.Pvp
{
    public class DroneRagePvpScoreboardUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_scoreboardText;

        [SerializeField]
        private GameObject m_visuals;

        private void Start()
        {
            if (!DroneRagePvpMode.IsPvpMode())
            {
                gameObject.SetActive(false);
                return;
            }

            m_visuals.SetActive(true);
        }

        private void Update()
        {
            if (!DroneRagePvpMode.IsPvpMode()) return;

            UpdateScoreboard();
        }

        private void UpdateScoreboard()
        {
            var matchController = DroneRagePvpMatchController.Instance;
            if (matchController == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("<size=40><b>PvP SCOREBOARD</b></size>");
            
            if (matchController.State == DroneRagePvpMatchController.MatchState.Running)
            {
                sb.AppendLine($"Time Remaining: {matchController.RemainingTime:F0}s");
            }
            else if (matchController.State == DroneRagePvpMatchController.MatchState.Ended)
            {
                sb.AppendLine("<color=red><b>MATCH ENDED</b></color>");
            }
            
            sb.AppendLine();
            sb.AppendLine("NAME\t\tKILLS\tDEATHS\tSCORE");
            sb.AppendLine("--------------------------------------------------");

            var sortedPlayers = Player.Player.Players
                .OrderByDescending(p => p.PlayerStats.PvpKills)
                .ThenByDescending(p => p.PlayerStats.PvpScore);

            foreach (var player in sortedPlayers)
            {
                var playerName = DiscoverPlayer.Get(player.Object.StateAuthority).PlayerName;
                if (player == Player.Player.LocalPlayer) playerName += " (You)";
                
                sb.AppendLine($"{playerName}\t{player.PlayerStats.PvpKills}\t{player.PlayerStats.PvpDeaths}\t{player.PlayerStats.PvpScore}");
            }

            m_scoreboardText.text = sb.ToString();
        }
    }
}