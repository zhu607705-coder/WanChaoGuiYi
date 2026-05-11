# 万朝归一 - 完整场景配乐设计方案

> 版本：v2.0
> 更新：2026-05-09
> 说明：扩展至40+场景，覆盖内政、外交、军事、事件全过程

---

## 一、现有配乐盘点

### 1.1 已有场景音乐（10首）

| 场景ID | 名称 | 情绪 | BPM | 状态 |
|--------|------|------|-----|------|
| scene_governance | 施政 | 和平肃穆 | 70 | ✅ |
| scene_war | 战争 | 激烈紧张 | 150 | ✅ |
| scene_palace_conspiracy | 宫变 | 紧张神秘 | 85 | ✅ |
| scene_famine | 饥荒 | 阴沉悲伤 | 55 | ✅ |
| scene_golden_age | 盛世 | 辉煌胜利 | 120 | ✅ |
| scene_decline | 末世 | 阴沉末日 | 60 | ✅ |
| scene_unification | 统一 | 凯旋辉煌 | 140 | ✅ |
| scene_banquet | 宴会 | 优雅欢快 | 90 | ✅ |
| scene_campaign | 出征 | 慷慨激昂 | 130 | ✅ |
| scene_funeral | 丧钟 | 悲壮哀婉 | 45 | ✅ |

### 1.2 已有编年事件音乐（29首）

覆盖：灾难(4)、军事(10)、政治(12)、盛世(3)

---

## 二、新增配乐设计方案（30首）

### 2.1 内政类 - Civil Governance (8首)

#### 【CIV-01】科举考试 Imperial Examination
```
场景描述：科举考试选拔人才，士子云集
情绪：紧张期待、公平公正
BPM：85
时长：60-90秒
乐器：古筝主旋律 + 琵琶 + 编钟 + 轻鼓
特点：
  - 开篇：紧张的等待氛围
  - 中段：公平竞争的庄严
  - 尾段：揭晓的期待与喜悦
音乐风格：古典庄重，带有学术气息
文件：scene_examination.mp3
```

#### 【CIV-02】农业丰收 Harvest Festival
```
场景描述：丰收季节，百姓欢庆
情绪：欢乐祥和、感恩自然
BPM：110
时长：60-90秒
乐器：笛子 + 笙 + 鼓 + 锣
特点：
  - 主旋律：欢快的笛子
  - 节奏：轻快的鼓点
  - 特色：加入农具声响采样
音乐风格：民间欢乐，田园诗意
文件：scene_harvest.mp3
```

#### 【CIV-03】商业繁荣 Trade Prosperity
```
场景描述：丝绸之路贸易繁盛，商旅往来
情绪：繁华热闹、商业活力
BPM：100
时长：60-90秒
乐器：琵琶 + 二胡 + 铃铛 + 鼓
特点：
  - 开篇：驼铃声与远方商队
  - 中段：市集繁荣的热闹
  - 尾段：交易成功的喜悦
音乐风格：丝绸之路异域风情
文件：scene_trade.mp3
```

#### 【CIV-04】建筑工程 Construction
```
场景描述：修建长城、宫殿、运河等大工程
情绪：宏伟壮观、人定胜天
BPM：95
时长：90-120秒
乐器：鼓 + 铙钹 + 古筝 + 低音弦乐
特点：
  - 节奏：劳动号子与鼓点结合
  - 特色：体现劳动人民的伟力
  - 情绪：逐渐走向辉煌
音乐风格：史诗般宏大
文件：scene_construction.mp3
```

#### 【CIV-05】祭祀典礼 Ancestor Worship
```
场景描述：祭祀天地祖先，祈求庇佑
情绪：庄严肃穆、神秘虔诚
BPM：60
时长：90-120秒
乐器：编钟 + 磬 + 笙 + 古琴
特点：
  - 开篇：寂静神秘
  - 中段：钟磬齐鸣的庄严
  - 尾段：祈求的虔诚
音乐风格：上古神秘，祭祀音乐
文件：scene_ceremony.mp3
```

