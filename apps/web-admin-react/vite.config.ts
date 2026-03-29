import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from "@tailwindcss/vite"

// https://vite.dev/config/
export default defineConfig({
  base: "/admin/",
  plugins: [react(), tailwindcss() ],
  server: {
    host: "0.0.0.0",
    port:5176,
        allowedHosts: [
      "fintrack.chenlis.local",
      "coffee.chenlis.local",
      "sushi.chenlis.local",
    ],
  }
})
