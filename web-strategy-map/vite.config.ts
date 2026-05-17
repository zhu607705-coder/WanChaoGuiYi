import { defineConfig } from 'vite';

export default defineConfig({
  server: {
    host: '127.0.0.1',
    port: 5177,
    strictPort: false
  },
  preview: {
    host: '127.0.0.1',
    port: 4177
  },
  build: {
    target: 'es2022',
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules/three/src/renderers/')) return 'three-renderer';
          if (id.includes('node_modules/three/examples/')) return 'three-controls';
          if (id.includes('node_modules/three/')) return 'three-core';
          if (id.includes('node_modules/')) return 'vendor';
        }
      }
    }
  }
});
