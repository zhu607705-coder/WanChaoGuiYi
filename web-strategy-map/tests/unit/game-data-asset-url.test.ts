import { describe, expect, it } from 'vitest';
import { gameDataAssetUrl } from '../../src/data';

/**
 * Bug under investigation: gameDataAssetUrl normalises a relative
 * path then encodes each segment with encodeURIComponent. But:
 *   1. It only strips LEADING slashes (`/^\/+/`). A `..` segment is
 *      preserved as `%2E%2E`, which servers may decode to `..` and
 *      traverse outside `/game-data`.
 *   2. Empty segments (`a//b`) become `%2F`-bracketed, which
 *      duplicates the path delimiter in URL encoded form.
 *   3. Backslashes (`a\b`) — common from Windows-built JSON — are
 *      preserved as `%5C` and may be decoded to `\` on some servers.
 *
 * Pinned invariants:
 *   1. `..` in the path must be rejected, not encoded.
 *   2. Empty input or pure-slash input must produce a deterministic
 *      empty-segment output.
 */
describe('gameDataAssetUrl', () => {
  it('rejects path traversal attempts with .. segments', () => {
    const result = gameDataAssetUrl('../../etc/passwd');
    // Today: returns '/game-data/%2E%2E/%2E%2E/etc/passwd' which
    // a permissive proxy could decode and serve.
    // Desired: throw, or strip the .. segments.
    expect(result).not.toMatch(/%2E%2E|\.\./i);
  });

  it('rejects backslashes in path segments', () => {
    const result = gameDataAssetUrl('art\\Portraits\\evil.png');
    expect(result).not.toMatch(/%5C|\\/);
  });

  it('handles empty input gracefully', () => {
    const result = gameDataAssetUrl('');
    // Today: returns '/game-data/' (trailing slash). Acceptable
    // but documented?
    expect(result).toBe('/game-data/');
  });
});
