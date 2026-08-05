import {
    Component,
    OnInit
} from '@angular/core';
import {
    FormBuilder,
    Validators
} from '@angular/forms';
import {
    ActivatedRoute,
    Router
} from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';

import {
    AuthService
} from '../../../core/auth/auth.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
    loading = false;
    errorMessage = '';

    readonly form = this.formBuilder.nonNullable.group({
        email: [
            '',
            [
                Validators.required,
                Validators.email
            ]
        ],
        password: [
            '',
            [
                Validators.required
            ]
        ]
    });

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly authService: AuthService,
        private readonly route: ActivatedRoute,
        private readonly router: Router
    ) {
    }

    ngOnInit(): void {
        if (this.authService.isAuthenticated()) {
            void this.router.navigate(['/projects']);
        }
    }

    submit(): void {
        this.errorMessage = '';

        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        this.loading = true;

        this.authService
            .login(this.form.getRawValue())
            .pipe(
                finalize(() => {
                    this.loading = false;
                })
            )
            .subscribe({
                next: () => {
                    const returnUrl =
                        this.route.snapshot.queryParamMap
                            .get('returnUrl') ?? '/projects';

                    void this.router.navigateByUrl(returnUrl);
                },
                error: (error: unknown) => {
                    this.errorMessage =
                        this.getErrorMessage(error);
                }
            });
    }

    private getErrorMessage(error: unknown): string {
        if (error instanceof HttpErrorResponse) {
            if (error.status === 401) {
                return 'Correo o contraseña incorrectos.';
            }

            if (
                typeof error.error?.detail === 'string'
            ) {
                return error.error.detail;
            }
        }

        return 'No fue posible iniciar sesión.';
    }
}
