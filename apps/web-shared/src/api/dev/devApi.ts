import type { AxiosInstance } from "axios"
import type { ApiResponse } from "../types"
import type { DevSeedResult } from "./types"

export function createDevApi(publicHttp: AxiosInstance) {
  function unwrapApiResponse<T>(
    response: { data: ApiResponse<T> },
    defaultMessage: string
  ): T {
    const result = response.data

    if (result.data == null) {
      throw new Error(result.message || defaultMessage)
    }

    return result.data
  }

  return {
    async seedDemoData(): Promise<DevSeedResult> {
      const response = await publicHttp.post<ApiResponse<DevSeedResult>>(
        "/api/dev/seed",
        {}
      )

      return unwrapApiResponse(response, "Seed demo data failed")
    },
  }
}