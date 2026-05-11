using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WanChaoGuiYi
{
    public sealed class AudioManifestBinder : MonoBehaviour
    {
        private const string MixerAssetPath = "Assets/Audio/Mixers/WanChaoGuiYiStrategy.mixer";
        private const string CueLibraryAssetPath = "Assets/Audio/AudioCueLibrary.asset";
        private const string SceneManifestAssetPath = "Assets/Data/Audio/scene_music.json";
        private const string EmperorManifestAssetPath = "Assets/Data/Audio/emperor_themes.json";
        private const string ChronicleManifestAssetPath = "Assets/Data/Audio/chronicle_event_music.json";
        private const string SceneClipAssetRoot = "Assets/Audio/Music/Scene/";
        private const string EmperorClipAssetRoot = "Assets/Audio/Music/Emperor/";
        private const string ChronicleClipAssetRoot = "Assets/Audio/Music/ChronicleEvent/";

        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioCueLibrary cueLibrary;
        [SerializeField] private TextAsset sceneMusicManifest;
        [SerializeField] private TextAsset emperorThemeManifest;
        [SerializeField] private TextAsset chronicleEventManifest;

        private readonly Dictionary<string, string> sceneCueIdsByScene = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> sceneNamesByCueId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> emperorCueIdsById = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> chronicleCueIdsById = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private int sceneCueCount;
        private int sceneClipCount;
        private int emperorCueCount;
        private int emperorClipCount;
        private int chronicleCueCount;
        private int chronicleClipCount;
        private int boundMixerRouteCount;
        private string boundMixerName;

        public void Bind(AudioManager manager)
        {
            if (manager == null) return;

            ResetRuntimeState();
            BindCueLibrary(manager);
            BindMixer(manager);
            BindSceneMusic(manager);
            BindEmperorThemes(manager);
            BindChronicleEventMusic(manager);
            manager.SetDynamicMusicState(manager.GetCurrentSnapshotName(), null, 0f);
        }

        public int GetSceneCueCount()
        {
            return sceneCueCount;
        }

        public int GetSceneClipCount()
        {
            return sceneClipCount;
        }

        public int GetEmperorCueCount()
        {
            return emperorCueCount;
        }

        public int GetEmperorClipCount()
        {
            return emperorClipCount;
        }

        public int GetChronicleCueCount()
        {
            return chronicleCueCount;
        }

        public int GetChronicleClipCount()
        {
            return chronicleClipCount;
        }

        public bool HasBoundMixer()
        {
            return !string.IsNullOrEmpty(boundMixerName);
        }

        public int GetBoundMixerRouteCount()
        {
            return boundMixerRouteCount;
        }

        public string GetBoundMixerName()
        {
            return boundMixerName;
        }

        public string GetSceneMusicCueId(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return null;
            string cueId;
            return sceneCueIdsByScene.TryGetValue(sceneName, out cueId) ? cueId : null;
        }

        public string GetSceneNameForCueId(string cueId)
        {
            if (string.IsNullOrEmpty(cueId)) return null;
            string sceneName;
            return sceneNamesByCueId.TryGetValue(cueId, out sceneName) ? sceneName : null;
        }

        public string GetEmperorMusicCueId(string emperorId)
        {
            if (string.IsNullOrEmpty(emperorId)) return null;
            string cueId;
            return emperorCueIdsById.TryGetValue(emperorId, out cueId) ? cueId : null;
        }

        public string GetChronicleMusicCueId(string eventId)
        {
            if (string.IsNullOrEmpty(eventId)) return null;
            string cueId;
            return chronicleCueIdsById.TryGetValue(eventId, out cueId) ? cueId : null;
        }

        private void ResetRuntimeState()
        {
            sceneCueIdsByScene.Clear();
            sceneNamesByCueId.Clear();
            emperorCueIdsById.Clear();
            chronicleCueIdsById.Clear();
            sceneCueCount = 0;
            sceneClipCount = 0;
            emperorCueCount = 0;
            emperorClipCount = 0;
            chronicleCueCount = 0;
            chronicleClipCount = 0;
            boundMixerRouteCount = 0;
            boundMixerName = null;
        }

        private void BindCueLibrary(AudioManager manager)
        {
            AudioCueLibrary library = cueLibrary != null ? cueLibrary : LoadAsset<AudioCueLibrary>(CueLibraryAssetPath);
            if (library != null)
            {
                cueLibrary = library;
                manager.BindLibrary(library);
            }
        }

        private void BindMixer(AudioManager manager)
        {
            AudioMixer mixer = audioMixer != null ? audioMixer : LoadAsset<AudioMixer>(MixerAssetPath);
            if (mixer == null) return;

            audioMixer = mixer;
            boundMixerName = mixer.name;
            manager.BindAudioMixer(mixer);
            boundMixerRouteCount += RegisterMixerRoute(manager, mixer, StrategyAudioLayer.Master, 1f);
            boundMixerRouteCount += RegisterMixerRoute(manager, mixer, StrategyAudioLayer.Music, 1f);
            boundMixerRouteCount += RegisterMixerRoute(manager, mixer, StrategyAudioLayer.Ambience, 1f);
            boundMixerRouteCount += RegisterMixerRoute(manager, mixer, StrategyAudioLayer.SFX, 1f);
            boundMixerRouteCount += RegisterMixerRoute(manager, mixer, StrategyAudioLayer.UI, 1f);
            boundMixerRouteCount += RegisterMixerRoute(manager, mixer, StrategyAudioLayer.War, 1f);

            manager.RegisterSnapshotProfile("Governance", mixer.FindSnapshot("Governance"), 1f, 0.88f, 0.82f, 1f, 1f, 0.12f, 0.65f);
            manager.RegisterSnapshotProfile("War", mixer.FindSnapshot("War"), 1f, 0.62f, 0.36f, 1f, 1f, 0.98f, 0.35f);
            manager.RegisterSnapshotProfile("Event", mixer.FindSnapshot("Event"), 1f, 0.72f, 0.58f, 0.9f, 1f, 0.42f, 0.5f);
        }

        private static int RegisterMixerRoute(AudioManager manager, AudioMixer mixer, StrategyAudioLayer layer, float defaultWeight)
        {
            AudioMixerGroup group = ResolveMixerGroup(mixer, layer);
            if (group == null) return 0;

            string layerName = layer.ToString();
            manager.RegisterMixerRoute(layerName, group, layerName + "Volume", defaultWeight);
            return 1;
        }

        private static AudioMixerGroup ResolveMixerGroup(AudioMixer mixer, StrategyAudioLayer layer)
        {
            if (mixer == null) return null;
            if (layer == StrategyAudioLayer.Master && mixer.outputAudioMixerGroup != null)
            {
                return mixer.outputAudioMixerGroup;
            }

            string layerName = layer.ToString();
            AudioMixerGroup[] groups = mixer.FindMatchingGroups(layerName);
            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i] != null && groups[i].name.Equals(layerName, StringComparison.OrdinalIgnoreCase))
                {
                    return groups[i];
                }
            }

            if (groups.Length > 0 && groups[0] != null)
            {
                return groups[0];
            }

            return mixer.outputAudioMixerGroup;
        }

        private void BindSceneMusic(AudioManager manager)
        {
            TextAsset manifest = sceneMusicManifest != null ? sceneMusicManifest : LoadAsset<TextAsset>(SceneManifestAssetPath);
            if (manifest == null) return;
            sceneMusicManifest = manifest;

            SceneMusicData data = JsonUtility.FromJson<SceneMusicData>(manifest.text);
            if (data == null || data.items == null) return;

            for (int i = 0; i < data.items.Length; i++)
            {
                SceneMusicItem item = data.items[i];
                if (item == null || string.IsNullOrEmpty(item.scene) || string.IsNullOrEmpty(item.musicCueId)) continue;

                AudioClip clip = LoadClip(SceneClipAssetRoot, item.fileName, "Audio/Music/Scene/");
                manager.RegisterSceneMusic(item.scene, item.musicCueId, clip);
                sceneCueIdsByScene[item.scene] = item.musicCueId;
                sceneNamesByCueId[item.musicCueId] = item.scene;
                sceneCueCount++;
                if (clip != null) sceneClipCount++;
            }
        }

        private void BindEmperorThemes(AudioManager manager)
        {
            TextAsset manifest = emperorThemeManifest != null ? emperorThemeManifest : LoadAsset<TextAsset>(EmperorManifestAssetPath);
            if (manifest == null) return;
            emperorThemeManifest = manifest;

            EmperorThemeData data = JsonUtility.FromJson<EmperorThemeData>(manifest.text);
            if (data == null || data.items == null) return;

            for (int i = 0; i < data.items.Length; i++)
            {
                EmperorThemeItem item = data.items[i];
                if (item == null || string.IsNullOrEmpty(item.emperorId) || string.IsNullOrEmpty(item.musicCueId)) continue;

                AudioClip clip = LoadClip(EmperorClipAssetRoot, item.fileName, "Audio/Music/Emperor/");
                manager.RegisterMusicLayerCue(item.musicCueId, clip, "Music");
                emperorCueIdsById[item.emperorId] = item.musicCueId;
                emperorCueCount++;
                if (clip != null) emperorClipCount++;
            }
        }

        private void BindChronicleEventMusic(AudioManager manager)
        {
            TextAsset manifest = chronicleEventManifest != null ? chronicleEventManifest : LoadAsset<TextAsset>(ChronicleManifestAssetPath);
            if (manifest == null) return;
            chronicleEventManifest = manifest;

            ChronicleEventData data = JsonUtility.FromJson<ChronicleEventData>(manifest.text);
            if (data == null || data.items == null) return;

            for (int i = 0; i < data.items.Length; i++)
            {
                ChronicleEventItem item = data.items[i];
                if (item == null || string.IsNullOrEmpty(item.eventId) || string.IsNullOrEmpty(item.musicCueId)) continue;

                AudioClip clip = LoadClip(ChronicleClipAssetRoot, item.fileName, "Audio/Music/ChronicleEvent/");
                manager.RegisterMusicLayerCue(item.musicCueId, clip, IsMilitaryCategory(item.category) ? "War" : "Music");
                chronicleCueIdsById[item.eventId] = item.musicCueId;
                chronicleCueCount++;
                if (clip != null) chronicleClipCount++;
            }
        }

        private static bool IsMilitaryCategory(string category)
        {
            return !string.IsNullOrEmpty(category) && category.Equals("military", StringComparison.OrdinalIgnoreCase);
        }

        private static AudioClip LoadClip(string assetRoot, string fileName, string resourcesRoot)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

#if UNITY_EDITOR
            AudioClip editorClip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetRoot + fileName);
            if (editorClip != null) return editorClip;
#endif

            return Resources.Load<AudioClip>(resourcesRoot + StripExtension(fileName));
        }

        private static T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null) return asset;
#endif
            return null;
        }

        private static string StripExtension(string fileName)
        {
            int index = fileName.LastIndexOf('.');
            return index > 0 ? fileName.Substring(0, index) : fileName;
        }

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
        }
    }
}
