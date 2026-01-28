import { defineConfig } from 'orval';
import { environment } from './src/Environment/environment.js'

export default defineConfig({
  gateway: {
    input: {
      target: environment.targetURL
    },
    output: {
      target: './src/api/clients',
      schemas: './src/api/models',
      client: 'axios',
      mode: 'tags',
      clean: true,
      override: {
        mutator: {
          path: 'src/api/http.ts',
          name: 'http',
          default: false
        }
      }
    }
  }
});
