import { describe, expect, it, beforeEach, afterEach, vi } from 'vitest';
import { loadStrategyDataset } from '../../src/data';

/**
 * Bug under investigation: loadStrategyDataset fetches many JSON
 * files together. Missing files and network failures used to surface
 * as generic Error/TypeError values, which made bootstrap handling and
 * diagnostics inconsistent. The loader now wraps those failures in a
 * domain-specific error while preserving the failed file/category.
 *
 * Pinned invariants:
 *   1. A 404 for any single dataset file should produce a domain
 *      error whose .name is e.g. 'StrategyDatasetLoadError', not
 *      a raw Error.
 *   2. The error message should reference WHICH file failed and
 *      its category (data/audio/map), not just the filename.
 */
describe('loadStrategyDataset', () => {
  let originalFetch: typeof globalThis.fetch;

  beforeEach(() => {
    originalFetch = globalThis.fetch;
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('throws domain-specific error when a JSON file is missing', async () => {
    globalThis.fetch = vi.fn(async (input) => {
      const url = typeof input === 'string' ? input : input.toString();
      if (url.endsWith('regions.json')) {
        return new Response('not found', { status: 404 });
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
    const error = caught as Error;
    // The message must mention regions.json AND be of a type
    // distinguishable from a generic Error.
    expect(error.message).toContain('regions.json');
    expect(error.name).not.toBe('Error');
  });

  it('handles network failures (TypeError) without leaking raw fetch error', async () => {
    globalThis.fetch = vi.fn(async () => {
      throw new TypeError('Failed to fetch');
    }) as typeof fetch;

    let caught: unknown = null;
    try {
      await loadStrategyDataset();
    } catch (err) {
      caught = err;
    }

    expect(caught).toBeTruthy();
    const error = caught as Error;
    // Pinning: a network error should be wrapped into a domain
    // error so callers can do `if (err.name === 'StrategyDatasetLoadError')`.
    expect(error.name).not.toBe('TypeError');
  });
});
