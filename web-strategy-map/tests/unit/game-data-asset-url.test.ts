import { describe, expect, it } from 'vitest';
import { gameDataAssetUrl } from '../../src/data';

/**
 * Bug under investigation: earlier gameDataAssetUrl behavior encoded
 * path traversal and Windows backslash input instead of normalising
 * them into a safe /game-data URL. The implementation now filters
 * unsafe path segments; these tests keep that URL boundary pinned.
 *
 * Pinned invariants:
 *   1. `..` in the path must be rejected, not encoded.
 *   2. Empty input or pure-slash input must produce a deterministic
 *      empty-segment output.
 */
describe('gameDataAssetUrl', () => {
  it('rejects path traversal attempts with .. segments', () => {
    const result = gameDataAssetUrl('../../etc/passwd');
    // Historical failure: encoded `..` segments could survive in the
    // URL. Current behavior strips them from the generated path.
    expect(result).not.toMatch(/%2E%2E|\.\./i);
  });

  it('rejects backslashes in path segments', () => {
    const result = gameDataAssetUrl('art\\Portraits\\evil.png');
    expect(result).not.toMatch(/%5C|\\/);
  });

  it('handles empty input gracefully', () => {
    const result = gameDataAssetUrl('');
    // Empty input is a stable boundary case used by callers that
    // already validated optional asset fields.
    expect(result).toBe('/game-data/');
  });
});
