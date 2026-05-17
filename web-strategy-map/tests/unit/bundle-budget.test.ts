import { describe, expect, it } from 'vitest';
import { existsSync, statSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

/**
 * Bug under investigation: Vite production build emits a single
 * ~800kB JS chunk + ~25kB CSS. Gzip drops it to ~205kB but the
 * un-gzipped wire cost is paid by every cold visitor. The build
 * itself emits an advisory; CI doesn't fail on it.
 *
 * Pinned invariants:
 *   1. The largest single JS chunk should not exceed 600 kB raw
 *      (today ~801kB, so this fails).
 *   2. The CSS bundle should stay under 50 kB raw (today ~25kB,
 *      passes — but pinning prevents regression).
 *   3. dist/ should contain at least one JS chunk other than the
 *      main bundle (i.e. there should be SOME code-splitting today).
 *
 * This test runs as a unit test that consults the most recent build
 * output. It skips itself if dist/ does not exist (e.g. in CI before
 * the build step). When dist/ DOES exist, it enforces the budget.
 */
describe('Vite bundle size budget', () => {
  const distDir = join(__dirname, '..', '..', 'dist');
  const assetsDir = join(distDir, 'assets');

  function listAssets(): string[] {
    if (!existsSync(assetsDir)) return [];
    return readdirSync(assetsDir);
  }

  it('skips when dist not built', () => {
    if (!existsSync(distDir)) {
      expect.soft(true).toBe(true);
      return;
    }
  });

  it('largest JS chunk stays under 600 kB raw', () => {
    if (!existsSync(assetsDir)) return;
    const jsFiles = listAssets().filter((f) => f.endsWith('.js'));
    expect(jsFiles.length).toBeGreaterThan(0);

    let largest = 0;
    let largestName = '';
    for (const f of jsFiles) {
      const size = statSync(join(assetsDir, f)).size;
      if (size > largest) {
        largest = size;
        largestName = f;
      }
    }
    // Today: ~801,510. Budget: 600,000.
    expect.soft(largest, `largest js chunk: ${largestName} = ${largest} bytes`).toBeLessThan(600_000);
  });

  it('CSS bundle stays under 50 kB raw', () => {
    if (!existsSync(assetsDir)) return;
    const cssFiles = listAssets().filter((f) => f.endsWith('.css'));
    if (cssFiles.length === 0) return;
    let largest = 0;
    for (const f of cssFiles) {
      const size = statSync(join(assetsDir, f)).size;
      if (size > largest) largest = size;
    }
    expect(largest).toBeLessThan(50_000);
  });

  it('build produces at least 2 JS chunks (some code splitting)', () => {
    if (!existsSync(assetsDir)) return;
    const jsFiles = listAssets().filter((f) => f.endsWith('.js') && !f.endsWith('.map'));
    // Today: just 1 chunk. Splitting would produce >=2.
    expect(jsFiles.length).toBeGreaterThanOrEqual(2);
  });
});
