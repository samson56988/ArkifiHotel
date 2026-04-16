import { DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import type { ApiResult } from '../../core/models/api-result.model';
import { PublicBookingApiService, type GuestBookingLookupDto } from '../../core/services/public-booking-api.service';

@Component({
  selector: 'app-booking-lookup',
  standalone: true,
  imports: [ReactiveFormsModule, DecimalPipe],
  templateUrl: './booking-lookup.component.html',
  styleUrl: './booking-lookup.component.scss',
})
export class BookingLookupComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(PublicBookingApiService);

  readonly form = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.minLength(4)]],
  });

  loading = false;
  result: GuestBookingLookupDto | null = null;
  errorMessage: string | null = null;

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const code = this.form.controls.code.value.trim();
    this.loading = true;
    this.result = null;
    this.errorMessage = null;

    this.api
      .lookupByCode(code)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.result = res.data;
            return;
          }

          this.errorMessage = res.message ?? 'Booking not found.';
        },
        error: (err: unknown) => {
          const r = err as ApiResult<GuestBookingLookupDto>;
          this.errorMessage = r?.message ?? 'Could not look up booking.';
        },
      });
  }

  formatDate(isoDate: string): string {
    if (!isoDate) {
      return '';
    }

    const d = new Date(isoDate + 'T12:00:00');
    return d.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
