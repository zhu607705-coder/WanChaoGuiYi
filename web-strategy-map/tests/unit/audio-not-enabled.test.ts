import { describe, expect, it } from 'vitest';
import { StrategyAudio, type AudioDebugState } from '../../src/audio';
import type { NarrationScript } from '../../src/types';

/**
 * Bug under investigation: StrategyAudio gates every play method
 * with `if (!this.enabled) return;`. Verify two contracts:
 *   1. setMode(...) before enable() updates the internal `mode`.
 *   2. getDebugState() initial state reports enabled=false.
 */
describe('StrategyAudio pre-enable behaviour', () => {
  function makeAudio(): StrategyAudio {
    const narration: NarrationScript = {
      schemaVersion: 1,
      description: '',
      tutorial: { title: '', segments: [] },
      emperor_voices: []
    };
    return new StrategyAudio([], [], [], narration);
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
});
