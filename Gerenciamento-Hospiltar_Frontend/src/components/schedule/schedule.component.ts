
import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { NotificationService } from '../../services/notification.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-schedule',
  templateUrl: './schedule.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class ScheduleComponent {
  private apiService = inject(ApiService);
  private notificationService = inject(NotificationService);
  private authService = inject(AuthService);

  activeTab = signal<'consulta' | 'exame'>('consulta');
  isLoading = signal(false);
  
  currentUser = this.authService.currentUser;

  model = {
    paciente: {
      nome: this.currentUser()?.name || '',
      cpf: this.currentUser()?.cpf || '',
      idade: 30,
      sexo: 'Masculino'
    },
    horario: '',
    medico: 'Pediatria', // specialty
    exame: 'Coleta de sangue' // exam type
  };

  selectTab(tab: 'consulta' | 'exame'): void {
    this.activeTab.set(tab);
  }

  async onSubmit(): Promise<void> {
    this.isLoading.set(true);
    const payload = {
        paciente: this.model.paciente,
        horario: this.model.horario,
        ...(this.activeTab() === 'consulta' && { medico: this.model.medico }),
        ...(this.activeTab() === 'exame' && { exame: this.model.exame })
    };

    try {
        const response = await this.apiService.schedule(payload);
        if (response.success) {
            this.notificationService.show(response.message, 'success');
            // Reset form could go here
        } else {
            this.notificationService.show(response.message, 'error');
        }
    } catch (error) {
        this.notificationService.show('Ocorreu um erro inesperado.', 'error');
    } finally {
        this.isLoading.set(false);
    }
  }
}
