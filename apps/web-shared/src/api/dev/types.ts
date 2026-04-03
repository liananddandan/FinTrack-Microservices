export type DemoTenantSeed = {
  tenantPublicId: string
  tenantName: string
  adminEmail: string
  adminPassword: string
  memberEmail: string
  memberPassword: string
  categoryCount: number
  productCount: number
  orderCount: number
}

export type DevSeedResult = {
  tenants: DemoTenantSeed[]
}