import { ChangeDetectionStrategy, Component, HostListener, inject, input, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import type { HotelShowcase } from '../../core/models/hotel-showcase.models';

@Component({
  selector: 'app-hotel-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './hotel-nav.component.html',
  styleUrl: './hotel-nav.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HotelNavComponent {
  private readonly ui = inject(HotelUiService);

  readonly storefront = input.required<HotelShowcase>();
  readonly solid = input(false);

  readonly scrolled = signal(false);

  @HostListener('window:scroll')
  onScroll(): void {
    this.scrolled.set(window.scrollY > 60);
  }

  isSolid(): boolean {
    return this.solid() || this.scrolled();
  }

  openBooking(event: Event): void {
    event.preventDefault();
    this.ui.openBooking();
  }
}
