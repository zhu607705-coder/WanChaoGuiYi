/**
 * 万朝归一 - 角色（帝王）主题音乐清单
 *
 * 13位帝王各有独特音乐主题，反映其历史背景和治国风格：
 *
 * qin_shi_hang   秦始皇  - 威严霸道，一统六合
 * liu_bang       刘邦    - 草根崛起，天命所归
 * han_wu_di      汉武帝  - 雄才大略，开疆拓土
 * cao_cao        曹操    - 乱世奸雄，慷慨悲歌
 * li_shi_min     李世民  - 天可汗，万国来朝
 * zhao_kuang_yin 赵匡胤  - 杯酒释兵权，文治天下
 * zhu_yuan_zhang 朱元璋  - 驱逐胡尘，重建华夏
 * kang_xi        康熙    - 康熙盛世，多元一体
 * yang_jian      杨坚    - 结束乱世，重归一统
 * chai_rong      柴荣    - 中原振兴，北伐幽燕
 * yuan_hong      元宏    - 孝文改革，汉化融合
 * shi_le         石勒    - 从奴隶到皇帝，逆天改命
 * liu_bei        刘备    - 汉室正统，仁德天下
 */

namespace WanChaoGuiYi.Audio
{
    [System.Serializable]
    public class EmperorThemeTrack
    {
        public string emperorId;
        public string musicCueId;
        public string fileName;
        public string mood;
        public int bpm;
        public string[] tags;
        public string historicalContext; // 历史背景描述
    }
}