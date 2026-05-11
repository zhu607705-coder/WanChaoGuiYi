import type { LabelAnchor, StrategyScene } from './scene';
import type { GameMode } from './types';

interface LabelNode {
  anchor: LabelAnchor;
  element: HTMLDivElement;
}

export class LabelManager {
  private readonly labels = new Map<string, LabelNode>();
  private mode: GameMode = 'governance';
  private frame = 0;

  constructor(private readonly layer: HTMLElement, private readonly scene: StrategyScene) {}

  setMode(mode: GameMode): void {
    this.mode = mode;
    this.update(true);
  }

  start(): void {
    const tick = () => {
      this.frame = requestAnimationFrame(tick);
      this.update(false);
    };
    tick();
  }

  stop(): void {
    cancelAnimationFrame(this.frame);
  }

  update(force: boolean): void {
    const anchors = this.scene.getLabelAnchors();
    const liveKeys = new Set<string>();
    for (const anchor of anchors) {
      const key = `${anchor.kind}:${anchor.id}`;
      liveKeys.add(key);
      let node = this.labels.get(key);
      if (!node) {
        const element = document.createElement('div');
        element.className = [
          'map-label',
          anchor.kind === 'army' ? 'army-label' : '',
          anchor.kind === 'building' ? 'building-label' : '',
          anchor.kind === 'landform' ? 'landform-label' : ''
        ].filter(Boolean).join(' ');
        element.dataset.labelId = anchor.id;
        element.dataset.labelKind = anchor.kind;
        this.layer.appendChild(element);
        node = { anchor, element };
        this.labels.set(key, node);
      }

      node.anchor = anchor;
      if (force || node.element.textContent !== anchor.text) {
        node.element.textContent = anchor.text;
      }
    }

    for (const [key, node] of this.labels) {
      if (!liveKeys.has(key)) {
        node.element.remove();
        this.labels.delete(key);
      }
    }

    this.layoutLabels();
  }

  private layoutLabels(): void {
    const width = window.innerWidth;
    const height = window.innerHeight;
    const distance = this.scene.getCameraDistance();
    const labelBudget = this.resolveBudget(width, height, distance);
    const placed: DOMRect[] = [];
    const nodes = [...this.labels.values()].sort((a, b) => b.anchor.priority - a.anchor.priority);

    for (const node of nodes) {
      const screen = this.scene.projectToScreen(node.anchor.position);
      const element = node.element;
      element.classList.toggle('war-only', node.anchor.kind === 'army');
      element.classList.toggle('governance-only', node.anchor.kind === 'building');
      element.classList.toggle('landform-callout', node.anchor.kind === 'landform');
      const modeHidden = node.anchor.kind === 'army' && this.mode !== 'war';
      const buildingHidden = node.anchor.kind === 'building' && this.mode !== 'governance';
      if (!screen.visible || modeHidden || buildingHidden) {
        element.classList.add('hidden');
        continue;
      }

      element.style.left = `${Math.round(screen.x)}px`;
      element.style.top = `${Math.round(screen.y)}px`;
      element.style.transform = 'translate(-50%, -50%)';
      element.classList.remove('hidden');
      const rect = element.getBoundingClientRect();
      const outside = rect.right < 0 || rect.left > width || rect.bottom < 0 || rect.top > height;
      const forceVisible = node.anchor.kind === 'landform' || (node.anchor.kind === 'building' && node.anchor.priority >= 200);
      const collides = !forceVisible && placed.some((other) => intersects(rect, other, 5));
      const overBudget = placed.length >= labelBudget && node.anchor.kind !== 'army';

      if (outside || collides || (overBudget && !forceVisible)) {
        element.classList.add('hidden');
        continue;
      }

      placed.push(rect);
    }

    document.documentElement.dataset.visibleLabels = String(placed.length);
  }

  private resolveBudget(width: number, height: number, distance: number): number {
    if (width <= 1040 || height <= 600) return this.mode === 'war' ? 10 : 8;
    if (distance > 20) return this.mode === 'war' ? 14 : 12;
    if (distance > 13) return this.mode === 'war' ? 24 : 20;
    return this.mode === 'war' ? 38 : 32;
  }
}

function intersects(a: DOMRect, b: DOMRect, padding: number): boolean {
  return !(
    a.right + padding < b.left ||
    a.left - padding > b.right ||
    a.bottom + padding < b.top ||
    a.top - padding > b.bottom
  );
}
