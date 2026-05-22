import { describe, expect, it } from 'vitest';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';

/**
 * Regression guard: the generated Jiuzhou map image is a runtime asset.
 * If it is missing or fails to decode, the scene should keep a visible
 * fallback plane instead of relying on TextureLoader's silent default path.
 *
 * Pinned invariants:
 *   1. The generated map texture load has an explicit onError callback.
 *   2. buildMapTexture keeps a fallback material path that can render before
 *      or after the texture load fails.
 */
describe('StrategyScene map texture loading', () => {
  const scenePath = join(__dirname, '..', '..', 'src', 'scene.ts');
  const source = readFileSync(scenePath, 'utf8');

  it('loads the map texture with an explicit error callback and fallback path', () => {
    const callBody = extractCallBody(source, 'new TextureLoader().load');
    const args = splitTopLevelArguments(callBody);

    expect(source).toContain('/game-data/map/jiuzhou_generated_map.png');
    expect(args[0]).toMatch(/mapTextureUrl|['"]\/game-data\/map\/jiuzhou_generated_map\.png['"]/);
    expect(args.length, 'TextureLoader.load should pass url, onLoad, onProgress, onError').toBeGreaterThanOrEqual(4);
    expect(args[3], 'TextureLoader.load onError callback should be explicit').toMatch(/=>|function/);
    expect(source, 'buildMapTexture should keep a named fallback path').toMatch(/fallback/i);
  });
});

function extractCallBody(source: string, callee: string): string {
  const calleeIndex = source.indexOf(callee);
  expect(calleeIndex, `${callee} call should exist`).toBeGreaterThanOrEqual(0);

  const openParen = source.indexOf('(', calleeIndex + callee.length);
  expect(openParen, `${callee} opening paren should exist`).toBeGreaterThanOrEqual(0);

  let depth = 0;
  let quote: '"' | "'" | '`' | null = null;
  let escaped = false;
  for (let i = openParen; i < source.length; i++) {
    const char = source[i];
    if (quote) {
      if (escaped) {
        escaped = false;
      } else if (char === '\\') {
        escaped = true;
      } else if (char === quote) {
        quote = null;
      }
      continue;
    }

    if (char === '"' || char === "'" || char === '`') {
      quote = char;
      continue;
    }
    if (char === '(') depth += 1;
    if (char === ')') {
      depth -= 1;
      if (depth === 0) return source.slice(openParen + 1, i);
    }
  }

  throw new Error(`Could not find ${callee} closing paren`);
}

function splitTopLevelArguments(callBody: string): string[] {
  const args: string[] = [];
  let start = 0;
  let depth = 0;
  let quote: '"' | "'" | '`' | null = null;
  let escaped = false;

  for (let i = 0; i < callBody.length; i++) {
    const char = callBody[i];
    if (quote) {
      if (escaped) {
        escaped = false;
      } else if (char === '\\') {
        escaped = true;
      } else if (char === quote) {
        quote = null;
      }
      continue;
    }

    if (char === '"' || char === "'" || char === '`') {
      quote = char;
      continue;
    }
    if (char === '(' || char === '[' || char === '{') depth += 1;
    if (char === ')' || char === ']' || char === '}') depth -= 1;
    if (char === ',' && depth === 0) {
      args.push(callBody.slice(start, i).trim());
      start = i + 1;
    }
  }

  args.push(callBody.slice(start).trim());
  return args.filter(Boolean);
}
