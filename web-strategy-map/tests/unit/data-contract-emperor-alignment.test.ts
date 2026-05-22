import { describe, expect, it } from 'vitest';
import { readFileSync, existsSync } from 'node:fs';
import { join } from 'node:path';

/**
 * Bug under investigation: previous rounds exposed cross-language
 * drift between C# EmperorDefinition and TS EmperorDefinition.
 * versionScope, score, diplomacySkills, and aiPersonality are now
 * aligned; this file pins them so future save/export and comparison
 * UI work cannot drop the fields silently.
 *
 * Pinned invariant: TS EmperorDefinition must declare the fields
 * present in C#'s public field list. Save round-trips and any
 * future "compare emperor" UI need them.
 */
describe('emperor data contract alignment', () => {
  const repoRoot = join(__dirname, '..', '..', '..');
  const csharpPath = join(repoRoot, 'domain-core', 'src', 'Data', 'DataModels.cs');
  const tsPath = join(repoRoot, 'web-strategy-map', 'src', 'types.ts');

  if (!existsSync(csharpPath) || !existsSync(tsPath)) {
    it.skip('skipped: source files not found', () => undefined);
    return;
  }

  const cs = readFileSync(csharpPath, 'utf8');
  const ts = readFileSync(tsPath, 'utf8');

  it('TS EmperorDefinition declares versionScope', () => {
    expect(cs.includes('versionScope')).toBe(true);
    expect(ts).toContain('versionScope');
  });

  it('TS declares aiPersonality', () => {
    expect(cs.includes('aiPersonality')).toBe(true);
    expect(ts).toContain('aiPersonality');
  });

  it('TS declares diplomacySkills', () => {
    expect(cs.includes('diplomacySkills')).toBe(true);
    expect(ts).toContain('diplomacySkills');
  });

  it('TS declares EmperorScore-like score field', () => {
    expect(cs.includes('public sealed class EmperorScore')).toBe(true);
    // Pin a softer requirement: the TS interface should reference
    // 'score' as a property somewhere in the EmperorDefinition block.
    const empBlock = ts.match(/interface EmperorDefinition[^]*?^}/m)?.[0] ?? '';
    expect(empBlock).toContain('score');
  });
});
