import { describe, expect, it } from 'vitest';
import { describeHeadlessReport } from './headless-report-helpers';

/**
 * Bug under investigation: previous round added numeric keyDeltas.
 * Verify the contract more thoroughly:
 *   - At least 50% of all keyDelta values across the report are
 *     numeric (not strings, not booleans).
 *   - Field names that look numeric (money, food, soldiers,
 *     contribution, percent, count) MUST have numeric values.
 */
describe('headless war report numeric coverage', () => {
  describeHeadlessReport('real latest-war-report.json', (report) => {
    it('at least 50% of all keyDelta entries have numeric before/after', () => {
      let total = 0;
      let numeric = 0;
      for (const sc of report.scenarios ?? []) {
        for (const d of sc.keyDeltas ?? []) {
          total++;
          if (typeof d.before === 'number' && typeof d.after === 'number') numeric++;
        }
      }
      expect.soft(
        total,
        `total keyDeltas: ${total}`
      ).toBeGreaterThan(0);
      if (total > 0) {
        const ratio = numeric / total;
        expect.soft(
          ratio,
          `numeric/${total} = ${(ratio * 100).toFixed(1)}%`
        ).toBeGreaterThan(0.5);
      }
    });

    it('keyDelta fields with numeric-sounding names have numeric values', () => {
      const numericLike = /money|food|soldiers|contribution|percent|count|integration|risk|legitimacy/i;
      const offenders: string[] = [];
      for (const sc of report.scenarios ?? []) {
        for (const d of sc.keyDeltas ?? []) {
          if (numericLike.test(d.field ?? '')) {
            if (typeof d.before !== 'number' || typeof d.after !== 'number') {
              offenders.push(`${sc.name}/${d.field} = ${JSON.stringify(d.before)} -> ${JSON.stringify(d.after)}`);
            }
          }
        }
      }
      expect.soft(
        offenders,
        `numeric-named fields with non-numeric values: ${offenders.slice(0, 3).join(' | ')}`
      ).toEqual([]);
    });
  });
});
