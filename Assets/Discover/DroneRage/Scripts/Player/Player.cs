// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Discover.DroneRage.Audio;
using Discover.DroneRage.Game;
using Discover.DroneRage.Pvp;
using Discover.DroneRage.UI.HealthIndicator;
using Discover.DroneRage.Weapons;
using Discover.Networking;
using Discover.Utilities;
using Fusion;
using Meta.Utilities;
using UnityEngine;

namespace Discover.DroneRage.Player
{
    public class Player : NetworkMultiton<Player>, IDamageable
    {
        private Transform m_headPose;
        private bool m_drivePoseFromHeadset;
        private bool m_loggedMissingCameraRig;

        [Networked]
        public float Health { get; set; } = 100f;

        [Networked]
        public NetworkBool IsDead { get; set; } = false;

        [Networked]
        public int PlayerUid { get; set; }

        public static Player LocalPlayer { get; private set; }

        public static IEnumerable<Player> Players => Instances;
        public static int NumPlayers => Instances.Count;
        public static int PlayersLeft => Instances.Count(p => !p.IsDead);

        [SerializeField, AutoSet]
        private PlayerStats m_playerStats;
        public PlayerStats PlayerStats => m_playerStats;

        public event Action OnHpChange;
        public event Action OnDeath;
        public event Action OnRespawn;

        protected new void Awake()
        {
            base.Awake();
            if (m_playerStats == null)
            {
                m_playerStats = GetComponent<PlayerStats>();
            }
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                LocalPlayer = this;
                PlayerUid = Runner.LocalPlayer.PlayerId;
            }
        }

        public void SetupPlayer()
        {
            if (!HasStateAuthority)
                return;

            m_drivePoseFromHeadset = true;
            TryBindHeadPose();
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !m_drivePoseFromHeadset)
                return;

            if (m_headPose == null)
            {
                TryBindHeadPose();
                if (m_headPose == null)
                {
                    if (!m_loggedMissingCameraRig)
                    {
                        Debug.LogWarning("[DroneRage] Player has no CameraRig/centerEyeAnchor yet; player pose will remain at spawn until available.", this);
                        m_loggedMissingCameraRig = true;
                    }

                    return;
                }
            }

