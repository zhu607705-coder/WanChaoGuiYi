import { describe, expect, it, beforeEach, afterEach, vi } from 'vitest';
import { loadStrategyDataset, StrategyDatasetLoadError } from '../../src/data';

/**
 * Bug under investigation: previous round added StrategyDatasetLoadError.
 * But what happens when:
 *   1. The JSON is well-formed but its schema doesn't match
 *      JsonCollection<T> (e.g. items is undefined, or is an object
 *      not an array)?
 *   2. The JSON file is empty?
 *   3. The JSON is valid but contains items with duplicate ids?
 *
 * Today only fetch errors and JSON.parse errors are wrapped. Schema
 * mismatches throw later inside loadStrategyDataset's destructuring,
 * producing TypeError("cannot read properties of undefined").
 */
describe('loadStrategyDataset schema robustness', () => {
  let originalFetch: typeof globalThis.fetch;

  beforeEach(() => {
    originalFetch = globalThis.fetch;
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('rejects with StrategyDatasetLoadError when items field is missing', async () => {
    globalThis.fetch = vi.fn(async (input) => {
      const url = typeof input === 'string' ? input : input.toString();
      if (url.endsWith('regions.json')) {
        return new Response(JSON.stringify({}), { status: 200 });
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
    // Pinning: even schema mismatches must surface as our own
    // domain error, not a downstream TypeError.
    expect(caught).toBeInstanceOf(StrategyDatasetLoadError);
  });

  it('rejects with StrategyDatasetLoadError when JSON is empty string', async () => {
    globalThis.fetch = vi.fn(async (input) => {
      const url = typeof input === 'string' ? input : input.toString();
      if (url.endsWith('regions.json')) {
        return new Response('', { status: 200 });
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
