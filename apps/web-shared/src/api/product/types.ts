export type ProductItem = {
  publicId: string
  categoryPublicId: string
  categoryName: string
  name: string
  description: string | null
  price: number
  imageUrl: string | null
  displayOrder: number
  isAvailable: boolean
}

export type CreateProductRequest = {
  categoryPublicId: string
  name: string
  description: string | null
  price: number
  imageUrl: string | null
  displayOrder: number | null
}

export type UpdateProductRequest = {
  categoryPublicId: string
  name: string
  description: string | null
  price: number
  imageUrl: string | null
  displayOrder: number | null
  isAvailable: boolean
}