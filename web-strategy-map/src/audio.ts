import type {
  ChronicleEventMusicCue,
  EmperorThemeCue,
  GameMode,
  NarrationScript,
  NarrationSegment,
  SceneMusicCue
} from './types';

export interface AudioDebugState {
  enabled: boolean;
  mode: GameMode;
  currentMusicCue: string;
  currentNarration: string;
  currentVoice: string;
  sceneCueCount: number;
  emperorThemeCount: number;
  chronicleEventCount: number;
  lastError: string;
}

export class StrategyAudio {
  private enabled = false;
  private mode: GameMode = 'governance';
  private readonly musicByScene = new Map<string, SceneMusicCue>();
  private readonly emperorThemeById = new Map<string, EmperorThemeCue>();
  private readonly chronicleEventById = new Map<string, ChronicleEventMusicCue>();
  private readonly narrationByTrigger = new Map<string, NarrationSegment>();
  private musicElement: HTMLAudioElement | null = null;
  private narrationElement: HTMLAudioElement | null = null;
  private voiceElement: HTMLAudioElement | null = null;
  private selectedEmperorId = 'qin_shi_huang';
  private currentMusicCue = '未启用';
  private currentNarration = '未播放';
  private currentVoice = '未播放';
  private lastError = '';

  constructor(
    sceneMusic: SceneMusicCue[],
    emperorThemes: EmperorThemeCue[],
    chronicleEvents: ChronicleEventMusicCue[],
    private readonly narration: NarrationScript
  ) {
    for (const cue of sceneMusic) {
      this.musicByScene.set(cue.scene.toLowerCase(), cue);
    }
    for (const cue of emperorThemes) {
      this.emperorThemeById.set(cue.emperorId, cue);
    }
    for (const cue of chronicleEvents) {
      this.chronicleEventById.set(cue.eventId, cue);
      this.chronicleEventById.set(cue.musicCueId, cue);
    }
    for (const segment of narration.tutorial.segments) {
      this.narrationByTrigger.set(segment.trigger, segment);
    }
  }

  async enable(): Promise<void> {
    this.enabled = true;
    await this.setMode(this.mode);
    await this.playNarration('game_start');
  }

  isEnabled(): boolean {
    return this.enabled;
  }

  async setEmperor(emperorId: string): Promise<void> {
    this.selectedEmperorId = emperorId;
    await this.playVoice('select');
    await this.playEmperorTheme(emperorId);
  }

  async setMode(mode: GameMode): Promise<void> {
    this.mode = mode;
    if (!this.enabled) return;
    const cue = this.musicByScene.get(mode === 'war' ? 'war' : 'governance');
    if (!cue) return;
    await this.playMusic(cue);
  }

  async playGovernanceAction(): Promise<void> {
    await this.playNarration('first_governance_action');
    await this.playVoice('select');
    if (this.enabled) {
      const cue = this.chronicleEventById.get('wang_anshi_reform') ?? this.chronicleEventById.get('event_wang_anshi_reform');
      if (cue) await this.playMusic(cue, 'chronicle-event');
    }
  }

  async playWarAction(): Promise<void> {
    await this.playNarration('first_war');
    await this.playVoice('attack');
    if (this.enabled) {
      const cue = this.musicByScene.get('campaign') ?? this.musicByScene.get('war');
      if (cue) await this.playMusic(cue);
    }
  }

  async playLogisticsAction(): Promise<void> {
    await this.playVoice('defend');
  }

  async playEmperorTheme(emperorId = this.selectedEmperorId): Promise<void> {
    if (!this.enabled) return;
    const cue = this.emperorThemeById.get(emperorId);
    if (!cue) return;
    await this.playMusic(cue, 'emperor');
  }

  async playEventCue(eventId = 'yellow_river_flood'): Promise<void> {
    if (!this.enabled) return;
    const cue = this.chronicleEventById.get(eventId);
    if (!cue) return;
    await this.playNarration('first_event');
    await this.playMusic(cue, 'chronicle-event');
  }

  getDebugState(): AudioDebugState {
    return {
      enabled: this.enabled,
      mode: this.mode,
      currentMusicCue: this.currentMusicCue,
      currentNarration: this.currentNarration,
      currentVoice: this.currentVoice,
      sceneCueCount: this.musicByScene.size,
      emperorThemeCount: this.emperorThemeById.size,
      chronicleEventCount: new Set([...this.chronicleEventById.values()].map((cue) => cue.musicCueId)).size,
      lastError: this.lastError
    };
  }

  private async playMusic(cue: SceneMusicCue | EmperorThemeCue | ChronicleEventMusicCue, group: 'scene' | 'emperor' | 'chronicle-event' = 'scene'): Promise<void> {
    const source = `/game-data/audio/music/${group}/${cue.fileName}`;
    if (this.musicElement?.dataset.source === source) return;

    const previous = this.musicElement;
    const next = new Audio(source);
    next.dataset.source = source;
    next.loop = true;
    next.volume = 0.46;
    this.musicElement = next;
    this.currentMusicCue = `${group}:${cue.musicCueId}`;
    await this.tryPlay(next);

    if (previous) {
      previous.pause();
      previous.src = '';
    }
  }

  private async playNarration(trigger: string): Promise<void> {
    if (!this.enabled) return;
    const segment = this.narrationByTrigger.get(trigger);
    if (!segment) return;
    const element = new Audio(`/game-data/audio/narration/${segment.segmentId}.mp3`);
    element.volume = 0.84;
    this.narrationElement?.pause();
    this.narrationElement = element;
    this.currentNarration = segment.segmentId;
    await this.tryPlay(element);
  }

  private async playVoice(action: 'select' | 'attack' | 'defend'): Promise<void> {
    if (!this.enabled) return;
    const emperorId = this.selectedEmperorId;
    const line = this.narration.emperor_voices.find((voice) => voice.emperorId === emperorId)?.lines[action] ?? action;
    const element = new Audio(`/game-data/audio/emperor-voice/${emperorId}_${action}.mp3`);
    element.volume = 0.9;
    this.voiceElement?.pause();
    this.voiceElement = element;
    this.currentVoice = `${emperorId}_${action}: ${line}`;
    await this.tryPlay(element);
  }

  private async tryPlay(element: HTMLAudioElement): Promise<void> {
    try {
      this.lastError = '';
      // Suppress audio fetch errors from console
      element.addEventListener('error', () => { /* Audio file not available, silently skip */ }, { once: true });
      await element.play();
    } catch (error) {
      this.lastError = error instanceof Error ? error.message : String(error);
    }
  }
}
