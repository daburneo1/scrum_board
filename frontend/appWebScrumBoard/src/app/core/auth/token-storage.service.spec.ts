import { TokenStorageService } from './token-storage.service';
import { LoginResponse } from './auth.models';

describe('TokenStorageService', () => {
    let service: TokenStorageService;

    const futureSession: LoginResponse = {
        accessToken: 'access-token',
        tokenType: 'Bearer',
        expiresAt: new Date(Date.now() + 60_000).toISOString(),
        user: {
            id: 'user-1',
            email: 'admin@scrumboard.local',
            name: 'Admin'
        }
    };

    beforeEach(() => {
        localStorage.clear();
        service = new TokenStorageService();
    });

    afterEach(() => {
        localStorage.clear();
    });

    it('saves and returns the current session', () => {
        service.saveSession(futureSession);

        expect(service.getSession()).toEqual(futureSession);
    });

    it('returns the access token from the saved session', () => {
        service.saveSession(futureSession);

        expect(service.getAccessToken()).toBe('access-token');
    });

    it('reports authenticated when the token has not expired', () => {
        service.saveSession(futureSession);

        expect(service.isAuthenticated()).toBeTrue();
    });

    it('clears an expired session and reports unauthenticated', () => {
        service.saveSession({
            ...futureSession,
            expiresAt: new Date(Date.now() - 60_000).toISOString()
        });

        expect(service.isAuthenticated()).toBeFalse();
        expect(service.getSession()).toBeNull();
    });
});
