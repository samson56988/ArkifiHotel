import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { PlatformDashboardStats } from '../../core/models/platform.models';
import { PlatformApiService } from '../../core/services/platform-api.service';
import { ToastService } from '../../core/services/toast.service';
import { PlatformWorkspaceComponent } from '../../layouts/platform-workspace/platform-workspace.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, PlatformWorkspaceComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly api = inject(PlatformApiService);
  private readonly toast = inject(ToastService);

  readonly stats = signal<PlatformDashboardStats | null>(null);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.api
      .getDashboard()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => {
          if (result.success && result.data) {
            this.stats.set(result.data);
            return;
          }
          this.toast.showFailedApi(result, 'Dashboard');
        },
        error: () => this.toast.error('Could not load dashboard.', 'Dashboard'),
      });
  }
}
