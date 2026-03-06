import { http } from "./http";

export type LoginRequest = {
  email: string;
  password: string;
};

export type LoginResponse = {
  accessToken: string;
  refreshToken?: string;
};

export async function login(request: LoginRequest) {
  const { data } = await http.post<LoginResponse>("/api/account/login", request);
  return data;
}