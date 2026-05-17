import { describe, expect, it } from 'vitest';
import { existsSync, statSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

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
describe('Vite three-renderer chunk regression guard', () => {
  const distDir = join(__dirname, '..', '..', 'dist');
  const assetsDir = join(distDir, 'assets');

  it('three-renderer chunk stays under 400 kB', () => {
    if (!existsSync(assetsDir)) return;
    const files = readdirSync(assetsDir).filter(
      (f) => f.endsWith('.js') && !f.endsWith('.map') && f.startsWith('three-renderer')
    );
    if (files.length === 0) return;
    expect(files.length).toBe(1);
    const size = statSync(join(assetsDir, files[0])).size;
    expect(size, `${files[0]} = ${size} bytes`).toBeLessThan(400_000);
  });

  it('three.js is split into at least 3 chunks', () => {
    if (!existsSync(assetsDir)) return;
    const threeChunks = readdirSync(assetsDir).filter(
      (f) => f.endsWith('.js') && !f.endsWith('.map') && f.startsWith('three-')
    );
    expect.soft(threeChunks.length, `three chunks: ${threeChunks.join(', ')}`).toBeGreaterThanOrEqual(3);
  });
});
