import { defineConfig } from 'orval';

export default defineConfig({
  gateway: {
    input: {
      target: 'https://localhost:7249/swagger/v1/swagger.json'
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
