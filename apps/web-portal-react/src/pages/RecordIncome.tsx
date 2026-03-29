import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { createDonation } from "../api/transactions"
import { HiOutlineBanknotes, HiOutlineArrowLeft } from "react-icons/hi2"

type DonationForm = {
  amount: string
  currency: string
  description: string
  source: string
  reference: string
}

export default function RecordIncome() {
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")
  const [successMessage, setSuccessMessage] = useState("")
  const [form, setForm] = useState<DonationForm>({
    amount: "10",
    currency: "NZD",
    description: "",
    source: "Donation",
    reference: "",
  })

  function updateField<K extends keyof DonationForm>(
    key: K,
    value: DonationForm[K]
  ) {
    setForm((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  async function submitDonation(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()

    setErrorMessage("")
    setSuccessMessage("")

    const amount = Number(form.amount)

    if (!amount || amount < 1) {
      setErrorMessage("Amount is required and must be greater than 0.")
      return
    }

    setLoading(true)

    try {
      const extraDescription = [
        form.source ? `Source: ${form.source}` : "",
        form.reference ? `Reference: ${form.reference}` : "",
        form.description ? `Note: ${form.description}` : "",
      ]
        .filter(Boolean)
        .join(" | ")

      await createDonation({
        title: "Donation",
        description: extraDescription,
        amount,
        currency: form.currency,
      })

      setSuccessMessage("Income recorded successfully.")

      setTimeout(() => {
        navigate("/portal/home", { replace: true })
      }, 600)
    } catch {
      setErrorMessage("Income record request failed.")
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
                  <HiOutlineBanknotes className="h-5 w-5" />
                </div>
                <div>
                  <p className="text-sm font-medium text-slate-800">
                    Retail Operations Platform
                  </p>
                  <p className="text-xs text-slate-500">Income record</p>
                </div>
              </div>

              <h1 className="mt-8 text-3xl font-semibold tracking-tight text-slate-800">
                Record income
              </h1>

              <p className="mt-4 max-w-2xl text-base leading-7 text-slate-600">
                Add an income entry to the current tenant ledger. This records
                incoming funds for the workspace rather than processing a live
                payment.
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

          <form className="mt-10 space-y-6" onSubmit={submitDonation}>
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
                  disabled
                  className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-slate-100 !px-3 !text-sm !text-slate-600 outline-none"
                />
              </div>
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Income source
              </label>
              <select
                value={form.source}
                onChange={(e) => updateField("source", e.target.value)}
                className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !text-sm !text-slate-800 outline-none focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
              >
                <option value="Donation">Donation</option>
                <option value="Sponsorship">Sponsorship</option>
                <option value="Grant">Grant</option>
                <option value="Other">Other</option>
              </select>
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Reference
              </label>
              <input
                type="text"
                value={form.reference}
                onChange={(e) => updateField("reference", e.target.value)}
                placeholder="Optional reference or memo"
                className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !text-sm !text-slate-800 outline-none placeholder:!text-slate-400 focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Note
              </label>
              <textarea
                value={form.description}
                onChange={(e) => updateField("description", e.target.value)}
                placeholder="Optional note"
                rows={4}
                className="!block !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !py-3 !text-sm !text-slate-800 outline-none placeholder:!text-slate-400 focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
              />
            </div>

            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4 text-sm leading-6 text-slate-600">
              This action records an income entry in the current tenant ledger.
              It does not process a real payment transaction.
            </div>

            {errorMessage ? (
              <div
                className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
                role="alert"
              >
                {errorMessage}
              </div>
            ) : null}

            {successMessage ? (
              <div
                className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"
                role="status"
              >
                {successMessage}
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
                {loading ? "Submitting..." : "Record income"}
              </button>
            </div>
          </form>
        </section>
      </div>
    </div>
  )
}