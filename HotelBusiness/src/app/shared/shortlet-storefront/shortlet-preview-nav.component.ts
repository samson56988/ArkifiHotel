import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import type { ShortletShowcase } from '../../core/models/shortlet-showcase.models';
import type { ShortletPreviewPage } from '../../core/utils/shortlet-preview.mapper';

@Component({
  selector: 'app-shortlet-preview-nav',
  standalone: true,
  templateUrl: './shortlet-preview-nav.component.html',
  styleUrl: './shortlet-preview-nav.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletPreviewNavComponent {
  readonly shortlet = input.required<ShortletShowcase>();
  readonly page = input.required<ShortletPreviewPage>();
}
