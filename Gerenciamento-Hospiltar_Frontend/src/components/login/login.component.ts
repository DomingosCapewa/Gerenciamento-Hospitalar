import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule],
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router: Router = inject(Router);

  loginAs(role: 'Usuário' | 'Médico' | 'Admin'): void {
    this.authService.login(role);
    this.router.navigate(['/dashboard']);
  }
  
}