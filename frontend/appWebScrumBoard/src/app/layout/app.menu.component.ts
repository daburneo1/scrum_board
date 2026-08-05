import { Component } from '@angular/core';
import { AuthService } from '../core/auth/auth.service';

@Component({
    selector: 'app-menu',
    templateUrl: './app.menu.component.html'
})
export class AppMenuComponent {

    constructor(private readonly authService: AuthService) { }

    logout(): void {
        this.authService.logout();
    }
}