#### 【CIV-06】瘟疫蔓延 Plague
```
场景描述：瘟疫横行，哀鸿遍野
情绪：阴森恐怖、绝望无助
BPM：50
时长：60-90秒
乐器：箫 + 低音弦乐 + 锣 + 颤音琴
特点：
  - 开篇：诡异的氛围
  - 中段：疫情蔓延的紧张
  - 尾段：绝望的哀鸣
音乐风格：阴郁压抑，带有不祥之感
文件：scene_plague.mp3
```

#### 【CIV-07】改革变法 Reform
```
场景描述：推行新政变法，触动利益集团
情绪：紧张激进、阻力重重
BPM：105
时长：60-90秒
乐器：鼓 + 琵琶急促 + 古筝 + 弦乐
特点：
  - 开篇：改革的决心
  - 中段：遭遇阻力的紧张
  - 尾段：成败未定的悬念
音乐风格：紧张激烈，带有冲突感
文件：scene_reform.mp3
```

#### 【CIV-08】灾害预警 Natural Disaster
```
场景描述：洪水、旱灾预警，朝廷应对
情绪：紧张紧迫、危机四伏
BPM：90
时长：60-90秒
乐器：鼓急促 + 琵琶紧张 + 低音弦乐
特点：
  - 开篇：警报般的紧迫
  - 中段：危机应对的紧张
  - 尾段：危机解除或继续恶化
音乐风格：危机感强，节奏紧凑
文件：scene_disaster_warning.mp3
```

---

### 2.2 外交类 - Diplomacy (5首)

#### 【DIP-01】外交往来 Diplomatic Mission
```
场景描述：接待外国使节，展示国威
情绪：庄重典雅、大国风范
BPM：80
时长：60-90秒
乐器：编钟 + 古琴 + 琵琶 + 笛
特点：
  - 开篇：典雅的大国气度
  - 中段：文化交流的友好
  - 尾段：使节离去的礼节
音乐风格：古典优雅，礼仪感强
文件：scene_diplomacy.mp3
```

#### 【DIP-02】外交谈判 Negotiation
```
场景描述：两国谈判，唇枪舌剑
情绪：紧张博弈、尔虞我诈
BPM：75
时长：60-90秒
乐器：古筝紧张 + 低音弦乐 + 偶尔的尖锐音
特点：
  - 开篇：暗流涌动
  - 中段：谈判博弈的紧张
  - 尾段：达成协议或破裂
音乐风格：紧张悬疑，带有权谋感
文件：scene_negotiation.mp3
```

#### 【DIP-03】联盟缔结 Alliance
```
场景描述：两国结盟，共抗强敌
情绪：庄严神圣、同盟情谊
BPM：100
时长：60-90秒
乐器：鼓 + 钟 + 弦乐 + 琵琶
特点：
  - 开篇：结盟仪式的庄严
  - 中段：同盟的团结力量
  - 尾段：共同对敌的决心
音乐风格：庄重有力，带有誓约感
文件：scene_alliance.mp3
```

#### 【DIP-04】朝贡体系 Tribute System
```
场景描述：万国来朝，藩属进贡
情绪：威严繁华、天朝上国
BPM：110
时长：90-120秒
乐器：编钟 + 鼓 + 笛 + 笙
特点：
  - 开篇：各国使节云集
  - 中段：朝贡的盛大场面
  - 尾段：天朝威严的展示
音乐风格：盛大繁华，帝国气象
文件：scene_tribute.mp3
```

#### 【DIP-05】战争赔款 Reparations
```
场景描述：战败赔款，屈辱求和
情绪：屈辱压抑、忍辱负重
BPM：55
时长：60-90秒
乐器：低音弦乐 + 箫 + 低沉鼓点
特点：
  - 开篇：沉重的屈辱感
  - 中段：被迫签署条约
  - 尾段：卧薪尝胆的决心
音乐风格：压抑沉重，带有不屈感
文件：scene_reparations.mp3
```

---

### 2.3 军事类 - Military (8首)

