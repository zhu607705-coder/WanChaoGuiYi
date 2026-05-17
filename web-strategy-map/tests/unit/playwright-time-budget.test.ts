import { describe, expect, it } from 'vitest';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';

/**
 * Bug under investigation: the main Playwright test
 * `loads real 56-region map and supports core decisions` carries
 * `test.setTimeout(150_000)` (2.5 min). The actual reported runtime
 * is ~2.3 min. That's a sign the test is doing too much in one
 * scenario.
 *
 * Pinned invariants:
 *   1. No single Playwright test should set a timeout > 90 seconds.
 *      (90s is generous; full suite is ~6.8 min today.)
 *   2. The number of `expect.poll(...)` calls per test should be
 *      bounded. Each poll round-trips through `debug(page)` which
 *      reads the entire StrategyApp debug state.
 */
describe('Playwright per-test time budget', () => {
  const specPath = join(__dirname, '..', 'strategy-map.spec.ts');
  const text = readFileSync(specPath, 'utf8');

  it('no test sets a timeout greater than 90 seconds', () => {
    // Match `test.setTimeout(<n>_<m>);` with n*1000+m treating
    // underscores as separators.
    const matches = [...text.matchAll(/test\.setTimeout\(\s*([0-9_]+)\s*\)/g)];
    expect(matches.length).toBeGreaterThan(0);
    for (const m of matches) {
      const ms = Number.parseInt(m[1].replace(/_/g, ''), 10);
      expect.soft(ms, `setTimeout ${ms} ms`).toBeLessThanOrEqual(90_000);
    }
  });

  it('most-instrumented test has at most 30 expect.poll calls', () => {
    // Count expect.poll inside each `test('...')` block.
    const testBlocks = text.split(/test\(['"`]/);
    let maxPolls = 0;
    let maxLine = '';
    for (let i = 1; i < testBlocks.length; i++) {
      const block = testBlocks[i];
      const polls = (block.match(/expect\.poll\(/g) ?? []).length;
      if (polls > maxPolls) {
        maxPolls = polls;
        const titleEnd = block.search(/['"`]/);
        maxLine = block.slice(0, titleEnd).slice(0, 80);
      }
    }
    expect.soft(
      maxPolls,
      `most-instrumented test '${maxLine}' has ${maxPolls} expect.poll calls; consider splitting`
    ).toBeLessThanOrEqual(30);
  });
});
