import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ShortletContextService } from '../../core/services/shortlet-context.service';
import type { ShortletHost } from '../../core/models/shortlet-showcase.models';

@Component({
  selector: 'app-shortlet-host-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './shortlet-host-card.component.html',
  styleUrl: './shortlet-host-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletHostCardComponent {
  readonly ctx = inject(ShortletContextService);
  readonly host = input.required<ShortletHost>();
  readonly compact = input(false);
}
