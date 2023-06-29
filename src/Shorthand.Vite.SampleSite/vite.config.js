/* global __dirname */
import { defineConfig } from 'vite';
import { resolve } from 'node:path';

export default defineConfig({
  build: {
    outDir: resolve(__dirname, './wwwroot/'),
    emptyOutDir: false,
    manifest: true,
    rollupOptions: {
      input: resolve(__dirname, './wwwroot/js/site.js')
    }
  }
});
