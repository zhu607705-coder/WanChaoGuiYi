using System;
using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class ChronicleMusicDispatcher : MonoBehaviour
    {
        [Header("Manifest JSONs")]
        [SerializeField] private TextAsset sceneMusicManifest;
        [SerializeField] private TextAsset emperorThemeManifest;
        [SerializeField] private TextAsset chronicleEventManifest;

        [Header("Audio Paths (relative to Resources folder)")]
        [SerializeField] private string sceneMusicPath = "Audio/Music/Scene/";
        [SerializeField] private string emperorMusicPath = "Audio/Music/Emperor/";
        [SerializeField] private string chronicleMusicPath = "Audio/Music/ChronicleEvent/";

        [Header("Crossfade")]
        [SerializeField] private float crossfadeDuration = 2f;

        [Header("Master Volume")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;

        private AudioManager audioManager;
        private AudioSource sceneSourceA;
        private AudioSource sceneSourceB;
        private AudioSource emperorSourceA;
        private AudioSource emperorSourceB;
        private AudioSource eventSourceA;
        private AudioSource eventSourceB;
        private bool useSourceA = true;

        private string activeSceneCueId;
        private string activeEmperorCueId;
        private string activeEventCueId;
        private bool initialized;

        private float sceneVolume = 1f;
        private float emperorVolume = 0.75f;
        private float eventVolume = 0.9f;

        private readonly Dictionary<string, AudioClip> sceneClips = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, AudioClip> emperorClips = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, AudioClip> chronicleClips = new Dictionary<string, AudioClip>();

        public string ActiveSceneCueId => activeSceneCueId;
        public string ActiveEmperorCueId => activeEmperorCueId;
        public string ActiveEventCueId => activeEventCueId;

        private void Awake()
        {
            audioManager = GetComponent<AudioManager>();
            if (audioManager == null)
            {
                audioManager = FindObjectOfType<AudioManager>();
            }

            BuildAudioSources();
            LoadManifests();
            initialized = true;
        }

        private void BuildAudioSources()
        {
            GameObject sceneObj = new GameObject("MusicDispatcher_Scene");
            sceneObj.transform.SetParent(transform, false);
            sceneSourceA = sceneObj.AddComponent<AudioSource>();
            sceneSourceB = sceneObj.AddComponent<AudioSource>();
            ConfigureSource(sceneSourceA);
            ConfigureSource(sceneSourceB);

            GameObject emperorObj = new GameObject("MusicDispatcher_Emperor");
            emperorObj.transform.SetParent(transform, false);
            emperorSourceA = emperorObj.AddComponent<AudioSource>();
            emperorSourceB = emperorObj.AddComponent<AudioSource>();
            ConfigureSource(emperorSourceA);
            ConfigureSource(emperorSourceB);

            GameObject eventObj = new GameObject("MusicDispatcher_Event");
            eventObj.transform.SetParent(transform, false);
            eventSourceA = eventObj.AddComponent<AudioSource>();
            eventSourceB = eventObj.AddComponent<AudioSource>();
            ConfigureSource(eventSourceA);
            ConfigureSource(eventSourceB);
        }

        private void ConfigureSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
        }

        private void LoadManifests()
        {
            LoadSceneManifest();
            LoadEmperorManifest();
            LoadChronicleManifest();
        }

        private void LoadSceneManifest()
        {
            if (sceneMusicManifest == null) return;

            try
            {
                SceneMusicData data = JsonUtility.FromJson<SceneMusicData>(sceneMusicManifest.text);
                if (data?.items == null) return;

                foreach (SceneMusicItem item in data.items)
                {
                    string path = sceneMusicPath + item.fileName;
                    AudioClip clip = LoadClip(path);
                    if (clip != null)
                    {
                        sceneClips[item.musicCueId] = clip;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ChronicleMusicDispatcher] Failed to load scene manifest: " + ex.Message);
            }
        }

        private void LoadEmperorManifest()
        {
            if (emperorThemeManifest == null) return;

            try
            {
                EmperorThemeData data = JsonUtility.FromJson<EmperorThemeData>(emperorThemeManifest.text);
                if (data?.items == null) return;

                foreach (EmperorThemeItem item in data.items)
                {
                    string path = emperorMusicPath + item.fileName;
                    AudioClip clip = LoadClip(path);
                    if (clip != null)
                    {
                        emperorClips[item.musicCueId] = clip;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ChronicleMusicDispatcher] Failed to load emperor manifest: " + ex.Message);
            }
        }

        private void LoadChronicleManifest()
        {
            if (chronicleEventManifest == null) return;

            try
            {
                ChronicleEventData data = JsonUtility.FromJson<ChronicleEventData>(chronicleEventManifest.text);
                if (data?.items == null) return;

                foreach (ChronicleEventItem item in data.items)
                {
                    string path = chronicleMusicPath + item.fileName;
                    AudioClip clip = LoadClip(path);
                    if (clip != null)
                    {
                        chronicleClips[item.musicCueId] = clip;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ChronicleMusicDispatcher] Failed to load chronicle manifest: " + ex.Message);
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

        private void Update()
        {
            if (!initialized) return;
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            float effectiveMaster = masterVolume;

            if (sceneSourceA.isPlaying) sceneSourceA.volume = sceneVolume * effectiveMaster;
            if (sceneSourceB.isPlaying) sceneSourceB.volume = sceneVolume * effectiveMaster;
            if (emperorSourceA.isPlaying) emperorSourceA.volume = emperorVolume * effectiveMaster;
            if (emperorSourceB.isPlaying) emperorSourceB.volume = emperorVolume * effectiveMaster;
            if (eventSourceA.isPlaying) eventSourceA.volume = eventVolume * effectiveMaster;
            if (eventSourceB.isPlaying) eventSourceB.volume = eventVolume * effectiveMaster;
        }

        public void PlayScene(string sceneCueId)
        {
            if (string.IsNullOrEmpty(sceneCueId) || sceneCueId == activeSceneCueId) return;
            if (!sceneClips.TryGetValue(sceneCueId, out AudioClip clip) || clip == null) return;

            StopScene();
            AudioSource target = sceneSourceA;
            sceneSourceA.clip = clip;
            sceneSourceA.volume = 0f;
            sceneSourceA.Play();
            StartCoroutine(CrossfadeSceneCoroutine(0f, sceneVolume * masterVolume));
            activeSceneCueId = sceneCueId;

            NotifyAudioManager(sceneCueId);
        }

        public void PlayEmperorTheme(string emperorCueId)
        {
            if (string.IsNullOrEmpty(emperorCueId) || emperorCueId == activeEmperorCueId) return;
            if (!emperorClips.TryGetValue(emperorCueId, out AudioClip clip) || clip == null) return;

            StopEmperor();
            emperorSourceA.clip = clip;
            emperorSourceA.volume = 0f;
            emperorSourceA.Play();
            StartCoroutine(CrossfadeEmperorCoroutine(0f, emperorVolume * masterVolume));
            activeEmperorCueId = emperorCueId;
        }

        public void PlayChronicleEvent(string eventCueId)
        {
            if (string.IsNullOrEmpty(eventCueId) || eventCueId == activeEventCueId) return;
            if (!chronicleClips.TryGetValue(eventCueId, out AudioClip clip) || clip == null) return;

            StopChronicleEvent();
            eventSourceA.clip = clip;
            eventSourceA.volume = 0f;
            eventSourceA.Play();
            StartCoroutine(CrossfadeEventCoroutine(0f, eventVolume * masterVolume));
            activeEventCueId = eventCueId;
        }

        public void StopScene()
        {
            StartCoroutine(CrossfadeSceneCoroutine(sceneSourceA.volume, 0f, () =>
            {
                sceneSourceA.Stop();
                sceneSourceA.clip = null;
            }));
        }

        public void StopEmperor()
        {
            StartCoroutine(CrossfadeEmperorCoroutine(emperorSourceA.volume, 0f, () =>
            {
                emperorSourceA.Stop();
                emperorSourceA.clip = null;
            }));
        }

        public void StopChronicleEvent()
        {
            StartCoroutine(CrossfadeEventCoroutine(eventSourceA.volume, 0f, () =>
            {
                eventSourceA.Stop();
                eventSourceA.clip = null;
            }));
            activeEventCueId = null;
        }

        public void SetSceneVolume(float volume)
        {
            sceneVolume = Mathf.Clamp01(volume);
        }

        public void SetEmperorVolume(float volume)
        {
            emperorVolume = Mathf.Clamp01(volume);
        }

        public void SetEventVolume(float volume)
        {
            eventVolume = Mathf.Clamp01(volume);
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
        }

        private void NotifyAudioManager(string sceneCueId)
        {
            if (audioManager == null) return;

            if (sceneCueId.Contains("War") || sceneCueId.Contains("Campaign") || sceneCueId.Contains("Unification"))
            {
                audioManager.SetDynamicMusicState("War", null, 0.8f);
            }
            else if (sceneCueId.Contains("Event") || sceneCueId.Contains("Conspiracy") || sceneCueId.Contains("Funeral") || sceneCueId.Contains("Decline"))
            {
                audioManager.SetDynamicMusicState("Event", null, 0.5f);
            }
            else
            {
                audioManager.SetDynamicMusicState("Governance", null, 0.2f);
            }
        }

        private System.Collections.IEnumerator CrossfadeSceneCoroutine(float from, float to, Action onComplete = null)
        {
            float elapsed = 0f;
            float startVolume = from;
            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / crossfadeDuration;
                sceneSourceA.volume = Mathf.Lerp(startVolume, to, t) * masterVolume;
                yield return null;
            }

            sceneSourceA.volume = to * masterVolume;
            onComplete?.Invoke();
        }

        private System.Collections.IEnumerator CrossfadeEmperorCoroutine(float from, float to, Action onComplete = null)
        {
            float elapsed = 0f;
            float startVolume = from;
            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / crossfadeDuration;
                emperorSourceA.volume = Mathf.Lerp(startVolume, to, t) * masterVolume;
                yield return null;
            }

            emperorSourceA.volume = to * masterVolume;
            onComplete?.Invoke();
        }

        private System.Collections.IEnumerator CrossfadeEventCoroutine(float from, float to, Action onComplete = null)
        {
            float elapsed = 0f;
            float startVolume = from;
            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / crossfadeDuration;
                eventSourceA.volume = Mathf.Lerp(startVolume, to, t) * masterVolume;
                yield return null;
            }

            eventSourceA.volume = to * masterVolume;
            onComplete?.Invoke();
        }

        public int LoadedSceneCount => sceneClips.Count;
        public int LoadedEmperorCount => emperorClips.Count;
        public int LoadedChronicleCount => chronicleClips.Count;

        [Serializable]
        private sealed class SceneMusicData
        {
            public SceneMusicItem[] items;
        }

        [Serializable]
        private sealed class SceneMusicItem
        {
            public string scene;
            public string musicCueId;
            public string fileName;
            public string mood;
            public int bpm;
            public string[] tags;
        }

        [Serializable]
        private sealed class EmperorThemeData
        {
            public EmperorThemeItem[] items;
        }

        [Serializable]
        private sealed class EmperorThemeItem
        {
            public string emperorId;
            public string musicCueId;
            public string fileName;
            public string mood;
            public int bpm;
            public string[] tags;
        }

        [Serializable]
        private sealed class ChronicleEventData
        {
            public ChronicleEventItem[] items;
        }

        [Serializable]
        private sealed class ChronicleEventItem
        {
            public string eventId;
            public string musicCueId;
            public string fileName;
            public string category;
            public string mood;
            public int bpm;
            public string[] tags;
        }
    }
}
