import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { NotfoundComponent } from './demo/components/notfound/notfound.component';
import { AppLayoutComponent } from "./layout/app.layout.component";
import {authChildGuard, authGuard} from "./core/auth/auth.guard";

@NgModule({
    imports: [
        RouterModule.forRoot(
            [
                {
                    path: '',
                    component: AppLayoutComponent,
                    canActivate: [authGuard],
                    canActivateChild: [authChildGuard],
                    children: [
                        {
                            path: '',
                            redirectTo: 'projects',
                            pathMatch: 'full'
                        },
                        {
                            path: 'projects',
                            loadChildren: () =>
                                import(
                                    './features/projects/projects.module'
                                    ).then(module => module.ProjectsModule)
                        }
                    ]
                },
                {
                    path: 'auth',
                    loadChildren: () =>
                        import(
                            './demo/components/auth/auth.module'
                            ).then(module => module.AuthModule)
                },
                {
                    path: 'notfound',
                    component: NotfoundComponent
                },
                {
                    path: '**',
                    redirectTo: '/notfound'
                }
            ],
            {
                scrollPositionRestoration: 'enabled',
                anchorScrolling: 'enabled',
                onSameUrlNavigation: 'reload'
            }
        )
    ],
    exports: [RouterModule]
})
export class AppRoutingModule {
}
