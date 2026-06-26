import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import type { ShowcaseMenuItem } from '../../core/models/restaurant.models';
import { PublicRestaurantOrderApiService } from '../../core/services/public-restaurant-order-api.service';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { formatNaira } from '../../core/utils/hotel-theme';
import { HotelFooterComponent } from '../../shared/hotel-storefront/hotel-footer.component';

type MenuTab = 'food' | 'drinks';
type GuestType = 'inRestaurant' | 'roomGuest';

interface CartLine {
  itemId: string;
  name: string;
  price: number;
  quantity: number;
}

@Component({
  selector: 'app-storefront-restaurant',
  standalone: true,
  imports: [RouterLink, FormsModule, HotelFooterComponent],
  templateUrl: './storefront-restaurant.component.html',
  styleUrl: './storefront-restaurant.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontRestaurantComponent {
  readonly ctx = inject(StorefrontContextService);
  private readonly orderApi = inject(PublicRestaurantOrderApiService);
  readonly formatNaira = formatNaira;

  readonly storefront = computed(() => this.ctx.storefront()!);
  readonly restaurant = computed(() => this.storefront().restaurant);

  readonly activeTab = signal<MenuTab>('food');
  readonly activeCategoryId = signal<string | null>(null);
  readonly cart = signal<Map<string, CartLine>>(new Map());
  readonly checkoutOpen = signal(false);
  readonly guestType = signal<GuestType>('inRestaurant');
  readonly roomNumber = signal('');
  readonly guestPhone = signal('');
  readonly guestEmail = signal('');
  readonly checkoutError = signal<string | null>(null);
  readonly submitting = signal(false);

  readonly categories = computed((): import('../../core/models/restaurant.models').ShowcaseMenuCategory[] => {
    const menu = this.restaurant();
    if (!menu) {
      return [];
    }
    return this.activeTab() === 'food' ? menu.foodCategories : menu.drinkCategories;
  });

  readonly activeCategory = computed(() => {
    const cats = this.categories();
    if (cats.length === 0) {
      return null;
    }
    const id = this.activeCategoryId();
    return cats.find((c) => c.id === id) ?? cats[0];
  });

  readonly visibleItems = computed(() => this.activeCategory()?.items ?? []);

  readonly cartLines = computed(() => Array.from(this.cart().values()));
  readonly cartCount = computed(() => this.cartLines().reduce((sum, line) => sum + line.quantity, 0));
  readonly cartTotal = computed(() =>
    this.cartLines().reduce((sum, line) => sum + line.price * line.quantity, 0),
  );

  setTab(tab: MenuTab): void {
    this.activeTab.set(tab);
    const cats = tab === 'food' ? this.restaurant()?.foodCategories : this.restaurant()?.drinkCategories;
    this.activeCategoryId.set(cats?.[0]?.id ?? null);
  }

  selectCategory(categoryId: string): void {
    this.activeCategoryId.set(categoryId);
  }

  quantityFor(itemId: string): number {
    return this.cart().get(itemId)?.quantity ?? 0;
  }

  addToCart(item: ShowcaseMenuItem): void {
    this.cart.update((current) => {
      const next = new Map(current);
      const existing = next.get(item.id);
      if (existing) {
        next.set(item.id, { ...existing, quantity: existing.quantity + 1 });
      } else {
        next.set(item.id, {
          itemId: item.id,
          name: item.name,
          price: item.price,
          quantity: 1,
        });
      }
      return next;
    });
  }

  decrementItem(itemId: string): void {
    this.cart.update((current) => {
      const next = new Map(current);
      const existing = next.get(itemId);
      if (!existing) {
        return next;
      }
      if (existing.quantity <= 1) {
        next.delete(itemId);
      } else {
        next.set(itemId, { ...existing, quantity: existing.quantity - 1 });
      }
      return next;
    });
  }

  openCheckout(): void {
    if (this.cartCount() === 0) {
      return;
    }
    this.checkoutError.set(null);
    this.checkoutOpen.set(true);
  }

  closeCheckout(): void {
    if (this.submitting()) {
      return;
    }
    this.checkoutOpen.set(false);
  }

  submitOrder(): void {
    const sf = this.storefront();
    const locationId = sf.activeLocationId;
    if (!locationId) {
      this.checkoutError.set('Select a hotel branch before ordering.');
      return;
    }

    const phone = this.normalizePhone(this.guestPhone());
    if (!phone) {
      this.checkoutError.set('Enter your phone number (e.g. +2348012345678 or 08012345678).');
      return;
    }

    const email = this.guestEmail().trim().toLowerCase();
    if (!email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      this.checkoutError.set('Enter a valid email address for your order confirmation.');
      return;
    }

    if (this.guestType() === 'roomGuest' && !this.roomNumber().trim()) {
      this.checkoutError.set('Enter your room number.');
      return;
    }

    this.submitting.set(true);
    this.checkoutError.set(null);

    this.orderApi
      .createCheckout(sf.slug, {
        locationId,
        guestType: this.guestType(),
        roomNumber: this.guestType() === 'roomGuest' ? this.roomNumber().trim() : null,
        guestPhone: phone,
        guestEmail: email,
        items: this.cartLines().map((line) => ({
          menuItemId: line.itemId,
          quantity: line.quantity,
        })),
      })
      .subscribe({
        next: (res) => {
          this.submitting.set(false);
          if (res.success && res.data?.paymentUrl) {
            window.location.href = res.data.paymentUrl;
            return;
          }
          this.checkoutError.set(res.message ?? 'Could not start payment. Try again.');
        },
        error: (err: unknown) => {
          this.submitting.set(false);
          const apiErr = err as { message?: string };
          this.checkoutError.set(apiErr?.message ?? 'Could not place order. Try again.');
        },
      });
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
