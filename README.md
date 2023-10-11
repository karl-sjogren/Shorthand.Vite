# Shorthand.Vite [![Build develop](https://github.com/karl-sjogren/Shorthand.Vite/actions/workflows/build.yml/badge.svg)](https://github.com/karl-sjogren/Shorthand.Vite/actions/workflows/build.yml) [![codecov](https://codecov.io/gh/karl-sjogren/Shorthand.Vite/branch/develop/graph/badge.svg?token=3ZIGV5QHEB)](https://codecov.io/gh/karl-sjogren/Shorthand.Vite)

An easy way to reference Vite assets in ASP.NET Core projects.

## Installation

```sh
> dotnet add package Shorthand.Vite
```

## Usage

### Basic setup

In your `Startup.cs` file, add the following to the `ConfigureServices` method:

```csharp
public void ConfigureServices(IServiceCollection services) {
    services.AddVite();
}
```

Or if you use the modern setup with the `Program.cs` file:

```csharp
builder.Services.AddVite();
```

Then in your `vite.config.js` file you need at least the following:

```js
/* global __dirname */
import { defineConfig } from 'vite';
import { resolve } from 'node:path';

export default defineConfig({
  build: {
    outDir: resolve(__dirname, './wwwroot/'),
    manifest: true, // Generate a manifest.json file for production builds
    rollupOptions: {
      input: resolve(__dirname, './wwwroot/js/site.js') // Point this to your main entry point
    }
  },
  server: {
    strictPort: true // We don't want random ports
  }
});
```

Then open up your layout `cshtml`file and add the following at the top:

```html
@inject Shorthand.Vite.Contracts.IViteService Vite
```

If you want to use this in several files you can put that in the
`_ViewImports.cshtml` file instead.

Then in your `head` tag, add the following:

```html
    <environment names="Production">
        @* Preload the module script, almost always a good idea *@
        <link rel="modulepreload" href="@await Vite.GetAssetUrlAsync("wwwroot/js/site.js")" as="script" />
        @* Load the stylesheet manually in production mode *@
        <link rel="stylesheet" href="@await Vite.GetAssetUrlAsync("style.css")" />
    </environment>
```

And at the bottom of your `body` tag, add the following:

```html
    @* Load the main module script, during development this will also load the stylesheet *@
    <script type="module" src="@await Vite.GetAssetUrlAsync("wwwroot/js/site.js")"></script>
```

For a more complete example, see the sample project in the repository.

### Configuration

There are a few options that can be configured if the default values are
not suitable.

```csharp
public void ConfigureServices(IServiceCollection services) {
    services.AddVite(options => {
        options.Hostname = "localhost";
        options.Port = 5173;
        options.Https = false;
    });
}
```

### Experimental features

In the 0.3.0 release a proxy feature was added that allows you to make all the
requests to the main site and have a middleware that proxies it to the Vite Dev
Server. This is useful if you want to use external tools such as BrowserStack or
the iOS Simulator on OSX and don't want to setup the Vite Dev Server to be accessible
from external clients.

This is done using `YARP.ReverseProxy` which also supports proxying websockets
so HMR (Hot Module Replacement) will work as well.

This will be an opt-in feature and will not be enabled by default for now.

To enable the proxy mode a new middlware has to be registered during development.

```csharp
if(app.Environment.IsDevelopment()) {
    app.UseViteDevServerProxy();
}
```

This should go before `app.UseStaticFiles()` in your pipeline.

To setup communication between the proxy and the Vite Dev Server, the following
Vite plugin also needs to be installed.

```sh
> npm install --save-dev vite-plugin-shorthand-aspnetcore
```

Then in your `vite.config.js` file you need to add the following:

```js
import ViteAspNetCore from 'vite-plugin-shorthand-aspnetcore';

export default defineConfig({
  plugins: [
    ViteAspNetCore()
  ],
  ...
});
```

## Sample site

There is a sample proejct in the repository that shows a sample setup
with a simple Vite project and an ASP.NET Core project. It is located
at `src/Shorthand.Vite.SampleSite`.
