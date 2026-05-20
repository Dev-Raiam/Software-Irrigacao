// namespace IrrigacaoInteligente.Domain.Entities;

// public class BlocoTelemetriaProtocolo
// {
//     public Guid ControladorId { get; init; }
//     public int IntervaloLeitura { get; init; }
//     public List<Leitura> Leituras { get; set; } = [];

//     public class Leitura
//     {
//         public Guid DispositivoId { get; set; }
//         public string Descricao { get; set; } = null!;
//         public string EnderecoFisico { get; set; } = null!;
//         public string TipoSinal { get; set; } = null!;
//         public Comunicacao Comunicacao { get; set; } = null!;
//         public Protocolo Protocolo { get; set; } = null!;
//         public List<Registrador> Leituras { get; set; } = [];
//     }

//     public class Comunicacao
//     {
//         public int SlaveId { get; set; }
//         public int BaudRate { get; set; }
//         public int DataBits { get; set; }
//         public string Parity { get; set; } = null!;
//         public int StopBits { get; set; }
//         public int TimeoutMs { get; set; }
//         public int Retries { get; set; }
//     }

//     public class Protocolo
//     {
//         public string Tipo { get; set; } = null!;
//         public List<Funcao> Funcoes { get; set; } = [];
//     }

//     public class Funcao
//     {
//         public int Codigo { get; set; }
//         public string Descricao { get; set; } = null!;
//     }

//     public class Registrador
//     {
//         public string Name { get; set; } = null!;
//         public int Adress { get; set; }
//         public int Quantity { get; set; }
//         public string DataType { get; set; } = null!;
//         public double Scale { get; set; }
//         public string Unit { get; set; } = null!;
//     }
// }
