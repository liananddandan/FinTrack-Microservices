import { useEffect, useState } from "react"
import { getPlatformOverview } from "../api/account"
import { platformAuthStore } from "../lib/authStore"

export default function Overview() {
  const [message, setMessage] = useState("Loading...")
  const [error, setError] = useState("")

  useEffect(() => {
    async function load() {
      try {
        const result = await getPlatformOverview()
        setMessage(result.message)
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load overview.")
      }
    }

    void load()
  }, [])

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto max-w-6xl">
        <div className="rounded-3xl border border-slate-200 bg-white p-8 shadow-sm">
          <h1 className="text-2xl font-semibold text-slate-800">
            Platform Overview
          </h1>

          <p className="mt-2 text-sm text-slate-500">
            Welcome back, {platformAuthStore.userName || platformAuthStore.userEmail || "Admin"}.
          </p>

          <div className="mt-6 rounded-2xl border border-slate-200 bg-slate-50 p-4">
            <div className="text-sm text-slate-600">
              Platform role:{" "}
              <span className="font-semibold text-slate-800">
                {platformAuthStore.platformRole || "Unknown"}
              </span>
            </div>

            {!error ? (
              <div className="mt-3 text-sm text-slate-700">{message}</div>
            ) : (
              <div className="mt-3 text-sm text-rose-700">{error}</div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}