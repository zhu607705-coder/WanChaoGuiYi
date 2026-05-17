import { describe, expect, it } from 'vitest';
import { readFileSync, existsSync } from 'node:fs';
import { join } from 'node:path';

/**
 * Bug under investigation: the C# DataModels.cs and the TS types.ts
 * both describe the same JSON schema. A field added or renamed in
 * C# must propagate to TS, otherwise Web silently ignores fields
 * that the headless adapter requires (or vice versa).
 *
 * This test reads both source files and pins the names of key
 * fields that MUST appear in both. When a field drifts (renamed in
 * one, not the other), the test fails on the side that's behind.
 */
describe('cross-language data contract alignment', () => {
  const repoRoot = join(__dirname, '..', '..', '..');
  const csharpPath = join(repoRoot, 'domain-core', 'src', 'Data', 'DataModels.cs');
  const tsPath = join(repoRoot, 'web-strategy-map', 'src', 'types.ts');

  if (!existsSync(csharpPath) || !existsSync(tsPath)) {
    it.skip('skipped: source files not found at expected paths', () => undefined);
    return;
  }

  const cs = readFileSync(csharpPath, 'utf8');
  const ts = readFileSync(tsPath, 'utf8');

  function inBoth(field: string): boolean {
    return cs.includes(field) && ts.includes(field);
  }

  function presentInCsButNotTs(fields: string[]): string[] {
    return fields.filter((f) => cs.includes(f) && !ts.includes(f));
  }

  // RegionDefinition fields. C# has gameplaySourceReference,
  // regionSpecialization, supplyNode, eraProfile that are NOT in TS.
  // They are arguably designer-side metadata, but TS code that round-trips
  // a save should at least preserve them as opaque.
  it('RegionDefinition fields agree on critical gameplay surface', () => {
    const critical = [
      'population',
      'foodOutput',
      'taxOutput',
      'manpower',
      'localPower',
      'rebellionRisk',
      'neighbors',
      'landStructure',
      'legitimacyMemory',
      'terrain'
    ];
    for (const f of critical) {
      expect.soft(inBoth(f), `${f} should appear in both DataModels.cs and types.ts`).toBe(true);
    }
  });

  it('TS types must declare gameplaySourceReference if C# uses it', () => {
    // Today this fails: gameplaySourceReference is in C# but missing
    // from TS RegionDefinition. Save round-trips drop the field.
    const drift = presentInCsButNotTs(['gameplaySourceReference']);
    expect(drift, `fields present in C# but missing in TS: ${drift.join(', ')}`).toEqual([]);
  });

  it('TS types must declare regionSpecialization if C# uses it', () => {
    const drift = presentInCsButNotTs(['regionSpecialization']);
    expect(drift, `fields present in C# but missing in TS: ${drift.join(', ')}`).toEqual([]);
  });

  it('TS types must declare supplyNode if C# uses it', () => {
    const drift = presentInCsButNotTs(['supplyNode']);
    expect(drift, `fields present in C# but missing in TS: ${drift.join(', ')}`).toEqual([]);
  });
});
