import { tenantHttp } from "./http";
import type { ApiResponse } from "./types";

export interface CreateDonationRequest {
  title: string;
  description?: string;
  amount: number;
  currency: string;
}

export interface CreateDonationResult {
  transactionPublicId: string;
  tenantPublicId: string;
  tenantName: string;
  type: string;
  amount: number;
  currency: string;
  status: string;
  paymentStatus: string;
  paymentReference?: string;
  failureReason?: string;
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

export async function createDonation(
  payload: CreateDonationRequest
): Promise<CreateDonationResult> {
  const response = await tenantHttp.post<ApiResponse<CreateDonationResult>>(
    "/api/transactions/donations",
    payload
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Donation failed");
  }

  return result.data;
}

export async function getMyTransactions(
  pageNumber = 1,
  pageSize = 10
): Promise<PagedResult<TransactionListItem>> {
  const response = await tenantHttp.get<ApiResponse<PagedResult<TransactionListItem>>>(
    "/api/transactions/my",
    {
      params: {
        pageNumber,
        pageSize,
      },
    }
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to fetch transactions");
  }

  return result.data;
}