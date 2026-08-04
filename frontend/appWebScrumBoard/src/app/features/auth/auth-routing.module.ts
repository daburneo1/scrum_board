import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: 'login',
                loadChildren: () =>
                    import('./login/login.module')
                        .then(module => module.LoginModule)
            },
            {
                path: '**',
                redirectTo: '/notfound'
            }
        ])
    ],
    exports: [RouterModule]
})
export class AuthRoutingModule {
}
