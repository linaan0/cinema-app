import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Dev server proxies /api and /hubs straight to the API Gateway / Ingress,
// so the frontend code never needs to know individual service ports.
// In docker-compose, set VITE_API_BASE to wherever the gateway/ingress lives;
// for plain `npm run dev` against docker-compose services, point it at
// http://localhost:8081 etc. per-call (see src/api.js).
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
  },
})
