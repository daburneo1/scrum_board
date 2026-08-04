export interface LoginRequest {
    email: string;
    password: string;
}

export interface AuthenticatedUser {
    id: string;
    name: string;
    email: string;
}

export interface LoginResponse {
    accessToken: string;
    tokenType: string;
    expiresAt: string;
    user: AuthenticatedUser;
}
