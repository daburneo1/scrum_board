import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ProjectsRoutingModule } from './projects-routing.module';

import {ReactiveFormsModule} from "@angular/forms";
import {TableModule} from "primeng/table";
import {ButtonModule} from "primeng/button";
import {DialogModule} from "primeng/dialog";
import {InputTextModule} from "primeng/inputtext";
import {InputTextareaModule} from "primeng/inputtextarea";
import {CalendarModule} from "primeng/calendar";
import {DropdownModule} from "primeng/dropdown";
import {ToolbarModule} from "primeng/toolbar";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {ToastModule} from "primeng/toast";
import {TagModule} from "primeng/tag";
import {ConfirmationService, MessageService} from "primeng/api";
import {ProjectListComponent} from "./project-list/project-list.component";


@NgModule({
    declarations: [
        ProjectListComponent
    ],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ProjectsRoutingModule,
        TableModule,
        ButtonModule,
        DialogModule,
        InputTextModule,
        InputTextareaModule,
        CalendarModule,
        DropdownModule,
        ToolbarModule,
        ConfirmDialogModule,
        ToastModule,
        TagModule
    ],
    providers: [
        ConfirmationService,
        MessageService
    ]
})
export class ProjectsModule {
}
