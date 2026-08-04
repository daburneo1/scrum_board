import {ButtonModule} from "primeng/button";
import {TagModule} from "primeng/tag";
import {Component, inject, OnInit, OnDestroy} from "@angular/core";
import {CommonModule} from "@angular/common";
import {HttpErrorResponse, HttpResponse} from "@angular/common/http";
import {ActivatedRoute} from "@angular/router";
import {BoardService} from "../services/board.service";
import {
    BoardColumn,
    BoardTask,
    ProjectTaskFilters,
    ProjectBoard,
    UserOption,
    WorkItemPriority,
    MoveTaskResponse, BoardChangedNotification, RealtimeConnectionState,
} from "../models/board.models";
import {
    EMPTY,
    catchError,
    debounceTime,
    distinctUntilChanged,
    finalize,
    map,
    switchMap,
    tap
} from "rxjs";
import {ReactiveFormsModule, FormBuilder, Validators} from "@angular/forms";
import {DialogModule} from "primeng/dialog";
import {InputTextModule} from "primeng/inputtext";
import {InputTextareaModule} from "primeng/inputtextarea";
import {DropdownModule} from "primeng/dropdown";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {TooltipModule} from "primeng/tooltip";
import {ConfirmationService} from "primeng/api";
import {
    CdkDragDrop,
    DragDropModule,
    moveItemInArray,
    transferArrayItem
} from '@angular/cdk/drag-drop';
import {
    Subject,
    auditTime,
    filter,
    takeUntil
} from 'rxjs';
import {BoardRealtimeService} from "../services/board-realtime.service";
import {ProjectReportFormat, ProjectReportService} from "../services/project-report.service";


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
        ConfirmDialogModule,
        TooltipModule,
        DragDropModule
    ],
    templateUrl: './project-board.component.html',
    styleUrl: './project-board.component.scss'
})
export class ProjectBoardComponent implements OnInit, OnDestroy {
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
    assigneeFilterOptions: Array<{
        id: string | null;
        name: string;
        email: string;
    }> = [
        {
            id: null,
            name: 'Todos los responsables',
            email: ''
        }
    ];

    readonly priorityOptions = [
        {label: 'Baja', value: WorkItemPriority.Low},
        {label: 'Media', value: WorkItemPriority.Medium},
        {label: 'Alta', value: WorkItemPriority.High},
        {label: 'Crítica', value: WorkItemPriority.Critical}
    ];

