import { Injectable } from '@angular/core';
import { AuthenticatedUser, LoginResponse } from './auth.models';

@Injectable({
    providedIn: 'root'
})
export class TokenStorageService {
    private readonly storageKey = 'scrumboard.auth-session';

    saveSession(session: LoginResponse): void {
        localStorage.setItem(
            this.storageKey,
            JSON.stringify(session)
        );
    }

    getSession(): LoginResponse | null {
        const storedValue = localStorage.getItem(this.storageKey);

        if (!storedValue) {
            return null;
        }

        try {
            return JSON.parse(storedValue) as LoginResponse;
        } catch {
            this.clearSession();
            return null;
        }
    }

    getAccessToken(): string | null {
        return this.getSession()?.accessToken ?? null;
    }

    getCurrentUser(): AuthenticatedUser | null {
        return this.getSession()?.user ?? null;
    }

    isAuthenticated(): boolean {
        const session = this.getSession();

        if (!session?.accessToken || !session.expiresAt) {
            return false;
        }

        const expiration = Date.parse(session.expiresAt);

        if (!Number.isFinite(expiration) || expiration <= Date.now()) {
            this.clearSession();
            return false;
        }

        return true;
    }

    clearSession(): void {
        localStorage.removeItem(this.storageKey);
    }
}