#### 【MIL-01】军事训练 Military Training
```
场景描述：士兵日常训练，操练阵法
情绪：严肃紧张、士气高昂
BPM：120
时长：60-90秒
乐器：鼓 + 铙钹 + 笛 + 弦乐
特点：
  - 节奏：整齐的脚步声与鼓点
  - 特色：军号元素
  - 情绪：逐渐高涨的士气
音乐风格：严肃有力，军事气息
文件：scene_military_training.mp3
```

#### 【MIL-02】围城攻城 Siege
```
场景描述：围城攻城，战况激烈
情绪：惨烈紧张、殊死搏斗
BPM：140
时长：90-120秒
乐器：鼓急促 + 铙钹 + 呐喊 + 弦乐
特点：
  - 开篇：攻城的号角
  - 中段：攻城战的惨烈
  - 尾段：城破或坚守
音乐风格：惨烈悲壮，战斗感强
文件：scene_siege.mp3
```

#### 【MIL-03】水战 naval Battle
```
场景描述：水上交战，战船交锋
情绪：波澜壮阔、水火交融
BPM：130
时长：60-90秒
乐器：鼓 + 锣 + 弦乐急促 + 水声采样
特点：
  - 开篇：水浪声与战鼓
  - 中段：战船交锋的激烈
  - 尾段：胜负已分
音乐风格：水战特色，波澜壮阔
文件：scene_naval_battle.mp3
```

#### 【MIL-04】骑兵突袭 Cavalry Raid
```
场景描述：骑兵突袭，快速打击
情绪：风驰电掣、闪电战
BPM：145
时长：60-90秒
乐器：鼓急促 + 马蹄声采样 + 弦乐急速
特点：
  - 节奏：马蹄飞驰的急促
  - 特色：突袭的闪电感
  - 尾段：突袭成功或撤退
音乐风格：速度感强，节奏紧凑
文件：scene_cavalry_raid.mp3
```

#### 【MIL-05】俘虏审讯 Prisoner Interrogation
```
场景描述：审讯俘虏，套取情报
情绪：阴森恐怖、压迫感强
BPM：60
时长：60-90秒
乐器：低音弦乐 + 偶尔的尖锐音效 + 锣
特点：
  - 开篇：阴暗的氛围
  - 中段：审讯的压迫
  - 尾段：情报获取或失败
音乐风格：阴森悬疑，心理压迫
文件：scene_interrogation.mp3
```

#### 【MIL-06】将军出征 General Departure
```
场景描述：大将军领兵出征，送行场面
情绪：慷慨激昂、壮志豪情
BPM：125
时长：90-120秒
乐器：鼓 + 琵琶激昂 + 笛 + 弦乐
特点：
  - 开篇：送行的感人场面
  - 中段：将军的豪情壮志
  - 尾段：出征的坚定
音乐风格：慷慨激昂，英雄气概
文件：scene_general_departure.mp3
```

#### 【MIL-07】间谍行动 Spy Operation
```
场景描述：间谍潜伏，暗中行动
情绪：神秘紧张、步步惊心
BPM：70
时长：60-90秒
乐器：低音弦乐 + 箫低沉 + 偶尔的脚步声
特点：
  - 开篇：夜色的神秘
  - 中段：潜入的紧张
  - 尾段：任务成功或暴露
音乐风格：神秘悬疑，紧张感强
文件：scene_spy_operation.mp3
```

#### 【MIL-08】投降受降 Surrender
```
场景描述：一方投降，接受投降
情绪：胜利威严、失败屈辱
BPM：80
时长：60-90秒
乐器：鼓庄严 + 钟 + 弦乐
特点：
  - 开篇：投降的屈辱
  - 中段：受降的威严
  - 尾段：胜利者的荣耀
音乐风格：对比强烈，胜利感
文件：scene_surrender.mp3
```

---

### 2.4 宫廷类 - Court Politics (5首)

#### 【CRT-01】皇位继承 Succession
```
场景描述：皇位继承，明争暗斗
情绪：紧张压抑、危机四伏
BPM：85
时长：90-120秒
乐器：古筝紧张 + 低音弦乐 + 偶尔的尖锐音
特点：
  - 开篇：表面的平静
  - 中段：暗中的争夺
  - 尾段：继承确立
音乐风格：紧张悬疑，权谋感强
文件：scene_succession.mp3
```

