
import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
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
export class ScheduleComponent implements OnInit {
  private apiService = inject(ApiService);
  private notificationService = inject(NotificationService);
  private authService = inject(AuthService);

  activeTab = signal<'consulta' | 'exame'>('consulta');
  isLoading = signal(false);
  medicos = signal<Array<{ IdMedico: number; Nome: string; CRM: string; Especialidade: string }>>([]);
  selectedMedicoId = signal<number | null>(null);
  
  currentUser = this.authService.currentUser;

  model = {
    paciente: {
      nome: this.currentUser()?.name || '',
      cpf: this.currentUser()?.cpf || '',
      idade: 30,
      sexo: 'Masculino'
    },
    horario: '',
    medico: '',
    exame: ''
  };

  async ngOnInit(): Promise<void> {
    await this.loadMedicos();
  }

  selectTab(tab: 'consulta' | 'exame'): void {
    this.activeTab.set(tab);
  }

  async loadMedicos(): Promise<void> {
    try {
      const list = await this.apiService.listMedicos();
      this.medicos.set(list);
      if (list.length > 0) {
        this.selectedMedicoId.set(list[0].IdMedico);
        this.model.medico = `${list[0].Nome} (${list[0].Especialidade})`;
      }
    } catch (error) {
      this.notificationService.show('Não foi possível carregar a lista de médicos.', 'error');
    }
  }

  onMedicoChange(idValue: string): void {
    const id = Number(idValue);
    this.selectedMedicoId.set(isNaN(id) ? null : id);
    const med = this.medicos().find(m => m.IdMedico === id);
    if (med) {
      this.model.medico = `${med.Nome} (${med.Especialidade})`;
    }
  }

  async onSubmit(): Promise<void> {
    this.isLoading.set(true);
    const horarioIso = this.model.horario ? new Date(this.model.horario).toISOString() : '';
    if (!horarioIso) {
      this.notificationService.show('Informe a data e hora.', 'error');
      this.isLoading.set(false);
      return;
    }

    if (this.activeTab() === 'consulta') {
      const medicoId = this.selectedMedicoId();
      if (!medicoId) {
        this.notificationService.show('Selecione um médico.', 'error');
        this.isLoading.set(false);
        return;
      }
      const especialidade = this.medicos().find(m => m.IdMedico === medicoId)?.Especialidade;
      if (especialidade) {
        try {
          await this.apiService.checkDisponibilidade(especialidade, horarioIso, medicoId);
        } catch (e: any) {
          const msg = e?.error?.mensagem || 'Horário indisponível para este médico/especialidade.';
          this.notificationService.show(msg, 'error');
          this.isLoading.set(false);
          return;
        }
      }
    }

    const payload = {
        paciente: this.model.paciente,
        horario: horarioIso,
        ...(this.activeTab() === 'consulta' && { medico: this.model.medico }),
        ...(this.activeTab() === 'exame' && { exame: this.model.exame })
    };

    try {
        const response = await this.apiService.schedule(payload);
        if (response.success) {
            this.notificationService.show(response.message, 'success');
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
