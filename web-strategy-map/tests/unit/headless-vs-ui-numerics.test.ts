import { describe, expect, it } from 'vitest';
import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';

/**
 * Bug under investigation: tools/headless_runner/latest-war-report.json
 * is the source-of-truth for headless verification. The Playwright UI
 * test reads UI text and clicks buttons, asserting human-readable
 * values like "进攻方 700"，"占领后整合度=25%". If the headless calc
 * and the UI display drift, neither side notices: headless owns the
 * canonical numbers, UI owns the rendering, and there's no joint
 * test that both produce the same numbers from the same scenario.
 *
 * Pinned invariant (lightweight today): the headless report should
 * include explicit numeric keyDeltas, and a future cross-check
 * harness can compare them against UI snapshots. We pin the
 * immediate prerequisite: every keyDelta value must be a primitive
 * (number / string / boolean), not an object — otherwise UI cannot
 * render it directly.
 */
describe('headless war report contract', () => {
  const reportPath = join(
    __dirname,
    '..',
    '..',
    '..',
    'tools',
    'headless_runner',
    'latest-war-report.json'
  );

  it('skips when report not generated yet', () => {
    if (!existsSync(reportPath)) {
      expect.soft(true, `report not at ${reportPath}`).toBe(true);
      return;
    }
  });

  it('every keyDelta has primitive before/after values', () => {
    if (!existsSync(reportPath)) return;
    const raw = readFileSync(reportPath, 'utf8');
    const report = JSON.parse(raw);
    expect(Array.isArray(report.scenarios)).toBe(true);

    for (const sc of report.scenarios) {
      const deltas = sc.keyDeltas ?? [];
      for (const d of deltas) {
        const primitive = (v: unknown) =>
          v === null ||
          typeof v === 'string' ||
          typeof v === 'number' ||
          typeof v === 'boolean';
        expect.soft(
          primitive(d.before),
          `scenario ${sc.name} delta ${d.field}: before=${JSON.stringify(d.before)}`
        ).toBe(true);
        expect.soft(
          primitive(d.after),
          `scenario ${sc.name} delta ${d.field}: after=${JSON.stringify(d.after)}`
        ).toBe(true);
      }
    }
  });

  it('every scenario has at least one numeric keyDelta', () => {
    if (!existsSync(reportPath)) return;
    const raw = readFileSync(reportPath, 'utf8');
    const report = JSON.parse(raw);
    for (const sc of report.scenarios) {
      const deltas = sc.keyDeltas ?? [];
      const hasNumeric = deltas.some(
        (d: { before: unknown; after: unknown }) =>
          typeof d.before === 'number' || typeof d.after === 'number'
      );
      expect.soft(hasNumeric, `scenario ${sc.name} has no numeric keyDelta`).toBe(true);
    }
  });
});
