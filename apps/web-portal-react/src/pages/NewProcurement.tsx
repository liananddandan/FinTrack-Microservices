import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { createProcurement } from "../api/transactions"
import "./NewProcurement.css"

export default function NewProcurement() {
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")

  const [form, setForm] = useState({
    title: "",
    description: "",
    amount: "0", // 👈 用 string，避免 number input bug
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

  async function submit() {
    setLoading(true)
    setErrorMessage("")

    try {
      const amountNumber = Number(form.amount)

      if (Number.isNaN(amountNumber) || amountNumber <= 0) {
        setErrorMessage("Amount must be greater than 0.")
        return
      }

      const result = await createProcurement({
        title: form.title,
        description: form.description,
        amount: amountNumber,
        currency: form.currency,
      })

      navigate(`/transactions/${result.transactionPublicId}`)
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to create procurement."
      setErrorMessage(message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="page">
      <div className="card">
        <div className="title">New Procurement Draft</div>

        {errorMessage && (
          <div className="error-box" role="alert">
            {errorMessage}
          </div>
        )}

        <div className="form">
          <div className="form-item">
            <label>Title</label>
            <input
              type="text"
              value={form.title}
              onChange={(e) => updateField("title", e.target.value)}
            />
          </div>

          <div className="form-item">
            <label>Description</label>
            <textarea
              value={form.description}
              onChange={(e) => updateField("description", e.target.value)}
            />
          </div>

          <div className="form-item">
            <label>Amount</label>
            <input
              type="number"
              value={form.amount}
              onChange={(e) => updateField("amount", e.target.value)}
            />
          </div>

          <div className="form-item">
            <label>Currency</label>
            <input
              type="text"
              value={form.currency}
              onChange={(e) => updateField("currency", e.target.value)}
            />
          </div>

          <button
            className="primary-btn"
            disabled={loading}
            onClick={submit}
          >
            {loading ? "Saving..." : "Save Draft"}
          </button>
        </div>
      </div>
    </div>
  )
}