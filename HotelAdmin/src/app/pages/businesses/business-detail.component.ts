import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { PlatformBusinessDetail } from '../../core/models/platform.models';
import { PlatformApiService } from '../../core/services/platform-api.service';
import { ToastService } from '../../core/services/toast.service';
import { PlatformWorkspaceComponent } from '../../layouts/platform-workspace/platform-workspace.component';

@Component({
  selector: 'app-business-detail',
  standalone: true,
  imports: [RouterLink, DatePipe, ReactiveFormsModule, PlatformWorkspaceComponent],
  templateUrl: './business-detail.component.html',
  styleUrl: './business-detail.component.scss',
})
export class BusinessDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(PlatformApiService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);

  readonly business = signal<PlatformBusinessDetail | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    status: ['Active'],
    adminNotes: [''],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('businessId');
    if (!id) {
      this.loading.set(false);
      return;
    }

    this.api
      .getBusiness(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => {
          if (result.success && result.data) {
            this.business.set(result.data);
            this.form.patchValue({
              status: result.data.status,
              adminNotes: result.data.adminNotes ?? '',
            });
            return;
          }
          this.toast.showFailedApi(result, 'Business');
        },
        error: () => this.toast.error('Could not load business.', 'Business'),
      });
  }

  save(): void {
    const b = this.business();
    if (!b) {
      return;
    }

    this.saving.set(true);
    const { status, adminNotes } = this.form.getRawValue();
    this.api
      .updateBusiness(b.id, { status, adminNotes })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (result) => {
          if (result.success && result.data) {
            this.business.set(result.data);
            this.toast.success('Business updated.');
            return;
          }
          this.toast.showFailedApi(result, 'Update failed');
        },
        error: () => this.toast.error('Could not save changes.', 'Update failed'),
      });
  }
}
