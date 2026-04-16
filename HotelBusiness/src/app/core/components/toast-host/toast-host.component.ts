import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast-host',
  standalone: true,
  templateUrl: './toast-host.component.html',
  styleUrl: './toast-host.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastHostComponent {
  readonly toast = inject(ToastService);

  ariaLive(variant: string): 'assertive' | 'polite' {
    return variant === 'error' || variant === 'warning' ? 'assertive' : 'polite';
  }
}
