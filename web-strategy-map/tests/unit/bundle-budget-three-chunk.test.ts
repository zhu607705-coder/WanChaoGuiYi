import { expect, it } from 'vitest';
import { BUNDLE_SIZE_BUDGETS, describeBuiltBundle, largestAsset } from './bundle-budget-helpers';

/**
 * Bug under investigation: earlier builds split three.js into a
 * single 571,890 byte chunk, still over Vite's built-in 500 kB
 * advisory. The current build splits three into smaller chunks; this
 * file keeps that budget from regressing.
 *
 * The advisory exists because chunks > 500 kB significantly hurt
 * cold-load performance on slow networks (3G, rural). For a
 * historical strategy game targeting Chinese desktop players, this
 * is acceptable but not ideal. We pin the target as 500 kB.
 *
 * Pinned invariant: NO single JS chunk should exceed 500 kB.
 *
 * If this fails again, check for:
 *   - broad `import * as THREE from 'three'` imports
 *   - new renderer/post-processing modules pulled into the hot path
 *   - manual chunk settings collapsing back into one three bundle
 */
describeBuiltBundle('Vite three.js chunk budget', (bundle) => {
  it('no single JS chunk exceeds Vite advisory limit (500 kB)', () => {
    const jsFiles = bundle.jsFiles;
    expect(jsFiles.length).toBeGreaterThan(0);

    const sizes: Record<string, number> = {};
    for (const f of jsFiles) {
      sizes[f] = bundle.sizeOf(f);
    }
    const largest = largestAsset(bundle, jsFiles);
    expect(
      largest.size,
      `largest chunk: ${largest.name} = ${largest.size} bytes; all chunks: ${JSON.stringify(sizes)}`
    ).toBeLessThan(BUNDLE_SIZE_BUDGETS.viteAdvisoryChunkBytes);
  });

  it('index chunk stays under 250 kB (regression guard)', () => {
    const jsFiles = bundle.jsFiles.filter((f) => f.startsWith('index'));
    expect(jsFiles.length).toBe(1);
    const size = bundle.sizeOf(jsFiles[0]);
    expect(size, `${jsFiles[0]} = ${size} bytes`).toBeLessThan(BUNDLE_SIZE_BUDGETS.maxIndexChunkBytes);
  });
});
