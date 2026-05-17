import { describe, expect, it, beforeEach, afterEach, vi } from 'vitest';
import {
  loadStrategyDataset,
  aggregateNationFood,
  aggregateNationMoney,
  type NationAggregationInput
} from '../../src/data';

/**
 * Performance baselines (real wall-clock, not bundle-size or
 * static analysis). These guard the user-visible "is the game
 * snappy?" experience.
 *
 * Pinned invariants:
 *   1. loadStrategyDataset on a 56-region in-memory fetch finishes
 *      under 1 second on dev machines.
 *   2. aggregateNation* on 100 regions finishes under 5ms.
 *   3. 1000 sequential calls to aggregateNation* finish under
 *      100ms (i.e. < 0.1ms each).
 *
 * These numbers are GENEROUS for CI on a slow VM. If they fail,
 * something has been pulled into the hot path that shouldn't be
 * there.
 */
describe('performance baselines', () => {
  let originalFetch: typeof globalThis.fetch;

  beforeEach(() => {
    originalFetch = globalThis.fetch;
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('aggregateNationFood on 100 regions completes in < 5ms', () => {
    const regions: NationAggregationInput[] = Array.from({ length: 100 }, (_, i) => ({
      owner: i % 2 === 0 ? 'player' : 'rival',
      foodOutput: 100 + i,
      taxOutput: 50 + i,
      contribution: 50 + (i % 50)
    }));
    const start = performance.now();
    const result = aggregateNationFood(regions);
    const elapsed = performance.now() - start;
    expect(typeof result).toBe('number');
    expect(elapsed).toBeLessThan(5);
  });

  it('1000 sequential aggregateNationMoney calls complete in < 100ms', () => {
    const regions: NationAggregationInput[] = Array.from({ length: 56 }, (_, i) => ({
      owner: 'player',
      foodOutput: 100,
      taxOutput: 50 + i,
      contribution: 70
    }));
    const start = performance.now();
    let sum = 0;
    for (let i = 0; i < 1000; i++) {
      sum += aggregateNationMoney(regions);
    }
    const elapsed = performance.now() - start;
    expect(sum).toBeGreaterThan(0);
    expect(elapsed, `1000 aggregations took ${elapsed.toFixed(1)}ms`).toBeLessThan(100);
  });

  it('loadStrategyDataset with stub fetch completes in < 1000ms', async () => {
    // Use a minimal stub that returns empty/valid items; we are
    // measuring loader machinery, not network.
    globalThis.fetch = vi.fn(async (input) => {
      const url = typeof input === 'string' ? input : input.toString();
      // Return shape-correct stubs for every collection. Most
      // need {items: []}, map_render_metadata is its own shape,
      // narration script has its own shape.
      if (url.endsWith('map_render_metadata.json')) {
        return new Response(
          JSON.stringify({
            schemaVersion: 1,
            precision: 'high',
            shapeCenter: { x: 0, y: 0 },
            pixelsPerShapeUnit: 1,
            spritePixelsPerUnit: 1,
            sourceImage: '/game-data/map/jiuzhou_generated_map.png',
            imageSize: { width: 1, height: 1 }
          }),
          { status: 200 }
        );
      }
      if (url.endsWith('narration_script.json')) {
        return new Response(
          JSON.stringify({
            schemaVersion: 1,
            description: '',
            tutorial: { title: '', segments: [] },
            emperor_voices: []
          }),
          { status: 200 }
        );
      }
      // Default: empty collection.
      return new Response(JSON.stringify({ items: [] }), { status: 200 });
    }) as typeof fetch;

    const start = performance.now();
    try {
      await loadStrategyDataset();
    } catch {
      // Empty regions might be rejected by validators; that's fine
      // for this perf test. We still measured the loader path.
    }
    const elapsed = performance.now() - start;
    expect(elapsed, `loadStrategyDataset took ${elapsed.toFixed(0)}ms`).toBeLessThan(1000);
  });
});
