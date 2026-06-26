import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type {
  EventHallRequestDetailDto,
  EventHallRequestListItemDto,
} from '../../core/models/event-hall.models';
import { BusinessEventHallsApiService } from '../../core/services/business-event-halls-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-event-hall-requests-list',
  standalone: true,
  imports: [BusinessWorkspaceComponent, DatePipe, FormsModule, RouterLink],
  templateUrl: './event-hall-requests-list.component.html',
  styleUrl: './event-hall-requests-list.component.scss',
})
export class EventHallRequestsListComponent implements OnInit {
  private readonly api = inject(BusinessEventHallsApiService);
  private readonly toast = inject(ToastService);

  readonly requests = signal<EventHallRequestListItemDto[]>([]);
  readonly selectedId = signal<string | null>(null);
  readonly detail = signal<EventHallRequestDetailDto | null>(null);
  readonly detailLoading = signal(false);
  readonly loading = signal(false);
  statusFilter = '';
  readonly loadFailed = signal(false);
  readonly initialLoadDone = signal(false);
  readonly updating = signal(false);

  readonly statusOptions = ['Pending', 'Approved', 'Rejected', 'Cancelled'] as const;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);
    this.api.listRequests(this.statusFilter || undefined).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.requests.set(res.data);
        } else {
          this.requests.set([]);
          this.loadFailed.set(true);
          this.toast.showFailedApi(res, 'Requests');
        }
        this.initialLoadDone.set(true);
        this.loading.set(false);
      },
      error: () => {
        this.requests.set([]);
        this.loadFailed.set(true);
        this.initialLoadDone.set(true);
        this.loading.set(false);
        this.toast.error('Could not load requests.', 'Event halls');
      },
    });
  }

  onFilterChange(): void {
    this.selectedId.set(null);
    this.detail.set(null);
    this.load();
  }

  openRequest(id: string): void {
    this.selectedId.set(id);
    this.detailLoading.set(true);
    this.api
      .getRequest(id)
      .pipe(finalize(() => this.detailLoading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.detail.set(res.data);
            return;
          }
          this.toast.showFailedApi(res, 'Request');
        },
        error: () => this.toast.error('Could not load request.', 'Event halls'),
      });
  }

  closeDetail(): void {
    this.selectedId.set(null);
    this.detail.set(null);
  }

  updateStatus(status: string): void {
    const id = this.selectedId();
    if (!id) {
      return;
    }
    this.updating.set(true);
    this.api
      .updateRequestStatus(id, status)
      .pipe(finalize(() => this.updating.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.detail.set(res.data);
            this.load();
            this.toast.success(`Request marked as ${status}.`, 'Event halls');
            return;
          }
          this.toast.showFailedApi(res, 'Update status');
        },
        error: () => this.toast.error('Could not update status.', 'Event halls'),
      });
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Approved':
        return 'status status--ok';
      case 'Rejected':
      case 'Cancelled':
        return 'status status--bad';
      default:
        return 'status status--pending';
    }
  }
}
