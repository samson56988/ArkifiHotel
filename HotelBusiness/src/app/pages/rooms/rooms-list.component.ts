import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { BusinessRoomsApiService } from '../../core/services/business-rooms-api.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';
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

  rooms: BusinessRoomSummaryDto[] = [];
  loading = false;
  error: string | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;
    this.api
      .listRooms()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((res) => {
        if (res.success && res.data) {
          this.rooms = res.data;
          return;
        }

        this.error = res.message ?? 'Could not load rooms.';
        if (res.code === 'Unauthorized' || res.message?.includes('401')) {
          this.error = 'Please sign in again to manage rooms.';
        }
      });
  }

  imageUrl(path: string | null | undefined): string {
    if (!path) {
      return '';
    }

    return this.api.resolveImageUrl(path);
  }
}
