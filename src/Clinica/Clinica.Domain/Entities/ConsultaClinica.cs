public class ConsultaClinica
{
    public int Id {get;set;}
    public int PacienteId {get;set;}
    public int MedicoId {get;set;}
    public DateTime DataHora {get;set;}
    public string Sintomas {get;set;} = null!;
}
