import { describe, expect, it, beforeEach, afterEach, vi } from 'vitest';
import { loadStrategyDataset, StrategyDatasetLoadError } from '../../src/data';

/**
 * Bug under investigation: NonUnityJsonDataRepository (C# headless)
 * REQUIRES unique ids — its Register<T>() throws "Duplicate ... id".
 * loadStrategyDataset (TS Web) has NO such check. The same JSON
 * payload that breaks headless tests passes through Web silently
 * with second-write-wins semantics.
 *
 * That cross-boundary divergence is the bug: a corrupted/malicious
 * dataset can run in Web while breaking headless validation. Or
 * conversely, a fix in headless test fixtures may not propagate to
 * Web.
 *
 * Pinned invariant: when the same regions.json contains two items
 * with the same id, loadStrategyDataset must throw, just like the
 * headless adapter.
 */
describe('loadStrategyDataset id-uniqueness contract', () => {
  let originalFetch: typeof globalThis.fetch;

  beforeEach(() => {
    originalFetch = globalThis.fetch;
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('rejects duplicate region ids the way the C# headless adapter does', async () => {
    globalThis.fetch = vi.fn(async (input) => {
      const url = typeof input === 'string' ? input : input.toString();
      if (url.endsWith('regions.json')) {
        return new Response(
          JSON.stringify({
            items: [
              { id: 'guanzhong', name: '关中', terrain: 'plain', population: 0, foodOutput: 0, taxOutput: 0, manpower: 0, landStructure: {}, legitimacyMemory: [], localPower: 0, neighbors: [] },
              { id: 'guanzhong', name: '关中-DUP', terrain: 'plain', population: 0, foodOutput: 0, taxOutput: 0, manpower: 0, landStructure: {}, legitimacyMemory: [], localPower: 0, neighbors: [] }
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

    expect(caught).toBeTruthy();
    expect(caught).toBeInstanceOf(StrategyDatasetLoadError);
  });

  it('rejects an item with missing id', async () => {
    globalThis.fetch = vi.fn(async (input) => {
      const url = typeof input === 'string' ? input : input.toString();
      if (url.endsWith('regions.json')) {
        return new Response(
          JSON.stringify({ items: [{ name: 'no-id', terrain: '', population: 0, foodOutput: 0, taxOutput: 0, manpower: 0, landStructure: {}, legitimacyMemory: [], localPower: 0, neighbors: [] }] }),
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
