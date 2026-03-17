import { Routes, Route, Navigate } from "react-router-dom"
import AuthGuard from "./components/AuthGuard"

import Login from "./pages/Login"
import RegisterUser from "./pages/RegisterUser"
import RegisterTenant from "./pages/RegisterTenant"
import WaitingMembership from "./pages/WaitingMembership"
import Home from "./pages/Home"
import Donation from "./pages/Donation"
import MyTransactions from "./pages/MyTransactions"
import TransactionDetail from "./pages/TransactionDetail"
import NewProcurement from "./pages/NewProcurement"

export default function AppRoutes() {
    return (
        <Routes>
            {/* redirect */}
            <Route path="/" element={<Navigate to="/login" />} />

            {/* public */}
            <Route
                path="/login"
                element={
                    <AuthGuard public>
                        <Login />
                    </AuthGuard>
                }
            />

            <Route
                path="/register-user"
                element={
                    <AuthGuard public>
                        <RegisterUser />
                    </AuthGuard>
                }
            />

            <Route
                path="/register-tenant"
                element={
                    <AuthGuard public>
                        <RegisterTenant />
                    </AuthGuard>
                }
            />

            {/* auth only */}
            <Route
                path="/waiting-membership"
                element={
                    <AuthGuard requireAuth>
                        <WaitingMembership />
                    </AuthGuard>
                }
            />

            {/* auth + tenant */}
            <Route
                path="/home"
                element={
                    <AuthGuard requireAuth requireTenant>
                        <Home />
                    </AuthGuard>
                }
            />

            <Route
                path="/donate"
                element={
                    <AuthGuard requireAuth requireTenant>
                        <Donation />
                    </AuthGuard>
                }
            />
            <Route
                path="/my-transactions"
                element={
                    <AuthGuard requireAuth requireTenant>
                        <MyTransactions />
                    </AuthGuard>
                }
            />
            <Route
                path="/transactions/:transactionPublicId"
                element={
                    <AuthGuard requireAuth requireTenant>
                        <TransactionDetail />
                    </AuthGuard>
                }
            />
            <Route
                path="/procurements/new"
                element={
                    <AuthGuard requireAuth requireTenant>
                        <NewProcurement />
                    </AuthGuard>
                }
            />
        </Routes>
    )
}