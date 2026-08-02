import { inject } from '@angular/core';
import {
    CanActivateChildFn,
    CanActivateFn,
    Router
} from '@angular/router';

import { TokenStorageService } from './token-storage.service';

function validateSession(url: string): boolean | ReturnType<Router['createUrlTree']> {
    const tokenStorage = inject(TokenStorageService);
    const router = inject(Router);

    if (tokenStorage.isAuthenticated()) {
        return true;
    }

    return router.createUrlTree(
        ['/auth/login'],
        {
            queryParams: {
                returnUrl: url
            }
        }
    );
}

export const authGuard: CanActivateFn = (_route, state) =>
    validateSession(state.url);

export const authChildGuard: CanActivateChildFn = (_route, state) =>
    validateSession(state.url);
