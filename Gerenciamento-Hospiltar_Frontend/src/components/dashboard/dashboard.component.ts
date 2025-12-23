
import { Component, ChangeDetectionStrategy, inject, computed, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class DashboardComponent {
  private authService = inject(AuthService);
  currentUser = this.authService.currentUser;

  welcomeMessage = computed(() => {
    const role = this.currentUser()?.role;
    switch (role) {
      case 'Admin':
        return 'Você tem controle total sobre o sistema.';
      case 'Medico':
        return 'Gerencie suas consultas e pacientes.';
      case 'Usuario':
        return 'Agende e visualize suas consultas e exames.';
      default:
        return '';
    }
  });
}
