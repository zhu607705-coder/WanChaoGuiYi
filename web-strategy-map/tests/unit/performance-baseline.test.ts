import { describe, expect, it, beforeEach, afterEach, vi } from 'vitest';
import { readFileSync } from 'node:fs';
import {
  loadStrategyDataset,
  aggregateNationFood,
  aggregateNationMoney,
  type NationAggregationInput
} from '../../src/data';

/**
 * Performance baselines (real wall-clock, not bundle-size or
 * static analysis). These guard the user-visible "is the game
 * snappy?" experience.
 *
 * Pinned invariants:
 *   1. loadStrategyDataset on a 56-region in-memory fetch finishes
 *      under 1 second on dev machines.
 *   2. aggregateNation* on 100 regions finishes under 5ms.
 *   3. 1000 sequential calls to aggregateNation* finish under
 *      100ms (i.e. < 0.1ms each).
 *
 * These numbers are GENEROUS for CI on a slow VM. If they fail,
 * something has been pulled into the hot path that shouldn't be
 * there.
 */
describe('performance baselines', () => {
  let originalFetch: typeof globalThis.fetch;
  const gameDataSourceRoot = new URL('../../game-data-source/', import.meta.url);

  beforeEach(() => {
    originalFetch = globalThis.fetch;
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('aggregateNationFood on 100 regions completes in < 5ms', () => {
    const regions: NationAggregationInput[] = Array.from({ length: 100 }, (_, i) => ({
      owner: i % 2 === 0 ? 'player' : 'rival',
      foodOutput: 100 + i,
      taxOutput: 50 + i,
      contribution: 50 + (i % 50)
    }));
    const start = performance.now();
    const result = aggregateNationFood(regions);
    const elapsed = performance.now() - start;
    expect(typeof result).toBe('number');
    expect(elapsed).toBeLessThan(5);
  });

  it('1000 sequential aggregateNationMoney calls complete in < 100ms', () => {
    const regions: NationAggregationInput[] = Array.from({ length: 56 }, (_, i) => ({
      owner: 'player',
      foodOutput: 100,
      taxOutput: 50 + i,
      contribution: 70
    }));
    const start = performance.now();
    let sum = 0;
    for (let i = 0; i < 1000; i++) {
      sum += aggregateNationMoney(regions);
    }
    const elapsed = performance.now() - start;
    expect(sum).toBeGreaterThan(0);
    expect(elapsed, `1000 aggregations took ${elapsed.toFixed(1)}ms`).toBeLessThan(100);
  });

  it('loadStrategyDataset with a valid stub dataset completes in < 1000ms', async () => {
    globalThis.fetch = vi.fn(async (input) => {
      const url = typeof input === 'string' ? input : input.toString();
      if (url.endsWith('regions.json')) {
        return jsonResponse({
          items: [
            makeRegion('guanzhong', '关中', ['hanzhong']),
            makeRegion('hanzhong', '汉中', ['guanzhong']),
            makeRegion('hexi', '河西', ['liangzhou']),
            makeRegion('liangzhou', '凉州', ['hexi'])
          ]
        });
      }
      if (url.endsWith('map_region_shapes.json')) {
        return jsonResponse({
          items: [
            makeShape('guanzhong', 0, 0),
            makeShape('hanzhong', 3, 0),
            makeShape('hexi', -3, 1),
            makeShape('liangzhou', -5, 1)
          ]
        });
      }
      if (url.endsWith('units.json')) {
        return jsonResponse({
          items: [
            {
              id: 'infantry',
              name: '步军',
              category: 'infantry',
              upkeep: { food: 1, money: 1 },
              stats: { attack: 10, defense: 12, mobility: 4, siege: 2, supplyUse: 1 }
            },
            {
              id: 'cavalry',
              name: '骑军',
              category: 'cavalry',
              upkeep: { food: 2, money: 2 },
              stats: { attack: 14, defense: 8, mobility: 10, siege: 1, supplyUse: 2 }
            }
          ]
        });
      }
      if (url.endsWith('map_render_metadata.json')) {
        return jsonResponse({
          schemaVersion: 1,
          precision: 'high',
          shapeCenter: { x: 0, y: 0 },
          pixelsPerShapeUnit: 1,
          spritePixelsPerUnit: 1,
          sourceImage: '/game-data/map/jiuzhou_generated_map.png',
          imageSize: { width: 1, height: 1 }
        });
      }
      if (url.endsWith('narration_script.json')) {
        return jsonResponse({
          schemaVersion: 1,
          description: '',
          tutorial: { title: '', segments: [] },
          emperor_voices: []
        });
      }
      return jsonResponse({ items: [] });
    }) as typeof fetch;

    const start = performance.now();
    const dataset = await loadStrategyDataset();
    const elapsed = performance.now() - start;
    expect(dataset.regions).toHaveLength(4);
    expect(dataset.route.from.definition.id).toBe('guanzhong');
    expect(dataset.route.target.definition.id).toBe('hanzhong');
    expect(elapsed, `loadStrategyDataset took ${elapsed.toFixed(0)}ms`).toBeLessThan(1000);
  });

  it('loadStrategyDataset with real 56-region fixtures completes in < 1000ms', async () => {
    installRealGameDataFetch(gameDataSourceRoot);

    const start = performance.now();
    const dataset = await loadStrategyDataset();
    const elapsed = performance.now() - start;

    expect(dataset.regions).toHaveLength(56);
    expect(dataset.regionById.size).toBe(56);
    expect(dataset.chronicleEvents).toHaveLength(200);
    expect(dataset.audio.sceneMusic.length).toBeGreaterThan(0);
    expect(dataset.audio.narration.tutorial.segments.length).toBeGreaterThan(0);
    expect(elapsed, `real loadStrategyDataset took ${elapsed.toFixed(0)}ms`).toBeLessThan(1000);
  });
});

function jsonResponse(value: unknown): Response {
  return new Response(JSON.stringify(value), { status: 200 });
}

function rawJsonResponse(raw: string): Response {
  return new Response(raw, {
    status: 200,
    headers: { 'content-type': 'application/json' }
  });
}

function installRealGameDataFetch(gameDataSourceRoot: URL): void {
  globalThis.fetch = vi.fn(async (input) => {
    const urlText = typeof input === 'string'
      ? input
      : input instanceof URL
        ? input.toString()
        : input.url;
    const pathname = new URL(urlText, 'http://unit.test').pathname;
    const relativePath = pathname.startsWith('/game-data/')
      ? pathname.slice('/game-data/'.length)
      : '';
    if (relativePath === '') {
      return new Response('not found', { status: 404 });
    }

    const fixtureUrl = new URL(relativePath, gameDataSourceRoot);
    try {
      return rawJsonResponse(readFileSync(fixtureUrl, 'utf8'));
    } catch (error) {
      return new Response(error instanceof Error ? error.message : 'not found', { status: 404 });
    }
  }) as typeof fetch;
}

function makeRegion(id: string, name: string, neighbors: string[]): Record<string, unknown> {
  return {
    id,
    name,
    terrain: 'plain',
    population: 100000,
    foodOutput: 120,
    taxOutput: 80,
    manpower: 40,
    landStructure: {},
    legitimacyMemory: [],
    localPower: 20,
    rebellionRisk: 4,
    neighbors
  };
}

function makeShape(regionId: string, x: number, y: number): Record<string, unknown> {
  return {
    id: `shape_${regionId}`,
    regionId,
    center: { x, y },
    boundary: [
      { x: x - 0.5, y: y - 0.5 },
      { x: x + 0.5, y: y - 0.5 },
      { x: x + 0.5, y: y + 0.5 },
      { x: x - 0.5, y: y + 0.5 }
    ]
  };
}
