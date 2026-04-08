import { useSyncExternalStore } from "react"
import { platformAuthStore } from "./platformAuthStore"

export function usePlatformAuthStore() {
  return useSyncExternalStore(
    platformAuthStore.subscribe,
    platformAuthStore.getState,
    platformAuthStore.getState
  )
}