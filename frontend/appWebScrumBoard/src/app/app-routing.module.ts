import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { AppLayoutComponent } from "./layout/app.layout.component";
import {authChildGuard, authGuard} from "./core/auth/auth.guard";
import {NotFoundComponent} from "./core/not-found/not-found.component";

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
                            './features/auth/auth.module'
                            ).then(module => module.AuthModule)
                },
                {
                    path: 'notfound',
                    component: NotFoundComponent
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
