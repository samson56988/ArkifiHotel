import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { HotelFooterComponent } from '../../shared/hotel-storefront/hotel-footer.component';
import { RoomCardComponent } from '../../shared/hotel-storefront/room-card.component';

@Component({
  selector: 'app-storefront-rooms',
  standalone: true,
  imports: [RouterLink, RoomCardComponent, HotelFooterComponent],
  templateUrl: './storefront-rooms.component.html',
  styleUrl: './storefront-rooms.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontRoomsComponent {
  readonly ctx = inject(StorefrontContextService);

  readonly storefront = computed(() => this.ctx.storefront()!);

  readonly activeFilter = signal('All');

  readonly locationFilters = computed(() => {
    const names = this.storefront()
      .rooms.map((r) => r.locationName)
      .filter((n): n is string => !!n);
    return ['All', ...new Set(names)];
  });

  readonly filteredRooms = computed(() => {
    const filter = this.activeFilter();
    const rooms = this.storefront().rooms;
    if (filter === 'All') {
      return rooms;
    }
    return rooms.filter((r) => r.locationName === filter);
  });

  setFilter(filter: string): void {
    this.activeFilter.set(filter);
  }
}
