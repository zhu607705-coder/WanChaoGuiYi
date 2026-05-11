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
    sourcemap: true
  }
});
