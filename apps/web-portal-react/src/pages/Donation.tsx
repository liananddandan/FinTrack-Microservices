import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { createDonation } from "../api/transactions"
import "./Donation.css"

type DonationForm = {
    amount: string
    currency: string
    description: string
}

export default function Donation() {
    const navigate = useNavigate()

    const [loading, setLoading] = useState(false)
    const [errorMessage, setErrorMessage] = useState("")
    const [successMessage, setSuccessMessage] = useState("")
    const [form, setForm] = useState<DonationForm>({
        amount: "10",
        currency: "NZD",
        description: "",
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
        console.log("Submitting donation", { amount, currency: form.currency, description: form.description })
        if (!amount || amount < 1) {
            setErrorMessage("Amount is required and must be greater than 0.")
            return
        }

        setLoading(true)

        try {
            await createDonation({
                title: "Donation",
                description: form.description,
                amount: amount,
                currency: form.currency,
            })

            setSuccessMessage("Donation successful")
            navigate("/home")
        } catch {
            setErrorMessage("Donation request failed")
        } finally {
            setLoading(false)
        }
    }

    function goBack() {
        navigate("/home")
    }

    return (
        <div className="donation-page">
            <div className="donation-shell">
                <div className="page-header">
                    <h1>Make a donation</h1>
                    <p>Support your organization by making a donation.</p>
                </div>

                <div className="donation-card">
                    <form onSubmit={submitDonation}>
                        <div className="form-item">
                            <label htmlFor="amount">Amount</label>
                            <input
                                id="amount"
                                type="number"
                                min={1}
                                value={form.amount}
                                onChange={(e) =>
                                    updateField("amount", e.target.value)
                                }
                            />
                        </div>

                        <div className="form-item">
                            <label htmlFor="currency">Currency</label>
                            <input
                                id="currency"
                                type="text"
                                value={form.currency}
                                disabled
                            />
                        </div>

                        <div className="form-item">
                            <label htmlFor="description">Description</label>
                            <textarea
                                id="description"
                                value={form.description}
                                onChange={(e) => updateField("description", e.target.value)}
                                placeholder="Optional message"
                                rows={4}
                            />
                        </div>

                        {errorMessage ? (
                            <div className="form-alert error" role="alert">
                                {errorMessage}
                            </div>
                        ) : null}

                        {successMessage ? (
                            <div className="form-alert success" role="status">
                                {successMessage}
                            </div>
                        ) : null}

                        <div className="actions">
                            <button
                                type="button"
                                className="secondary-btn"
                                onClick={goBack}
                                disabled={loading}
                            >
                                Cancel
                            </button>

                            <button
                                type="submit"
                                className="primary-btn"
                                disabled={loading}
                            >
                                {loading ? "Submitting..." : "Donate"}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    )
}