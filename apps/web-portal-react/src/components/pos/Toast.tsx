type ToastType = "success" | "error"

type Props = {
  open: boolean
  type: ToastType
  message: string
}

export default function Toast({ open, type, message }: Props) {
  if (!open || !message) return null

  return (
    <div className="fixed right-4 top-4 z-[80]">
      <div
        className={[
          "min-w-[280px] rounded-2xl border px-4 py-3 shadow-lg",
          type === "success"
            ? "border-emerald-200 bg-emerald-50 text-emerald-700"
            : "border-rose-200 bg-rose-50 text-rose-700",
        ].join(" ")}
        role="status"
      >
        <div className="text-sm font-medium">{message}</div>
      </div>
    </div>
  )
}