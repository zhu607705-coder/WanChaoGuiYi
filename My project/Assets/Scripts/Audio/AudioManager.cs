using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace WanChaoGuiYi
{
    public sealed class AudioManager : MonoBehaviour
    {
        private sealed class CueRuntime
        {
            public string id;
            public AudioClip clip;
            public StrategyAudioLayer layer;
            public bool spatial;
            public float volume;
            public float spatialBlend;
            public bool loop;
            public int priority;
        }

        private sealed class LayerRuntime
        {
            public StrategyAudioLayer layer;
            public GameObject root;
            public readonly List<AudioSource> pool = new List<AudioSource>();
            public AudioMixerGroup mixerGroup;
            public string exposedVolumeParameter;
            public float weight = 1f;
            public int cursor;
        }

        private sealed class SnapshotRuntime
        {
            public string name;
            public AudioMixerSnapshot mixerSnapshot;
            public float transitionSeconds;
            public readonly Dictionary<StrategyAudioLayer, float> weights = new Dictionary<StrategyAudioLayer, float>();
        }

        private static readonly StrategyAudioLayer[] LayerOrder =
        {
            StrategyAudioLayer.Master,
            StrategyAudioLayer.Music,
            StrategyAudioLayer.Ambience,
            StrategyAudioLayer.SFX,
            StrategyAudioLayer.UI,
            StrategyAudioLayer.War
        };

        [SerializeField] private AudioCueLibrary cueLibrary;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private int musicPoolSize = 6;
        [SerializeField] private int ambiencePoolSize = 6;
        [SerializeField] private int sfxPoolSize = 12;
        [SerializeField] private int uiPoolSize = 10;
        [SerializeField] private int warPoolSize = 12;

        private readonly Dictionary<string, CueRuntime> cues = new Dictionary<string, CueRuntime>();
        private readonly Dictionary<string, string> sceneMusicCueIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> regionMusicCueIds = new Dictionary<string, string>();
        private readonly Dictionary<StrategyAudioLayer, LayerRuntime> layers = new Dictionary<StrategyAudioLayer, LayerRuntime>();
        private readonly Dictionary<string, float> snapshotLayerWeights = new Dictionary<string, float>();
        private readonly Dictionary<string, SnapshotRuntime> snapshotProfiles = new Dictionary<string, SnapshotRuntime>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<AudioSource, CueRuntime> sourceCues = new Dictionary<AudioSource, CueRuntime>();
        private AudioSource activeSceneMusicSource;
        private string activeSceneMusicCueId;
        private AudioSource activeRegionMusicSource;
        private string activeRegionMusicCueId;
        private string currentSnapshotName = "Governance";
        private string lastCueId;
        private string lastCueRegionId;
        private Vector3 lastCueWorldPosition;

        public string LastCueId { get { return lastCueId; } }
        public string LastCueRegionId { get { return lastCueRegionId; } }
        public Vector3 LastCueWorldPosition { get { return lastCueWorldPosition; } }
        public string CurrentSnapshotName { get { return currentSnapshotName; } }
        public string ActiveSceneMusicCueId { get { return activeSceneMusicCueId; } }
        public string ActiveRegionMusicCueId { get { return activeRegionMusicCueId; } }

        private void Awake()
        {
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            EnsureInitialized();
        }

        public void BindLibrary(AudioCueLibrary library)
        {
            cueLibrary = library;
            RegisterLibrary(library);
        }

        public string[] GetMixerLayerNames()
        {
            EnsureInitialized();
            string[] names = new string[LayerOrder.Length];
            for (int i = 0; i < LayerOrder.Length; i++)
            {
                names[i] = LayerOrder[i].ToString();
            }

            return names;
        }

        public int GetPoolSize(string layerName)
        {
            EnsureInitialized();
            StrategyAudioLayer layer;
            if (!TryParseLayer(layerName, out layer)) return 0;
            LayerRuntime runtime;
            return layers.TryGetValue(layer, out runtime) ? runtime.pool.Count : 0;
        }

        public string GetLastCueId()
        {
            return lastCueId;
        }

        public string GetLastCueRegionId()
        {
            return lastCueRegionId;
        }

        public Vector3 GetLastCueWorldPosition()
        {
            return lastCueWorldPosition;
        }

        public string GetCurrentSnapshotName()
        {
            return currentSnapshotName;
        }

        public string GetActiveRegionMusicCueId()
        {
            return activeRegionMusicCueId;
        }

        public string GetActiveSceneMusicCueId()
        {
            return activeSceneMusicCueId;
        }

        public int GetRegionMusicCueCount()
        {
            EnsureInitialized();
            return regionMusicCueIds.Count;
        }

        public string GetRegionMusicCueId(string regionId)
        {
            EnsureInitialized();
            string cueId;
            return !string.IsNullOrEmpty(regionId) && regionMusicCueIds.TryGetValue(regionId, out cueId) ? cueId : null;
        }

        public string GetAudioMixerName()
        {
            return audioMixer != null ? audioMixer.name : null;
        }

        public int GetMixerRouteCountWithGroups()
        {
            EnsureInitialized();
            int count = 0;
            foreach (LayerRuntime runtime in layers.Values)
            {
                if (runtime != null && runtime.mixerGroup != null)
                {
                    count++;
                }
            }

            return count;
        }

        public int GetActiveSourceCount(string layerName)
        {
            EnsureInitialized();
            StrategyAudioLayer layer;
            if (!TryParseLayer(layerName, out layer)) return 0;
            LayerRuntime runtime;
            return layers.TryGetValue(layer, out runtime) ? CountActiveSources(runtime) : 0;
        }

        public int GetTotalActiveSourceCount()
        {
            EnsureInitialized();
            int count = 0;
            foreach (LayerRuntime runtime in layers.Values)
            {
                count += CountActiveSources(runtime);
            }

            return count;
        }

        public int GetTotalPoolSize()
        {
            EnsureInitialized();
            int count = 0;
            foreach (LayerRuntime runtime in layers.Values)
            {
                if (runtime != null) count += runtime.pool.Count;
            }

            return count;
        }

        public float GetPoolPressure(string layerName)
        {
            EnsureInitialized();
            StrategyAudioLayer layer;
            if (!TryParseLayer(layerName, out layer)) return 0f;
            LayerRuntime runtime;
            if (!layers.TryGetValue(layer, out runtime) || runtime.pool.Count == 0) return 0f;
            return (float)CountActiveSources(runtime) / runtime.pool.Count;
        }

        public string GetAudioDebugSummary()
        {
            EnsureInitialized();
            int active = GetTotalActiveSourceCount();
            int total = GetTotalPoolSize();
            return string.Format(
                "音频 状态={0} 场景={1} 区域={2} 最近={3}/{4} 锚点={5:F1},{6:F1} 路由={7} 池={8}/{9} War={10:0.00}",
                string.IsNullOrEmpty(currentSnapshotName) ? "-" : currentSnapshotName,
                string.IsNullOrEmpty(activeSceneMusicCueId) ? "-" : activeSceneMusicCueId,
                string.IsNullOrEmpty(activeRegionMusicCueId) ? "-" : activeRegionMusicCueId,
                string.IsNullOrEmpty(lastCueId) ? "-" : lastCueId,
                string.IsNullOrEmpty(lastCueRegionId) ? "-" : lastCueRegionId,
                lastCueWorldPosition.x,
                lastCueWorldPosition.y,
                GetMixerRouteCountWithGroups(),
                active,
                total,
                GetLayerWeight("War"));
        }

        public bool HasCue(string cueId)
        {
            EnsureInitialized();
            return !string.IsNullOrEmpty(cueId) && cues.ContainsKey(cueId);
        }

        public int GetRegisteredCueCountByPrefix(string prefix)
        {
            EnsureInitialized();
            int count = 0;
            foreach (string cueId in cues.Keys)
            {
                if (string.IsNullOrEmpty(prefix) || cueId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        public string GetCueClipName(string cueId)
        {
            EnsureInitialized();
            CueRuntime cue;
            return !string.IsNullOrEmpty(cueId) && cues.TryGetValue(cueId, out cue) && cue.clip != null ? cue.clip.name : null;
        }

        public float GetLayerWeight(string layerName)
        {
            EnsureInitialized();
            StrategyAudioLayer layer;
            if (!TryParseLayer(layerName, out layer)) return 0f;
            LayerRuntime runtime;
            return layers.TryGetValue(layer, out runtime) ? runtime.weight : 0f;
        }

        public float GetSnapshotTransitionSeconds(string snapshotName)
        {
            EnsureInitialized();
            SnapshotRuntime snapshot;
            return snapshotProfiles.TryGetValue(snapshotName, out snapshot) ? snapshot.transitionSeconds : 0f;
        }

        public void RegisterRuntimeCue(string cueId, AudioClip clip, string layerName, bool spatial, float volume, float spatialBlend)
        {
            StrategyAudioLayer layer;
            if (!TryParseLayer(layerName, out layer)) layer = StrategyAudioLayer.SFX;
            RegisterCue(cueId, clip, layer, spatial, volume, spatialBlend, false, 128);
        }

        public void RegisterRuntimeCue(string cueId, AudioClip clip, string layerName, bool spatial, float volume, float spatialBlend, bool loop, int priority)
        {
            StrategyAudioLayer layer;
            if (!TryParseLayer(layerName, out layer)) layer = StrategyAudioLayer.SFX;
            RegisterCue(cueId, clip, layer, spatial, volume, spatialBlend, loop, priority);
        }

        public void RegisterMusicLayerCue(string cueId, AudioClip clip, string layerName)
        {
            StrategyAudioLayer layer;
            if (!TryParseLayer(layerName, out layer)) layer = StrategyAudioLayer.Music;
            RegisterCue(cueId, clip, layer, false, 1f, 0f, true, 96);
        }

        public void RegisterSceneMusic(string sceneName, string cueId, AudioClip clip)
        {
            if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(cueId)) return;

            StrategyAudioLayer layer = sceneName.Equals("War", StringComparison.OrdinalIgnoreCase) || sceneName.Equals("Campaign", StringComparison.OrdinalIgnoreCase) ? StrategyAudioLayer.War : StrategyAudioLayer.Music;
            RegisterCue(cueId, clip, layer, false, 0.82f, 0f, true, 96);
            sceneMusicCueIds[sceneName] = cueId;
            sceneMusicCueIds[cueId] = cueId;
        }

        public void RegisterRegionMusic(string regionId, string cueId, AudioClip clip)
        {
            if (string.IsNullOrEmpty(regionId) || string.IsNullOrEmpty(cueId)) return;

            RegisterCue(cueId, clip, StrategyAudioLayer.Music, false, 1f, 0f, true, 96);
            regionMusicCueIds[regionId] = cueId;
        }

        public void BindAudioMixer(AudioMixer mixer)
        {
            audioMixer = mixer;
            ApplySnapshotMixerParameters();
        }

        public void RegisterMixerRoute(string layerName, AudioMixerGroup mixerGroup, string exposedVolumeParameter, float defaultWeight)
        {
            EnsureInitialized();
            StrategyAudioLayer layer;
            if (!TryParseLayer(layerName, out layer)) return;

            LayerRuntime runtime;
            if (!layers.TryGetValue(layer, out runtime)) return;
            runtime.mixerGroup = mixerGroup;
            runtime.exposedVolumeParameter = exposedVolumeParameter;
            SetLayerWeight(layer, defaultWeight);

            for (int i = 0; i < runtime.pool.Count; i++)
            {
                if (runtime.pool[i] != null)
                {
                    runtime.pool[i].outputAudioMixerGroup = mixerGroup;
                }
            }
        }

        public void RegisterSnapshotProfile(
            string snapshotName,
            float masterWeight,
            float musicWeight,
            float ambienceWeight,
            float sfxWeight,
            float uiWeight,
            float warWeight,
            float transitionSeconds)
        {
            SnapshotRuntime snapshot = CreateSnapshotRuntime(snapshotName, null, transitionSeconds);
            snapshot.weights[StrategyAudioLayer.Master] = Mathf.Clamp01(masterWeight);
            snapshot.weights[StrategyAudioLayer.Music] = Mathf.Clamp01(musicWeight);
            snapshot.weights[StrategyAudioLayer.Ambience] = Mathf.Clamp01(ambienceWeight);
            snapshot.weights[StrategyAudioLayer.SFX] = Mathf.Clamp01(sfxWeight);
            snapshot.weights[StrategyAudioLayer.UI] = Mathf.Clamp01(uiWeight);
            snapshot.weights[StrategyAudioLayer.War] = Mathf.Clamp01(warWeight);
            snapshotProfiles[snapshot.name] = snapshot;
        }

        public void RegisterSnapshotProfile(
            string snapshotName,
            AudioMixerSnapshot mixerSnapshot,
            float masterWeight,
            float musicWeight,
            float ambienceWeight,
            float sfxWeight,
            float uiWeight,
            float warWeight,
            float transitionSeconds)
        {
            SnapshotRuntime snapshot = CreateSnapshotRuntime(snapshotName, mixerSnapshot, transitionSeconds);
            snapshot.weights[StrategyAudioLayer.Master] = Mathf.Clamp01(masterWeight);
            snapshot.weights[StrategyAudioLayer.Music] = Mathf.Clamp01(musicWeight);
            snapshot.weights[StrategyAudioLayer.Ambience] = Mathf.Clamp01(ambienceWeight);
            snapshot.weights[StrategyAudioLayer.SFX] = Mathf.Clamp01(sfxWeight);
            snapshot.weights[StrategyAudioLayer.UI] = Mathf.Clamp01(uiWeight);
            snapshot.weights[StrategyAudioLayer.War] = Mathf.Clamp01(warWeight);
            snapshotProfiles[snapshot.name] = snapshot;
        }

        public AudioSource PlayCue(string cueId)
        {
            return PlayCue(cueId, null);
        }

        public AudioSource PlayCue(string cueId, string regionId)
        {
            EnsureInitialized();
            CueRuntime cue = ResolveCue(cueId);
            if (cue == null) return null;

            AudioSource source = AllocateSource(cue.layer);
            ConfigureSource(source, cue);
            source.spatialBlend = 0f;
            source.transform.localPosition = Vector3.zero;
            source.Play();
            RecordLastCue(cue.id, regionId, source.transform.position);
            return source;
        }

        public AudioSource PlaySpatialCue(string cueId, Vector3 worldPosition, string regionId)
        {
            EnsureInitialized();
            CueRuntime cue = ResolveCue(cueId);
            if (cue == null) return null;

            AudioSource source = AllocateSource(cue.layer);
            ConfigureSource(source, cue);
            source.spatialBlend = Mathf.Max(0.95f, cue.spatialBlend);
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 0.7f;
            source.maxDistance = 28f;
            source.transform.position = worldPosition;
            source.Play();
            RecordLastCue(cue.id, regionId, worldPosition);
            return source;
        }

        public void SetDynamicMusicState(string stateName, string regionId, float warPressure)
        {
            EnsureInitialized();
            currentSnapshotName = string.IsNullOrEmpty(stateName) ? "Governance" : stateName;

            SnapshotRuntime snapshot;
            if (snapshotProfiles.TryGetValue(currentSnapshotName, out snapshot))
            {
                ApplySnapshotProfile(snapshot);
            }
            else
            {
                bool war = currentSnapshotName == "War" || warPressure >= 0.65f;
                SetLayerWeight(StrategyAudioLayer.Music, war ? 0.62f : 0.88f);
                SetLayerWeight(StrategyAudioLayer.Ambience, war ? 0.36f : 0.82f);
                SetLayerWeight(StrategyAudioLayer.War, war ? Mathf.Clamp01(0.78f + warPressure * 0.2f) : Mathf.Clamp01(0.12f + warPressure * 0.6f));
                SetLayerWeight(StrategyAudioLayer.SFX, 1f);
                SetLayerWeight(StrategyAudioLayer.UI, 1f);
                SetLayerWeight(StrategyAudioLayer.Master, 1f);
            }

            PlaySceneMusic(currentSnapshotName);
            PlayRegionMusic(regionId);
            ApplySnapshotMixerParameters();
        }

        public void SetVolume(string layerName, float normalizedVolume)
        {
            StrategyAudioLayer layer;
            if (!TryParseLayer(layerName, out layer)) return;
            SetLayerWeight(layer, normalizedVolume);
            ApplySnapshotMixerParameters();
        }

        public void PlayRegionMusic(string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return;
            string cueId;
            if (!regionMusicCueIds.TryGetValue(regionId, out cueId))
            {
                cueId = EnsureGeneratedRegionTheme(regionId);
            }

            CueRuntime cue = ResolveCue(cueId);
            if (cue == null) return;

            if (activeRegionMusicSource == null)
            {
                activeRegionMusicSource = AllocateSource(StrategyAudioLayer.Music);
            }

            ConfigureSource(activeRegionMusicSource, cue);
            activeRegionMusicSource.loop = true;
            if (!activeRegionMusicSource.isPlaying)
            {
                activeRegionMusicSource.Play();
            }

            activeRegionMusicCueId = cueId;
        }

        public void PlaySceneMusic(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            string cueId;
            if (!sceneMusicCueIds.TryGetValue(sceneName, out cueId))
            {
                cueId = sceneName;
            }

            CueRuntime cue = ResolveCue(cueId);
            if (cue == null) return;

            if (activeSceneMusicSource == null)
            {
                activeSceneMusicSource = AllocateSource(cue.layer);
            }

            bool restart = activeSceneMusicSource.clip != cue.clip || !activeSceneMusicSource.isPlaying;
            ConfigureSource(activeSceneMusicSource, cue);
            activeSceneMusicSource.loop = true;
            if (restart)
            {
                activeSceneMusicSource.Play();
            }

            activeSceneMusicCueId = cueId;
        }

        private void EnsureInitialized()
        {
            if (layers.Count == 0)
            {
                BuildLayer(StrategyAudioLayer.Master, 1);
                BuildLayer(StrategyAudioLayer.Music, musicPoolSize);
                BuildLayer(StrategyAudioLayer.Ambience, ambiencePoolSize);
                BuildLayer(StrategyAudioLayer.SFX, sfxPoolSize);
                BuildLayer(StrategyAudioLayer.UI, uiPoolSize);
                BuildLayer(StrategyAudioLayer.War, warPoolSize);
                RegisterDefaultCues();
                RegisterDefaultSceneMusic();
                RegisterDefaultSnapshotProfiles();
                RegisterLibrary(cueLibrary);
                SetDynamicMusicState("Governance", null, 0f);
            }
        }

        private void BuildLayer(StrategyAudioLayer layer, int poolSize)
        {
            if (layers.ContainsKey(layer)) return;

            LayerRuntime runtime = new LayerRuntime();
            runtime.layer = layer;
            runtime.root = new GameObject("AudioLayer_" + layer);
            runtime.root.transform.SetParent(transform, false);
            runtime.weight = 1f;
            layers[layer] = runtime;

            int count = Mathf.Max(1, poolSize);
            for (int i = 0; i < count; i++)
            {
                GameObject sourceObject = new GameObject(layer + "_Source_" + i);
                sourceObject.transform.SetParent(runtime.root.transform, false);
                AudioSource source = sourceObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = layer == StrategyAudioLayer.War ? 1f : 0f;
                runtime.pool.Add(source);
            }
        }

        private void RegisterDefaultCues()
        {
            RegisterCue("ui_region_select", CreateRuntimePulse("ui_region_select"), StrategyAudioLayer.UI, false, 0.55f, 0f, false, 120);
            RegisterCue("ui_click", CreateRuntimePulse("ui_click"), StrategyAudioLayer.UI, false, 0.45f, 0f, false, 120);
            RegisterCue("governance_apply", CreateRuntimePulse("governance_apply"), StrategyAudioLayer.SFX, false, 0.65f, 0f, false, 112);
            RegisterCue("policy_apply", CreateRuntimePulse("policy_apply"), StrategyAudioLayer.SFX, false, 0.62f, 0f, false, 112);
            RegisterCue("war_contact", CreateRuntimePulse("war_contact"), StrategyAudioLayer.War, true, 0.9f, 1f, false, 80);
            RegisterCue("war_route", CreateRuntimePulse("war_route"), StrategyAudioLayer.War, true, 0.7f, 1f, false, 90);
            RegisterCue("war_occupied", CreateRuntimePulse("war_occupied"), StrategyAudioLayer.War, true, 0.95f, 1f, false, 70);
            RegisterCue("battle_report", CreateRuntimePulse("battle_report"), StrategyAudioLayer.War, false, 0.82f, 0f, false, 72);
            RegisterCue("war_declared", CreateRuntimePulse("war_declared"), StrategyAudioLayer.War, false, 0.95f, 0f, false, 68);
            RegisterCue("logistics_prepare", CreateRuntimePulse("logistics_prepare"), StrategyAudioLayer.War, true, 0.68f, 1f, false, 92);
            RegisterCue("logistics_convoy", CreateRuntimePulse("logistics_convoy"), StrategyAudioLayer.War, true, 0.66f, 1f, false, 94);
            RegisterCue("logistics_command", CreateRuntimePulse("logistics_command"), StrategyAudioLayer.UI, false, 0.55f, 0f, false, 112);
            RegisterCue("logistics_raided", CreateRuntimePulse("logistics_raided"), StrategyAudioLayer.War, true, 0.96f, 1f, false, 64);
            RegisterCue("occupation_pacification", CreateRuntimePulse("occupation_pacification"), StrategyAudioLayer.SFX, false, 0.72f, 0f, false, 108);
            RegisterCue("turn_tick", CreateRuntimePulse("turn_tick"), StrategyAudioLayer.UI, false, 0.42f, 0f, false, 118);
            RegisterCue("event_popup", CreateRuntimePulse("event_popup"), StrategyAudioLayer.UI, false, 0.62f, 0f, false, 112);
        }

        private void RegisterDefaultSceneMusic()
        {
            RegisterSceneMusic("Governance", "scene_governance", CreateRuntimePulse("scene_governance"));
            RegisterSceneMusic("War", "scene_war", CreateRuntimePulse("scene_war"));
            RegisterSceneMusic("Event", "scene_event", CreateRuntimePulse("scene_event"));
        }

        private void RegisterLibrary(AudioCueLibrary library)
        {
            if (library == null) return;

            foreach (AudioMixerLayerRouteDefinition route in library.MixerRoutes)
            {
                if (route == null) continue;
                RegisterMixerRoute(route.layer.ToString(), route.mixerGroup, route.exposedVolumeParameter, route.defaultWeight);
            }

            foreach (AudioSnapshotDefinition snapshot in library.Snapshots)
            {
                if (snapshot == null || string.IsNullOrEmpty(snapshot.name)) continue;
                RegisterSnapshotDefinition(snapshot);
            }

            foreach (AudioCueDefinition cue in library.Cues)
            {
                if (cue == null || string.IsNullOrEmpty(cue.id)) continue;
                RegisterCue(cue.id, cue.clip, cue.layer, cue.spatial, cue.volume, cue.spatialBlend, cue.loop, cue.priority);
            }

            foreach (SceneMusicDefinition music in library.SceneMusic)
            {
                if (music == null || string.IsNullOrEmpty(music.sceneName) || string.IsNullOrEmpty(music.cueId)) continue;
                RegisterSceneMusic(music.sceneName, music.cueId, music.clip);
            }

            foreach (RegionMusicDefinition music in library.RegionMusic)
            {
                if (music == null || string.IsNullOrEmpty(music.regionId) || string.IsNullOrEmpty(music.cueId)) continue;
                RegisterRegionMusic(music.regionId, music.cueId, music.clip);
            }
        }

        private void RegisterDefaultSnapshotProfiles()
        {
            RegisterSnapshotProfile("Governance", 1f, 0.88f, 0.82f, 1f, 1f, 0.12f, 0.65f);
            RegisterSnapshotProfile("War", 1f, 0.62f, 0.36f, 1f, 1f, 0.98f, 0.35f);
            RegisterSnapshotProfile("Event", 1f, 0.72f, 0.58f, 0.9f, 1f, 0.42f, 0.5f);
        }

        private void RegisterSnapshotDefinition(AudioSnapshotDefinition definition)
        {
            SnapshotRuntime snapshot = CreateSnapshotRuntime(definition.name, definition.mixerSnapshot, definition.transitionSeconds);
            foreach (AudioSnapshotLayerWeight layerWeight in definition.weights)
            {
                if (layerWeight == null) continue;
                snapshot.weights[layerWeight.layer] = Mathf.Clamp01(layerWeight.weight);
            }

            snapshotProfiles[snapshot.name] = snapshot;
        }

        private static SnapshotRuntime CreateSnapshotRuntime(string snapshotName, AudioMixerSnapshot mixerSnapshot, float transitionSeconds)
        {
            SnapshotRuntime snapshot = new SnapshotRuntime();
            snapshot.name = string.IsNullOrEmpty(snapshotName) ? "Governance" : snapshotName;
            snapshot.mixerSnapshot = mixerSnapshot;
            snapshot.transitionSeconds = Mathf.Max(0f, transitionSeconds);
            return snapshot;
        }

        private void RegisterCue(string cueId, AudioClip clip, StrategyAudioLayer layer, bool spatial, float volume, float spatialBlend, bool loop, int priority)
        {
            if (string.IsNullOrEmpty(cueId)) return;

            CueRuntime cue = new CueRuntime();
            cue.id = cueId;
            cue.clip = clip != null ? clip : CreateRuntimePulse(cueId);
            cue.layer = layer;
            cue.spatial = spatial;
            cue.volume = Mathf.Clamp01(volume);
            cue.spatialBlend = Mathf.Clamp01(spatialBlend);
            cue.loop = loop;
            cue.priority = Mathf.Clamp(priority, 0, 256);
            cues[cueId] = cue;
        }

        private CueRuntime ResolveCue(string cueId)
        {
            if (string.IsNullOrEmpty(cueId)) return null;
            CueRuntime cue;
            if (cues.TryGetValue(cueId, out cue)) return cue;

            RegisterCue(cueId, CreateRuntimePulse(cueId), StrategyAudioLayer.SFX, false, 0.5f, 0f, false, 128);
            return cues[cueId];
        }

        private AudioSource AllocateSource(StrategyAudioLayer layer)
        {
            LayerRuntime runtime;
            if (!layers.TryGetValue(layer, out runtime))
            {
                runtime = layers[StrategyAudioLayer.SFX];
            }

            for (int i = 0; i < runtime.pool.Count; i++)
            {
                AudioSource source = runtime.pool[i];
                if (source != null && !source.isPlaying)
                {
                    return source;
                }
            }

            runtime.cursor = (runtime.cursor + 1) % runtime.pool.Count;
            return runtime.pool[runtime.cursor];
        }

        private void ConfigureSource(AudioSource source, CueRuntime cue)
        {
            if (source == null || cue == null) return;

            source.clip = cue.clip;
            source.outputAudioMixerGroup = ResolveMixerGroup(cue.layer);
            sourceCues[source] = cue;
            source.volume = ComputeSourceVolume(cue);
            source.loop = cue.loop;
            source.priority = cue.priority;
            source.spatialBlend = cue.spatial ? cue.spatialBlend : 0f;
            source.playOnAwake = false;
        }

        private AudioMixerGroup ResolveMixerGroup(StrategyAudioLayer layer)
        {
            LayerRuntime runtime;
            if (layers.TryGetValue(layer, out runtime) && runtime.mixerGroup != null)
            {
                return runtime.mixerGroup;
            }

            return null;
        }

        private void SetLayerWeight(StrategyAudioLayer layer, float weight)
        {
            LayerRuntime runtime;
            if (!layers.TryGetValue(layer, out runtime)) return;
            runtime.weight = Mathf.Clamp01(weight);
            snapshotLayerWeights[layer.ToString()] = runtime.weight;

            for (int i = 0; i < runtime.pool.Count; i++)
            {
                AudioSource source = runtime.pool[i];
                CueRuntime cue;
                if (source != null && sourceCues.TryGetValue(source, out cue))
                {
                    source.volume = ComputeSourceVolume(cue);
                }
            }
        }

        private void ApplySnapshotProfile(SnapshotRuntime snapshot)
        {
            for (int i = 0; i < LayerOrder.Length; i++)
            {
                StrategyAudioLayer layer = LayerOrder[i];
                float weight;
                SetLayerWeight(layer, snapshot.weights.TryGetValue(layer, out weight) ? weight : GetLayerWeight(layer.ToString()));
            }

            if (snapshot.mixerSnapshot != null)
            {
                snapshot.mixerSnapshot.TransitionTo(snapshot.transitionSeconds);
            }
        }

        private void ApplySnapshotMixerParameters()
        {
            if (audioMixer == null) return;

            foreach (KeyValuePair<string, float> entry in snapshotLayerWeights)
            {
                StrategyAudioLayer layer;
                string parameterName = entry.Key + "Volume";
                LayerRuntime runtime;
                if (TryParseLayer(entry.Key, out layer) && layers.TryGetValue(layer, out runtime) && !string.IsNullOrEmpty(runtime.exposedVolumeParameter))
                {
                    parameterName = runtime.exposedVolumeParameter;
                }

                audioMixer.SetFloat(parameterName, LinearToDecibels(entry.Value));
            }
        }

        private float ComputeSourceVolume(CueRuntime cue)
        {
            if (cue == null) return 0f;

            LayerRuntime runtime;
            float layerWeight = layers.TryGetValue(cue.layer, out runtime) ? runtime.weight : 1f;
            return Mathf.Clamp01(cue.volume * layerWeight);
        }

        private static float LinearToDecibels(float value)
        {
            return Mathf.Log10(Mathf.Max(0.0001f, value)) * 20f;
        }

        private static int CountActiveSources(LayerRuntime runtime)
        {
            if (runtime == null) return 0;
            int count = 0;
            for (int i = 0; i < runtime.pool.Count; i++)
            {
                AudioSource source = runtime.pool[i];
                if (source != null && source.isPlaying)
                {
                    count++;
                }
            }

            return count;
        }

        private void RecordLastCue(string cueId, string regionId, Vector3 worldPosition)
        {
            lastCueId = cueId;
            lastCueRegionId = regionId;
            lastCueWorldPosition = worldPosition;
        }

        private static bool TryParseLayer(string layerName, out StrategyAudioLayer layer)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                layer = StrategyAudioLayer.SFX;
                return false;
            }

            return System.Enum.TryParse(layerName, true, out layer);
        }

        private static AudioClip CreateRuntimePulse(string name)
        {
            AudioClip clip = AudioClip.Create("runtime_" + name, 256, 1, 44100, false);
            float[] samples = new float[256];
            for (int i = 0; i < samples.Length; i++)
            {
                float envelope = 1f - (float)i / samples.Length;
                samples[i] = Mathf.Sin(i * 0.33f) * 0.08f * envelope;
            }

            clip.SetData(samples, 0);
            return clip;
        }

        private string EnsureGeneratedRegionTheme(string regionId)
        {
            string cueId = "region_theme_" + regionId;
            if (!cues.ContainsKey(cueId))
            {
                RegisterCue(cueId, CreateRegionThemeClip(regionId), StrategyAudioLayer.Music, false, 0.72f, 0f, true, 96);
            }

            regionMusicCueIds[regionId] = cueId;
            return cueId;
        }

        private static AudioClip CreateRegionThemeClip(string regionId)
        {
            int hash = string.IsNullOrEmpty(regionId) ? 0 : regionId.GetHashCode();
            int sampleRate = 44100;
            int sampleCount = sampleRate * 2;
            AudioClip clip = AudioClip.Create("runtime_region_theme_" + regionId, sampleCount, 1, sampleRate, false);
            float[] samples = new float[sampleCount];
            float baseFrequency = 130f + Mathf.Abs(hash % 180);
            float harmonic = baseFrequency * (1.5f + Mathf.Abs((hash >> 8) % 30) / 100f);
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Clamp01(i / 2048f) * Mathf.Clamp01((sampleCount - i) / 4096f);
                float drone = Mathf.Sin(t * baseFrequency * Mathf.PI * 2f) * 0.045f;
                float overtone = Mathf.Sin(t * harmonic * Mathf.PI * 2f) * 0.018f;
                samples[i] = (drone + overtone) * envelope;
            }

            clip.SetData(samples, 0);
            return clip;
        }
    }
}
