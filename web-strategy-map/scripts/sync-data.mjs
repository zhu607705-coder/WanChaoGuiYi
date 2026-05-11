import { copyFileSync, mkdirSync, existsSync, readdirSync, statSync } from 'node:fs';
import { dirname, resolve } from 'node:path';

const projectRoot = resolve(import.meta.dirname, '..', '..');
const appRoot = resolve(import.meta.dirname, '..');

const copies = [
  ['My project/Assets/Data/regions.json', 'public/game-data/data/regions.json'],
  ['My project/Assets/Data/map_region_shapes.json', 'public/game-data/data/map_region_shapes.json'],
  ['My project/Assets/Data/map_render_metadata.json', 'public/game-data/data/map_render_metadata.json'],
  ['My project/Assets/Data/historical_layers.json', 'public/game-data/data/historical_layers.json'],
  ['My project/Assets/Data/emperors.json', 'public/game-data/data/emperors.json'],
  ['My project/Assets/Data/policies.json', 'public/game-data/data/policies.json'],
  ['My project/Assets/Data/buildings.json', 'public/game-data/data/buildings.json'],
  ['My project/Assets/Data/units.json', 'public/game-data/data/units.json'],
  ['My project/Assets/Data/generals.json', 'public/game-data/data/generals.json'],
  ['My project/Assets/Data/route_networks.json', 'public/game-data/data/route_networks.json'],
  ['My project/Assets/Resources/Map/jiuzhou_generated_map.png', 'public/game-data/map/jiuzhou_generated_map.png'],
  ['My project/Assets/Data/Audio/scene_music.json', 'public/game-data/audio/scene_music.json'],
  ['My project/Assets/Data/Audio/emperor_themes.json', 'public/game-data/audio/emperor_themes.json'],
  ['My project/Assets/Data/Audio/chronicle_event_music.json', 'public/game-data/audio/chronicle_event_music.json'],
  ['My project/Assets/Data/Narration/narration_script.json', 'public/game-data/audio/narration_script.json']
];

for (const [source, target] of copies) {
  const sourcePath = resolve(projectRoot, source);
  const targetPath = resolve(appRoot, target);
  if (!existsSync(sourcePath)) {
    throw new Error(`Missing source asset: ${sourcePath}`);
  }

  mkdirSync(dirname(targetPath), { recursive: true });
  copyFileSync(sourcePath, targetPath);
}

const audioCopies = [
  ['My project/Assets/Audio/Music/Scene', 'public/game-data/audio/music/scene'],
  ['My project/Assets/Audio/Music/Emperor', 'public/game-data/audio/music/emperor'],
  ['My project/Assets/Audio/Music/ChronicleEvent', 'public/game-data/audio/music/chronicle-event'],
  ['My project/Assets/Audio/Narration', 'public/game-data/audio/narration'],
  ['My project/Assets/Audio/EmperorVoice', 'public/game-data/audio/emperor-voice']
];

let audioCount = 0;
for (const [source, target] of audioCopies) {
  audioCount += copyMp3Tree(resolve(projectRoot, source), resolve(appRoot, target));
}

console.log(`Synced ${copies.length} strategy-map data/assets and ${audioCount} audio files.`);

function copyMp3Tree(sourceDir, targetDir) {
  if (!existsSync(sourceDir)) {
    throw new Error(`Missing source audio directory: ${sourceDir}`);
  }

  let copied = 0;
  for (const entry of readdirSync(sourceDir)) {
    const sourcePath = resolve(sourceDir, entry);
    const targetPath = resolve(targetDir, entry);
    const stat = statSync(sourcePath);
    if (stat.isDirectory()) {
      copied += copyMp3Tree(sourcePath, targetPath);
      continue;
    }

    if (!entry.toLowerCase().endsWith('.mp3')) continue;
    mkdirSync(dirname(targetPath), { recursive: true });
    copyFileSync(sourcePath, targetPath);
    copied += 1;
  }

  return copied;
}
