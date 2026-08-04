import { Injectable } from '@angular/core';
import {
    HttpErrorResponse,
    HttpEvent,
    HttpHandler,
    HttpInterceptor,
    HttpRequest
} from '@angular/common/http';
import { Router } from '@angular/router';
import {
    Observable,
    catchError,
    throwError
} from 'rxjs';

import { environment } from '../../../environments/environment';
import { TokenStorageService } from './token-storage.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(
        private readonly tokenStorage: TokenStorageService,
        private readonly router: Router
    ) {
    }

    intercept(
        request: HttpRequest<unknown>,
        next: HttpHandler
    ): Observable<HttpEvent<unknown>> {
        const isApiRequest =
            request.url.startsWith(environment.apiBaseUrl);

        const isLoginRequest =
            request.url.includes('/auth/login');

        const accessToken =
            this.tokenStorage.getAccessToken();

        let authenticatedRequest = request;

        if (isApiRequest && accessToken && !isLoginRequest) {
            authenticatedRequest = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${accessToken}`
                }
            });
        }

        return next.handle(authenticatedRequest).pipe(
            catchError((error: unknown) => {
                if (
                    error instanceof HttpErrorResponse &&
                    error.status === 401 &&
                    isApiRequest &&
                    !isLoginRequest
                ) {
                    this.tokenStorage.clearSession();

                    void this.router.navigate(
                        ['/auth/login'],
                        {
                            queryParams: {
                                returnUrl: this.router.url
                            }
                        }
                    );
                }

                return throwError(() => error);
            })
        );
    }
}
