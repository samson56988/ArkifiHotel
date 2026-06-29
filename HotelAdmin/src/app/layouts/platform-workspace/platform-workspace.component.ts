import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthApiService } from '../../core/services/auth-api.service';

@Component({
  selector: 'app-platform-workspace',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './platform-workspace.component.html',
  styleUrl: './platform-workspace.component.scss',
})
export class PlatformWorkspaceComponent {
  private readonly auth = inject(AuthApiService);
  private readonly router = inject(Router);

  readonly account = this.auth.getAccount();

  signOut(): void {
    this.auth.logout().subscribe({
      next: () => void this.router.navigateByUrl('/login'),
      error: () => void this.router.navigateByUrl('/login'),
    });
  }
}
