import { ChangeDetectionStrategy, Component, HostListener, input, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import type { PublicStorefront } from '../../core/models/storefront-theme.models';

@Component({
  selector: 'app-hotel-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './hotel-nav.component.html',
  styleUrl: './hotel-nav.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HotelNavComponent {
  readonly storefront = input.required<PublicStorefront>();
  /** When true, nav uses solid background (subpages without hero). */
  readonly solid = input(false);

  readonly scrolled = signal(false);

  @HostListener('window:scroll')
  onScroll(): void {
    this.scrolled.set(window.scrollY > 60);
  }

  isSolid(): boolean {
    return this.solid() || this.scrolled();
  }
}
