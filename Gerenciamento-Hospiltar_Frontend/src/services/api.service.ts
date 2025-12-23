
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Appointment } from '../models/types';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = ((window as any).__env?.API_BASE_URL as string) || 'http://localhost:8085';

  async getAppointments(userCpf: string, userRole: 'Usuario' | 'Medico' | 'Admin'): Promise<Appointment[]> {
    if (userRole === 'Usuario') {
      try {
        const consultas = await this.http.get<any[]>(`${this.baseUrl}/agendamentos/api/agendamentos/consultas/paciente/${encodeURIComponent(userCpf)}`).toPromise();
        const mapped = (consultas || []).map(c => ({
          id: String(c.id ?? c.Id ?? Math.random()),
          patientName: c.paciente?.nome ?? c.Paciente?.Nome ?? '',
          patientCpf: c.paciente?.cpf ?? c.Paciente?.CPF ?? c.Paciente?.Cpf ?? userCpf,
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
    return [];
  }

  async schedule(data: any): Promise<{ success: boolean; message: string; code?: string }> {
    const isConsulta = !!data.medico;
    const url = isConsulta ? `${this.baseUrl}/api/cadastro/consulta` : `${this.baseUrl}/api/cadastro/exame`;
    const payload = isConsulta
      ? { Medico: data.medico, Paciente: { Nome: data.paciente.nome, CPF: data.paciente.cpf, Sexo: data.paciente.sexo, Idade: data.paciente.idade, DataNascimento: data.paciente.dataNascimento, Telefone: data.paciente.telefone, Email: data.paciente.email }, Horario: data.horario }
      : { Exame: data.exame, Paciente: { Nome: data.paciente.nome, CPF: data.paciente.cpf, Sexo: data.paciente.sexo, Idade: data.paciente.idade, DataNascimento: data.paciente.dataNascimento, Telefone: data.paciente.telefone, Email: data.paciente.email }, Horario: data.horario };

    try {
      const resp = await this.http.post<{ mensagem: string; codigo: number | string }>(url, payload).toPromise();
      return { success: true, message: resp?.mensagem || 'Agendado com sucesso!', code: String(resp?.codigo ?? '') };
    } catch (e: any) {
      const msg = e?.error?.message || e?.message || 'Falha ao agendar';
      return { success: false, message: msg };
    }
  }

  async attendByHorario(input: { pacienteCPF: string; horario: string; sintomas: string[] }): Promise<any> {
    const url = `${this.baseUrl}/clinica/api/clinica/consultas/atender-horario`;
    const body = { PacienteCPF: input.pacienteCPF, Horario: input.horario, Sintomas: input.sintomas };
    console.log('🔵 attendByHorario URL:', url);
    console.log('🔵 attendByHorario Body:', body);
    return this.http.post(url, body).toPromise();
  }

  async attendByCodigo(input: { pacienteCPF: string; consultaId: number; sintomas: string[]; horario?: string }): Promise<any> {
    const url = `${this.baseUrl}/clinica/api/clinica/consultas/atender-codigo`;
    const body: any = { PacienteCPF: input.pacienteCPF, ConsultaId: input.consultaId, Sintomas: input.sintomas };
    if (input.horario) body.Horario = input.horario;
    return this.http.post(url, body).toPromise();
  }

  async updateProcedimentoPrioridade(idExameAgendamento: number, prioridade: string, atualizadoPor?: string): Promise<any> {
    const url = `${this.baseUrl}/clinica/api/clinica/procedimentos/${idExameAgendamento}/prioridade`;
    return this.http.put(url, { Prioridade: prioridade, AtualizadoPor: atualizadoPor }).toPromise();
  }

  async updateProcedimentoStatus(idExameAgendamento: number, status: string, atualizadoPor?: string): Promise<any> {
    const url = `${this.baseUrl}/clinica/api/clinica/procedimentos/${idExameAgendamento}/status`;
    return this.http.put(url, { Status: status, AtualizadoPor: atualizadoPor }).toPromise();
  }
}
