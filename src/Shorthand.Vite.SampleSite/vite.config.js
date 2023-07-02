/* global __dirname */
import { defineConfig } from 'vite';
import { resolve } from 'node:path';

export default defineConfig({
  build: {
    outDir: resolve(__dirname, './wwwroot/'),
    manifest: true,
    cssCodeSplit: false,
    rollupOptions: {
      input: resolve(__dirname, './wwwroot/js/site.js')
    }
  },
  server: {
    strictPort: true
  }
});
