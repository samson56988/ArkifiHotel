import { ChangeDetectionStrategy, Component, HostListener, inject, input, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import { ShortletContextService } from '../../core/services/shortlet-context.service';
import type { ShortletShowcase } from '../../core/models/shortlet-showcase.models';

@Component({
  selector: 'app-shortlet-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './shortlet-nav.component.html',
  styleUrl: './shortlet-nav.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletNavComponent {
  private readonly ui = inject(HotelUiService);
  readonly ctx = inject(ShortletContextService);

  readonly shortlet = input.required<ShortletShowcase>();
  readonly transparent = input(true);

  readonly scrolled = signal(false);
  readonly menuOpen = signal(false);

  @HostListener('window:scroll')
  onScroll(): void {
    this.scrolled.set(window.scrollY > 40);
  }

  isSolid(): boolean {
    return !this.transparent() || this.scrolled();
  }

  toggleMenu(): void {
    this.menuOpen.update((v) => !v);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }

  openBooking(event: Event): void {
    event.preventDefault();
    this.ui.openBooking();
    this.closeMenu();
  }
}
