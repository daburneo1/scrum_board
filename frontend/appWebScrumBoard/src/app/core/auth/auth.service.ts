import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
    AuthenticatedUser,
    LoginRequest,
    LoginResponse
} from './auth.models';
import { TokenStorageService } from './token-storage.service';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly currentUserSubject =
        new BehaviorSubject<AuthenticatedUser | null>(
            this.tokenStorage.getCurrentUser()
        );

    readonly currentUser$ =
        this.currentUserSubject.asObservable();

    constructor(
        private readonly http: HttpClient,
        private readonly tokenStorage: TokenStorageService,
        private readonly router: Router
    ) {
    }

    login(request: LoginRequest): Observable<LoginResponse> {
        return this.http
            .post<LoginResponse>(
                `${environment.apiBaseUrl}/auth/login`,
                request
            )
            .pipe(
                tap(response => {
                    this.tokenStorage.saveSession(response);
                    this.currentUserSubject.next(response.user);
                })
            );
    }

    logout(redirect = true): void {
        this.tokenStorage.clearSession();
        this.currentUserSubject.next(null);

        if (redirect) {
            void this.router.navigate(['/auth/login']);
        }
    }

    isAuthenticated(): boolean {
        return this.tokenStorage.isAuthenticated();
    }
}
