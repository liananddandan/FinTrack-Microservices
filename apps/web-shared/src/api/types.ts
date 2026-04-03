export type ApiResponse<T> = {
  code: string
  message: string
  data: T
}

export type PagedResult<T> = {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}