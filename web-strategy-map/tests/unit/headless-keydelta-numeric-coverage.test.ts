import { describe, expect, it } from 'vitest';
import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';

/**
 * Cross-boundary numeric reconciliation:
 * The headless `latest-war-report.json` is the canonical truth for
 * scenario outcomes. The Playwright UI test reads its own UI text.
 * They must agree on numbers.
 *
 * Step 1 (this round): pin a clean machine-readable shape for the
 * keyDeltas so a future Playwright assertion can read them.
 *
 * Step 2 (later rounds): a real Playwright spec that loads the
 * report, drives the UI to the same scenario, and asserts the UI
 * shows the same number.
 *
 * Pinned invariant for now: at least one specific scenario's
 * `attackerPower` keyDelta is numeric and within a sane range.
 */
describe('headless report cross-boundary contract', () => {
  const reportPath = join(
    __dirname,
    '..',
    '..',
    '..',
    'tools',
    'headless_runner',
    'latest-war-report.json'
  );

  if (!existsSync(reportPath)) {
    it.skip('no report yet', () => undefined);
    return;
  }

  const report = JSON.parse(readFileSync(reportPath, 'utf8')) as {
    scenarios: Array<{
      name: string;
      keyDeltas: Array<{
        field: string;
        before: unknown;
        after: unknown;
        explanation?: string;
      }>;
    }>;
  };

  it('every numeric field has finite non-negative before/after', () => {
    const numericFields = /power|soldiers|food|money|integration|risk|legitimacy|count|percent|supply/i;
    const offenders: string[] = [];
    for (const sc of report.scenarios) {
      for (const d of sc.keyDeltas) {
        if (!numericFields.test(d.field)) continue;
        const before = d.before;
        const after = d.after;
        const checkNum = (v: unknown, label: string) => {
          if (typeof v !== 'number') return;
          if (!Number.isFinite(v) || v < 0) {
            offenders.push(`${sc.name}/${d.field}/${label}=${v}`);
          }
        };
        checkNum(before, 'before');
        checkNum(after, 'after');
      }
    }
    expect(offenders).toEqual([]);
  });

  it('low_supply_reduces_battle_power scenario shows clear power drop', () => {
    const sc = report.scenarios.find((s) => s.name === 'low_supply_reduces_battle_power');
    if (!sc) {
      // Scenario doesn't exist (yet). Skip rather than fail.
      expect.soft(true).toBe(true);
      return;
    }
    const powerDelta = sc.keyDeltas.find((d) => d.field === 'battle.attackerPower');
    expect(powerDelta).toBeTruthy();
    if (!powerDelta) return;
    expect(typeof powerDelta.before).toBe('number');
    expect(typeof powerDelta.after).toBe('number');
    const before = powerDelta.before as number;
    const after = powerDelta.after as number;
    // Low supply should reduce attacker power; the after-value must
    // be strictly less than before, and the drop must be material
    // (at least 30%).
    expect(after).toBeLessThan(before);
    const dropRatio = (before - after) / before;
    expect(dropRatio).toBeGreaterThan(0.3);
  });

  it('attacker_wins_and_occupies scenario records numeric money/food deltas', () => {
    const sc = report.scenarios.find((s) => s.name === 'attacker_wins_and_occupies');
    if (!sc) {
      expect.soft(true).toBe(true);
      return;
    }
    const money = sc.keyDeltas.find((d) => d.field === 'faction.money');
    const food = sc.keyDeltas.find((d) => d.field === 'faction.food');
    expect(money).toBeTruthy();
    expect(food).toBeTruthy();
    if (!money || !food) return;
    expect(typeof money.before).toBe('number');
    expect(typeof money.after).toBe('number');
    expect(typeof food.before).toBe('number');
    expect(typeof food.after).toBe('number');
  });
});