            var headRotation = m_headPose.rotation;
            var yawOnlyRotation = Quaternion.Euler(0f, headRotation.eulerAngles.y, 0f);
            transform.SetPositionAndRotation(m_headPose.position, yawOnlyRotation);
        }

        private void TryBindHeadPose()
        {
            if (m_headPose != null)
                return;

            var cameraRig = PhotonNetwork.CameraRig;
            if (cameraRig != null && cameraRig.centerEyeAnchor != null)
            {
                m_headPose = cameraRig.centerEyeAnchor;
            }
        }

        public static Player GetRandomLivePlayer()
        {
            var livePlayers = Instances.Where(p => !p.IsDead).ToList();
            if (livePlayers.Count == 0) return null;
            return livePlayers[UnityEngine.Random.Range(0, livePlayers.Count)];
        }

        public static Player GetClosestLivePlayer(Vector3 position)
        {
            return Instances.Where(p => !p.IsDead)
                .OrderBy(p => Vector3.Distance(p.transform.position, position))
                .FirstOrDefault();
        }

        public void Heal(float healing, IDamageable.DamageCallback callback = null)
        {
            if (!HasStateAuthority || IsDead) return;

            float oldHp = Health;
            Health = Mathf.Min(100f, Health + healing);
            float hpAffected = Health - oldHp;

            if (hpAffected > 0)
            {
                PlayerStats.HealingReceived += hpAffected;
                OnHpChange?.Invoke();
                if (DroneRageAudioManager.Instance != null)
                {
                    if (DroneRageAudioManager.Instance.HealSfx != null)
                    {
                        DroneRageAudioManager.Instance.HealSfx.Play();
                    }
                    DroneRageAudioManager.Instance.SetHealth((int)Health);
                }
            }
        }

        public void TakeDamage(float damage, Vector3 position, Vector3 normal, IDamageable.DamageCallback callback = null)
        {
            if (!HasStateAuthority || IsDead) return;

            float oldHp = Health;
            Health = Mathf.Max(0f, Health - damage);
            float hpAffected = oldHp - Health;
            bool isDead = Health <= 0;

            if (hpAffected > 0)
            {
                PlayerStats.DamageTaken += hpAffected;
                
                if (DroneRagePvpMode.IsPvpMode())
                {
                    if (DroneRagePvpMatchController.Instance != null && DroneRagePvpMatchController.Instance.State == DroneRagePvpMatchController.MatchState.Running)
                    {
                        PlayerStats.DamageTakenFromPlayers += hpAffected;
                    }
                }

                OnHpChange?.Invoke();

                if (DroneRageAudioManager.Instance != null)
                {
                    DroneRageAudioManager.Instance.SetHealth((int)Health);
                }

                if (isDead)
                {
                    Die();
                }
            }

            if (callback != null && hpAffected > 0)
            {
                callback(this, hpAffected, isDead);
            }
        }

        private void Die()
        {
            IsDead = true;
            OnDeath?.Invoke();

            if (DroneRagePvpMode.IsPvpMode())
            {
                var matchController = DroneRagePvpMatchController.Instance;
                if (matchController != null && matchController.State == DroneRagePvpMatchController.MatchState.Running)
                {
                    PlayerStats.PvpDeaths++;
                    StartCoroutine(RespawnSequence());
                }
            }
        }

        private IEnumerator RespawnSequence()
        {
            float delay = 5.0f;
            if (DroneRagePvpMatchController.Instance != null && DroneRagePvpMatchController.Instance.Config != null)
            {
                delay = DroneRagePvpMatchController.Instance.Config.respawnDelaySeconds;
            }

            yield return new WaitForSeconds(delay);

            if (DroneRagePvpMatchController.Instance != null && DroneRagePvpMatchController.Instance.State == DroneRagePvpMatchController.MatchState.Running)
            {
                RequestRespawnRPC();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RequestRespawnRPC()
        {
            if (!IsDead) return;

            var respawnHp = 100f;
            if (DroneRagePvpMatchController.Instance != null && DroneRagePvpMatchController.Instance.Config != null)
            {
                respawnHp = DroneRagePvpMatchController.Instance.Config.respawnHealth;
            }

            Health = respawnHp;
            IsDead = false;

            // Deterministic respawn position
            var angle = (float)PlayerUid * 1.5f;
            var radius = 2.0f;
            transform.position = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            transform.rotation = Quaternion.LookRotation(-transform.position.normalized, Vector3.up);

            OnRespawn?.Invoke();
            OnHpChange?.Invoke();

            if (DroneRageAudioManager.Instance != null)
            {
                DroneRageAudioManager.Instance.SetHealth((int)Health);
            }
        }

        public bool IsDetectable(Transform from)
        {
            return !IsDead;
        }

        public void TrackDamageStats(IDamageable damageableAffected, float hpAffected, bool targetDied)
        {
            TrackDamageStatsOwnerRPC(hpAffected, targetDied);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void TrackDamageStatsOwnerRPC(float hpAffected, bool targetDied)
        {
            PlayerStats.ShotsHit++;
            PlayerStats.DamageDealt += hpAffected;
            PlayerStats.EnemiesKilled += targetDied ? 1u : 0u;

            if (DroneRagePvpMode.IsPvpMode())
            {
                if (DroneRagePvpMatchController.Instance != null && DroneRagePvpMatchController.Instance.State == DroneRagePvpMatchController.MatchState.Running)
                {
                    PlayerStats.DamageDealtToPlayers += hpAffected;
                    if (targetDied)
                    {
                        PlayerStats.PvpKills++;
                        PlayerStats.PvpScore += 1000u;
                    }
                    PlayerStats.PvpScore += (uint)Mathf.Ceil(hpAffected) * 10u;
                }
            }

            var dmg = (uint)Mathf.Ceil(hpAffected);
            PlayerStats.Score += 10u * dmg + (targetDied ? 1000u : 0u);
        }

        public void OnWeaponFired(Vector3 shotOrigin, Vector3 shotDirection)
        {
            PlayerStats.ShotsFired++;
        }

        public void OnWaveAdvance()
        {
            PlayerStats.WavesSurvived++;
        }
    }
}