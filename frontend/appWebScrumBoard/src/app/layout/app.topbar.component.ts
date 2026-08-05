import { Component, ElementRef, ViewChild } from '@angular/core';
import { Observable } from 'rxjs';

import { AuthenticatedUser } from '../core/auth/auth.models';
import { AuthService } from '../core/auth/auth.service';
import { LayoutService } from "./service/app.layout.service";

@Component({
    selector: 'app-topbar',
    templateUrl: './app.topbar.component.html'
})
export class AppTopBarComponent {
    @ViewChild('menubutton') menuButton!: ElementRef;

    readonly currentUser$: Observable<AuthenticatedUser | null>;

    constructor(
        public layoutService: LayoutService,
        private readonly authService: AuthService
    ) {
        this.currentUser$ = this.authService.currentUser$;
    }

    getInitials(user: AuthenticatedUser): string {
        const source = user.name?.trim() || user.email;
        const parts = source.split(/[\s@._-]+/).filter(Boolean);

        return parts
            .slice(0, 2)
            .map(part => part.charAt(0).toUpperCase())
            .join('');
    }

    get menuExpanded(): boolean {
        return this.layoutService.isDesktop()
            ? !this.layoutService.state.staticMenuDesktopInactive
            : this.layoutService.state.staticMenuMobileActive;
    }
}
