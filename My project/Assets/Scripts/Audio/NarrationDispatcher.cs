using System;
using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class NarrationDispatcher : MonoBehaviour
    {
        [Header("Narration Script")]
        [SerializeField] private TextAsset narrationScriptJson;

        [Header("Audio Source")]
        [SerializeField] private AudioSource narrationSource;

        [Header("Emperor Voice Source")]
        [SerializeField] private AudioSource emperorVoiceSource;

        [Header("Voice Volume")]
        [Range(0f, 1f)]
        [SerializeField] private float narrationVolume = 0.85f;
        [Range(0f, 1f)]
        [SerializeField] private float emperorVoiceVolume = 0.9f;

        [Header("Audio Paths (relative to Resources folder)")]
        [SerializeField] private string narrationPath = "Audio/Narration/";
        [SerializeField] private string emperorVoicePath = "Audio/EmperorVoice/";

        private NarrationData narrationData;
        private bool initialized;

        private readonly Dictionary<string, AudioClip> narrationClips = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, Dictionary<string, AudioClip>> emperorVoiceClips =
            new Dictionary<string, Dictionary<string, AudioClip>>();

        public void Initialize()
        {
            if (initialized) return;

            if (narrationSource == null)
            {
                narrationSource = gameObject.AddComponent<AudioSource>();
                ConfigureSource(narrationSource);
            }

            if (emperorVoiceSource == null)
            {
                emperorVoiceSource = gameObject.AddComponent<AudioSource>();
                ConfigureSource(emperorVoiceSource);
            }

            LoadNarrationData();
            LoadAllNarrationClips();
            LoadAllEmperorVoiceClips();
            initialized = true;
        }

        private void Awake()
        {
            Initialize();
        }

        private static void ConfigureSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
        }

        private void LoadNarrationData()
        {
            if (narrationScriptJson == null) return;

            try
            {
                narrationData = JsonUtility.FromJson<NarrationData>(narrationScriptJson.text);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[NarrationDispatcher] Failed to parse narration JSON: " + ex.Message);
            }
        }

        private void LoadAllNarrationClips()
        {
            if (narrationData?.tutorial?.segments == null) return;

            foreach (TutorialSegment seg in narrationData.tutorial.segments)
            {
                if (string.IsNullOrEmpty(seg.segmentId)) continue;
                AudioClip clip = LoadClip(narrationPath + seg.segmentId + ".mp3");
                if (clip != null)
                {
                    narrationClips[seg.segmentId] = clip;
                }
            }
        }

        private void LoadAllEmperorVoiceClips()
        {
            if (narrationData?.emperor_voices == null) return;

            foreach (EmperorVoice ev in narrationData.emperor_voices)
            {
                if (string.IsNullOrEmpty(ev.emperorId)) continue;

                var lines = new Dictionary<string, AudioClip>();

                if (ev.lines != null)
                {
                    AddLine(ev.emperorId, lines, "select", ev.lines.select);
                    AddLine(ev.emperorId, lines, "idle", ev.lines.idle);
                    AddLine(ev.emperorId, lines, "attack", ev.lines.attack);
                    AddLine(ev.emperorId, lines, "defend", ev.lines.defend);
                    AddLine(ev.emperorId, lines, "victory", ev.lines.victory);
                    AddLine(ev.emperorId, lines, "defeat", ev.lines.defeat);
                    AddLine(ev.emperorId, lines, "special_event", ev.lines.special_event);
                }

                emperorVoiceClips[ev.emperorId] = lines;
            }
        }

        private void AddLine(string emperorId, Dictionary<string, AudioClip> lines, string key, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            string fileName = key + ".mp3";
            AudioClip clip = LoadClip(emperorVoicePath + emperorId + "/" + fileName);
            if (clip != null)
            {
                lines[key] = clip;
            }
        }

        private static AudioClip LoadClip(string path)
        {
            try
            {
                AudioClip clip = Resources.Load<AudioClip>(path);
                return clip;
            }
            catch
            {
                return null;
            }
        }

        public void PlayNarration(string segmentId)
        {
            if (string.IsNullOrEmpty(segmentId)) return;

            if (!narrationClips.TryGetValue(segmentId, out AudioClip clip) || clip == null)
            {
                Debug.LogWarning("[NarrationDispatcher] Narration clip not found: " + segmentId);
                return;
            }

            narrationSource.clip = clip;
            narrationSource.volume = narrationVolume;
            narrationSource.Play();
        }

        public void PlayEmperorVoice(string emperorId, string lineType)
        {
            if (string.IsNullOrEmpty(emperorId) || string.IsNullOrEmpty(lineType)) return;

            if (!emperorVoiceClips.TryGetValue(emperorId, out var lines)) return;
            if (!lines.TryGetValue(lineType, out AudioClip clip) || clip == null) return;

            emperorVoiceSource.clip = clip;
            emperorVoiceSource.volume = emperorVoiceVolume;
            emperorVoiceSource.Play();
        }

        public void PlayEmperorSelect(string emperorId)
        {
            PlayEmperorVoice(emperorId, "select");
        }

        public void PlayEmperorIdle(string emperorId)
        {
            PlayEmperorVoice(emperorId, "idle");
        }

        public void PlayEmperorAttack(string emperorId)
        {
            PlayEmperorVoice(emperorId, "attack");
        }

        public void PlayEmperorDefend(string emperorId)
        {
            PlayEmperorVoice(emperorId, "defend");
        }

        public void PlayEmperorVictory(string emperorId)
        {
            PlayEmperorVoice(emperorId, "victory");
        }

        public void PlayEmperorDefeat(string emperorId)
        {
            PlayEmperorVoice(emperorId, "defeat");
        }

        public void PlayEmperorSpecialEvent(string emperorId)
        {
            PlayEmperorVoice(emperorId, "special_event");
        }

        public void PlayTutorialIntro()
        {
            PlayNarration("tutorial_intro");
        }

        public void PlayTutorialSelectEmperor()
        {
            PlayNarration("tutorial_select_emperor");
        }

        public void PlayTutorialFirstGovernance()
        {
            PlayNarration("tutorial_first_governance");
        }

        public void PlayTutorialWar()
        {
            PlayNarration("tutorial_war_intro");
        }

        public void PlayTutorialEvent()
        {
            PlayNarration("tutorial_event_approach");
        }

        public void PlayTutorialVictory()
        {
            PlayNarration("tutorial_victory");
        }

        public void PlayTutorialDefeat()
        {
            PlayNarration("tutorial_defeat");
        }

        public void PlayTutorialProsperity()
        {
            PlayNarration("tutorial_prosperity");
        }

        public void PlayTutorialDecline()
        {
            PlayNarration("tutorial_decline");
        }

        public void PlayTutorialUnification()
        {
            PlayNarration("tutorial_unification");
        }

        public void StopNarration()
        {
            narrationSource.Stop();
            narrationSource.clip = null;
        }

        public void StopEmperorVoice()
        {
            emperorVoiceSource.Stop();
            emperorVoiceSource.clip = null;
        }

        public void StopAll()
        {
            StopNarration();
            StopEmperorVoice();
        }

        public void SetNarrationVolume(float volume)
        {
            narrationVolume = Mathf.Clamp01(volume);
            narrationSource.volume = narrationVolume;
        }

        public void SetEmperorVoiceVolume(float volume)
        {
            emperorVoiceVolume = Mathf.Clamp01(volume);
            emperorVoiceSource.volume = emperorVoiceVolume;
        }

        public bool IsNarrationPlaying => narrationSource != null && narrationSource.isPlaying;
        public bool IsEmperorVoicePlaying => emperorVoiceSource != null && emperorVoiceSource.isPlaying;

        public int LoadedNarrationCount => narrationClips.Count;
        public int LoadedEmperorCount => emperorVoiceClips.Count;

        [Serializable]
        private sealed class NarrationData
        {
            public Tutorial tutorial;
            public EmperorVoice[] emperor_voices;
        }

        [Serializable]
        private sealed class Tutorial
        {
            public TutorialSegment[] segments;
        }

        [Serializable]
        private sealed class TutorialSegment
        {
            public string segmentId;
            public string text;
            public string trigger;
            public int priority;
        }

        [Serializable]
        private sealed class EmperorVoice
        {
            public string emperorId;
            public string emperorName;
            public string voiceProfile;
            public string personality;
            public EmperorLines lines;
        }

        [Serializable]
        private sealed class EmperorLines
        {
            public string select;
            public string idle;
            public string attack;
            public string defend;
            public string victory;
            public string defeat;
            public string special_event;
        }
    }
}
