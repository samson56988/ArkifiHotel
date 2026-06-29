import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { EventHallSummaryDto } from '../../core/models/event-hall.models';
import { BusinessEventHallsApiService } from '../../core/services/business-event-halls-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-event-halls-list',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
  templateUrl: './event-halls-list.component.html',
  styleUrl: './event-halls-list.component.scss',
})
export class EventHallsListComponent implements OnInit {
  private readonly api = inject(BusinessEventHallsApiService);
  private readonly toast = inject(ToastService);

  readonly halls = signal<EventHallSummaryDto[]>([]);
  readonly initialLoadDone = signal(false);
  readonly loadFailed = signal(false);
  readonly loading = signal(false);
  showArchived = false;

  ngOnInit(): void {
    this.load();
  }

  onShowArchivedChange(event: Event): void {
    this.showArchived = (event.target as HTMLInputElement).checked;
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);
    this.api.list(this.showArchived).subscribe({
      next: (res) => {
        if (res.success) {
          this.halls.set(Array.isArray(res.data) ? res.data : []);
        } else {
          this.halls.set([]);
          this.loadFailed.set(true);
          this.toast.showFailedApi(res, 'Event halls');
        }
        this.initialLoadDone.set(true);
        this.loading.set(false);
      },
      error: () => {
        this.halls.set([]);
        this.loadFailed.set(true);
        this.initialLoadDone.set(true);
        this.loading.set(false);
        this.toast.error('Could not load event halls.', 'Event halls');
      },
    });
  }

  imageUrl(path: string | null | undefined): string {
    return path ? this.api.resolveImageUrl(path) : '';
  }

  formatPrice(amount: number): string {
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: 'NGN',
      maximumFractionDigits: 0,
    }).format(amount);
  }
}
