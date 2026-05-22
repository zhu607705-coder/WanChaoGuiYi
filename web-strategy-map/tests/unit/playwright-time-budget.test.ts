import { describe, expect, it } from 'vitest';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';

/**
 * Regression guard: Playwright E2E tests can become slow either by
 * raising local timeout budgets or by repeatedly polling the full
 * StrategyApp debug state.
 *
 * Pinned invariants:
 *   1. No single Playwright test should set a timeout > 90 seconds.
 *      (90s is generous; full suite is ~6.8 min today.)
 *   2. Any single test should stay under 30 expectDebug+expect.poll
 *      calls.
 *   3. The whole spec should stay under 200 expectDebug+expect.poll
 *      calls.
 */
describe('Playwright per-test time budget', () => {
  const specPath = join(__dirname, '..', 'strategy-map.spec.ts');
  const text = readFileSync(specPath, 'utf8');

  interface TestBlockBudget {
    readonly title: string;
    readonly expectPoll: number;
    readonly expectDebug: number;
    readonly totalPollLikeCalls: number;
  }

  function parseTestBlocks(): TestBlockBudget[] {
    return text
      .split(/\btest\(['"`]/)
      .slice(1)
      .map((block) => {
        const titleEnd = block.search(/['"`]/);
        const title = block.slice(0, titleEnd >= 0 ? titleEnd : 80).slice(0, 80);
        const expectPoll = (block.match(/expect\.poll\(/g) ?? []).length;
        const expectDebug = (block.match(/expectDebug\(/g) ?? []).length;
        return {
          title,
          expectPoll,
          expectDebug,
          totalPollLikeCalls: expectPoll + expectDebug,
        };
      });
  }

  it('no local test timeout budget is greater than 90 seconds', () => {
    // CI can multiply these budgets, but the checked base value stays local and human-scale.
    const literalMatches = [...text.matchAll(/test\.setTimeout\(\s*([0-9_]+)\s*\)/g)].map((m) => m[1]);
    const wrappedMatches = [...text.matchAll(/test\.setTimeout\(\s*playwrightTimeout\(\s*([0-9_]+)\s*\)\s*\)/g)].map(
      (m) => m[1]
    );
    const budgets = [...literalMatches, ...wrappedMatches];
    expect(budgets.length).toBeGreaterThan(0);
    for (const budget of budgets) {
      const ms = Number.parseInt(budget.replace(/_/g, ''), 10);
      expect.soft(ms, `local setTimeout budget ${ms} ms`).toBeLessThanOrEqual(90_000);
    }
  });

  it('most-instrumented test has at most 30 expectDebug+expect.poll calls', () => {
    const mostInstrumented = parseTestBlocks().reduce<TestBlockBudget | undefined>(
      (max, block) => (max === undefined || block.totalPollLikeCalls > max.totalPollLikeCalls ? block : max),
      undefined
    );
    expect(mostInstrumented).toBeDefined();
    expect.soft(
      mostInstrumented?.totalPollLikeCalls ?? 0,
      `most-instrumented test '${mostInstrumented?.title}' has ${mostInstrumented?.totalPollLikeCalls} expectDebug+poll calls; consider splitting`
    ).toBeLessThanOrEqual(30);
  });

  it('total expectDebug+expect.poll calls across the whole spec stays under 200', () => {
    const total = parseTestBlocks().reduce((sum, block) => sum + block.totalPollLikeCalls, 0);
    expect.soft(total, `${total} expectDebug+poll calls in entire spec`).toBeLessThan(200);
  });
});
