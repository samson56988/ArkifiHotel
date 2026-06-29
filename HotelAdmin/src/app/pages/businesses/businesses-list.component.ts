import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { PlatformBusinessSummary } from '../../core/models/platform.models';
import { PlatformApiService } from '../../core/services/platform-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-businesses-list',
  standalone: true,
  imports: [RouterLink, DatePipe],
  templateUrl: './businesses-list.component.html',
  styleUrl: './businesses-list.component.scss',
})
export class BusinessesListComponent implements OnInit {
  private readonly api = inject(PlatformApiService);
  private readonly toast = inject(ToastService);

  readonly businesses = signal<PlatformBusinessSummary[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.api
      .listBusinesses()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => {
          if (result.success && result.data) {
            this.businesses.set(result.data);
            return;
          }
          this.toast.showFailedApi(result, 'Businesses');
        },
        error: () => this.toast.error('Could not load businesses.', 'Businesses'),
      });
  }
}
