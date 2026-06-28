import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { PlatformActivityLog } from '../../core/models/platform.models';
import { PlatformApiService } from '../../core/services/platform-api.service';
import { ToastService } from '../../core/services/toast.service';
import { PlatformWorkspaceComponent } from '../../layouts/platform-workspace/platform-workspace.component';

@Component({
  selector: 'app-activity',
  standalone: true,
  imports: [RouterLink, DatePipe, PlatformWorkspaceComponent],
  templateUrl: './activity.component.html',
  styleUrl: './activity.component.scss',
})
export class ActivityComponent implements OnInit {
  private readonly api = inject(PlatformApiService);
  private readonly toast = inject(ToastService);

  readonly logs = signal<PlatformActivityLog[]>([]);
  readonly loading = signal(true);
  readonly totalCount = signal(0);

  ngOnInit(): void {
    this.api
      .listActivity({ page: 1, pageSize: 50 })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => {
          if (result.success && result.data) {
            this.logs.set(result.data.items);
            this.totalCount.set(result.data.totalCount);
            return;
          }
          this.toast.showFailedApi(result, 'Activity');
        },
        error: () => this.toast.error('Could not load activity.', 'Activity'),
      });
  }
}
