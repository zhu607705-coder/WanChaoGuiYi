import { describe, expect, it, beforeEach, afterEach, vi } from 'vitest';
import { loadStrategyDataset, StrategyDatasetLoadError } from '../../src/data';

/**
 * Bug under investigation: tools/validate_web_data_source.py runs at
 * build time and verifies every region neighbor is bidirectional.
 * But that's a STATIC check — it runs once, before deploy. At
 * RUNTIME, loadStrategyDataset doesn't re-verify. So:
 *   1. A modified game-data-source served from a CDN (without
 *      re-running validate) can have one-way neighbours.
 *   2. A modder hot-swapping regions.json behind the build can
 *      ship asymmetric edges.
 *   3. The Web routing & UI assumes bidirectional everywhere.
 *
 * Pinned invariant: loadStrategyDataset should fail (throw or
 * warn loudly) when region A lists B as neighbor but B doesn't
 * list A.
 */
describe('loadStrategyDataset neighbor symmetry contract', () => {
  let originalFetch: typeof globalThis.fetch;

  beforeEach(() => {
    originalFetch = globalThis.fetch;
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('rejects asymmetric neighbor edges', async () => {
    globalThis.fetch = vi.fn(async (input) => {
      const url = typeof input === 'string' ? input : input.toString();
      if (url.endsWith('regions.json')) {
        return new Response(
          JSON.stringify({
            items: [
              { id: 'a', name: 'A', terrain: 'plain', population: 0, foodOutput: 0, taxOutput: 0, manpower: 0, landStructure: {}, legitimacyMemory: [], localPower: 0, neighbors: ['b'] },
              { id: 'b', name: 'B', terrain: 'plain', population: 0, foodOutput: 0, taxOutput: 0, manpower: 0, landStructure: {}, legitimacyMemory: [], localPower: 0, neighbors: [] }
            ]
          }),
          { status: 200 }
        );
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
