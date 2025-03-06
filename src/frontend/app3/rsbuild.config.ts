import { defineConfig } from '@rsbuild/core';
import { pluginReact } from '@rsbuild/plugin-react';
import { pluginModuleFederation } from '@module-federation/rsbuild-plugin';

import pkg from './package.json';

// Documentation: https://module-federation.io/guide/start/quick-start.html

export default defineConfig({
  plugins: [
    pluginReact(),
    pluginModuleFederation({
      name: 'app3',
      exposes: {
        './Form': './src/Form.tsx',
      },
      shared: {
        ...pkg['dependencies'], // This will share all dependencies from package.json
        react: {
          singleton: true,
          requiredVersion: pkg['dependencies']['react'],
        },
        'react-dom': {
          singleton: true,
          requiredVersion: pkg['dependencies']['react-dom'],
        },
        "@emotion/react": { 
          singleton: true, 
          requiredVersion: pkg['dependencies']['@emotion/react'],
        },
      }
    }),
  ],
  server: {
    port: 3003,
  },
  html: {
    title: 'Form3 Application',
  },
});