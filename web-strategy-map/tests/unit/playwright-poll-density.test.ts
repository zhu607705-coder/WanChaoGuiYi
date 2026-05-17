import { describe, expect, it } from 'vitest';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';

/**
 * Bug under investigation: last round wrapped expect.poll into
 * expectDebug() — same network round-trips, prettier syntax. The
 * primary test still fires 90+ poll calls. Each poll evaluates
 * the StrategyApp __getDebugState__ which builds a 50+ field
 * object every time. Net effect: the main test reads the entire
 * app state ~90 times in one run.
 *
 * Pinned invariants:
 *   1. The previous "≤30 polls per test" threshold from round 12
 *      hasn't really been honored — expectDebug counts as a poll.
 *   2. We cap any single test's expectDebug+expect.poll at 30.
 */
describe('Playwright poll density budget', () => {
  const specPath = join(__dirname, '..', 'strategy-map.spec.ts');
  const text = readFileSync(specPath, 'utf8');

  it('most-instrumented test has at most 30 expectDebug+poll calls', () => {
    const testBlocks = text.split(/\btest\(['"`]/);
    let max = 0;
    let maxTitle = '';
    for (let i = 1; i < testBlocks.length; i++) {
      const block = testBlocks[i];
      const polls =
        (block.match(/expect\.poll\(/g) ?? []).length +
        (block.match(/expectDebug\(/g) ?? []).length;
      if (polls > max) {
        max = polls;
        const titleEnd = block.search(/['"`]/);
        maxTitle = block.slice(0, titleEnd).slice(0, 80);
      }
    }
    expect.soft(max, `'${maxTitle}' has ${max} poll calls; cap is 30`).toBeLessThanOrEqual(30);
  });

  it('total expect.poll+expectDebug across the whole spec stays under 200', () => {
    const total =
      (text.match(/expect\.poll\(/g) ?? []).length + (text.match(/expectDebug\(/g) ?? []).length;
    expect.soft(total, `${total} poll calls in entire spec`).toBeLessThan(200);
  });
});
