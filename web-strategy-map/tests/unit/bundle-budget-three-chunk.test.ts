import { describe, expect, it } from 'vitest';
import { existsSync, statSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

/**
 * Bug under investigation: previous round split three.js into its
 * own chunk (manualChunks). three-DGy14Mgf.js = 571,890 bytes,
 * still over Vite's built-in 500 kB advisory.
 *
 * The advisory exists because chunks > 500 kB significantly hurt
 * cold-load performance on slow networks (3G, rural). For a
 * historical strategy game targeting Chinese desktop players, this
 * is acceptable but not ideal. We pin the target as 500 kB.
 *
 * Pinned invariant: NO single JS chunk should exceed 500 kB.
 *
 * Today: three chunk is 571 kB, fails. The fix requires either:
 *   - Use three's modular imports (e.g. `three/examples/jsm/...`
 *     individually) plus tree-shaking analysis
 *   - Lazy-import three behind a "Start Game" button
 *   - Replace some three features (OrbitControls etc.) with
 *     lighter alternatives
 */
describe('Vite three.js chunk budget', () => {
  const distDir = join(__dirname, '..', '..', 'dist');
  const assetsDir = join(distDir, 'assets');

  it('no single JS chunk exceeds Vite advisory limit (500 kB)', () => {
    if (!existsSync(assetsDir)) {
      expect.soft(true, 'dist not built yet').toBe(true);
      return;
    }
    const jsFiles = readdirSync(assetsDir).filter((f) => f.endsWith('.js') && !f.endsWith('.map'));
    expect(jsFiles.length).toBeGreaterThan(0);

    const sizes: Record<string, number> = {};
    let largest = 0;
    let largestName = '';
    for (const f of jsFiles) {
      const size = statSync(join(assetsDir, f)).size;
      sizes[f] = size;
      if (size > largest) {
        largest = size;
        largestName = f;
      }
    }
    expect(
      largest,
      `largest chunk: ${largestName} = ${largest} bytes; all chunks: ${JSON.stringify(sizes)}`
    ).toBeLessThan(500_000);
  });

  it('index chunk stays under 250 kB (regression guard)', () => {
    if (!existsSync(assetsDir)) return;
    const jsFiles = readdirSync(assetsDir).filter(
      (f) => f.endsWith('.js') && !f.endsWith('.map') && f.startsWith('index')
    );
    expect(jsFiles.length).toBe(1);
    const size = statSync(join(assetsDir, jsFiles[0])).size;
    expect(size, `${jsFiles[0]} = ${size} bytes`).toBeLessThan(250_000);
  });
});
