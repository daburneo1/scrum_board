import {
    Component,
    OnDestroy,
    OnInit
} from '@angular/core';
import {
    FormBuilder,
    FormControl,
    Validators
} from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import {
    ConfirmationService,
    LazyLoadEvent,
    MessageService
} from 'primeng/api';
import {
    Subject,
    debounceTime,
    distinctUntilChanged,
    finalize,
    takeUntil
} from 'rxjs';

import {
    ProjectService
} from '../services/project.service';
import {
    Project,
    ProjectStatus,
    SaveProjectRequest
} from '../models/project.models';

@Component({
    selector: 'app-project-list',
    templateUrl: './project-list.component.html'
})
export class ProjectListComponent
    implements OnInit, OnDestroy {

    private readonly destroy$ = new Subject<void>();

    projects: Project[] = [];

    totalRecords = 0;
    first = 0;
    pageSize = 10;

    loading = false;
    saving = false;
    dialogVisible = false;

    editingProjectId: string | null = null;

    readonly filterControl =
        new FormControl('', {
            nonNullable: true
        });

    readonly statusOptions = [
        {
            label: 'Planificado',
            value: ProjectStatus.Planned
        },
        {
            label: 'Activo',
            value: ProjectStatus.Active
        },
        {
            label: 'Completado',
            value: ProjectStatus.Completed
        },
        {
            label: 'Cancelado',
            value: ProjectStatus.Cancelled
        }
    ];

    readonly form = this.formBuilder.group({
        name: [
            '',
            [
                Validators.required,
                Validators.maxLength(200)
            ]
        ],
        description: [
            '',
            [
                Validators.maxLength(2000)
            ]
        ],
        startDate: [
            null as Date | null,
            [
                Validators.required
            ]
        ],
        expectedEndDate: [
            null as Date | null,
            [
                Validators.required
            ]
        ],
        status: [
            ProjectStatus.Planned,
            [
                Validators.required
            ]
        ]
    });

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly projectService: ProjectService,
        private readonly confirmationService: ConfirmationService,
        private readonly messageService: MessageService
    ) {
    }

    ngOnInit(): void {
        this.filterControl.valueChanges
            .pipe(
                debounceTime(350),
                distinctUntilChanged(),
                takeUntil(this.destroy$)
            )
            .subscribe(() => {
                this.first = 0;
                this.loadProjects();
            });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    onLazyLoad(event: LazyLoadEvent): void {
        this.first = event.first ?? 0;
        this.pageSize = event.rows ?? 10;

        this.loadProjects();
    }

    loadProjects(): void {
        const pageNumber =
            Math.floor(this.first / this.pageSize) + 1;

        this.loading = true;

        this.projectService
            .getPaged({
                pageNumber,
                pageSize: this.pageSize,
                name: this.filterControl.value
            })
            .pipe(
                finalize(() => {
                    this.loading = false;
                })
            )
            .subscribe({
                next: response => {
                    this.projects = response.items;
                    this.totalRecords = response.totalCount;
                },
                error: error => {
                    this.showError(
                        error,
                        'No fue posible cargar los proyectos.'
                    );
                }
            });
    }

    openCreate(): void {
        this.editingProjectId = null;

        this.form.reset({
            name: '',
            description: '',
            startDate: new Date(),
            expectedEndDate: new Date(),
            status: ProjectStatus.Planned
        });

        this.dialogVisible = true;
    }

    openEdit(project: Project): void {
        this.editingProjectId = project.id;

        this.form.reset({
            name: project.name,
            description: project.description,
            startDate: this.parseDate(project.startDate),
            expectedEndDate:
                this.parseDate(project.expectedEndDate),
            status: project.status
        });

        this.dialogVisible = true;
    }

    save(): void {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        const value = this.form.getRawValue();

        if (
            !value.startDate ||
            !value.expectedEndDate
        ) {
            return;
        }

        if (
            value.expectedEndDate.getTime() <
            value.startDate.getTime()
        ) {
            this.messageService.add({
                severity: 'warn',
                summary: 'Fechas inválidas',
                detail:
                    'La fecha de finalización no puede ser anterior a la fecha de inicio.'
            });

            return;
        }

        const request: SaveProjectRequest = {
            name: value.name?.trim() ?? '',
            description:
                value.description?.trim() ?? '',
            startDate:
                this.formatDate(value.startDate),
            expectedEndDate:
                this.formatDate(value.expectedEndDate),
            status:
                value.status ?? ProjectStatus.Planned
        };

        this.saving = true;

        const operation = this.editingProjectId
            ? this.projectService.update(
                this.editingProjectId,
                request
            )
            : this.projectService.create(request);

        operation
            .pipe(
                finalize(() => {
                    this.saving = false;
                })
            )
            .subscribe({
                next: () => {
                    this.dialogVisible = false;

                    this.messageService.add({
                        severity: 'success',
                        summary: 'Operación exitosa',
                        detail: this.editingProjectId
                            ? 'Proyecto actualizado.'
                            : 'Proyecto creado.'
                    });

                    this.loadProjects();
                },
                error: error => {
                    this.showError(
                        error,
                        'No fue posible guardar el proyecto.'
                    );
                }
            });
    }

    confirmDelete(project: Project): void {
        this.confirmationService.confirm({
            header: 'Eliminar proyecto',
            icon: 'pi pi-exclamation-triangle',
            message:
                `¿Deseas eliminar el proyecto "${project.name}"?`,
            acceptLabel: 'Eliminar',
            rejectLabel: 'Cancelar',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.deleteProject(project);
            }
        });
    }

    getStatusLabel(status: ProjectStatus): string {
        return this.statusOptions.find(
            option => option.value === status
        )?.label ?? 'Desconocido';
    }

    getStatusSeverity(
        status: ProjectStatus
    ): 'info' | 'success' | 'warning' | 'danger' {
        switch (status) {
            case ProjectStatus.Active:
                return 'success';

            case ProjectStatus.Completed:
                return 'info';

            case ProjectStatus.Cancelled:
                return 'danger';

            default:
                return 'warning';
        }
    }

    private deleteProject(project: Project): void {
        this.projectService
            .delete(project.id)
            .subscribe({
                next: () => {
                    this.messageService.add({
                        severity: 'success',
                        summary: 'Proyecto eliminado',
                        detail:
                            `"${project.name}" fue eliminado.`
                    });

                    this.loadProjects();
                },
                error: error => {
                    this.showError(
                        error,
                        'No fue posible eliminar el proyecto.'
                    );
                }
            });
    }

    private formatDate(date: Date): string {
        const year = date.getFullYear();

        const month = String(
            date.getMonth() + 1
        ).padStart(2, '0');

        const day = String(
            date.getDate()
        ).padStart(2, '0');

        return `${year}-${month}-${day}`;
    }

    private parseDate(value: string): Date {
        return new Date(`${value}T00:00:00`);
    }

    private showError(
        error: unknown,
        fallback: string
    ): void {
        const detail =
            error instanceof HttpErrorResponse &&
            typeof error.error?.detail === 'string'
                ? error.error.detail
                : fallback;

        this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail
        });
    }
}
