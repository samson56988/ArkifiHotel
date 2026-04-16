import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { BusinessRoomsApiService } from '../../core/services/business-rooms-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';
import type { ApiResult } from '../../core/models/api-result.model';
import type { BusinessRoomSummaryDto } from '../../core/models/rooms.models';

@Component({
  selector: 'app-rooms-list',
  standalone: true,
  imports: [RouterLink, BusinessWorkspaceComponent, DecimalPipe],
  templateUrl: './rooms-list.component.html',
  styleUrl: './rooms-list.component.scss',
})
export class RoomsListComponent implements OnInit {
  private readonly api = inject(BusinessRoomsApiService);
  private readonly toast = inject(ToastService);

  rooms: BusinessRoomSummaryDto[] = [];
  loading = false;
  loadFailed = false;
  /** When true, list includes archived rooms (hidden from guests by default). */
  showArchived = false;

  ngOnInit(): void {
    this.load();
  }

  onShowArchivedChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.showArchived = input.checked;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.loadFailed = false;
    this.api
      .listRooms(this.showArchived)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.rooms = res.data;
            this.loadFailed = false;
            return;
          }

          this.rooms = [];
          this.loadFailed = true;
          if (res.code === 'Unauthorized' || res.message?.includes('401')) {
            this.toast.warning('Please sign in again to manage rooms.', 'Rooms');
            return;
          }

          this.toast.showFailedApi(res, 'Rooms');
        },
        error: (err: unknown) => {
          this.rooms = [];
          this.loadFailed = true;
          const res = err as ApiResult<BusinessRoomSummaryDto[]>;
          if (res && typeof res === 'object' && 'message' in res) {
            this.toast.showFailedApi(res, 'Rooms');
            return;
          }

          this.toast.error('Could not load rooms.', 'Rooms');
        },
      });
  }

  imageUrl(path: string | null | undefined): string {
    if (!path) {
      return '';
    }

    return this.api.resolveImageUrl(path);
  }
}
