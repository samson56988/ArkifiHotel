import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { formatNaira } from '../../core/utils/hotel-theme';
import type { ShowcaseRoom } from '../../core/models/hotel-showcase.models';
import { HotelFooterComponent } from '../../shared/hotel-storefront/hotel-footer.component';
import { RoomCardComponent } from '../../shared/hotel-storefront/room-card.component';

type SortOption = 'price-asc' | 'price-desc' | 'name';

@Component({
  selector: 'app-storefront-rooms',
  standalone: true,
  imports: [RouterLink, RoomCardComponent, HotelFooterComponent],
  templateUrl: './storefront-rooms.component.html',
  styleUrl: './storefront-rooms.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontRoomsComponent {
  private readonly ui = inject(HotelUiService);
  readonly ctx = inject(StorefrontContextService);

  readonly storefront = computed(() => this.ctx.storefront()!);

  readonly locationFilter = signal('All');
  readonly typeFilter = signal('All');
  readonly sortBy = signal<SortOption>('price-asc');

  readonly heroImage = computed(() => {
    const sf = this.storefront();
    return sf.heroImages[0] ?? sf.galleryImages[0] ?? null;
  });

  readonly featuredRoom = computed(() => {
    const rooms = this.storefront().rooms;
    return rooms.find((r) => r.featured) ?? rooms.find((r) => r.available) ?? rooms[0] ?? null;
  });

  readonly locationFilters = computed(() => {
    const names = this.storefront()
      .rooms.map((r) => r.locationName)
      .filter((n): n is string => !!n);
    return ['All', ...new Set(names)];
  });

  readonly typeFilters = computed(() => {
    const types = this.storefront().rooms.map((r) => r.roomType);
    return ['All', ...new Set(types)];
  });

  readonly stats = computed(() => {
    const rooms = this.storefront().rooms;
    const available = rooms.filter((r) => r.available).length;
    const prices = rooms.map((r) => r.basePricePerNight);
    const minPrice = prices.length ? Math.min(...prices) : 0;
    return { total: rooms.length, available, minPrice };
  });

  readonly filteredRooms = computed(() => {
    const loc = this.locationFilter();
    const type = this.typeFilter();
    const sort = this.sortBy();
    const featuredId = this.featuredRoom()?.id;

    let rooms = this.storefront().rooms.filter((r) => r.id !== featuredId);

    if (loc !== 'All') {
      rooms = rooms.filter((r) => r.locationName === loc);
    }
    if (type !== 'All') {
      rooms = rooms.filter((r) => r.roomType === type);
    }

    return this.sortRooms(rooms, sort);
  });

  readonly hasActiveFilters = computed(
    () => this.locationFilter() !== 'All' || this.typeFilter() !== 'All',
  );

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }

  setLocationFilter(filter: string): void {
    this.locationFilter.set(filter);
  }

  setTypeFilter(filter: string): void {
    this.typeFilter.set(filter);
  }

  setSort(value: SortOption): void {
    this.sortBy.set(value);
  }

  clearFilters(): void {
    this.locationFilter.set('All');
    this.typeFilter.set('All');
  }

  openBooking(room?: ShowcaseRoom | null): void {
    this.ui.openBooking(room ?? null);
  }

  private sortRooms(rooms: ShowcaseRoom[], sort: SortOption): ShowcaseRoom[] {
    const copy = [...rooms];
    switch (sort) {
      case 'price-desc':
        return copy.sort((a, b) => b.basePricePerNight - a.basePricePerNight);
      case 'name':
        return copy.sort((a, b) => a.name.localeCompare(b.name));
      default:
        return copy.sort((a, b) => a.basePricePerNight - b.basePricePerNight);
    }
  }
}
