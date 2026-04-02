import { getTenantContext, type TenantContextDto } from "../api/tenantContext"

type TenantContextState = {
  tenantContext: TenantContextDto | null
  loaded: boolean
}

let state: TenantContextState = {
  tenantContext: null,
  loaded: false,
}

const listeners = new Set<() => void>()

function notify() {
  listeners.forEach((listener) => listener())
}

export const tenantContextStore = {
  subscribe(listener: () => void) {
    listeners.add(listener)
    return () => {listeners.delete(listener)}
  },

  getState(): TenantContextState {
    return state
  },

  get tenantContext() {
    return state.tenantContext
  },

  get isLoaded() {
    return state.loaded
  },

  get hasTenantContext() {
    return !!state.tenantContext?.tenantPublicId
  },

  async initialize() {
    try {
      const context = await getTenantContext()

      state = {
        tenantContext: context,
        loaded: true,
      }
    } catch {
      state = {
        tenantContext: null,
        loaded: true,
      }
    }

    notify()
  },

  clear() {
    state = {
      tenantContext: null,
      loaded: false,
    }

    notify()
  },
}