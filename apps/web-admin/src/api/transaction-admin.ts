import { tenantHttp } from "./http";
import type { ApiResponse } from "./types";

export interface TenantTransactionSummary {
  tenantPublicId: string;
  tenantName: string;
  currentBalance: number;
  totalDonationAmount: number;
  totalProcurementAmount: number;
  totalTransactionCount: number;
}

export interface TransactionListItem {
  transactionPublicId: string;
  tenantPublicId: string;
  tenantName: string;
  type: string;
  title: string;
  amount: number;
  currency: string;
  status: string;
  paymentStatus: string;
  riskStatus: string;
  createdAtUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface TenantTransactionQuery {
  pageNumber?: number;
  pageSize?: number;
  type?: string;
  status?: string;
  paymentStatus?: string;
}

export async function getTenantTransactionSummary(): Promise<TenantTransactionSummary> {
  const response = await tenantHttp.get<ApiResponse<TenantTransactionSummary>>(
    "/api/transactions/summary"
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to fetch transaction summary.");
  }

  return result.data;
}

export async function getTenantTransactions(
  query: TenantTransactionQuery
): Promise<PagedResult<TransactionListItem>> {
  const response = await tenantHttp.get<ApiResponse<PagedResult<TransactionListItem>>>(
    "/api/transactions",
    {
      params: query,
    }
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to fetch tenant transactions.");
  }

  return result.data;
}