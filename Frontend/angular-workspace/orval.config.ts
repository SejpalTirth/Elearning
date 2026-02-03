import { defineConfig } from 'orval';

export default defineConfig({
  gateway: {
    input: {
      target: 'https://localhost:7249/swagger/v1/swagger.json'
    },
    output: {
      target: './libs/api/src/lib/clients',
      schemas: './libs/api/src/lib/models',
      client: 'angular',
      mode: 'tags',
      clean: true
    }
  }
});