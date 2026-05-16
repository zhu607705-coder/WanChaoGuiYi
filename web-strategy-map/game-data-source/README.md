# Web Game Data Source

This directory is the canonical engine-free asset source for `web-strategy-map`.
`npm run sync:data` copies it into the ignored `public/game-data` runtime folder
before dev, build, and Playwright runs.

Do not add new Web runtime data by reading from editor project folders. Runtime
JSON, map images, audio, and art assets should live here first. `audio/archive`
preserves migrated source MP3s that are not currently referenced by Web audio
manifests.
