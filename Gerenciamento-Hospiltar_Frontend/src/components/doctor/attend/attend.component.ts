import { Component, ChangeDetectionStrategy, inject, CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../services/api.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-attend',
  templateUrl: './attend.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, FormsModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AttendComponent {
  private api = inject(ApiService);
  private auth = inject(AuthService);

  mode = signal<'horario'|'codigo'>('horario');
  pacienteCPF = '';
  horario = '';
  consultaId?: number;
  sintomasText = '';

  isLoading = signal(false);
  result = signal<any | null>(null);
  error = signal<string | null>(null);

  async submit() {
    this.isLoading.set(true);
    this.error.set(null);
    this.result.set(null);
    const sintomas = this.sintomasText.split(',').map(s => s.trim()).filter(Boolean);
    try {
      const role = this.auth.currentUser()?.role;
      if (role !== 'Medico' && role !== 'Admin') {
        this.error.set('Acesso restrito a médicos ou administradores.');
        return;
      }
      let resp: any;
      if (this.mode() === 'horario') {
        resp = await this.api.attendByHorario({ pacienteCPF: this.pacienteCPF, horario: this.horario, sintomas });
      } else {
        if (!this.consultaId) { this.error.set('Informe o código da consulta.'); return; }
        resp = await this.api.attendByCodigo({ pacienteCPF: this.pacienteCPF, consultaId: this.consultaId, sintomas });
      }
      this.result.set(resp || {});
    } catch (e: any) {
      const msg = e?.error?.mensagem || e?.error?.message || e?.message || 'Falha no atendimento';
      this.error.set(msg);
    } finally {
      this.isLoading.set(false);
    }
  }
}
