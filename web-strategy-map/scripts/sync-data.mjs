import { copyFileSync, existsSync, mkdirSync, readdirSync, rmSync, statSync } from 'node:fs';
import { dirname, resolve } from 'node:path';

const appRoot = resolve(import.meta.dirname, '..');
const sourceRoot = resolve(appRoot, 'game-data-source');
const targetRoot = resolve(appRoot, 'public/game-data');

if (!existsSync(sourceRoot)) {
  throw new Error(`Missing canonical web data source: ${sourceRoot}`);
}

rmSync(targetRoot, { recursive: true, force: true });
const stats = copyTree(sourceRoot, targetRoot);

console.log(
  `Synced ${stats.files} web game-data source files: ${stats.json} json, ${stats.mp3} audio, ${stats.png} map/image.`
);

function copyTree(sourceDir, targetDir) {
  const totals = { files: 0, json: 0, mp3: 0, png: 0 };

  for (const entry of readdirSync(sourceDir)) {
    if (entry === 'README.md') continue;

    const sourcePath = resolve(sourceDir, entry);
    const targetPath = resolve(targetDir, entry);
    const stat = statSync(sourcePath);
    if (stat.isDirectory()) {
      addTotals(totals, copyTree(sourcePath, targetPath));
      continue;
    }

    mkdirSync(dirname(targetPath), { recursive: true });
    copyFileSync(sourcePath, targetPath);
    totals.files += 1;

    const lowerName = entry.toLowerCase();
    if (lowerName.endsWith('.json')) totals.json += 1;
    if (lowerName.endsWith('.mp3')) totals.mp3 += 1;
    if (lowerName.endsWith('.png')) totals.png += 1;
  }

  return totals;
}

function addTotals(target, source) {
  target.files += source.files;
  target.json += source.json;
  target.mp3 += source.mp3;
  target.png += source.png;
}
