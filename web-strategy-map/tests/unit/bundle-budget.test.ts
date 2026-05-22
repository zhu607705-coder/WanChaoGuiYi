import { expect, it } from 'vitest';
import {
  BUNDLE_SIZE_BUDGETS,
  describeBuiltBundle,
  largestAsset,
  totalAssetSize,
} from './bundle-budget-helpers';

/**
 * Bug under investigation: early Vite production builds emitted a
 * single ~800kB JS chunk + ~25kB CSS. Gzip reduced transfer size, but
 * raw chunk size still hurt cold-load performance and triggered Vite's
 * advisory. The current build is split; this file keeps the budget
 * from regressing.
 *
 * Pinned invariants:
 *   1. The largest single JS chunk should not exceed 600 kB raw.
 *   2. The CSS bundle should stay under 50 kB raw.
 *   3. dist/ should contain at least one JS chunk other than the
 *      main bundle (i.e. there should be SOME code-splitting today).
 *   4. Total JS should stay under 1.1 MB raw so code-splitting does
 *      not hide overall payload growth.
 *
 * This test runs as a unit test that consults the most recent build
 * output. It skips itself if dist/ does not exist (e.g. in CI before
 * the build step). When dist/ DOES exist, it enforces the budget.
 */
describeBuiltBundle('Vite bundle size budget', (bundle) => {
  it('largest JS chunk stays under 600 kB raw', () => {
    const jsFiles = bundle.jsFiles;
    expect(jsFiles.length).toBeGreaterThan(0);

    const largest = largestAsset(bundle, jsFiles);
    // Historical failure: ~801,510. Budget: 600,000.
    expect
      .soft(largest.size, `largest js chunk: ${largest.name} = ${largest.size} bytes`)
      .toBeLessThan(BUNDLE_SIZE_BUDGETS.maxJsChunkSoftBytes);
  });

  it('CSS bundle stays under 50 kB raw', () => {
    const cssFiles = bundle.cssFiles;
    if (cssFiles.length === 0) return;
    const largest = largestAsset(bundle, cssFiles);
    expect(largest.size).toBeLessThan(BUNDLE_SIZE_BUDGETS.maxCssBundleBytes);
  });

  it('total JS bundle stays under 1.1 MB raw', () => {
    const jsFiles = bundle.jsFiles;
    expect(jsFiles.length).toBeGreaterThan(0);
    const total = totalAssetSize(bundle, jsFiles);
    expect(total, `total js bundle size: ${total} bytes across ${jsFiles.length} chunks`).toBeLessThan(
      BUNDLE_SIZE_BUDGETS.maxTotalJsBytes
    );
  });

  it('build produces at least 2 JS chunks (some code splitting)', () => {
    const jsFiles = bundle.jsFiles;
    // Historical failure: just 1 chunk. Splitting should produce >=2.
    expect(jsFiles.length).toBeGreaterThanOrEqual(BUNDLE_SIZE_BUDGETS.minJsChunks);
  });
});
