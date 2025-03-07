import { defineConfig } from '@rsbuild/core';
import { pluginReact } from '@rsbuild/plugin-react';
import { pluginModuleFederation } from '@module-federation/rsbuild-plugin';
import config from './config.json';

// Documentation: https://module-federation.io/guide/start/quick-start.html

// See the config.json file for the MFEs to be loaded
const remotes = config.mfes.reduce((acc, mfe) => {
  acc[mfe.name] = `${mfe.name}@${mfe.location}`;
  return acc;
}, {});

export default defineConfig({
  plugins: [
    pluginReact(),
    pluginModuleFederation({
      name: 'host',
      remotes: {
        ...remotes, // Add the MFEs to be loaded ("<remote MFE name>": "<remote MFE name>@<remote MFE location>")
      },
      shared: ['react', 'react-dom'],
    }),
  ],
  server: {
    port: 3000,
  },
  html: {
    title: 'Host Application',
  },
});