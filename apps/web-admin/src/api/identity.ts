import { publicHttp } from "./http";

export type LoginRequest = { email: string; password: string };
export type LoginResponse = { accessToken: string; refreshToken?: string; tenantId?: string };

export async function login(req: LoginRequest) {
  const { data } = await publicHttp.post("/api/account/login", req);
  return data;
}