    readonly filterPriorityOptions = [
        {label: 'Todas', value: null},
        ...this.priorityOptions
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

    readonly filterForm = this.formBuilder.group({
        assigneeId: [null as string | null],
        priority: [null as WorkItemPriority | null],
        search: ['', [Validators.maxLength(100)]]
    });

    ngOnInit(): void {
        const projectId =
            this.route.snapshot.paramMap.get('projectId');

        if (!projectId) {
            this.errorMessage =
                'No se encontró el identificador del proyecto.';
            return;
        }

        this.loadUsers();

        this.filterForm.valueChanges
            .pipe(
                debounceTime(300),
                map(() => this.getActiveFilters()),
                distinctUntilChanged((previous, current) =>
                    this.areFiltersEqual(
                        previous,
                        current
                    )
                ),
                tap(() => {
                    this.loading = true;
                    this.errorMessage = '';
                }),
                switchMap(filters =>
                    this.boardService
                        .getBoard(projectId, filters)
                        .pipe(
                            finalize(() => {
                                this.loading = false;
                            }),
                            catchError(() => {
                                this.errorMessage =
                                    'No fue posible cargar el tablero.';

                                return EMPTY;
                            })
                        )
                ),
                takeUntil(this.destroy$)
            )
            .subscribe(board => {
                this.board = board;
            });

        this.boardRealtimeService
            .boardChanged$
            .pipe(
                filter(notification =>
                    notification.projectId === projectId
                ),
                auditTime(100),
                takeUntil(this.destroy$)
            )
            .subscribe(notification => {
                this.handleRealtimeChange(
                    notification,
                    projectId
                );
            });

        void this.connectRealtime(projectId);

        this.loadBoard(projectId);
    }

    private async connectRealtime(
        projectId: string
    ): Promise<void> {
        try {
            await this.boardRealtimeService.connect(
                projectId
            );
        } catch (error) {
            console.error(
                'Could not connect to the board hub.',
                error
            );

            /*
             * El tablero continúa siendo utilizable
             * mediante REST aunque falle SignalR.
             */
        }
    }

    private handleRealtimeChange(
        notification: BoardChangedNotification,
        projectId: string
    ): void {
        const occurredAt =
            Date.parse(notification.occurredAtUtc);

        if (Number.isFinite(occurredAt)) {
            const approximateLatency =
                Date.now() - occurredAt;

            console.info(
                'Board event received:',
                notification.changeType,
                `${approximateLatency} ms`
            );
        }

        /*
         * REST sigue siendo la fuente de verdad.
         */
        this.loadBoard(projectId, false);
    }

    loadBoard(
        projectId?: string,
        showLoading = true
    ): void {
        const resolvedProjectId =
            projectId ?? this.board?.projectId;

        if (!resolvedProjectId) {
            return;
        }

        if (showLoading) {
            this.loading = true;
        }

        this.errorMessage = '';

        this.boardService
            .getBoard(
                resolvedProjectId,
                this.getActiveFilters()
            )
            .pipe(
                finalize(() => {
                    if (showLoading) {
                        this.loading = false;
                    }
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

    ngOnDestroy(): void {
        console.log(
            '[ProjectBoardComponent] ngOnDestroy ejecutado'
        );

        this.destroy$.next();
        this.destroy$.complete();

        void this.boardRealtimeService
            .disconnect()
            .then(() => {
                console.log(
                    '[ProjectBoardComponent] SignalR desconectado'
                );
            })
            .catch(error => {
                console.error(
                    '[ProjectBoardComponent] Error al desconectar SignalR',
                    error
                );
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
            next: users => {
                this.users = users;
                this.assigneeFilterOptions = [
                    {
                        id: null,
                        name: 'Todos los responsables',
                        email: ''
                    },
                    ...users
                ];
            },
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

    movingTaskId: string | null = null;

    dropTask(
        event: CdkDragDrop<BoardTask[]>
    ): void {
        if (
            !this.board ||
            this.movingTaskId ||
            this.hasActiveFilters()
        ) {
            return;
        }

        if (
            event.previousContainer === event.container &&
            event.previousIndex === event.currentIndex
        ) {
            return;
        }

        const movedTask = event.item.data as BoardTask;

        const previousBoard =
            this.cloneBoard(this.board);

        if (event.previousContainer === event.container) {
            moveItemInArray(
                event.container.data,
                event.previousIndex,
                event.currentIndex
            );
        } else {
            transferArrayItem(
                event.previousContainer.data,
                event.container.data,
                event.previousIndex,
                event.currentIndex
            );
        }

        this.movingTaskId = movedTask.id;

        this.boardService
            .moveTask(
                this.board.projectId,
                movedTask.id,
                {
                    targetColumnId:
                    event.container.id,
                    targetIndex:
                    event.currentIndex
                }
            )
            .pipe(
                finalize(() => {
                    this.movingTaskId = null;
                })
            )
            .subscribe({
                next: response => {
                    if (this.hasActiveFilters()) {
                        this.loadBoard(
                            this.board?.projectId,
                            false
                        );
                        return;
                    }

                    this.applyCanonicalColumns(response);
                },
                error: error => {
                    this.board = previousBoard;

                    this.errorMessage = this.getErrorDetail(
                        error,
                        'No fue posible mover la tarea. El movimiento fue revertido.'
                    );
                }
            });
    }

    private cloneBoard(
        board: ProjectBoard
    ): ProjectBoard {
        return {
            ...board,
            columns: board.columns.map(column => ({
                ...column,
                tasks: [...column.tasks]
            }))
        };
    }

    private applyCanonicalColumns(
        response: MoveTaskResponse
    ): void {
        if (!this.board) {
            return;
        }

        const columnsById = new Map(
            response.affectedColumns.map(column => [
                column.id,
                column
            ])
        );

        this.board = {
            ...this.board,
            columns: this.board.columns.map(column =>
                columnsById.get(column.id) ?? column
            )
        };
    }

    trackTaskById(
        _index: number,
        task: BoardTask
    ): string {
        return task.id;
    }

    trackColumnById(
        _index: number,
        column: BoardColumn
    ): string {
        return column.id;
    }

    private readonly boardRealtimeService =
        inject(BoardRealtimeService);

    private readonly destroy$ =
        new Subject<void>();

    readonly realtimeState$ =
        this.boardRealtimeService.connectionState$;

    getRealtimeLabel(
        state: RealtimeConnectionState
    ): string {
        switch (state) {
            case 'connected':
                return 'Tiempo real conectado';

            case 'connecting':
                return 'Conectando...';

            case 'reconnecting':
                return 'Reconectando...';

            default:
                return 'Tiempo real desconectado';
        }
    }

    getRealtimeSeverity(
        state: RealtimeConnectionState
    ): 'success' | 'warning' | 'danger' | 'info' {
        switch (state) {
            case 'connected':
                return 'success';

            case 'connecting':
            case 'reconnecting':
                return 'warning';

            default:
                return 'danger';
        }
    }

    private readonly projectReportService =
        inject(ProjectReportService);

    downloadingReport:
        ProjectReportFormat | null = null;

    downloadReport(
        format: ProjectReportFormat
    ): void {
        if (
            !this.board ||
            this.downloadingReport
        ) {
            return;
        }

        this.downloadingReport = format;

        this.projectReportService
            .getReport(
                this.board.projectId,
                format,
                this.getActiveFilters()
            )
            .pipe(
                finalize(() => {
                    this.downloadingReport = null;
                })
            )
            .subscribe({
                next: response => {
                    this.saveReportFile(
                        response,
                        format
                    );
                },
                error: error => {
                    this.errorMessage =
                        'No fue posible descargar el reporte';
                }
            });
    }

    clearFilters(): void {
        this.filterForm.reset({
            assigneeId: null,
            priority: null,
            search: ''
        });
    }

    private getActiveFilters(): ProjectTaskFilters {
        const value = this.filterForm.getRawValue();

        return {
            assigneeId: value.assigneeId ?? null,
            priority: value.priority ?? null,
            search: value.search?.trim() || null
        };
    }

    hasActiveFilters(): boolean {
        const filters = this.getActiveFilters();

        return Boolean(
            filters.assigneeId ||
            filters.priority !== null ||
            filters.search
        );
    }

    private areFiltersEqual(
        previous: ProjectTaskFilters,
        current: ProjectTaskFilters
    ): boolean {
        return previous.assigneeId === current.assigneeId &&
            previous.priority === current.priority &&
            previous.search === current.search;
    }

    private saveReportFile(
        response: HttpResponse<Blob>,
        format: ProjectReportFormat
    ): void {
        const content = response.body;

        if (!content || content.size === 0) {
            throw new Error(
                'The report response was empty.'
            );
        }

        const contentDisposition =
            response.headers.get(
                'content-disposition'
            );

        const fileName =
            this.extractFileName(
                contentDisposition
            ) ??
            `project-report.${format}`;

        const objectUrl =
            URL.createObjectURL(content);

        const anchor =
            document.createElement('a');

        anchor.href = objectUrl;
        anchor.download = fileName;
        anchor.style.display = 'none';

        document.body.appendChild(anchor);

        anchor.click();
        anchor.remove();

        setTimeout(() => {
            URL.revokeObjectURL(objectUrl);
        });
    }

    private extractFileName(
        contentDisposition: string | null
    ): string | null {
        if (!contentDisposition) {
            return null;
        }

        const encodedMatch =
            /filename\*=UTF-8''([^;]+)/i
                .exec(contentDisposition);

        if (encodedMatch?.[1]) {
            try {
                return decodeURIComponent(
                    encodedMatch[1]
                );
            } catch {
                return encodedMatch[1];
            }
        }

        const simpleMatch =
            /filename="?([^";]+)"?/i
                .exec(contentDisposition);

        return simpleMatch?.[1]?.trim()
            ?? null;
    }
}
