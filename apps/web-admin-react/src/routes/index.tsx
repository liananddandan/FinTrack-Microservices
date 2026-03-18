import { BrowserRouter, Routes, Route } from "react-router-dom"
import Login from "../pages/Login"
import Dashboard from "../pages/Dashboard"

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        {/* public */}
        <Route path="/login" element={<Login />} />

        {/* protected (后面再加 AuthGuard) */}
        <Route path="/" element={<Dashboard />} />
      </Routes>
    </BrowserRouter>
  )
}