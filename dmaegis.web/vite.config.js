import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // Em desenvolvimento, /api vai para a API .NET: mesma origem, sem CORS, e o cookie funciona.
    proxy: {
      '/api': 'http://localhost:5080',
    },
  },
});
