import { BrowserRouter } from "react-router-dom"
import AppRoutes from "./AppRoutes"
import { HelmetProvider } from "react-helmet-async"

export default function App() {
  return (
    <HelmetProvider>
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </HelmetProvider>
  )
}