import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { describe, it } from 'vitest';

export interface HeadlessReport {
  readonly scenarios: readonly HeadlessScenario[];
}

export interface HeadlessScenario {
  readonly name: string;
  readonly keyDeltas: readonly HeadlessKeyDelta[];
}

export interface HeadlessKeyDelta {
  readonly field: string;
  readonly before: unknown;
  readonly after: unknown;
  readonly explanation?: string;
}

export const HEADLESS_WAR_REPORT_PATH = join(
  __dirname,
  '..',
  '..',
  '..',
  'tools',
  'headless_runner',
  'latest-war-report.json'
);

export function describeHeadlessReport(name: string, factory: (report: HeadlessReport) => void): void {
  if (!existsSync(HEADLESS_WAR_REPORT_PATH)) {
    describe(name, () => {
      it.skip('no report yet');
    });
    return;
  }

  const report = parseHeadlessReport(readFileSync(HEADLESS_WAR_REPORT_PATH, 'utf8'));
  describe(name, () => factory(report));
}

export function parseHeadlessReport(raw: string): HeadlessReport {
  const parsed = JSON.parse(raw) as unknown;
  if (!isRecord(parsed)) {
    throw new Error('report root must be an object');
  }
  if (!Array.isArray(parsed.scenarios) || parsed.scenarios.length === 0) {
    throw new Error('scenarios must be a non-empty array');
  }

  return {
    scenarios: parsed.scenarios.map((scenario, index) => {
      if (!isRecord(scenario)) {
        throw new Error(`scenario at index ${index} must be an object`);
      }
      const name = typeof scenario.name === 'string' ? scenario.name : `#${index}`;
      if (!Array.isArray(scenario.keyDeltas)) {
        throw new Error(`scenario ${name} keyDeltas must be an array`);
      }
      return {
        name,
        keyDeltas: scenario.keyDeltas.map((delta, deltaIndex) => {
          if (!isRecord(delta)) {
            throw new Error(`scenario ${name} keyDelta ${deltaIndex} must be an object`);
          }
          return {
            field: typeof delta.field === 'string' ? delta.field : `#${deltaIndex}`,
            before: delta.before,
            after: delta.after,
            explanation: typeof delta.explanation === 'string' ? delta.explanation : undefined,
          };
        }),
      };
    }),
  };
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}
