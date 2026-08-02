import {ButtonModule} from "primeng/button";
import {TagModule} from "primeng/tag";
import {Component, inject, OnInit} from "@angular/core";
import {CommonModule} from "@angular/common";
import {HttpErrorResponse} from "@angular/common/http";
import {ActivatedRoute} from "@angular/router";
import {BoardService} from "../services/board.service";
import {
    BoardColumn,
    BoardTask,
    ProjectBoard,
    UserOption,
    WorkItemPriority
} from "../models/board.models";
import {finalize} from "rxjs";
import {ReactiveFormsModule, FormBuilder, Validators} from "@angular/forms";
import {DialogModule} from "primeng/dialog";
import {InputTextModule} from "primeng/inputtext";
import {InputTextareaModule} from "primeng/inputtextarea";
import {DropdownModule} from "primeng/dropdown";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {ConfirmationService} from "primeng/api";


@Component({
    selector: 'app-project-board',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        TagModule,
        DialogModule,
        InputTextModule,
        InputTextareaModule,
        DropdownModule,
        ConfirmDialogModule
    ],
    templateUrl: './project-board.component.html',
    styleUrl: './project-board.component.scss'
})
export class ProjectBoardComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly boardService = inject(BoardService);
    private readonly formBuilder = inject(FormBuilder);
    private readonly confirmationService = inject(ConfirmationService);

    board: ProjectBoard | null = null;

    loading = false;
    reorderingColumns = false;
    errorMessage = '';
    columnDialogVisible = false;
    savingColumn = false;
    editingColumnId: string | null = null;
    taskDialogVisible = false;
    savingTask = false;
    editingTaskId: string | null = null;
    users: UserOption[] = [];

    readonly priorityOptions = [
        {label: 'Baja', value: WorkItemPriority.Low},
        {label: 'Media', value: WorkItemPriority.Medium},
        {label: 'Alta', value: WorkItemPriority.High},
        {label: 'Crítica', value: WorkItemPriority.Critical}
    ];

    readonly columnForm = this.formBuilder.group({
        name: ['', [Validators.required, Validators.maxLength(200)]]
    });

    readonly taskForm = this.formBuilder.group({
        title: ['', [Validators.required, Validators.maxLength(200)]],
        description: ['', [Validators.maxLength(2000)]],
        priority: [WorkItemPriority.Medium, Validators.required],
        assignedUserId: [null as string | null],
        columnId: ['', Validators.required]
    });

    ngOnInit(): void {
        const projectId =
            this.route.snapshot.paramMap.get('projectId');

        if (!projectId) {
            this.errorMessage =
                'No se encontró el identificador del proyecto.';
            return;
        }

        this.loadBoard(projectId);
        this.loadUsers();
    }

    loadBoard(projectId?: string): void {
        const resolvedProjectId =
            projectId ?? this.board?.projectId;

        if (!resolvedProjectId) {
            return;
        }

        this.loading = true;
        this.errorMessage = '';

        this.boardService
            .getBoard(resolvedProjectId)
            .pipe(
                finalize(() => {
                    this.loading = false;
                })
            )
            .subscribe({
                next: board => {
                    this.board = board;
                },
                error: () => {
                    this.errorMessage =
                        'No fue posible cargar el tablero.';
                }
            });
    }

    openCreateColumn(): void {
        this.editingColumnId = null;
        this.columnForm.reset({name: ''});
        this.columnDialogVisible = true;
    }

    openEditColumn(column: BoardColumn): void {
        this.editingColumnId = column.id;
        this.columnForm.reset({name: column.name});
        this.columnDialogVisible = true;
    }

    saveColumn(): void {
        if (!this.board || this.columnForm.invalid) {
            this.columnForm.markAllAsTouched();
            return;
        }

        const name = this.columnForm.controls.name.value?.trim() ?? '';
        if (!name) {
            this.columnForm.controls.name.setErrors({required: true});
            return;
        }

        const projectId = this.board.projectId;
        const operation = this.editingColumnId
            ? this.boardService.updateColumn(projectId, this.editingColumnId, name)
            : this.boardService.createColumn(projectId, name);

        this.savingColumn = true;
        this.errorMessage = '';
        operation.pipe(finalize(() => this.savingColumn = false)).subscribe({
            next: () => {
                this.columnDialogVisible = false;
                this.loadBoard(projectId);
            },
            error: () => {
                this.errorMessage = 'No fue posible guardar la columna.';
            }
        });
    }

    confirmDeleteColumn(column: BoardColumn): void {
        if (!this.board) {
            return;
        }

        const projectId = this.board.projectId;
        this.confirmationService.confirm({
            header: 'Eliminar columna',
            icon: 'pi pi-exclamation-triangle',
            message: `¿Deseas eliminar la columna "${column.name}"?`,
            acceptLabel: 'Eliminar',
            rejectLabel: 'Cancelar',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.boardService.deleteColumn(projectId, column.id).subscribe({
                    next: () => this.loadBoard(projectId),
                    error: error => {
                        this.errorMessage = this.getErrorDetail(
                            error,
                            'No fue posible eliminar la columna.'
                        );
                    }
                });
            }
        });
    }

    openCreateTask(column: BoardColumn): void {
        this.editingTaskId = null;
        this.taskForm.reset({
            title: '',
            description: '',
            priority: WorkItemPriority.Medium,
            assignedUserId: null,
            columnId: column.id
        });
        this.taskDialogVisible = true;
    }

    openEditTask(task: BoardTask): void {
        this.editingTaskId = task.id;
        this.taskForm.reset({
            title: task.title,
            description: task.description,
            priority: task.priority,
            assignedUserId: task.assignedUserId,
            columnId: task.columnId
        });
        this.taskDialogVisible = true;
    }

    saveTask(): void {
        if (!this.board || this.taskForm.invalid) {
            this.taskForm.markAllAsTouched();
            return;
        }

        const value = this.taskForm.getRawValue();
        const title = value.title?.trim() ?? '';
        if (!title || !value.columnId) {
            this.taskForm.controls.title.setErrors({required: true});
            return;
        }

        const projectId = this.board.projectId;
        const request = {
            title,
            description: value.description?.trim() ?? '',
            priority: value.priority ?? WorkItemPriority.Medium,
            assignedUserId: value.assignedUserId,
            columnId: value.columnId
        };
        const operation = this.editingTaskId
            ? this.boardService.updateTask(projectId, this.editingTaskId, request)
            : this.boardService.createTask(projectId, request);

        this.savingTask = true;
        this.errorMessage = '';
        operation.pipe(finalize(() => this.savingTask = false)).subscribe({
            next: () => {
                this.taskDialogVisible = false;
                this.loadBoard(projectId);
            },
            error: () => {
                this.errorMessage = 'No fue posible guardar la tarea.';
            }
        });
    }

    confirmDeleteTask(task: BoardTask): void {
        if (!this.board) {
            return;
        }

        const projectId = this.board.projectId;
        this.confirmationService.confirm({
            header: 'Eliminar tarea',
            icon: 'pi pi-exclamation-triangle',
            message: `¿Deseas eliminar la tarea "${task.title}"?`,
            acceptLabel: 'Eliminar',
            rejectLabel: 'Cancelar',
            acceptButtonStyleClass: 'p-button-danger',
            accept: () => {
                this.boardService.deleteTask(projectId, task.id).subscribe({
                    next: () => this.loadBoard(projectId),
                    error: () => {
                        this.errorMessage = 'No fue posible eliminar la tarea.';
                    }
                });
            }
        });
    }

    getPriorityLabel(priority: WorkItemPriority): string {
        return this.priorityOptions.find(option => option.value === priority)?.label
            ?? 'Desconocida';
    }

    getPrioritySeverity(
        priority: WorkItemPriority
    ): 'info' | 'success' | 'warning' | 'danger' {
        switch (priority) {
            case WorkItemPriority.Low:
                return 'info';
            case WorkItemPriority.High:
                return 'warning';
            case WorkItemPriority.Critical:
                return 'danger';
            default:
                return 'success';
        }
    }

    private loadUsers(): void {
        this.boardService.getUsers().subscribe({
            next: users => this.users = users,
            error: () => {
                this.errorMessage = 'No fue posible cargar los usuarios.';
            }
        });
    }

    private getErrorDetail(error: unknown, fallback: string): string {
        if (
            error instanceof HttpErrorResponse &&
            typeof error.error?.detail === 'string' &&
            error.error.detail.trim()
        ) {
            return error.error.detail;
        }

        return fallback;
    }

    moveColumn(
        currentIndex: number,
        direction: -1 | 1
    ): void {
        if (
            !this.board ||
            this.reorderingColumns
        ) {
            return;
        }

        const targetIndex =
            currentIndex + direction;

        if (
            targetIndex < 0 ||
            targetIndex >= this.board.columns.length
        ) {
            return;
        }

        /*
         * Conservamos el estado anterior para poder
         * revertir la actualización optimista.
         */
        const previousBoard = this.board;

        const reorderedColumns = [
            ...this.board.columns
        ];

        [
            reorderedColumns[currentIndex],
            reorderedColumns[targetIndex]
        ] = [
            reorderedColumns[targetIndex],
            reorderedColumns[currentIndex]
        ];

        /*
         * Actualización optimista:
         * la interfaz cambia antes de recibir
         * la respuesta del servidor.
         */
        this.board = {
            ...this.board,
            columns: reorderedColumns
        };

        this.reorderingColumns = true;
        this.errorMessage = '';

        this.boardService
            .reorderColumns(
                this.board.projectId,
                reorderedColumns.map(
                    column => column.id
                )
            )
            .pipe(
                finalize(() => {
                    this.reorderingColumns = false;
                })
            )
            .subscribe({
                next: () => {
                    /*
                     * Recargamos para obtener el orden
                     * canónico persistido por el backend.
                     */
                    this.loadBoard(previousBoard.projectId);
                },
                error: () => {
                    /*
                     * Rollback visible si el servidor falla.
                     */
                    this.board = previousBoard;

                    this.errorMessage =
                        'No fue posible reordenar las columnas.';
                }
            });
    }
}