#### 【CRT-02】太后垂帘 Empress Dowager
```
场景描述：太后临朝，垂帘听政
情绪：威严神秘、幕后操控
BPM：65
时长：60-90秒
乐器：古琴 + 编钟低沉 + 箫
特点：
  - 开篇：帘幕后的神秘
  - 中段：太后的威严
  - 尾段：权力的展示
音乐风格：神秘威严，幕后感
文件：scene_empress_dowager.mp3
```

#### 【CRT-03】党争激烈 Faction Struggle
```
场景描述：朝中党争，互相倾轧
情绪：混乱紧张、勾心斗角
BPM：90
时长：60-90秒
乐器：琵琶急促 + 鼓紧张 + 弦乐
特点：
  - 开篇：朝堂上的暗流
  - 中段：党争的激烈
  - 尾段：一派胜利
音乐风格：紧张混乱，冲突感强
文件：scene_faction_struggle.mp3
```

#### 【CRT-04】告老还乡 Retirement
```
场景描述：大臣告老还乡，功成身退
情绪：感慨释然、功成身退
BPM：70
时长：60-90秒
乐器：古琴 + 箫 + 笛轻柔
特点：
  - 开篇：离别的感慨
  - 中段：回顾一生的辉煌
  - 尾段：释然的离去
音乐风格：感慨释然，田园诗意
文件：scene_retirement.mp3
```

#### 【CRT-05】流亡生涯 Exile
```
场景描述：被贬流放，流亡天涯
情绪：凄凉悲怆、颠沛流离
BPM：50
时长：90-120秒
乐器：箫低沉 + 古琴悲伤 + 琵琶凄凉
特点：
  - 开篇：离别的悲伤
  - 中段：流亡的艰辛
  - 尾段：坚韧不拔的意志
音乐风格：凄凉悲怆，带有不屈
文件：scene_exile.mp3
```

---

### 2.5 特殊事件类 - Special Events (4首)

#### 【EVT-01】天象异变 Celestial Omen
```
场景描述：流星、日食、异象出现
情绪：神秘震撼、天命警示
BPM：55
时长：60-90秒
乐器：编钟 + 磬 + 箫空灵 + 低音弦乐
特点：
  - 开篇：天象的震撼
  - 中段：神秘的变化
  - 尾段：解读的悬念
音乐风格：空灵神秘，天象感
文件：scene_celestial_omen.mp3
```

#### 【EVT-02】祥瑞降临 Auspicious Sign
```
场景描述：凤凰、麒麟等祥瑞出现
情绪：吉祥喜庆、天降祥瑞
BPM：100
时长：60-90秒
乐器：笙 + 笛 + 鼓 + 钟
特点：
  - 开篇：祥和的氛围
  - 中段：祥瑞的喜悦
  - 尾段：国运昌盛的预兆
音乐风格：吉祥如意，喜庆祥和
文件：scene_auspicious_sign.mp3
```

#### 【EVT-03】刺杀行动 Assassination
```
场景描述：刺客潜入，暗杀行动
情绪：紧张惊险、命悬一线
BPM：130
时长：60-90秒
乐器：鼓急促 + 弦乐紧张 + 短促音效
特点：
  - 开篇：夜色的紧张
  - 中段：刺杀的惊险
  - 尾段：成功或失败
音乐风格：惊险紧张，动作感强
文件：scene_assassination.mp3
```

#### 【EVT-04】王朝更替 Dynasty Collapse
```
场景描述：王朝覆灭，新朝建立
情绪：史诗悲壮、时代变迁
BPM：100
时长：120-180秒
乐器：鼓 + 钟 + 弦乐 + 琵琶
特点：
  - 开篇：旧朝的衰落
  - 中段：覆灭的悲壮
  - 尾段：新朝的曙光
音乐风格：史诗般宏大，悲壮感
文件：scene_dynasty_collapse.mp3
```

