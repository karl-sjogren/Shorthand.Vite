/* global __dirname */
import { defineConfig } from 'vite';
import { resolve } from 'node:path';
import ViteAspNetCore from 'vite-plugin-shorthand-aspnetcore';
import Inspect from 'vite-plugin-inspect'

export default defineConfig({
  plugins: [
    Inspect(),
    ViteAspNetCore()
  ],
  build: {
    outDir: resolve(__dirname, './wwwroot/'),
    manifest: true,
    cssCodeSplit: false,
    rollupOptions: {
      input: resolve(__dirname, './Scripts/site.js')
    }
  },
  server: {
    strictPort: true
  }
});
