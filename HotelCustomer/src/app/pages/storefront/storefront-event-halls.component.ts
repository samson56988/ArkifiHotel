import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DEMO_HOTEL_SLUGS } from '../../core/data/mock-hotels.data';
import type { ShowcaseEventHall } from '../../core/models/event-hall.models';
import { PublicEventHallApiService } from '../../core/services/public-event-hall-api.service';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { formatNaira } from '../../core/utils/hotel-theme';
import { HotelFooterComponent } from '../../shared/hotel-storefront/hotel-footer.component';

@Component({
  selector: 'app-storefront-event-halls',
  standalone: true,
  imports: [RouterLink, FormsModule, HotelFooterComponent],
  templateUrl: './storefront-event-halls.component.html',
  styleUrl: './storefront-event-halls.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontEventHallsComponent {
  readonly ctx = inject(StorefrontContextService);
  private readonly eventHallApi = inject(PublicEventHallApiService);
  readonly formatNaira = formatNaira;

  readonly storefront = computed(() => this.ctx.storefront()!);
  readonly page = computed(() => this.storefront().eventHallsPage);
  readonly halls = computed(() => this.storefront().eventHalls);

  readonly requestOpen = signal(false);
  readonly selectedHall = signal<ShowcaseEventHall | null>(null);
  readonly guestName = signal('');
  readonly guestEmail = signal('');
  readonly guestPhone = signal('');
  readonly eventDate = signal('');
  readonly eventEndDate = signal('');
  readonly eventPurpose = signal('');
  readonly eventPurposeOther = signal('');
  readonly notes = signal('');
  readonly formError = signal<string | null>(null);
  readonly submitting = signal(false);
  readonly successMessage = signal<string | null>(null);

  readonly eventPurposeOptions = [
    'Wedding',
    'Conference / Meeting',
    'Birthday party',
    'Corporate event',
    'Religious ceremony',
    'Social gathering',
    'Other',
  ] as const;

  openRequest(hall: ShowcaseEventHall): void {
    this.selectedHall.set(hall);
    this.formError.set(null);
    this.successMessage.set(null);
    this.requestOpen.set(true);
  }

  closeRequest(): void {
    if (this.submitting()) {
      return;
    }
    this.requestOpen.set(false);
    this.selectedHall.set(null);
  }

  submitRequest(): void {
    const sf = this.storefront();
    const hall = this.selectedHall();
    const locationId = sf.activeLocationId;

    if (!hall || !locationId) {
      this.formError.set('Select a hotel branch before submitting a request.');
      return;
    }

    const name = this.guestName().trim();
    const email = this.guestEmail().trim();
    const phone = this.normalizePhone(this.guestPhone());
    const start = this.eventDate().trim();

    if (name.length < 2) {
      this.formError.set('Enter your full name.');
      return;
    }
    if (!email.includes('@')) {
      this.formError.set('Enter a valid email address.');
      return;
    }
    if (!phone) {
      this.formError.set('Enter your phone number (e.g. +2348012345678 or 08012345678).');
      return;
    }
    if (!start) {
      this.formError.set('Select your event date.');
      return;
    }

    const purpose = this.resolveEventPurpose();
    if (!purpose) {
      this.formError.set('Select or describe the purpose of your event.');
      return;
    }

    const end = this.eventEndDate().trim();
    if (end && end < start) {
      this.formError.set('End date cannot be before start date.');
      return;
    }

    this.submitting.set(true);
    this.formError.set(null);

    if (DEMO_HOTEL_SLUGS.includes(sf.slug)) {
      setTimeout(() => {
        this.submitting.set(false);
        this.successMessage.set(
          'Your request has been submitted. Our events team will review availability and contact you shortly — no payment is required at this stage.',
        );
        this.guestName.set('');
        this.guestEmail.set('');
        this.guestPhone.set('');
        this.eventDate.set('');
        this.eventEndDate.set('');
        this.eventPurpose.set('');
        this.eventPurposeOther.set('');
        this.notes.set('');
      }, 600);
      return;
    }

    this.eventHallApi
      .createRequest(sf.slug, {
        locationId,
        eventHallId: hall.id,
        guestName: name,
        guestEmail: email,
        guestPhone: phone,
        eventDate: start,
        eventEndDate: end || null,
        eventPurpose: purpose,
        notes: this.notes().trim() || null,
      })
      .subscribe({
        next: (res) => {
          this.submitting.set(false);
          if (res.success && res.data) {
            this.successMessage.set(res.data.message);
            this.guestName.set('');
            this.guestEmail.set('');
            this.guestPhone.set('');
            this.eventDate.set('');
            this.eventEndDate.set('');
            this.notes.set('');
            return;
          }
          this.formError.set(res.message ?? 'Could not submit request. Try again.');
        },
        error: (err: unknown) => {
          this.submitting.set(false);
          const apiErr = err as { message?: string };
          this.formError.set(apiErr?.message ?? 'Could not submit request. Try again.');
        },
      });
  }

  capacityLabel(hall: ShowcaseEventHall): string {
    if (!hall.maxCapacity) {
      return 'Capacity on request';
    }
    return `Up to ${hall.maxCapacity} guests`;
  }

  resolveEventPurpose(): string {
    const selected = this.eventPurpose().trim();
    if (!selected) {
      return '';
    }
    if (selected === 'Other') {
      return this.eventPurposeOther().trim();
    }
    return selected;
  }

  private normalizePhone(raw: string): string | null {
    let phone = raw.trim().replace(/\s/g, '');
    if (!phone.startsWith('+')) {
      if (phone.startsWith('234') && phone.length >= 12) {
        phone = `+${phone}`;
      } else if (phone.startsWith('0') && phone.length >= 10) {
        phone = `+234${phone.slice(1)}`;
      }
    }

    if (!phone.startsWith('+') || phone.length < 8) {
      return null;
    }

    return phone;
  }
}
