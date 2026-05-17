import { describe, expect, it, beforeEach, afterEach, vi } from 'vitest';
import { loadStrategyDataset, StrategyDatasetLoadError } from '../../src/data';

/**
 * Bug under investigation: validate_web_data_source.py checks that
 * every region in regions.json has a matching shape in
 * map_region_shapes.json. Runtime loadStrategyDataset doesn't.
 * If a region is added to regions.json but a typo prevents the
 * corresponding shape from being added, Web will fail with a
 * downstream error like "shape.boundary is undefined" deep inside
 * scene.ts — far from the cause.
 *
 * Pinned invariant: loadStrategyDataset must fail loudly when a
 * region has no matching shape entry.
 */
describe('loadStrategyDataset region/shape coverage', () => {
  let originalFetch: typeof globalThis.fetch;

  beforeEach(() => {
    originalFetch = globalThis.fetch;
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('rejects regions without matching shapes', async () => {
    globalThis.fetch = vi.fn(async (input) => {
      const url = typeof input === 'string' ? input : input.toString();
      if (url.endsWith('regions.json')) {
        return new Response(
          JSON.stringify({
            items: [
              { id: 'a', name: 'A', terrain: 'plain', population: 0, foodOutput: 0, taxOutput: 0, manpower: 0, landStructure: {}, legitimacyMemory: [], localPower: 0, neighbors: [] }
            ]
          }),
          { status: 200 }
        );
      }
      if (url.endsWith('map_region_shapes.json')) {
        // Shape table empty: 'a' has no shape.
        return new Response(JSON.stringify({ items: [] }), { status: 200 });
      }
      return new Response(JSON.stringify({ items: [] }), { status: 200 });
    }) as typeof fetch;

    let caught: unknown = null;
    try {
      await loadStrategyDataset();
    } catch (err) {
      caught = err;
    }

    expect(caught).toBeInstanceOf(StrategyDatasetLoadError);
  });
});
