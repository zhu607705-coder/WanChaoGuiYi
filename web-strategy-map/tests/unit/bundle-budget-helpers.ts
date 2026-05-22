import { existsSync, readdirSync, statSync } from 'node:fs';
import { join } from 'node:path';
import { describe, it } from 'vitest';

export const BUNDLE_SIZE_BUDGETS = {
  maxJsChunkSoftBytes: 600_000,
  viteAdvisoryChunkBytes: 500_000,
  maxIndexChunkBytes: 250_000,
  maxThreeRendererChunkBytes: 400_000,
  maxCssBundleBytes: 50_000,
  maxTotalJsBytes: 1_100_000,
  minJsChunks: 2,
  minThreeChunks: 3,
} as const;

export interface BuiltBundleAssets {
  readonly assetsDir: string;
  readonly files: readonly string[];
  readonly jsFiles: readonly string[];
  readonly cssFiles: readonly string[];
  sizeOf(fileName: string): number;
}

export interface SizedAsset {
  readonly name: string;
  readonly size: number;
}

export function loadBuiltBundleAssets(): BuiltBundleAssets | undefined {
  const distDir = join(__dirname, '..', '..', 'dist');
  const assetsDir = join(distDir, 'assets');
  if (!existsSync(assetsDir)) return undefined;

  const files = readdirSync(assetsDir);
  const jsFiles = files.filter((f) => f.endsWith('.js') && !f.endsWith('.map'));
  const cssFiles = files.filter((f) => f.endsWith('.css'));

  return {
    assetsDir,
    files,
    jsFiles,
    cssFiles,
    sizeOf: (fileName: string) => statSync(join(assetsDir, fileName)).size,
  };
}

export function describeBuiltBundle(name: string, factory: (bundle: BuiltBundleAssets) => void): void {
  const bundle = loadBuiltBundleAssets();
  if (bundle === undefined) {
    describe(name, () => {
      it.skip('dist not built yet');
    });
    return;
  }

  describe(name, () => factory(bundle));
}

export function largestAsset(bundle: BuiltBundleAssets, files: readonly string[]): SizedAsset {
  return files.reduce<SizedAsset>(
    (largest, fileName) => {
      const size = bundle.sizeOf(fileName);
      return size > largest.size ? { name: fileName, size } : largest;
    },
    { name: '', size: 0 }
  );
}

export function totalAssetSize(bundle: BuiltBundleAssets, files: readonly string[]): number {
  return files.reduce((sum, fileName) => sum + bundle.sizeOf(fileName), 0);
}
