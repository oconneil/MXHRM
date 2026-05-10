import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth';
import { GlobalErrorAlert } from './core/components/global-error-alert/global-error-alert';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, GlobalErrorAlert],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly router = inject(Router);

  authService = inject(AuthService);

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}