---

## 三、配乐文件清单

### 3.1 新增配乐汇总（30首）

| 编号 | 文件名 | 场景 | BPM | 情绪 |
|------|--------|------|-----|------|
| CIV-01 | scene_examination.mp3 | 科举考试 | 85 | 紧张期待 |
| CIV-02 | scene_harvest.mp3 | 农业丰收 | 110 | 欢乐祥和 |
| CIV-03 | scene_trade.mp3 | 商业繁荣 | 100 | 繁华热闹 |
| CIV-04 | scene_construction.mp3 | 建筑工程 | 95 | 宏伟壮观 |
| CIV-05 | scene_ceremony.mp3 | 祭祀典礼 | 60 | 庄严肃穆 |
| CIV-06 | scene_plague.mp3 | 瘟疫蔓延 | 50 | 阴森恐怖 |
| CIV-07 | scene_reform.mp3 | 改革变法 | 105 | 紧张激进 |
| CIV-08 | scene_disaster_warning.mp3 | 灾害预警 | 90 | 紧张紧迫 |
| DIP-01 | scene_diplomacy.mp3 | 外交往来 | 80 | 庄重典雅 |
| DIP-02 | scene_negotiation.mp3 | 外交谈判 | 75 | 紧张博弈 |
| DIP-03 | scene_alliance.mp3 | 联盟缔结 | 100 | 庄严神圣 |
| DIP-04 | scene_tribute.mp3 | 朝贡体系 | 110 | 威严繁华 |
| DIP-05 | scene_reparations.mp3 | 战争赔款 | 55 | 屈辱压抑 |
| MIL-01 | scene_military_training.mp3 | 军事训练 | 120 | 严肃紧张 |
| MIL-02 | scene_siege.mp3 | 围城攻城 | 140 | 惨烈紧张 |
| MIL-03 | scene_naval_battle.mp3 | 水战 | 130 | 波澜壮阔 |
| MIL-04 | scene_cavalry_raid.mp3 | 骑兵突袭 | 145 | 风驰电掣 |
| MIL-05 | scene_interrogation.mp3 | 俘虏审讯 | 60 | 阴森恐怖 |
| MIL-06 | scene_general_departure.mp3 | 将军出征 | 125 | 慷慨激昂 |
| MIL-07 | scene_spy_operation.mp3 | 间谍行动 | 70 | 神秘紧张 |
| MIL-08 | scene_surrender.mp3 | 投降受降 | 80 | 胜利威严 |
| CRT-01 | scene_succession.mp3 | 皇位继承 | 85 | 紧张压抑 |
| CRT-02 | scene_empress_dowager.mp3 | 太后垂帘 | 65 | 威严神秘 |
| CRT-03 | scene_faction_struggle.mp3 | 党争激烈 | 90 | 混乱紧张 |
| CRT-04 | scene_retirement.mp3 | 告老还乡 | 70 | 感慨释然 |
| CRT-05 | scene_exile.mp3 | 流亡生涯 | 50 | 凄凉悲怆 |
| EVT-01 | scene_celestial_omen.mp3 | 天象异变 | 55 | 神秘震撼 |
| EVT-02 | scene_auspicious_sign.mp3 | 祥瑞降临 | 100 | 吉祥喜庆 |
| EVT-03 | scene_assassination.mp3 | 刺杀行动 | 130 | 紧张惊险 |
| EVT-04 | scene_dynasty_collapse.mp3 | 王朝更替 | 100 | 史诗悲壮 |

### 3.2 完整配乐清单（40首）

#### 内政类 Civil (8+2=10首)
- scene_governance ✅
- scene_examination 🆕
- scene_harvest 🆕
- scene_trade 🆕
- scene_construction 🆕
- scene_ceremony 🆕
- scene_plague 🆕
- scene_reform 🆕
- scene_disaster_warning 🆕
- scene_famine ✅

