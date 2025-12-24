import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-medico-register',
  templateUrl: './medico-register.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class MedicoRegisterComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);

  isLoading = signal(false);
  currentUser = this.authService.currentUser;

  form = {
    nome: this.currentUser()?.name || '',
    crm: '',
    especialidade: ''
  };

  ngOnInit(): void {
    const user = this.currentUser();
    if (!user) {
      this.notificationService.show('Você precisa estar autenticado.', 'error');
      this.router.navigate(['/login']);
      return;
    }
    if (user.role !== 'Medico') {
      this.notificationService.show('Apenas médicos podem completar este registro.', 'error');
      this.router.navigate(['/dashboard']);
      return;
    }
  }

  async onSubmit(): Promise<void> {
    if (!this.form.nome || !this.form.crm || !this.form.especialidade) {
      this.notificationService.show('Nome, CRM e Especialidade são obrigatórios.', 'error');
      return;
    }

    this.isLoading.set(true);
    try {
      const payload = {
        Nome: this.form.nome,
        CRM: this.form.crm,
        Especialidade: this.form.especialidade
      };

      const response = await this.apiService.registerMedico(payload);
      if (response) {
        this.authService.markRegistrationAsComplete();
        this.notificationService.show('Registro de médico concluído com sucesso!', 'success');
        setTimeout(() => this.router.navigate(['/dashboard']), 2000);
      }
    } catch (error: any) {
      const msg = error?.error?.mensagem || error?.message || 'Erro ao registrar médico.';
      this.notificationService.show(msg, 'error');
    } finally {
      this.isLoading.set(false);
    }
  }
}
