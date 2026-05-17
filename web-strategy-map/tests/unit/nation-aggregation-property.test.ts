import { describe, expect, it } from 'vitest';
import * as fc from 'fast-check';
import { aggregateNationFood } from '../../src/data';

/**
 * Property-based test for the nation aggregation logic in data.ts:
 *
 *   nation.food = sum over player regions of (foodOutput * contribution / 100)
 *
 * Bug under investigation: The aggregation rounds via Math.round AT
 * THE END, but contribution % can be on each region. Floating point
 * accumulation order matters; running sum + final round can produce
 * results that differ from per-region round + sum by 1 (off-by-one).
 * That difference is small but visible in the UI (`299` vs `300`).
 *
 * Pinned invariants (universal across any region set):
 *   1. nation.food >= 0 if all foodOutput >= 0 and contribution >= 0.
 *   2. nation.food == 0 when no player regions.
 *   3. nation.food deterministic for same input.
 */

const aggregateFood = aggregateNationFood;

describe('nation food aggregation property tests', () => {
  it('returns non-negative when inputs are non-negative', () => {
    fc.assert(
      fc.property(
        fc.array(
          fc.record({
            owner: fc.constantFrom('player', 'rival'),
            foodOutput: fc.integer({ min: 0, max: 1000 }),
            contribution: fc.integer({ min: 0, max: 100 })
          }),
          { maxLength: 30 }
        ),
        (regions) => {
          const result = aggregateFood(regions);
          expect(result).toBeGreaterThanOrEqual(0);
        }
      )
    );
  });

  it('returns zero when no player regions', () => {
    fc.assert(
      fc.property(
        fc.array(
          fc.record({
            owner: fc.constant('rival'),
            foodOutput: fc.integer({ min: 0, max: 1000 }),
            contribution: fc.integer({ min: 0, max: 100 })
          }),
          { maxLength: 30 }
        ),
        (regions) => {
          const result = aggregateFood(regions);
          expect(result).toBe(0);
        }
      )
    );
  });

  it('is deterministic for the same input', () => {
    fc.assert(
      fc.property(
        fc.array(
          fc.record({
            owner: fc.constantFrom('player', 'rival'),
            foodOutput: fc.integer({ min: 0, max: 1000 }),
            contribution: fc.integer({ min: 0, max: 100 })
          }),
          { maxLength: 30 }
        ),
        (regions) => {
          const a = aggregateFood(regions);
          const b = aggregateFood([...regions]);
          expect(a).toBe(b);
        }
      )
    );
  });

  it('handles negative contribution gracefully (clamped to 0)', () => {
    fc.assert(
      fc.property(
        fc.array(
          fc.record({
            owner: fc.constantFrom('player', 'rival'),
            foodOutput: fc.integer({ min: 0, max: 1000 }),
            // Pathological: -50 should never happen but who clamps?
            contribution: fc.integer({ min: -50, max: 100 })
          }),
          { minLength: 1, maxLength: 10 }
        ),
        (regions) => {
          const result = aggregateFood(regions);
          // Strong invariant: a region with negative contribution
          // should not subtract from the nation total. Today the
          // formula uses `r.contribution / 100` directly, so negative
          // contribution DOES subtract. UI shows nonsense numbers.
          // Pin: result must be >= 0 even with negative inputs.
          expect(result).toBeGreaterThanOrEqual(0);
        }
      )
    );
  });
});
