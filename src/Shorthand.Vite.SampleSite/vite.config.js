/* global __dirname */
import { defineConfig } from 'vite';
import { resolve } from 'node:path';

export default defineConfig({
  build: {
    outDir: resolve(__dirname, './tmp/'),
    emptyOutDir: false,
    manifest: true,
    cssCodeSplit: false,
    sourcemap: true,
    modulePreload: {
      polyfill: false
    },
    rollupOptions: {
      input: resolve(__dirname, './wwwroot/js/site.js')
    }
  },
  server: {
    strictPort: true
  }
});
