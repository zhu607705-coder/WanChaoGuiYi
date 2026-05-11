/**
 * 万朝归一 - 音乐场景清单
 *
 * 场景音乐 (Scene Music):
 * - 施政 (Governance): 和平时期，治理天下
 * - 战争 (War): 战时状态，边境冲突
 * - 宫变 (PalaceConspiracy): 宫廷政变阴谋
 * - 饥荒 (Famine): 自然灾害与饥荒
 * - 盛世 (GoldenAge): 盛世华章，万国来朝
 * - 末世 (Decline): 王朝衰落，崩溃前夜
 * - 统一 (Unification): 天下归一，新朝建立
 * - 宴会 (Banquet): 宫廷宴席，欢庆
 * - 出征 (Campaign): 大军出征，行军
 * - 丧钟 (Funeral): 王朝覆灭，哀乐
 *
 * 与 AudioManager 的 StrategyAudioLayer 对应:
 * - Governance 场景: Music=0.88, Ambience=0.82, War=0.12
 * - War 场景: Music=0.62, Ambience=0.36, War=0.98
 * - Event 场景: Music=0.72, Ambience=0.58, War=0.42
 */

namespace WanChaoGuiYi.Audio
{
    [System.Serializable]
    public enum StrategyScene
    {
        Governance,       // 施政
        War,             // 战争
        PalaceConspiracy, // 宫变
        Famine,          // 饥荒
        GoldenAge,       // 盛世
        Decline,         // 末世
        Unification,     // 统一
        Banquet,         // 宴会
        Campaign,        // 出征
        Funeral          // 丧钟
    }

    [System.Serializable]
    public class SceneMusicTrack
    {
        public StrategyScene scene;
        public string musicCueId;
        public string fileName;
        public string mood;
        public int bpm;
        public string[] tags;
    }
}