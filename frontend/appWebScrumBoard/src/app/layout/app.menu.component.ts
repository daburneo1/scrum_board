import { OnInit } from '@angular/core';
import { Component } from '@angular/core';
import { LayoutService } from './service/app.layout.service';
import { AuthService } from '../core/auth/auth.service';

@Component({
    selector: 'app-menu',
    templateUrl: './app.menu.component.html'
})
export class AppMenuComponent implements OnInit {

    model: any[] = [];

    constructor(public layoutService: LayoutService, private readonly authService: AuthService) { }

    ngOnInit(): void {
        this.model = [
            {
                label: 'ScrumBoard',
                items: [
                    {
                        label: 'Proyectos',
                        icon: 'pi pi-fw pi-folder',
                        routerLink: ['/projects']
                    },
                    {
                        label: 'Cerrar sesion',
                        icon: 'pi pi-fw pi-sign-out',
                        command: () => {
                            this.authService.logout();
                        }
                    }
                ]
            }
        ];
    }
}
