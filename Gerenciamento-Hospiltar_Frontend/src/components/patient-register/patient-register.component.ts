import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-patient-register',
  templateUrl: './patient-register.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class PatientRegisterComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);

  isLoading = signal(false);
  currentUser = this.authService.currentUser;

  form = {
    nome: this.currentUser()?.name || '',
    cpf: this.currentUser()?.cpf || '',
    idade: 30,
    sexo: 'Masculino',
    dataNascimento: '',
    telefone: '',
    email: this.currentUser()?.email || ''
  };

  ngOnInit(): void {
    const user = this.currentUser();
    if (!user) {
      this.notificationService.show('Você precisa estar autenticado.', 'error');
      this.router.navigate(['/login']);
      return;
    }
    if (user.role !== 'Usuario') {
      this.notificationService.show('Apenas usuários podem completar este registro.', 'error');
      this.router.navigate(['/dashboard']);
      return;
    }
  }

  async onSubmit(): Promise<void> {
    if (!this.form.nome || !this.form.cpf) {
      this.notificationService.show('Nome e CPF são obrigatórios.', 'error');
      return;
    }

    this.isLoading.set(true);
    try {
      const payload = {
        Nome: this.form.nome,
        CPF: this.form.cpf,
        Idade: this.form.idade,
        Sexo: this.form.sexo,
        DataNascimento: this.form.dataNascimento ? new Date(this.form.dataNascimento).toISOString() : null,
        Telefone: this.form.telefone || null,
        Email: this.form.email || null
      };

      const response = await this.apiService.registerPatient(payload);
      if (response) {
        this.authService.markRegistrationAsComplete();
        this.notificationService.show('Registro de paciente concluído com sucesso!', 'success');
        setTimeout(() => this.router.navigate(['/dashboard']), 2000);
      }
    } catch (error: any) {
      const msg = error?.error?.mensagem || error?.message || 'Erro ao registrar paciente.';
      this.notificationService.show(msg, 'error');
    } finally {
      this.isLoading.set(false);
    }
  }
}
