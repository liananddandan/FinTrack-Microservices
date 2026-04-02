import { useEffect } from "react"
import { BrowserRouter, Navigate, Route, Routes, useNavigate } from "react-router-dom"
import Login from "../pages/Login"
import Overview from "../pages/Overview"
import RequireAuth from "./RequireAuth"
import { registerNavigate } from "../lib/http"

function NavigationRegistrar() {
  const navigate = useNavigate()

  useEffect(() => {
    registerNavigate(navigate)
  }, [navigate])

  return null
}

export default function AppRouter() {
  return (
    <BrowserRouter>
      <NavigationRegistrar />

      <Routes>
        <Route path="/login" element={<Login />} />

        <Route element={<RequireAuth />}>
          <Route path="/overview" element={<Overview />} />
        </Route>

        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  )
}