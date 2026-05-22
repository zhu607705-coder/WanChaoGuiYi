import { afterEach, describe, expect, it, vi } from 'vitest';
import { StrategyAudio, type AudioDebugState } from '../../src/audio';
import type { NarrationScript, SceneMusicCue } from '../../src/types';

/**
 * Bug under investigation: StrategyAudio gates every play method
 * with `if (!this.enabled) return;`. Verify two contracts:
 *   1. setMode(...) before enable() updates the internal `mode`.
 *   2. getDebugState() initial state reports enabled=false.
 */
describe('StrategyAudio pre-enable behaviour', () => {
  const originalAudio = globalThis.Audio;

  afterEach(() => {
    globalThis.Audio = originalAudio;
    vi.restoreAllMocks();
  });

  function makeAudio(): StrategyAudio {
    const narration: NarrationScript = {
      schemaVersion: 1,
      description: '',
      tutorial: { title: '', segments: [] },
      emperor_voices: []
    };
    return new StrategyAudio([], [], [], narration);
  }

  function makeSceneCue(scene: string, musicCueId: string): SceneMusicCue {
    return {
      scene,
      musicCueId,
      fileName: `${musicCueId}.mp3`,
      mood: 'test',
      bpm: 80,
      tags: [],
      description: ''
    };
  }

  function installAudioMock(play: () => Promise<void>): string[] {
    const sources: string[] = [];
    function AudioMock(this: HTMLAudioElement, source?: string): void {
      sources.push(source ?? '');
      Object.assign(this, {
        dataset: {},
        addEventListener: vi.fn(),
        load: vi.fn(),
        pause: vi.fn(),
        play,
        src: source ?? '',
        loop: false,
        volume: 1
      });
    }
    globalThis.Audio = vi.fn(AudioMock) as unknown as typeof Audio;
    return sources;
  }

  function installAudioMockWithErrorEvent(
    play: () => Promise<void>,
    mediaError: { code: number; message?: string } | null = null
  ): { sources: string[]; triggerError: () => void } {
    const sources: string[] = [];
    const errorHandlers: Array<(() => void) | undefined> = [];
    function AudioMock(this: HTMLAudioElement, source?: string): void {
      const index = sources.length;
      sources.push(source ?? '');
      Object.assign(this, {
        dataset: {},
        addEventListener: vi.fn((eventName: string, handler: EventListenerOrEventListenerObject) => {
          if (eventName === 'error' && typeof handler === 'function') {
            errorHandlers[index] = () => handler(new Event('error'));
          }
        }),
        error: mediaError,
        load: vi.fn(),
        pause: vi.fn(),
        play,
        src: source ?? '',
        loop: false,
        volume: 1
      });
    }
    globalThis.Audio = vi.fn(AudioMock) as unknown as typeof Audio;
    return {
      sources,
      triggerError: () => errorHandlers.at(-1)?.()
    };
  }

  function installAudioMockWithIndexedErrorEvents(play: () => Promise<void>): { sources: string[]; elements: HTMLAudioElement[]; triggerErrorAt: (index: number) => void } {
    const sources: string[] = [];
    const elements: HTMLAudioElement[] = [];
    const errorHandlers: Array<(() => void) | undefined> = [];
    function AudioMock(this: HTMLAudioElement, source?: string): void {
      const index = sources.length;
      sources.push(source ?? '');
      elements.push(this);
      Object.assign(this, {
        dataset: {},
        addEventListener: vi.fn((eventName: string, handler: EventListenerOrEventListenerObject) => {
          if (eventName === 'error' && typeof handler === 'function') {
            errorHandlers[index] = () => handler(new Event('error'));
          }
        }),
        load: vi.fn(),
        pause: vi.fn(),
        play,
        src: source ?? '',
        loop: false,
        volume: 1
      });
    }
    globalThis.Audio = vi.fn(AudioMock) as unknown as typeof Audio;
    return {
      sources,
      elements,
      triggerErrorAt: (index: number) => errorHandlers[index]?.()
    };
  }

  function installAudioMockWithMediaEvents(
    play: () => Promise<void>,
    mediaProgress?: { duration: number; bufferedEnd: number }
  ): { triggerEventAt: (index: number, eventName: string) => void } {
    const eventHandlers: Array<Record<string, () => void>> = [];
    function AudioMock(this: HTMLAudioElement, source?: string): void {
      const index = eventHandlers.length;
      eventHandlers.push({});
      Object.assign(this, {
        dataset: {},
        addEventListener: vi.fn((eventName: string, handler: EventListenerOrEventListenerObject) => {
          if (typeof handler === 'function') {
            eventHandlers[index][eventName] = () => handler(new Event(eventName));
          }
        }),
        load: vi.fn(),
        pause: vi.fn(),
        play,
        src: source ?? '',
        loop: false,
        volume: 1
      });
      if (mediaProgress) {
        Object.defineProperty(this, 'duration', { configurable: true, value: mediaProgress.duration });
        Object.defineProperty(this, 'buffered', {
          configurable: true,
          value: {
            length: 1,
            end: vi.fn(() => mediaProgress.bufferedEnd)
          }
        });
      }
    }
    globalThis.Audio = vi.fn(AudioMock) as unknown as typeof Audio;
    return {
      triggerEventAt: (index: number, eventName: string) => eventHandlers[index]?.[eventName]?.()
    };
  }

  it('setMode before enable updates mode for later enable', async () => {
    const audio = makeAudio();
    await audio.setMode('war');
    const debug: AudioDebugState = audio.getDebugState();
    expect(debug.mode).toBe('war');
  });

  it('getDebugState reports enabled=false initially', () => {
    const audio = makeAudio();
    const debug = audio.getDebugState();
    expect(debug.enabled).toBe(false);
    expect(debug.lastError).toBe('');
  });

  it('enable preserves a mode selected before audio is enabled', async () => {
    const audio = makeAudio();
    await audio.setMode('war');
    await audio.enable();
    const debug = audio.getDebugState();
    expect(debug.enabled).toBe(true);
    expect(debug.mode).toBe('war');
  });

  it('clears lastError after a later successful playback', async () => {
    const play = vi.fn()
      .mockRejectedValueOnce(new Error('autoplay blocked'))
      .mockResolvedValue(undefined);
    installAudioMock(play);

    const audio = new StrategyAudio(
      [makeSceneCue('war', 'war_theme'), makeSceneCue('governance', 'governance_theme')],
      [],
      [],
      { schemaVersion: 1, description: '', tutorial: { title: '', segments: [] }, emperor_voices: [] }
    );

    await audio.setMode('war');
    await audio.enable();
    expect(audio.getDebugState().lastError).toBe('autoplay blocked');

    await audio.setMode('governance');
    expect(audio.getDebugState().lastError).toBe('');
    expect(play).toHaveBeenCalledTimes(2);
  });

  it('retries the same music cue after an initial playback failure', async () => {
    const play = vi.fn()
      .mockRejectedValueOnce(new Error('autoplay blocked'))
      .mockResolvedValue(undefined);
    installAudioMock(play);

    const audio = new StrategyAudio(
      [makeSceneCue('governance', 'governance_theme')],
      [],
      [],
      { schemaVersion: 1, description: '', tutorial: { title: '', segments: [] }, emperor_voices: [] }
    );

    await audio.enable();
    expect(audio.getDebugState().lastError).toBe('autoplay blocked');

    await audio.setMode('governance');
    expect(audio.getDebugState().lastError).toBe('');
    expect(play).toHaveBeenCalledTimes(2);
  });

  it('normalizes audio sources through the shared game-data asset URL boundary', async () => {
    const sources = installAudioMock(vi.fn().mockResolvedValue(undefined));
    const audio = new StrategyAudio(
      [{ ...makeSceneCue('governance', 'governance_theme'), fileName: '..\\governance_theme.mp3' }],
      [
        {
          emperorId: '../bad',
          musicCueId: 'bad_theme',
          fileName: '..\\bad_theme.mp3',
          mood: 'test',
          bpm: 80,
          tags: [],
          historicalContext: ''
        }
      ],
      [],
      {
        schemaVersion: 1,
        description: '',
        tutorial: {
          title: '',
          segments: [{ segmentId: '../intro', text: 'intro', trigger: 'game_start', priority: 1 }]
        },
        emperor_voices: [
          {
            emperorId: '../bad',
            emperorName: 'bad',
            voiceProfile: 'test',
            personality: 'test',
            lines: { select: 'select' }
          }
        ]
      }
    );

    await audio.enable();
    await audio.setEmperor('../bad');

    expect(sources).toEqual([
      '/game-data/audio/music/scene/governance_theme.mp3',
      '/game-data/audio/narration/intro.mp3',
      '/game-data/audio/emperor-voice/bad_select.mp3',
      '/game-data/audio/music/emperor/bad_theme.mp3'
    ]);
    expect(sources.join('\n')).not.toMatch(/\.\.|\\/);
  });

  it('surfaces media loading errors in debug state', async () => {
    const { sources, triggerError } = installAudioMockWithErrorEvent(vi.fn().mockResolvedValue(undefined));
    const audio = new StrategyAudio(
      [makeSceneCue('governance', 'governance_theme')],
      [],
      [],
      { schemaVersion: 1, description: '', tutorial: { title: '', segments: [] }, emperor_voices: [] }
    );

    await audio.enable();
    triggerError();

    expect(audio.getDebugState().lastError).toBe('Audio failed to load: /game-data/audio/music/scene/governance_theme.mp3');
    expect(sources).toEqual(['/game-data/audio/music/scene/governance_theme.mp3']);
  });

  it('classifies media loading error codes in debug state', async () => {
    const { triggerError } = installAudioMockWithErrorEvent(
      vi.fn().mockResolvedValue(undefined),
      { code: 4, message: 'unsupported codec' }
    );
    const audio = new StrategyAudio(
      [makeSceneCue('governance', 'governance_theme')],
      [],
      [],
      { schemaVersion: 1, description: '', tutorial: { title: '', segments: [] }, emperor_voices: [] }
    );

    await audio.enable();
    triggerError();

    expect(audio.getDebugState().lastError).toBe('Audio failed to load (source not supported: unsupported codec): /game-data/audio/music/scene/governance_theme.mp3');
  });

  it('reports media loading stage transitions in debug state', async () => {
    let resolvePlay!: () => void;
    const play = vi.fn(() => new Promise<void>((resolve) => {
      resolvePlay = resolve;
    }));
    const { triggerEventAt } = installAudioMockWithMediaEvents(play);
    const audio = new StrategyAudio(
      [makeSceneCue('governance', 'governance_theme')],
      [],
      [],
      { schemaVersion: 1, description: '', tutorial: { title: '', segments: [] }, emperor_voices: [] }
    );

    const enableTask = audio.enable();
    triggerEventAt(0, 'loadstart');
    expect(audio.getDebugState().loadingStage).toBe('loading');
    expect(audio.getDebugState().loadingMessage).toBe('音频加载中');

    triggerEventAt(0, 'canplay');
    expect(audio.getDebugState().loadingStage).toBe('canplay');
    expect(audio.getDebugState().loadingMessage).toBe('音频可播放');

    resolvePlay();
    await enableTask;
    expect(audio.getDebugState().loadingStage).toBe('playing');
    expect(audio.getDebugState().loadingMessage).toBe('');
  });

  it('reports buffered progress percentage in debug state', async () => {
    const { triggerEventAt } = installAudioMockWithMediaEvents(
      vi.fn(() => new Promise<void>(() => { /* keep playback pending */ })),
      { duration: 10, bufferedEnd: 5 }
    );
    const audio = new StrategyAudio(
      [makeSceneCue('governance', 'governance_theme')],
      [],
      [],
      { schemaVersion: 1, description: '', tutorial: { title: '', segments: [] }, emperor_voices: [] }
    );

    void audio.enable();
    triggerEventAt(0, 'progress');

    expect(audio.getDebugState().loadingStage).toBe('buffering');
    expect(audio.getDebugState().loadingProgress).toBe(50);
    expect(audio.getDebugState().loadingMessage).toBe('音频缓冲中 50%');
  });

  it('clamps buffered progress percentage boundaries in debug state', () => {
    const cases = [
      { duration: 10, bufferedEnd: 15, expectedProgress: 100, expectedMessage: '音频缓冲中 100%' },
      { duration: 10, bufferedEnd: -1, expectedProgress: 0, expectedMessage: '音频缓冲中 0%' },
      { duration: 0, bufferedEnd: 5, expectedProgress: null, expectedMessage: '音频缓冲中' }
    ];

    for (const testCase of cases) {
      const { triggerEventAt } = installAudioMockWithMediaEvents(
        vi.fn(() => new Promise<void>(() => { /* keep playback pending */ })),
        { duration: testCase.duration, bufferedEnd: testCase.bufferedEnd }
      );
      const audio = new StrategyAudio(
        [makeSceneCue('governance', 'governance_theme')],
        [],
        [],
        { schemaVersion: 1, description: '', tutorial: { title: '', segments: [] }, emperor_voices: [] }
      );

      void audio.enable();
      triggerEventAt(0, 'progress');

      expect(audio.getDebugState().loadingStage).toBe('buffering');
      expect(audio.getDebugState().loadingProgress).toBe(testCase.expectedProgress);
      expect(audio.getDebugState().loadingMessage).toBe(testCase.expectedMessage);
    }
  });

  it('ignores stale media loading errors after switching music elements', async () => {
    const { sources, triggerErrorAt } = installAudioMockWithIndexedErrorEvents(vi.fn().mockResolvedValue(undefined));
    const audio = new StrategyAudio(
      [makeSceneCue('governance', 'governance_theme'), makeSceneCue('war', 'war_theme')],
      [],
      [],
      { schemaVersion: 1, description: '', tutorial: { title: '', segments: [] }, emperor_voices: [] }
    );

    await audio.enable();
    await audio.setMode('war');
    triggerErrorAt(0);

    expect(audio.getDebugState().lastError).toBe('');
    expect(audio.getDebugState().currentMusicCue).toBe('scene:war_theme');
    expect(sources).toEqual([
      '/game-data/audio/music/scene/governance_theme.mp3',
      '/game-data/audio/music/scene/war_theme.mp3'
    ]);
  });

  it('releases stale narration and voice sources after replacement', async () => {
    const { elements, sources, triggerErrorAt } = installAudioMockWithIndexedErrorEvents(vi.fn().mockResolvedValue(undefined));
    const audio = new StrategyAudio(
      [],
      [],
      [],
      {
        schemaVersion: 1,
        description: '',
        tutorial: {
          title: '',
          segments: [
            { segmentId: 'intro', text: 'intro', trigger: 'game_start', priority: 1 },
            { segmentId: 'governance', text: 'governance', trigger: 'first_governance_action', priority: 1 }
          ]
        },
        emperor_voices: [
          {
            emperorId: 'qin_shi_huang',
            emperorName: '秦始皇',
            voiceProfile: 'test',
            personality: 'test',
            lines: { select: 'select', defend: 'defend' }
          }
        ]
      }
    );

    await audio.enable();
    await audio.playGovernanceAction();
    await audio.playLogisticsAction();

    expect(sources).toEqual([
      '/game-data/audio/narration/intro.mp3',
      '/game-data/audio/narration/governance.mp3',
      '/game-data/audio/emperor-voice/qin_shi_huang_select.mp3',
      '/game-data/audio/emperor-voice/qin_shi_huang_defend.mp3'
    ]);
    expect(elements[0]?.src).toBe('');
    expect(elements[2]?.src).toBe('');
    expect(elements[0]?.load).toHaveBeenCalledTimes(1);
    expect(elements[2]?.load).toHaveBeenCalledTimes(1);

    triggerErrorAt(0);
    triggerErrorAt(2);

    expect(audio.getDebugState().lastError).toBe('');
    expect(audio.getDebugState().currentNarration).toBe('governance');
    expect(audio.getDebugState().currentVoice).toBe('qin_shi_huang_defend: defend');
  });
});
