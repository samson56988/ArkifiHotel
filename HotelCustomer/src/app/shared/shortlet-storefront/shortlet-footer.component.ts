import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ShortletContextService } from '../../core/services/shortlet-context.service';
import type { ShortletShowcase } from '../../core/models/shortlet-showcase.models';

@Component({
  selector: 'app-shortlet-footer',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './shortlet-footer.component.html',
  styleUrl: './shortlet-footer.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletFooterComponent {
  readonly ctx = inject(ShortletContextService);
  readonly shortlet = input.required<ShortletShowcase>();
  readonly currentYear = new Date().getFullYear();
}
