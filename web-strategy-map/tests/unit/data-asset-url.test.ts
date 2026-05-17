import { describe, expect, it, beforeEach, afterEach, vi } from 'vitest';
import { loadStrategyDataset } from '../../src/data';

/**
 * Bug under investigation: loadStrategyDataset uses Promise.all to
 * fetch ~14 JSON files. If any one of them returns a non-OK status,
 * loadJson throws "Failed to load X: status". Promise.all rejects
 * with that single error, but:
 *   1. There's no retry, no fallback, no partial loading.
 *   2. The thrown Error message exposes the file path; the bootstrap
 *      catch-all in main.ts shows it directly to the user without
 *      sanitising. This is a minor information leak (file structure)
 *      and a useless error UX.
 *   3. If the FETCH itself fails (network error, AbortController),
 *      the Promise rejects with whatever fetch produces — typically
 *      a TypeError ("Failed to fetch"). loadStrategyDataset doesn't
 *      wrap, so users see the bare TypeError.
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
    // distinguishable from a generic Error. Today the implementation
    // throws `new Error("Failed to load regions.json: 404")` — a raw
    // Error. Pinning: the error class should be a named subclass.
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
