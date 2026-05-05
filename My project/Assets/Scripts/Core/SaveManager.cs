using System.IO;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class SaveManager : MonoBehaviour
    {
        private const string DefaultSaveName = "wanchao_save.json";

        public void Save(GameState state, string saveName = DefaultSaveName)
        {
            string path = BuildPath(saveName);
            string json = JsonUtility.ToJson(state, true);
            File.WriteAllText(path, json);
        }

        public GameState Load(string saveName = DefaultSaveName)
        {
            string path = BuildPath(saveName);
            if (!File.Exists(path))
            {
                Debug.LogWarning("Save file not found: " + path);
                return null;
            }

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameState>(json);
        }

        private static string BuildPath(string saveName)
        {
            return Path.Combine(Application.persistentDataPath, saveName);
        }
    }
}
