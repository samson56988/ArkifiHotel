import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { ApiResult } from '../../core/models/api-result.model';
import { BusinessCustomersApiService } from '../../core/services/business-customers-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, BusinessWorkspaceComponent],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.scss',
})
export class CustomerFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessCustomersApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(320)]],
    phone: [''],
    notes: [''],
  });

  readonly customerId = signal<string | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);

  get isCreateMode(): boolean {
    return this.customerId() === null;
  }

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      this.initFromRoute(params.get('customerId'));
    });
  }

  private initFromRoute(paramId: string | null): void {
    this.customerId.set(paramId);
    this.form.reset({ fullName: '', email: '', phone: '', notes: '' });

    if (!paramId) {
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.api
      .getCustomer(paramId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (detail) => {
          if (!detail.success || !detail.data) {
            this.toast.error(detail.message ?? 'Customer not found.', 'Customer');
            return;
          }

          const d = detail.data;
          this.form.patchValue({
            fullName: d.fullName,
            email: d.email,
            phone: d.phone ?? '',
            notes: d.notes ?? '',
          });
        },
        error: (err: unknown) => {
          const r = err as ApiResult<unknown>;
          if (r && typeof r === 'object' && 'message' in r) {
            this.toast.showFailedApi(r, 'Customer');
            return;
          }

          this.toast.error('Could not load customer.', 'Customer');
        },
      });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const body = {
      fullName: raw.fullName.trim(),
      email: raw.email.trim(),
      phone: raw.phone.trim() || null,
      notes: raw.notes.trim() || null,
    };

    const id = this.customerId();
    this.saving.set(true);
    const req$ = this.isCreateMode ? this.api.createCustomer(body) : this.api.updateCustomer(id!, body);

    req$.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.toast.showFailedApi(res, 'Customer');
          return;
        }

        if (this.isCreateMode) {
          this.toast.success('Customer created.', 'Customer');
          void this.router.navigate(['/customers'], { replaceUrl: true });
          return;
        }

        this.toast.success('Customer updated.', 'Customer');
        void this.router.navigate(['/customers'], { replaceUrl: true });
      },
      error: (err: unknown) => {
        const r = err as ApiResult<unknown>;
        if (r && typeof r === 'object' && 'message' in r) {
          this.toast.showFailedApi(r, 'Customer');
          return;
        }

        this.toast.error('Request failed.', 'Customer');
      },
    });
  }

  deleteCustomer(): void {
    const id = this.customerId();
    if (!id || this.isCreateMode) {
      return;
    }

    if (!globalThis.confirm('Delete this customer? This cannot be undone.')) {
      return;
    }

    this.saving.set(true);
    this.api
      .deleteCustomer(id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.showFailedApi(res, 'Customer');
            return;
          }

          void this.router.navigateByUrl('/customers');
          this.toast.success('Customer removed.', 'Customer');
        },
        error: (err: unknown) => {
          const r = err as ApiResult<unknown>;
          if (r && typeof r === 'object' && 'message' in r) {
            this.toast.showFailedApi(r, 'Customer');
            return;
          }

          this.toast.error('Could not delete.', 'Customer');
        },
      });
  }
}
