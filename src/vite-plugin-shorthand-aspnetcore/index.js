
const plugin = () => {
  return {
    name: 'vite-plugin-shorthand-aspnetcore',

    config: () => ({
      build: {
        cssCodeSplit: false,
        manifest: true
      },
      server: {
        strictPort: true,
        hmr: {
          path: '/.vite/hmr'
        }
      }
    }),

    configureServer({ middlewares, moduleGraph, watcher }) {
      middlewares.use((req, res, next) => {
        if(req.url === '/.shorthand-vite/modules') {
          const modulePaths = [...moduleGraph.urlToModuleMap.keys()];

          res.setHeader('Content-Type', 'application/json');
          res.end(JSON.stringify(modulePaths));
          return;
        }
        next();
      })
    }
  };
};

module.exports = plugin;