#### 军事类 Military (8+2=10首)
- scene_war ✅
- scene_campaign ✅
- scene_military_training 🆕
- scene_siege 🆕
- scene_naval_battle 🆕
- scene_cavalry_raid 🆕
- scene_interrogation 🆕
- scene_general_departure 🆕
- scene_spy_operation 🆕
- scene_surrender 🆕

#### 外交类 Diplomacy (5首) 🆕
- scene_diplomacy 🆕
- scene_negotiation 🆕
- scene_alliance 🆕
- scene_tribute 🆕
- scene_reparations 🆕

#### 宫廷类 Court (5+3=8首)
- scene_palace_conspiracy ✅
- scene_succession 🆕
- scene_empress_dowager 🆕
- scene_faction_struggle 🆕
- scene_retirement 🆕
- scene_exile 🆕
- scene_funeral ✅
- scene_banquet ✅

#### 盛世/衰败 (2+2=4首)
- scene_golden_age ✅
- scene_decline ✅
- scene_unification ✅
- scene_dynasty_collapse 🆕

#### 特殊事件 (4首) 🆕
- scene_celestial_omen 🆕
- scene_auspicious_sign 🆕
- scene_assassination 🆕
- scene_plague (移至内政)

---

## 四、音乐制作规范

### 4.1 音频格式
- 格式：MP3
- 比特率：192-256 kbps
- 采样率：44100 Hz
- 声道：立体声

### 4.2 Unity 导入设置
```
Load Type: Streaming（背景音乐）/ Decompress on Load（配音）
Compression: Vorbis
Quality: 70%
Priority: Normal
Volume: 1.0
```

### 4.3 循环友好设计
- 背景音乐需首尾自然衔接
- 建议设置8-16小节循环
- 特殊事件音乐无需循环

### 4.4 时长标准
- 背景循环音乐：60-90秒
- 特殊事件音乐：60-120秒
- 史诗级音乐：90-180秒

### 4.5 音量规范
- 主音量：0.7-0.8
- 战斗音量：0.8-0.9
- 氛围音量：0.5-0.6

---

## 五、触发机制设计

### 5.1 内政触发
| 事件 | 触发条件 | 配乐 |
|------|----------|------|
| 科举考试 | exam_event = true | scene_examination |
| 丰收季节 | harvest_event = true | scene_harvest |
| 贸易繁荣 | trade_level > 80 | scene_trade |
| 大型建筑 | construction_event = true | scene_construction |
| 祭祀大典 | ceremony_event = true | scene_ceremony |
| 瘟疫蔓延 | plague_event = true | scene_plague |
| 改革变法 | reform_event = true | scene_reform |
| 自然灾害 | disaster_event = true | scene_disaster_warning |

### 5.2 外交触发
| 事件 | 触发条件 | 配乐 |
|------|----------|------|
| 接待使节 | diplomacy_event = true | scene_diplomacy |
| 外交谈判 | negotiation_event = true | scene_negotiation |
| 缔结联盟 | alliance_event = true | scene_alliance |
| 万国来朝 | tribute_event = true | scene_tribute |
| 战争赔款 | reparations_event = true | scene_reparations |

### 5.3 军事触发
| 事件 | 触发条件 | 配乐 |
|------|----------|------|
| 军事训练 | training_event = true | scene_military_training |
| 围城战 | siege_event = true | scene_siege |
| 水上战斗 | naval_event = true | scene_naval_battle |
| 骑兵突袭 | cavalry_raid = true | scene_cavalry_raid |
| 审讯俘虏 | interrogation = true | scene_interrogation |
| 将军出征 | general_departure = true | scene_general_departure |
| 间谍行动 | spy_operation = true | scene_spy_operation |
| 投降受降 | surrender_event = true | scene_surrender |

### 5.4 宫廷触发
| 事件 | 触发条件 | 配乐 |
|------|----------|------|
| 皇位继承 | succession_event = true | scene_succession |
| 太后垂帘 | empress_dowager = true | scene_empress_dowager |
| 党争 | faction_event = true | scene_faction_struggle |
| 大臣退休 | retirement_event = true | scene_retirement |
| 流放 | exile_event = true | scene_exile |

