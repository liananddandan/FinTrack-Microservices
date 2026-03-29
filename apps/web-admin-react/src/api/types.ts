export type ApiResponse<T> = {
  code: string
  message: string
  data: T
}

export type PagedResult<T> = {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
}