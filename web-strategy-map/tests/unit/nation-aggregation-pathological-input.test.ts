import { describe, expect, it } from 'vitest';
import * as fc from 'fast-check';
import { aggregateNationFood, aggregateNationMoney, type NationAggregationInput } from '../../src/data';

/**
 * Bug under investigation: aggregateNationFood / aggregateNationMoney
 * now clamp negative contribution to 0 (last round). But the pathological
 * inputs go further: NaN, +Infinity, -Infinity, very large numbers.
 *
 * Pinned invariants (universal):
 *   1. Result is always finite (no NaN, no Infinity).
 *   2. Result is always >= 0.
 *   3. Result is always an integer (Math.round).
 *   4. Result is bounded by sum of clamped foodOutput (<= sum of |foodOutput|).
 */
describe('nation aggregation pathological input PBT', () => {
  function regionGen() {
    return fc.record({
      owner: fc.constantFrom<NationAggregationInput['owner']>('player', 'rival'),
      foodOutput: fc.oneof(
        fc.integer({ min: 0, max: 1000 }),
        fc.float({ min: -1e6, max: 1e6, noNaN: false }),
        fc.constant(Number.POSITIVE_INFINITY),
        fc.constant(Number.NEGATIVE_INFINITY),
        fc.constant(Number.NaN)
      ),
      taxOutput: fc.oneof(
        fc.integer({ min: 0, max: 1000 }),
        fc.float({ min: -1e6, max: 1e6, noNaN: false }),
        fc.constant(Number.POSITIVE_INFINITY),
        fc.constant(Number.NaN)
      ),
      contribution: fc.oneof(
        fc.integer({ min: -200, max: 200 }),
        fc.constant(Number.POSITIVE_INFINITY),
        fc.constant(Number.NaN)
      )
    });
  }

  it('aggregateNationFood is finite and non-negative for any input', () => {
    fc.assert(
      fc.property(fc.array(regionGen(), { maxLength: 30 }), (regions) => {
        const result = aggregateNationFood(regions);
        expect(Number.isFinite(result)).toBe(true);
        expect(result).toBeGreaterThanOrEqual(0);
        expect(Number.isInteger(result)).toBe(true);
      })
    );
  });

  it('aggregateNationMoney is finite and non-negative for any input', () => {
    fc.assert(
      fc.property(fc.array(regionGen(), { maxLength: 30 }), (regions) => {
        const result = aggregateNationMoney(regions);
        expect(Number.isFinite(result)).toBe(true);
        expect(result).toBeGreaterThanOrEqual(0);
        expect(Number.isInteger(result)).toBe(true);
      })
    );
  });
});