### 5.5 特殊事件触发
| 事件 | 触发条件 | 配乐 |
|------|----------|------|
| 天象异变 | celestial_event = true | scene_celestial_omen |
| 祥瑞降临 | auspicious_event = true | scene_auspicious_sign |
| 刺杀行动 | assassination = true | scene_assassination |
| 王朝更替 | dynasty_collapse = true | scene_dynasty_collapse |

---

## 六、文件存储结构

```
Assets/Audio/Music/Scene/
├── scene_governance.mp3          ✅ 已有
├── scene_war.mp3                 ✅ 已有
├── scene_palace_conspiracy.mp3    ✅ 已有
├── scene_famine.mp3               ✅ 已有
├── scene_golden_age.mp3           ✅ 已有
├── scene_decline.mp3              ✅ 已有
├── scene_unification.mp3          ✅ 已有
├── scene_banquet.mp3              ✅ 已有
├── scene_campaign.mp3             ✅ 已有
├── scene_funeral.mp3             ✅ 已有
│
├── Civil/                        🆕 内政类
│   ├── scene_examination.mp3
│   ├── scene_harvest.mp3
│   ├── scene_trade.mp3
│   ├── scene_construction.mp3
│   ├── scene_ceremony.mp3
│   ├── scene_plague.mp3
│   ├── scene_reform.mp3
│   └── scene_disaster_warning.mp3
│
├── Diplomacy/                    🆕 外交类
│   ├── scene_diplomacy.mp3
│   ├── scene_negotiation.mp3
│   ├── scene_alliance.mp3
│   ├── scene_tribute.mp3
│   └── scene_reparations.mp3
│
├── Military/                     🆕 军事类
│   ├── scene_military_training.mp3
│   ├── scene_siege.mp3
│   ├── scene_naval_battle.mp3
│   ├── scene_cavalry_raid.mp3
│   ├── scene_interrogation.mp3
│   ├── scene_general_departure.mp3
│   ├── scene_spy_operation.mp3
│   └── scene_surrender.mp3
│
├── Court/                        🆕 宫廷类
│   ├── scene_succession.mp3
│   ├── scene_empress_dowager.mp3
│   ├── scene_faction_struggle.mp3
│   ├── scene_retirement.mp3
│   └── scene_exile.mp3
│
└── Special/                     🆕 特殊事件
    ├── scene_celestial_omen.mp3
    ├── scene_auspicious_sign.mp3
    ├── scene_assassination.mp3
    └── scene_dynasty_collapse.mp3
```

---

## 七、附录：乐器音色参考

### 中国传统乐器
| 乐器 | 音色特点 | 适用场景 |
|------|----------|----------|
| 编钟 | 庄重典雅 | 祭祀、宫廷 |
| 磬 | 空灵神秘 | 祭祀、宗教 |
| 古琴 | 典雅深沉 | 宫廷、文人 |
| 古筝 | 多变丰富 | 通用 |
| 琵琶 | 表现力强 | 战斗、紧张 |
| 二胡 | 悲怆悠扬 | 悲伤、流亡 |
| 笛 | 悠扬清亮 | 田园、欢庆 |
| 箫 | 低沉空灵 | 悲伤、神秘 |
| 笙 | 和谐欢快 | 喜庆、吉祥 |
| 唢呐 | 高亢激昂 | 婚礼、庆典 |
| 鼓 | 节奏感强 | 战斗、仪式 |
| 铙钹 | 激烈响亮 | 战斗、庆典 |
| 锣 | 震撼有力 | 战斗、警报 |

### 打击乐节奏型
| 类型 | 节奏型 | 适用场景 |
|------|--------|----------|
| 战鼓 | 咚咚-咚咚 | 战争、出征 |
| 仪鼓 | 咚咚咚-咚 | 祭祀、典礼 |
| 花鼓 | 轻快活泼 | 丰收、节日 |
| 丧鼓 | 沉重缓慢 | 丧葬、哀伤 |

---

*本设计方案覆盖《万朝归一》完整游戏流程，建议优先制作高优先级配乐（见配乐清单中标注）。*
