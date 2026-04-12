export type CheckoutCartItem = {
  id: string
  name: string
  price: number
  quantity: number
  categoryName: string
}

export type CheckoutDraft = {
  customerName: string
  items: CheckoutCartItem[]
}

type Listener = () => void

class CheckoutStore {
  private listeners = new Set<Listener>()

  draft: CheckoutDraft | null = null

  setDraft(draft: CheckoutDraft) {
    this.draft = draft
    this.emit()
  }

  clear() {
    this.draft = null
    this.emit()
  }

  subscribe(listener: Listener) {
    this.listeners.add(listener)
    return () => {
      this.listeners.delete(listener)
    }
  }

  private emit() {
    for (const listener of this.listeners) {
      listener()
    }
  }
}

export const checkoutStore = new CheckoutStore()