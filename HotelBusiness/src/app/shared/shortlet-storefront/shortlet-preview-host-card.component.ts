import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import type { ShortletHost } from '../../core/models/shortlet-showcase.models';

@Component({
  selector: 'app-shortlet-preview-host-card',
  standalone: true,
  templateUrl: './shortlet-preview-host-card.component.html',
  styleUrl: './shortlet-preview-host-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletPreviewHostCardComponent {
  readonly host = input.required<ShortletHost>();
  readonly compact = input(false);
}
