import { BrowserRouter, Routes, Route } from "react-router-dom"
import Login from "../pages/Login"

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        {/* public */}
        <Route path="/login" element={<Login />} />

      </Routes>
    </BrowserRouter>
  )
}