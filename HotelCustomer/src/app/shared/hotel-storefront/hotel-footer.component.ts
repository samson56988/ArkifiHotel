import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import type { HotelShowcase } from '../../core/models/hotel-showcase.models';

@Component({
  selector: 'app-hotel-footer',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './hotel-footer.component.html',
  styleUrl: './hotel-footer.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HotelFooterComponent {
  private readonly ui = inject(HotelUiService);

  readonly storefront = input.required<HotelShowcase>();

  readonly year = new Date().getFullYear();

  readonly copyright = computed(() => {
    const sf = this.storefront();
    return sf.theme.footer.copyrightText || `© ${this.year} ${sf.businessName}. All rights reserved.`;
  });

  readonly whatsAppLink = computed(() => {
    const link = this.storefront().socialLinks.find((l) => l.platform === 'WhatsApp');
    return link?.url ?? null;
  });

  contactName = '';
  contactEmail = '';
  contactMessage = '';

  submitContact(event: Event): void {
    event.preventDefault();
    this.ui.showToast('Message sent! We will get back to you shortly.');
    this.contactName = '';
    this.contactEmail = '';
    this.contactMessage = '';
  }

  openBooking(): void {
    this.ui.openBooking();
  }
}
