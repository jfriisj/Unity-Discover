// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections;
using Discover.DroneRage.Pvp;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Discover.DroneRage.UI.WaveCompletionUI
{
    public class WaveCompletionUIController : MonoBehaviour
    {
        [SerializeField]
        private string m_waveCompleteText = "> WAVE_# C0MPLETE.";

        [SerializeField]
        private float m_fadeInTime = 0.1f;

        [SerializeField]
        private float m_displayTime = 3.0f;

        [SerializeField]
        private float m_fadeOutTime = 0.5f;


        [SerializeField]
        private MonoBehaviour[] m_textEffects = Array.Empty<MonoBehaviour>();

        [SerializeField]
        private TMP_Text m_text = null;

        [SerializeField]
        private CanvasGroup m_canvasGroup;

        private Coroutine m_showUICoroutine;

        private void Awake()
        {
            Assert.IsNotNull(m_text, $"{m_text} cannot be null.");
            Assert.IsNotNull(m_canvasGroup, $"{m_canvasGroup} cannot be null.");
        }

        private void Start()
        {
            if (DroneRagePvpMode.IsPvpMode())
            {
                m_canvasGroup.alpha = 1.0f;
                if (m_showUICoroutine != null)
                {
                    StopCoroutine(m_showUICoroutine);
                    m_showUICoroutine = null;
                }
            }
        }

        private void OnDisable()
        {
            StopShowUI();
        }

        public void ShowWaveCompleteUI(int waveNumber)
        {
            foreach (var effect in m_textEffects)
            {
                effect.enabled = false;
            }

            StartShowUI();

            m_text.text = m_waveCompleteText.Replace("#", $"{waveNumber}");

            foreach (var effect in m_textEffects)
            {
                effect.enabled = true;
            }
        }

        private void StopShowUI()
        {
            if (m_showUICoroutine != null)
            {
                StopCoroutine(m_showUICoroutine);
                m_showUICoroutine = null;
                
                // In PvP mode we want to stay visible
                if (!DroneRagePvpMode.IsPvpMode())
                {
                    m_canvasGroup.alpha = 0.0f;
                }
                else
                {
                    m_canvasGroup.alpha = 1.0f;
                }
            }
        }

        private void StartShowUI()
        {
            if (m_showUICoroutine != null)
            {
                StopCoroutine(m_showUICoroutine);
            }
            m_showUICoroutine = StartCoroutine(ShowUI());
        }

        private IEnumerator ShowUI()
        {
            yield return FadeUIIn();
            yield return new WaitForSeconds(m_displayTime);
            
            // In PvP mode, if the match is running or ended, we tend to stay visible
            // However, the "Wave Complete" text should ideally disappear. 
            // For now, if in PvP, we just keep the whole thing visible if the match is ended.
            if (DroneRagePvpMode.IsPvpMode())
            {
                var matchController = DroneRagePvpMatchController.Instance;
                if (matchController != null && matchController.State == DroneRagePvpMatchController.MatchState.Ended)
                {
                    m_canvasGroup.alpha = 1.0f;
                    m_showUICoroutine = null;
                    yield break;
                }
                
                // If match is still running, we might want to fade out the WAVE text but the scoreboard stays.
                // Since they share the CanvasGroup, we'll just keep it visible for now in PvP.
                m_canvasGroup.alpha = 1.0f;
                m_showUICoroutine = null;
                yield break;
            }

            yield return FadeUIOut();

            gameObject.SetActive(false);
            m_showUICoroutine = null;
        }

        private IEnumerator FadeUIIn()
        {
            if (DroneRagePvpMode.IsPvpMode())
            {
                m_canvasGroup.alpha = 1f;
                yield break;
            }

            m_canvasGroup.alpha = 0.0f;
            var time = 0f;
            while (time < m_fadeInTime)
            {
                time += Time.deltaTime;
                var progress = Mathf.Clamp01(time / m_fadeInTime);
                var ease = 1 - Mathf.Pow(1 - progress, 4); // outQuart
                var value = Mathf.Lerp(0, 1, ease);
                m_canvasGroup.alpha = value;
                yield return null;
            }
            m_canvasGroup.alpha = 1;
        }

        private IEnumerator FadeUIOut()
        {
            var time = 0f;
            while (time < m_fadeOutTime)
            {
                time += Time.deltaTime;
                var value = Mathf.Lerp(1, 0, time / m_fadeOutTime); // Linear
                m_canvasGroup.alpha = value;
                yield return null;
            }
            m_canvasGroup.alpha = 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_canvasGroup == null)
            {
                m_canvasGroup = GetComponent<CanvasGroup>();
            }
        }
#endif
    }
}