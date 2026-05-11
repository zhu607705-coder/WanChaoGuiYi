/**
 * 万朝归一 - 历史大事件音乐清单
 *
 * 200个编年事件中，精选最重要、最有戏剧张力的30+个事件
 * 每个事件有专属音乐，在事件发生时触发
 *
 * 分类:
 * - 军事 (military): 战争、战役、军事行动
 * - 政治 (political): 政变、禅让、政治事件
 * - 灾害 (disaster): 洪水、干旱、瘟疫
 * - 盛世 (prosperity): 盛世景象、文化繁荣
 * - 阴谋 (conspiracy): 宫廷阴谋、刺杀
 */

namespace WanChaoGuiYi.Audio
{
    [System.Serializable]
    public enum ChronicleEventCategory
    {
        Military,
        Political,
        Disaster,
        Prosperity,
        Conspiracy
    }

    [System.Serializable]
    public class ChronicleEventMusicTrack
    {
        public string eventId;
        public string musicCueId;
        public string fileName;
        public ChronicleEventCategory category;
        public string mood;
        public int bpm;
        public string[] tags;
    }
}