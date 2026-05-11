using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace WanChaoGuiYi.EditorTools
{
    public static class StrategyAudioAssetBuilder
    {
        private const string MixerPath = "Assets/Audio/Mixers/WanChaoGuiYiStrategy.mixer";
        private const string MixerGuid = "8f8c4b32b3629cf01505618200f57526";
        private const string CueLibraryPath = "Assets/Audio/AudioCueLibrary.asset";
        private const string SceneManifestPath = "Assets/Data/Audio/scene_music.json";
        private const string EmperorManifestPath = "Assets/Data/Audio/emperor_themes.json";
        private const string ChronicleManifestPath = "Assets/Data/Audio/chronicle_event_music.json";
        private const string RegionsDataPath = "Assets/Data/regions.json";
        private const string HistoricalLayersDataPath = "Assets/Data/historical_layers.json";
        private const string SceneClipRoot = "Assets/Audio/Music/Scene/";
        private const string EmperorClipRoot = "Assets/Audio/Music/Emperor/";
        private const string ChronicleClipRoot = "Assets/Audio/Music/ChronicleEvent/";

        [MenuItem("WanChaoGuiYi/Audio/Build Strategy Audio Assets")]
        public static void BuildAudioAssets()
        {
            EnsureAssetFolder("Assets/Audio");
            EnsureAssetFolder("Assets/Audio/Mixers");
            EnsureMixerMetaGuid();

            EnsureCleanAudioMixerAsset();
            AudioMixer mixer = EnsureAudioMixer();
            EnsureMixerGroups(mixer);
            PruneDuplicateLayerGroups(mixer);
            EnsureMixerSnapshots(mixer);
            BuildCueLibrary(mixer);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[StrategyAudioAssetBuilder] Built strategy audio assets.");
        }

        private static AudioMixer EnsureAudioMixer()
        {
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer != null) return mixer;

            Type controllerType = Type.GetType("UnityEditor.Audio.AudioMixerController, UnityEditor");
            if (controllerType == null)
            {
                throw new InvalidOperationException("UnityEditor.Audio.AudioMixerController is unavailable.");
            }

            MethodInfo createMethod = controllerType.GetMethod("CreateMixerControllerAtPath", BindingFlags.Public | BindingFlags.Static);
            if (createMethod == null)
            {
                throw new InvalidOperationException("AudioMixerController.CreateMixerControllerAtPath is unavailable.");
            }

            createMethod.Invoke(null, new object[] { MixerPath });
            AssetDatabase.ImportAsset(MixerPath);
            mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer == null)
            {
                throw new InvalidOperationException("Failed to create AudioMixer at " + MixerPath);
            }

            return mixer;
        }

        private static void EnsureCleanAudioMixerAsset()
        {
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (!ShouldRebuildMixer(mixer)) return;

            Debug.Log("[StrategyAudioAssetBuilder] Rebuilding strategy audio mixer to remove duplicate layer groups.");
            string mixerFile = GetMixerFilesystemPath();
            if (File.Exists(mixerFile))
            {
                File.Delete(mixerFile);
            }

            EnsureMixerMetaGuid();
            AssetDatabase.ImportAsset("Assets/Audio/Mixers");
            AssetDatabase.Refresh();
        }

        private static bool ShouldRebuildMixer(AudioMixer mixer)
        {
            if (HasDuplicateSerializedLayerGroups()) return true;
            if (mixer == null) return false;

            object controller = mixer;
            HashSet<UnityEngine.Object> activeChildren = GetMasterChildObjects(controller);
            int strategyGroupCount = 0;
            int activeStrategyChildCount = 0;

            System.Collections.IEnumerable groups = TryInvoke(controller, "GetAllAudioGroupsSlow") as System.Collections.IEnumerable;
            if (groups == null) return false;

            foreach (object group in groups)
            {
                UnityEngine.Object unityGroup = group as UnityEngine.Object;
                if (unityGroup == null) continue;
                if (!IsStrategyLayerGroupName(unityGroup.name)) continue;

                strategyGroupCount++;
                if (activeChildren.Contains(unityGroup))
                {
                    activeStrategyChildCount++;
                }
            }

            return strategyGroupCount > 5 || activeStrategyChildCount > 5;
        }

        private static bool HasDuplicateSerializedLayerGroups()
        {
            string mixerFile = GetMixerFilesystemPath();
            if (!File.Exists(mixerFile)) return false;

            int strategyLayerGroupCount = 0;
            bool inGroupController = false;
            foreach (string line in File.ReadLines(mixerFile))
            {
                if (line.StartsWith("--- !u!", StringComparison.Ordinal))
                {
                    inGroupController = false;
                    continue;
                }

                if (line.Equals("AudioMixerGroupController:", StringComparison.Ordinal))
                {
                    inGroupController = true;
                    continue;
                }

                if (!inGroupController || !line.StartsWith("  m_Name: ", StringComparison.Ordinal)) continue;

                string groupName = line.Substring("  m_Name: ".Length).Trim();
                if (IsStrategyLayerGroupName(groupName))
                {
                    strategyLayerGroupCount++;
                }

                inGroupController = false;
            }

            return strategyLayerGroupCount > 5;
        }

        private static string GetMixerFilesystemPath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, MixerPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static void EnsureMixerMetaGuid()
        {
            string metaFile = GetMixerFilesystemPath() + ".meta";
            if (File.Exists(metaFile) && HasHexMetaGuid(metaFile)) return;

            File.WriteAllText(
                metaFile,
                "fileFormatVersion: 2\n" +
                "guid: " + MixerGuid + "\n" +
                "NativeFormatImporter:\n" +
                "  externalObjects: {}\n" +
                "  mainObjectFileID: 24100000\n" +
                "  userData: \n" +
                "  assetBundleName: \n" +
                "  assetBundleVariant: \n");
        }

        private static bool HasHexMetaGuid(string metaFile)
        {
            foreach (string line in File.ReadLines(metaFile))
            {
                if (!line.StartsWith("guid: ", StringComparison.Ordinal)) continue;
                string guid = line.Substring("guid: ".Length).Trim();
                if (guid.Length != 32) return false;

                for (int i = 0; i < guid.Length; i++)
                {
                    char value = guid[i];
                    bool hex =
                        (value >= '0' && value <= '9') ||
                        (value >= 'a' && value <= 'f') ||
                        (value >= 'A' && value <= 'F');
                    if (!hex) return false;
                }

                return true;
            }

            return false;
        }

        private static void EnsureMixerGroups(AudioMixer mixer)
        {
            if (mixer == null) return;

            EnsureMixerGroup(mixer, "Music");
            EnsureMixerGroup(mixer, "Ambience");
            EnsureMixerGroup(mixer, "SFX");
            EnsureMixerGroup(mixer, "UI");
            EnsureMixerGroup(mixer, "War");
            EditorUtility.SetDirty(mixer);
        }

        private static void EnsureMixerGroup(AudioMixer mixer, string groupName)
        {
            object controller = mixer;
            object group = FindMasterChildGroup(controller, groupName) ?? FindExactGroupController(controller, groupName);
            if (group == null)
            {
                group = TryInvoke(controller, "CreateNewGroup", groupName, true);
            }

            if (group == null)
            {
                group = TryInvoke(controller, "CreateNewGroup", groupName, false);
            }

            if (group == null)
            {
                group = TryInvoke(controller, "CreateNewGroup", groupName);
            }

            if (group == null)
            {
                group = FindExactGroupController(controller, groupName);
            }

            if (group != null)
            {
                EnsureGroupUnderMaster(controller, group);
                EnsureGroupInCurrentView(controller, group);
                return;
            }

            if (FindExactGroup(mixer, groupName) == null)
            {
                Debug.LogWarning("[StrategyAudioAssetBuilder] Unable to create mixer group: " + groupName);
            }
        }

        private static void PruneDuplicateLayerGroups(AudioMixer mixer)
        {
            if (mixer == null) return;

            object controller = mixer;
            HashSet<UnityEngine.Object> activeChildren = GetMasterChildObjects(controller);
            List<object> duplicates = new List<object>();
            System.Collections.IEnumerable groups = TryInvoke(controller, "GetAllAudioGroupsSlow") as System.Collections.IEnumerable;
            if (groups == null) return;

            foreach (object group in groups)
            {
                UnityEngine.Object unityGroup = group as UnityEngine.Object;
                if (unityGroup == null || activeChildren.Contains(unityGroup)) continue;
                if (IsStrategyLayerGroupName(unityGroup.name))
                {
                    duplicates.Add(group);
                }
            }

            DeleteGroups(controller, duplicates);
            if (duplicates.Count > 0)
            {
                TryInvoke(controller, "SanitizeGroupViews");
                EditorUtility.SetDirty(mixer);
            }
        }

        private static void EnsureMixerSnapshots(AudioMixer mixer)
        {
            if (mixer == null) return;

            EnsureSnapshot(mixer, "Governance");
            EnsureSnapshot(mixer, "War");
            EnsureSnapshot(mixer, "Event");
            EditorUtility.SetDirty(mixer);
        }

        private static void EnsureSnapshot(AudioMixer mixer, string snapshotName)
        {
            if (mixer.FindSnapshot(snapshotName) != null) return;

            object controller = mixer;
            object snapshot = FindSnapshotController(controller, snapshotName);
            if (snapshot == null)
            {
                object[] snapshots = GetSnapshotControllers(controller);
                if (snapshots.Length == 1 && ObjectNameEquals(snapshots[0], "Snapshot"))
                {
                    snapshot = snapshots[0];
                }
            }

            if (snapshot == null)
            {
                snapshot = CloneSnapshotFromTarget(controller);
            }

            if (snapshot != null)
            {
                SetObjectName(snapshot, snapshotName);
                UnityEngine.Object snapshotObject = snapshot as UnityEngine.Object;
                if (snapshotObject != null) EditorUtility.SetDirty(snapshotObject);
                EditorUtility.SetDirty(mixer);
            }
            else if (mixer.FindSnapshot(snapshotName) == null)
            {
                Debug.LogWarning("[StrategyAudioAssetBuilder] Unable to create mixer snapshot: " + snapshotName);
            }
        }

        private static void BuildCueLibrary(AudioMixer mixer)
        {
            AudioCueLibrary library = AssetDatabase.LoadAssetAtPath<AudioCueLibrary>(CueLibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<AudioCueLibrary>();
                AssetDatabase.CreateAsset(library, CueLibraryPath);
            }

            SerializedObject serialized = new SerializedObject(library);
            WriteMixerRoutes(serialized.FindProperty("mixerRoutes"), mixer);
            WriteSnapshots(serialized.FindProperty("snapshots"), mixer);
            WriteSceneMusic(serialized.FindProperty("sceneMusic"));
            WriteRegionMusic(serialized.FindProperty("regionMusic"));
            WriteRuntimeCues(serialized.FindProperty("cues"));
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(library);
        }

        private static void WriteMixerRoutes(SerializedProperty routes, AudioMixer mixer)
        {
            if (routes == null) return;

            StrategyAudioLayer[] layers =
            {
                StrategyAudioLayer.Master,
                StrategyAudioLayer.Music,
                StrategyAudioLayer.Ambience,
                StrategyAudioLayer.SFX,
                StrategyAudioLayer.UI,
                StrategyAudioLayer.War
            };

            routes.arraySize = layers.Length;
            for (int i = 0; i < layers.Length; i++)
            {
                SerializedProperty route = routes.GetArrayElementAtIndex(i);
                StrategyAudioLayer layer = layers[i];
                route.FindPropertyRelative("layer").enumValueIndex = (int)layer;
                route.FindPropertyRelative("mixerGroup").objectReferenceValue = ResolveMixerGroup(mixer, layer);
                route.FindPropertyRelative("exposedVolumeParameter").stringValue = layer + "Volume";
                route.FindPropertyRelative("defaultWeight").floatValue = 1f;
            }
        }

        private static void WriteSnapshots(SerializedProperty snapshots, AudioMixer mixer)
        {
            if (snapshots == null) return;

            snapshots.arraySize = 3;
            WriteSnapshot(snapshots.GetArrayElementAtIndex(0), mixer, "Governance", 0.65f, new float[] { 1f, 0.88f, 0.82f, 1f, 1f, 0.12f });
            WriteSnapshot(snapshots.GetArrayElementAtIndex(1), mixer, "War", 0.35f, new float[] { 1f, 0.62f, 0.36f, 1f, 1f, 0.98f });
            WriteSnapshot(snapshots.GetArrayElementAtIndex(2), mixer, "Event", 0.5f, new float[] { 1f, 0.72f, 0.58f, 0.9f, 1f, 0.42f });
        }

        private static void WriteSnapshot(SerializedProperty snapshot, AudioMixer mixer, string snapshotName, float transitionSeconds, float[] weights)
        {
            snapshot.FindPropertyRelative("name").stringValue = snapshotName;
            snapshot.FindPropertyRelative("mixerSnapshot").objectReferenceValue = mixer != null ? mixer.FindSnapshot(snapshotName) : null;
            snapshot.FindPropertyRelative("transitionSeconds").floatValue = transitionSeconds;

            SerializedProperty layerWeights = snapshot.FindPropertyRelative("weights");
            layerWeights.arraySize = weights.Length;
            for (int i = 0; i < weights.Length; i++)
            {
                SerializedProperty layerWeight = layerWeights.GetArrayElementAtIndex(i);
                layerWeight.FindPropertyRelative("layer").enumValueIndex = i;
                layerWeight.FindPropertyRelative("weight").floatValue = weights[i];
            }
        }

        private static void WriteSceneMusic(SerializedProperty sceneMusic)
        {
            if (sceneMusic == null) return;

            SceneMusicData data = LoadJson<SceneMusicData>(SceneManifestPath);
            SceneMusicItem[] items = data != null && data.items != null ? data.items : new SceneMusicItem[0];
            sceneMusic.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                SerializedProperty entry = sceneMusic.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("sceneName").stringValue = items[i].scene;
                entry.FindPropertyRelative("cueId").stringValue = items[i].musicCueId;
                entry.FindPropertyRelative("clip").objectReferenceValue = LoadClip(SceneClipRoot, items[i].fileName);
            }
        }

        private static void WriteRegionMusic(SerializedProperty regionMusic)
        {
            if (regionMusic == null) return;

            RegionsData regions = LoadJson<RegionsData>(RegionsDataPath);
            RegionDataItem[] items = regions != null && regions.items != null ? regions.items : new RegionDataItem[0];
            Dictionary<string, HistoricalLayerItem> historicalLayers = LoadHistoricalLayersByRegionId();

            regionMusic.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                RegionDataItem region = items[i];
                string regionId = region != null ? region.id : string.Empty;
                HistoricalLayerItem historicalLayer;
                historicalLayers.TryGetValue(regionId ?? string.Empty, out historicalLayer);

                RegionMusicAsset asset = ResolveRegionMusicAsset(region, historicalLayer);
                SerializedProperty entry = regionMusic.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("regionId").stringValue = regionId;
                entry.FindPropertyRelative("cueId").stringValue = "region_music_" + regionId;
                entry.FindPropertyRelative("clip").objectReferenceValue = LoadClipAtPath(asset.assetPath);
            }
        }

        private static void WriteRuntimeCues(SerializedProperty cues)
        {
            if (cues == null) return;

            List<CueAssetRecord> records = new List<CueAssetRecord>();
            EmperorThemeData emperors = LoadJson<EmperorThemeData>(EmperorManifestPath);
            if (emperors != null && emperors.items != null)
            {
                for (int i = 0; i < emperors.items.Length; i++)
                {
                    EmperorThemeItem item = emperors.items[i];
                    records.Add(new CueAssetRecord(item.musicCueId, LoadClip(EmperorClipRoot, item.fileName), StrategyAudioLayer.Music, false, 1f, 0f, true, 96));
                }
            }

            ChronicleEventData chronicle = LoadJson<ChronicleEventData>(ChronicleManifestPath);
            if (chronicle != null && chronicle.items != null)
            {
                for (int i = 0; i < chronicle.items.Length; i++)
                {
                    ChronicleEventItem item = chronicle.items[i];
                    StrategyAudioLayer layer = IsMilitaryCategory(item.category) ? StrategyAudioLayer.War : StrategyAudioLayer.Music;
                    records.Add(new CueAssetRecord(item.musicCueId, LoadClip(ChronicleClipRoot, item.fileName), layer, false, 0.9f, 0f, true, 96));
                }
            }

            cues.arraySize = records.Count;
            for (int i = 0; i < records.Count; i++)
            {
                CueAssetRecord record = records[i];
                SerializedProperty cue = cues.GetArrayElementAtIndex(i);
                cue.FindPropertyRelative("id").stringValue = record.id;
                cue.FindPropertyRelative("clip").objectReferenceValue = record.clip;
                cue.FindPropertyRelative("layer").enumValueIndex = (int)record.layer;
                cue.FindPropertyRelative("spatial").boolValue = record.spatial;
                cue.FindPropertyRelative("volume").floatValue = record.volume;
                cue.FindPropertyRelative("spatialBlend").floatValue = record.spatialBlend;
                cue.FindPropertyRelative("loop").boolValue = record.loop;
                cue.FindPropertyRelative("priority").intValue = record.priority;
            }
        }

        private static AudioClip LoadClip(string root, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            return AssetDatabase.LoadAssetAtPath<AudioClip>(root + fileName);
        }

        private static AudioClip LoadClipAtPath(string assetPath)
        {
            return string.IsNullOrEmpty(assetPath) ? null : AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }

        private static T LoadJson<T>(string path) where T : class
        {
            TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            return text != null ? JsonUtility.FromJson<T>(text.text) : null;
        }

        private static Dictionary<string, HistoricalLayerItem> LoadHistoricalLayersByRegionId()
        {
            Dictionary<string, HistoricalLayerItem> layers = new Dictionary<string, HistoricalLayerItem>(StringComparer.OrdinalIgnoreCase);
            HistoricalLayerData data = LoadJson<HistoricalLayerData>(HistoricalLayersDataPath);
            if (data == null || data.items == null) return layers;

            for (int i = 0; i < data.items.Length; i++)
            {
                HistoricalLayerItem item = data.items[i];
                if (item == null || string.IsNullOrEmpty(item.regionId)) continue;
                layers[item.regionId] = item;
            }

            return layers;
        }

        private static RegionMusicAsset ResolveRegionMusicAsset(RegionDataItem region, HistoricalLayerItem historicalLayer)
        {
            string id = region != null ? region.id : string.Empty;
            string terrain = region != null ? region.terrain : string.Empty;

            if (Matches(id, terrain, historicalLayer, "guanzhong", "xianyang", "yongzhou", "qin_han_ritual_core", "law"))
            {
                return new RegionMusicAsset(EmperorClipRoot + "emperor_qin_shi_huang.mp3");
            }

            if (Matches(id, terrain, historicalLayer, "chang_an", "luoyang", "luoyi", "capital", "ritual", "imperial_core"))
            {
                return new RegionMusicAsset(EmperorClipRoot + "emperor_li_shi_min.mp3");
            }

            if (Matches(id, terrain, historicalLayer, "bashu", "chengdu", "yizhou", "basin"))
            {
                return new RegionMusicAsset(EmperorClipRoot + "emperor_liu_bei.mp3");
            }

            if (Matches(id, terrain, historicalLayer, "xuchang", "ye", "guandu", "yanzhou", "military", "frontier_clans"))
            {
                return new RegionMusicAsset(EmperorClipRoot + "emperor_cao_cao.mp3");
            }

            if (Matches(id, terrain, historicalLayer, "frontier", "corridor", "horse", "border", "wuyuan", "shuofang", "liangzhou", "longxi", "hexi", "xiyu", "xiazhou", "cavalry", "frontier_scouts"))
            {
                return new RegionMusicAsset(ChronicleClipRoot + "event_huo_qubing_lightning_war.mp3");
            }

            if (Matches(id, terrain, historicalLayer, "river_delta", "jiangdong", "jianye", "yangzhou", "jingkou", "guangling"))
            {
                return new RegionMusicAsset(SceneClipRoot + "scene_golden_age.mp3");
            }

            if (Matches(id, terrain, historicalLayer, "river_plain", "huainan", "shouchun", "jingxiang", "jingzhou"))
            {
                return new RegionMusicAsset(ChronicleClipRoot + "event_yellow_river_flood.mp3");
            }

            if (Matches(id, terrain, historicalLayer, "subtropical", "nanhai", "lingnan", "jiaozhi", "qinzhou", "yongzhou_south"))
            {
                return new RegionMusicAsset(SceneClipRoot + "scene_banquet.mp3");
            }

            if (Matches(id, terrain, historicalLayer, "mountain", "plateau", "minyue", "yun_gui", "jianning", "dali", "guizhou", "haicheng"))
            {
                return new RegionMusicAsset(ChronicleClipRoot + "event_xuanzang_pilgrimage.mp3");
            }

            if (Matches(id, terrain, historicalLayer, "qilu", "qingzhou", "taishan", "culture", "academy"))
            {
                return new RegionMusicAsset(SceneClipRoot + "scene_unification.mp3");
            }

            return new RegionMusicAsset(SceneClipRoot + "scene_governance.mp3");
        }

        private static bool Matches(string id, string terrain, HistoricalLayerItem historicalLayer, params string[] needles)
        {
            for (int i = 0; i < needles.Length; i++)
            {
                string needle = needles[i];
                if (ContainsIgnoreCase(id, needle) || ContainsIgnoreCase(terrain, needle))
                {
                    return true;
                }

                if (ContainsAny(historicalLayer != null ? historicalLayer.geographyTags : null, needle) ||
                    ContainsAny(historicalLayer != null ? historicalLayer.customTags : null, needle) ||
                    ContainsAny(historicalLayer != null ? historicalLayer.strategicResources : null, needle) ||
                    ContainsAny(historicalLayer != null ? historicalLayer.weaponTraditions : null, needle))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsAny(string[] values, string needle)
        {
            if (values == null) return false;
            for (int i = 0; i < values.Length; i++)
            {
                if (ContainsIgnoreCase(values[i], needle)) return true;
            }

            return false;
        }

        private static bool ContainsIgnoreCase(string value, string needle)
        {
            return !string.IsNullOrEmpty(value) &&
                !string.IsNullOrEmpty(needle) &&
                value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static AudioMixerGroup ResolveMixerGroup(AudioMixer mixer, StrategyAudioLayer layer)
        {
            if (mixer == null) return null;
            if (layer == StrategyAudioLayer.Master && mixer.outputAudioMixerGroup != null)
            {
                return mixer.outputAudioMixerGroup;
            }

            AudioMixerGroup exact = FindExactGroup(mixer, layer.ToString());
            return exact != null ? exact : mixer.outputAudioMixerGroup;
        }

        private static AudioMixerGroup FindExactGroup(AudioMixer mixer, string groupName)
        {
            if (mixer == null) return null;
            AudioMixerGroup[] groups = mixer.FindMatchingGroups(groupName);
            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i] != null && groups[i].name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                {
                    return groups[i];
                }
            }

            return null;
        }

        private static void EnsureGroupUnderMaster(object controller, object group)
        {
            object masterGroup = GetPropertyValue(controller, "masterGroup");
            UnityEngine.Object masterObject = masterGroup as UnityEngine.Object;
            UnityEngine.Object groupObject = group as UnityEngine.Object;
            if (masterObject == null || groupObject == null || ReferenceEquals(masterObject, groupObject)) return;

            SerializedObject serializedMaster = new SerializedObject(masterObject);
            SerializedProperty children = serializedMaster.FindProperty("m_Children");
            if (children == null || ContainsObjectReference(children, groupObject)) return;

            int index = children.arraySize;
            children.InsertArrayElementAtIndex(index);
            children.GetArrayElementAtIndex(index).objectReferenceValue = groupObject;
            serializedMaster.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(masterObject);
        }

        private static HashSet<UnityEngine.Object> GetMasterChildObjects(object controller)
        {
            HashSet<UnityEngine.Object> children = new HashSet<UnityEngine.Object>();
            object masterGroup = GetPropertyValue(controller, "masterGroup");
            UnityEngine.Object masterObject = masterGroup as UnityEngine.Object;
            if (masterObject == null) return children;

            SerializedObject serializedMaster = new SerializedObject(masterObject);
            SerializedProperty childArray = serializedMaster.FindProperty("m_Children");
            if (childArray == null) return children;

            for (int i = 0; i < childArray.arraySize; i++)
            {
                UnityEngine.Object child = childArray.GetArrayElementAtIndex(i).objectReferenceValue;
                if (child != null)
                {
                    children.Add(child);
                }
            }

            return children;
        }

        private static void DeleteGroups(object controller, List<object> groups)
        {
            if (controller == null || groups == null || groups.Count == 0) return;

            Type groupType = groups[0].GetType();
            Array groupArray = Array.CreateInstance(groupType, groups.Count);
            for (int i = 0; i < groups.Count; i++)
            {
                groupArray.SetValue(groups[i], i);
            }

            TryInvoke(controller, "DeleteGroups", groupArray);
        }

        private static object FindExactGroupController(object controller, string groupName)
        {
            if (controller == null || string.IsNullOrEmpty(groupName)) return null;

            System.Collections.IEnumerable groups = TryInvoke(controller, "GetAllAudioGroupsSlow") as System.Collections.IEnumerable;
            if (groups != null)
            {
                foreach (object group in groups)
                {
                    if (ObjectNameEquals(group, groupName))
                    {
                        return group;
                    }
                }
            }

            AudioMixer mixer = controller as AudioMixer;
            return mixer != null ? FindExactGroup(mixer, groupName) : null;
        }

        private static object FindMasterChildGroup(object controller, string groupName)
        {
            if (controller == null || string.IsNullOrEmpty(groupName)) return null;

            object masterGroup = GetPropertyValue(controller, "masterGroup");
            UnityEngine.Object masterObject = masterGroup as UnityEngine.Object;
            if (masterObject == null) return null;

            SerializedObject serializedMaster = new SerializedObject(masterObject);
            SerializedProperty children = serializedMaster.FindProperty("m_Children");
            if (children == null) return null;

            for (int i = 0; i < children.arraySize; i++)
            {
                UnityEngine.Object child = children.GetArrayElementAtIndex(i).objectReferenceValue;
                if (child != null && child.name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }
            }

            return null;
        }

        private static object FindSnapshotController(object controller, string snapshotName)
        {
            object[] snapshots = GetSnapshotControllers(controller);
            for (int i = 0; i < snapshots.Length; i++)
            {
                if (ObjectNameEquals(snapshots[i], snapshotName))
                {
                    return snapshots[i];
                }
            }

            return null;
        }

        private static object CloneSnapshotFromTarget(object controller)
        {
            object[] before = GetSnapshotControllers(controller);
            TryInvoke(controller, "CloneNewSnapshotFromTarget", true);
            object[] after = GetSnapshotControllers(controller);

            if (after.Length > before.Length)
            {
                for (int i = 0; i < after.Length; i++)
                {
                    bool existed = false;
                    for (int b = 0; b < before.Length; b++)
                    {
                        if (ReferenceEquals(after[i], before[b]))
                        {
                            existed = true;
                            break;
                        }
                    }

                    if (!existed) return after[i];
                }

                return after[after.Length - 1];
            }

            return null;
        }

        private static object[] GetSnapshotControllers(object controller)
        {
            System.Collections.IEnumerable snapshots = GetPropertyValue(controller, "snapshots") as System.Collections.IEnumerable;
            if (snapshots == null) return new object[0];

            List<object> values = new List<object>();
            foreach (object snapshot in snapshots)
            {
                if (snapshot != null)
                {
                    values.Add(snapshot);
                }
            }

            return values.ToArray();
        }

        private static void EnsureGroupInCurrentView(object controller, object group)
        {
            if (controller == null || group == null) return;

            if (!CurrentViewContainsGroup(controller, group))
            {
                TryInvoke(controller, "AddGroupToCurrentView", group);
            }

            TryInvoke(controller, "SanitizeGroupViews");
        }

        private static bool CurrentViewContainsGroup(object controller, object targetGroup)
        {
            System.Collections.IEnumerable groups = TryInvoke(controller, "GetCurrentViewGroupList") as System.Collections.IEnumerable;
            if (groups == null) return false;

            string targetName = GetObjectName(targetGroup);
            foreach (object group in groups)
            {
                if (ReferenceEquals(group, targetGroup) || (!string.IsNullOrEmpty(targetName) && ObjectNameEquals(group, targetName)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ObjectNameEquals(object value, string expectedName)
        {
            string name = GetObjectName(value);
            return !string.IsNullOrEmpty(name) && name.Equals(expectedName, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetObjectName(object value)
        {
            UnityEngine.Object unityObject = value as UnityEngine.Object;
            if (unityObject != null) return unityObject.name;

            PropertyInfo property = value != null ? value.GetType().GetProperty("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) : null;
            object propertyValue = property != null ? property.GetValue(value, null) : null;
            return propertyValue as string;
        }

        private static void SetObjectName(object value, string name)
        {
            if (value == null || string.IsNullOrEmpty(name)) return;

            UnityEngine.Object unityObject = value as UnityEngine.Object;
            if (unityObject != null)
            {
                unityObject.name = name;
                return;
            }

            PropertyInfo property = value.GetType().GetProperty("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.CanWrite)
            {
                property.SetValue(value, name, null);
            }
        }

        private static object GetPropertyValue(object target, string propertyName)
        {
            if (target == null || string.IsNullOrEmpty(propertyName)) return null;

            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return property != null ? property.GetValue(target, null) : null;
        }

        private static bool ContainsObjectReference(SerializedProperty arrayProperty, UnityEngine.Object expected)
        {
            if (arrayProperty == null || expected == null) return false;

            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                if (arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue == expected)
                {
                    return true;
                }
            }

            return false;
        }

        private static object TryInvoke(object target, string methodName, params object[] args)
        {
            if (target == null) return null;

            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name != methodName) continue;

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != args.Length) continue;

                try
                {
                    return method.Invoke(target, args);
                }
                catch (TargetInvocationException)
                {
                }
                catch (ArgumentException)
                {
                }
            }

            return null;
        }

        private static bool IsMilitaryCategory(string category)
        {
            return !string.IsNullOrEmpty(category) && category.Equals("military", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStrategyLayerGroupName(string groupName)
        {
            return groupName == "Music" ||
                groupName == "Ambience" ||
                groupName == "SFX" ||
                groupName == "UI" ||
                groupName == "War";
        }

        private static void EnsureAssetFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;

            string[] parts = assetPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private sealed class CueAssetRecord
        {
            public readonly string id;
            public readonly AudioClip clip;
            public readonly StrategyAudioLayer layer;
            public readonly bool spatial;
            public readonly float volume;
            public readonly float spatialBlend;
            public readonly bool loop;
            public readonly int priority;

            public CueAssetRecord(string id, AudioClip clip, StrategyAudioLayer layer, bool spatial, float volume, float spatialBlend, bool loop, int priority)
            {
                this.id = id;
                this.clip = clip;
                this.layer = layer;
                this.spatial = spatial;
                this.volume = volume;
                this.spatialBlend = spatialBlend;
                this.loop = loop;
                this.priority = priority;
            }
        }

        private struct RegionMusicAsset
        {
            public readonly string assetPath;

            public RegionMusicAsset(string assetPath)
            {
                this.assetPath = assetPath;
            }
        }

        [Serializable]
        private sealed class RegionsData
        {
            public RegionDataItem[] items;
        }

        [Serializable]
        private sealed class RegionDataItem
        {
            public string id;
            public string terrain;
        }

        [Serializable]
        private sealed class HistoricalLayerData
        {
            public HistoricalLayerItem[] items;
        }

        [Serializable]
        private sealed class HistoricalLayerItem
        {
            public string regionId;
            public string[] geographyTags;
            public string[] customTags;
            public string[] strategicResources;
            public string[] weaponTraditions;
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
            public string musicCueId;
            public string fileName;
            public string category;
        }
    }
}
