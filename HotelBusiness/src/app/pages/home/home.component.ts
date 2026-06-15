import { ChangeDetectionStrategy, Component, HostListener, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {
  readonly year = new Date().getFullYear();
  readonly navScrolled = signal(false);

  @HostListener('window:scroll')
  onWindowScroll(): void {
    this.navScrolled.set(window.scrollY > 60);
  }
}
