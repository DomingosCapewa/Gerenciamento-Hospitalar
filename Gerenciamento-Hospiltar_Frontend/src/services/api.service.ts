
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Appointment } from '../models/types';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = ((window as any).__env?.API_BASE_URL as string) || 'http://localhost:8085';

  async getAppointments(userCpf: string, userRole: 'Usuário' | 'Médico' | 'Admin'): Promise<Appointment[]> {
    if (userRole === 'Usuário') {
      try {
        const consultas = await this.http.get<any[]>(`${this.baseUrl}/agendamentos/api/agendamentos/consultas/paciente/${encodeURIComponent(userCpf)}`).toPromise();
        const mapped = (consultas || []).map(c => ({
          id: String(c.id ?? c.Id ?? Math.random()),
          patientName: c.paciente?.nome ?? c.Paciente?.Nome ?? '',
          patientCpf: c.paciente?.cpf ?? c.Paciente?.Cpf ?? userCpf,
          type: 'Consulta',
          specialtyOrExam: c.medico ?? c.Medico ?? '',
          dateTime: c.horario ?? c.Horario ?? '',
          status: 'Marcado'
        } as Appointment));
        return mapped;
      } catch {
        return [];
      }
    }
    // Médicos/Admin: for now return empty until backend endpoints are defined
    return [];
  }

  async schedule(data: any): Promise<{ success: boolean; message: string; code?: string }> {
    const isConsulta = !!data.medico;
    const url = isConsulta ? `${this.baseUrl}/api/cadastro/consulta` : `${this.baseUrl}/api/cadastro/exame`;
    const payload = isConsulta
      ? { Medico: data.medico, Paciente: { Nome: data.paciente.nome, Cpf: data.paciente.cpf }, Horario: data.horario }
      : { Exame: data.exame, Paciente: { Nome: data.paciente.nome, Cpf: data.paciente.cpf }, Horario: data.horario };

    try {
      const resp = await this.http.post<{ mensagem: string; codigo: number | string }>(url, payload).toPromise();
      return { success: true, message: resp?.mensagem || 'Agendado com sucesso!', code: String(resp?.codigo ?? '') };
    } catch (e: any) {
      const msg = e?.error?.message || e?.message || 'Falha ao agendar';
      return { success: false, message: msg };
    }
  }
}
