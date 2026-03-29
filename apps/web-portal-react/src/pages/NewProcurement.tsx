import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { createProcurement } from "../api/transactions"
import { HiOutlineDocumentText, HiOutlineArrowLeft } from "react-icons/hi2"

export default function NewProcurement() {
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")

  const [form, setForm] = useState({
    title: "",
    description: "",
    amount: "0",
    currency: "NZD",
  })

  function updateField<K extends keyof typeof form>(
    key: K,
    value: (typeof form)[K]
  ) {
    setForm((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  async function submit(event?: React.FormEvent<HTMLFormElement>) {
    event?.preventDefault()

    setLoading(true)
    setErrorMessage("")

    try {
      const amountNumber = Number(form.amount)

      if (!form.title.trim()) {
        setErrorMessage("Title is required.")
        return
      }

      if (Number.isNaN(amountNumber) || amountNumber <= 0) {
        setErrorMessage("Amount must be greater than 0.")
        return
      }

      const result = await createProcurement({
        title: form.title.trim(),
        description: form.description.trim(),
        amount: amountNumber,
        currency: form.currency,
      })

      navigate(`/portal/transactions/${result.transactionPublicId}`, {
        replace: true,
      })
    } catch (err) {
      const message =
        typeof err === "object" &&
          err !== null &&
          "response" in err &&
          typeof (err as any).response?.data?.message === "string"
          ? (err as any).response.data.message
          : err instanceof Error
            ? err.message
            : "Failed to create procurement."

      setErrorMessage(message)
    } finally {
      setLoading(false)
    }
  }

  function goBack() {
    navigate("/portal/home")
  }

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto max-w-3xl">
        <section className="rounded-3xl border border-slate-200 bg-white p-8 shadow-sm sm:p-10">
          <div className="flex items-start justify-between gap-6">
            <div>
              <div className="inline-flex items-center gap-3 rounded-full border border-slate-200 bg-slate-50 px-4 py-2">
                <div className="flex h-9 w-9 items-center justify-center rounded-full bg-indigo-50 text-indigo-600">
                  <HiOutlineDocumentText className="h-5 w-5" />
                </div>
                <div>
                  <p className="text-sm font-medium text-slate-800">
                    Retail Operations Platform
                  </p>
                  <p className="text-xs text-slate-500">Procurement request</p>
                </div>
              </div>

              <h1 className="mt-8 text-3xl font-semibold tracking-tight text-slate-800">
                New procurement request
              </h1>

              <p className="mt-4 max-w-2xl text-base leading-7 text-slate-600">
                Create a procurement draft in the current workspace. You can save
                the draft now and submit it for approval later.
              </p>
            </div>

            <button
              type="button"
              onClick={goBack}
              disabled={loading}
              className="inline-flex items-center gap-2 rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600 disabled:opacity-60"
            >
              <HiOutlineArrowLeft className="h-4 w-4" />
              Back
            </button>
          </div>

          <form className="mt-10 space-y-6" onSubmit={submit}>
            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Title
              </label>
              <input
                type="text"
                value={form.title}
                onChange={(e) => updateField("title", e.target.value)}
                placeholder="e.g. Purchase office supplies"
                className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !text-sm !text-slate-800 outline-none placeholder:!text-slate-400 focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Description
              </label>
              <textarea
                value={form.description}
                onChange={(e) => updateField("description", e.target.value)}
                placeholder="Describe the purpose of this request"
                rows={4}
                className="!block !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !py-3 !text-sm !text-slate-800 outline-none placeholder:!text-slate-400 focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
              />
            </div>

            <div className="grid gap-6 sm:grid-cols-[1fr_180px]">
              <div>
                <label className="mb-2 block text-sm font-medium text-slate-700">
                  Amount
                </label>
                <input
                  type="number"
                  min={1}
                  value={form.amount}
                  onChange={(e) => updateField("amount", e.target.value)}
                  className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !text-sm !text-slate-800 outline-none placeholder:!text-slate-400 focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
                />
              </div>

              <div>
                <label className="mb-2 block text-sm font-medium text-slate-700">
                  Currency
                </label>
                <input
                  type="text"
                  value={form.currency}
                  onChange={(e) => updateField("currency", e.target.value)}
                  className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !text-sm !text-slate-800 outline-none focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
                />
              </div>
            </div>

            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4 text-sm leading-6 text-slate-600">
              This action creates a procurement draft. The request can be submitted
              for approval from the detail page after it is created.
            </div>

            {errorMessage ? (
              <div
                className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
                role="alert"
              >
                {errorMessage}
              </div>
            ) : null}

            <div className="flex flex-wrap justify-end gap-3">
              <button
                type="button"
                onClick={goBack}
                disabled={loading}
                className="inline-flex items-center rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600 disabled:opacity-60"
              >
                Cancel
              </button>

              <button
                type="submit"
                disabled={loading}
                className="inline-flex items-center rounded-full bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {loading ? "Saving..." : "Save draft"}
              </button>
            </div>
          </form>
        </section>
      </div>
    </div>
  )
}