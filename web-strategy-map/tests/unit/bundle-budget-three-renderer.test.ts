import { expect, it } from 'vitest';
import { BUNDLE_SIZE_BUDGETS, describeBuiltBundle } from './bundle-budget-helpers';

/**
 * Bug under investigation: agent split three into 4 chunks
 * (core 202K + renderer 354K + controls 113K + runtime 0K).
 * The largest chunk now is three-renderer at 353,931 bytes — well
 * below the 500K Vite advisory but still significant.
 *
 * Pinned invariant: the three-renderer chunk must stay under
 * 400 kB. WebGLRenderer is the heaviest piece of three; if it
 * grows past 400K, that's a smell — check if a new feature
 * (post-processing, shaders) is being pulled into the main render
 * path when it shouldn't be.
 */
describeBuiltBundle('Vite three-renderer chunk regression guard', (bundle) => {
  it('three-renderer chunk stays under 400 kB', () => {
    const files = bundle.jsFiles.filter((f) => f.startsWith('three-renderer'));
    if (files.length === 0) return;
    expect(files.length).toBe(1);
    const size = bundle.sizeOf(files[0]);
    expect(size, `${files[0]} = ${size} bytes`).toBeLessThan(BUNDLE_SIZE_BUDGETS.maxThreeRendererChunkBytes);
  });

  it('three.js is split into at least 3 chunks', () => {
    const threeChunks = bundle.jsFiles.filter((f) => f.startsWith('three-'));
    expect
      .soft(threeChunks.length, `three chunks: ${threeChunks.join(', ')}`)
      .toBeGreaterThanOrEqual(BUNDLE_SIZE_BUDGETS.minThreeChunks);
  });
});
