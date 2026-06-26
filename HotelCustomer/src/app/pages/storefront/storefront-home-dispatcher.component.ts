import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { StorefrontEntryService } from '../../core/services/storefront-entry.service';
import { StorefrontHomeComponent } from '../storefront/storefront-home.component';
import { ShortletHomeComponent } from '../shortlet/shortlet-home.component';

@Component({
  selector: 'app-storefront-home-dispatcher',
  standalone: true,
  imports: [StorefrontHomeComponent, ShortletHomeComponent],
  template: `
    @if (entry.kind() === 'shortlet') {
      <app-shortlet-home />
    } @else {
      <app-storefront-home />
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontHomeDispatcherComponent {
  readonly entry = inject(StorefrontEntryService);
}
