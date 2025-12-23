import { Component, ChangeDetectionStrategy, inject, CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-procedures',
  templateUrl: './procedures.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, FormsModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class ProceduresComponent {
  private api = inject(ApiService);
  private auth = inject(AuthService);

  exameId?: number;
  prioridade = '';
  status = '';

  isLoading = signal(false);
  message = signal<string | null>(null);
  error = signal<string | null>(null);

  async updatePrioridade() {
    this.isLoading.set(true);
    this.message.set(null);
    this.error.set(null);
    try {
      const role = this.auth.currentUser()?.role;
      if (role !== 'Medico' && role !== 'Admin') { this.error.set('Acesso restrito a médicos ou administradores.'); return; }
      if (!this.exameId) { this.error.set('Informe o ID do exame agendado.'); return; }
      const resp = await this.api.updateProcedimentoPrioridade(this.exameId, this.prioridade, this.auth.currentUser()?.name);
      this.message.set(resp?.message || 'Prioridade atualizada');
    } catch (e: any) {
      this.error.set(e?.error?.message || e?.message || 'Falha ao atualizar prioridade');
    } finally { this.isLoading.set(false); }
  }

  async updateStatus() {
    this.isLoading.set(true);
    this.message.set(null);
    this.error.set(null);
    try {
      const role = this.auth.currentUser()?.role;
      if (role !== 'Medico' && role !== 'Admin') { this.error.set('Acesso restrito a médicos ou administradores.'); return; }
      if (!this.exameId) { this.error.set('Informe o ID do exame agendado.'); return; }
      const resp = await this.api.updateProcedimentoStatus(this.exameId, this.status, this.auth.currentUser()?.name);
      this.message.set(resp?.message || 'Status atualizado');
    } catch (e: any) {
      this.error.set(e?.error?.message || e?.message || 'Falha ao atualizar status');
    } finally { this.isLoading.set(false); }
  }
}
