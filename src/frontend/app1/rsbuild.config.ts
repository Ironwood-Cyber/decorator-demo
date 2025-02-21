import { defineConfig } from '@rsbuild/core';
import { pluginReact } from '@rsbuild/plugin-react';
import { pluginModuleFederation } from '@module-federation/rsbuild-plugin';

import pkg from './package.json';

export default defineConfig({
  plugins: [
    pluginReact(),
    pluginModuleFederation({
      name: 'app1',
      exposes: {
        './Form': './src/Form.tsx',
      },
      shared: {
        ...pkg['dependencies'],
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
    port: 3001,
  },
  html: {
    title: 'Form1 Application',
  },
});