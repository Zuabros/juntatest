using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Juntador_de_Atestados
{


 // ----------------------------------------
 // FORM PRINCIPAL
 // ----------------------------------------
 public partial class Form1 : Form
 {
	public Form1()
	{
	 InitializeComponent(); // inicializa os controles
	 this.Load += Form1_Load; // associa evento de load
	}
	// ----------------------------------------
	// CLASSE ESTADO_JANELA
	// Armazena posi��o da janela em JSON
	// ----------------------------------------
	class estado_janela
	{
	 public int left { get; set; }  // posi��o horizontal
	 public int top { get; set; }   // posi��o vertical
	}
	// ----------------------------------------
	// RESTAURA POSI��O SALVA DA JANELA
	// ----------------------------------------
	private void Form1_Load(object sender, EventArgs e)
	{
	 try
	 {
		if (File.Exists("juntatest.cfg"))
		{
		 string json = File.ReadAllText("juntatest.cfg");
		 var estado = JsonSerializer.Deserialize<estado_janela>(json);

		 if (estado != null)
		 {
			this.Left = estado.left;
			this.Top = estado.top;
		 }
		}
	 }
	 catch
	 {
		// ignora erro silenciosamente
	 }
	}

	// ----------------------------------------
	// EVENTO TEXTCHANGED - tb_atestados
	// Calcula o maior per�odo confluente a
	// partir da entrada digitada em tb_atestados
	// ----------------------------------------
	private void tb_atestados_TextChanged(object sender, EventArgs e)
	{
	 tb_resultado.Text = ""; // limpa resultado anterior

	 try
	 {
		string input = tb_atestados.Text.Trim(); // pega texto digitado
		if (string.IsNullOrWhiteSpace(input)) return; // ignora vazio

		string[] partes = input.Split(' '); // separa por espa�o
		List<(DateTime inicio, DateTime fim)> periodos = new(); // lista de per�odos
		DateTime agora = DateTime.Today; // data atual

		// ----------------------------------------
		// MODO ESPECIAL: DUAS DATAS (ou data + dias)
		// ----------------------------------------
		if (partes.Length == 2)
		{
		 string a = partes[0];
		 string b = partes[1];

		 bool a_data = tenta_parse_data(a, agora, out DateTime data1);
		 bool b_data = tenta_parse_data(b, agora, out DateTime data2);

		 if (a_data && b_data)
		 {
			if (data2 < data1) throw new Exception(); // fim antes do in�cio
			int dias = (data2 - data1).Days + 1;
			DateTime fim = data1.AddDays(dias - 1);
			periodos.Add((data1, fim));
		 }
		 else if (a_data && int.TryParse(b, out int dias2))
		 {
			DateTime fim = data1.AddDays(dias2 - 1);
			periodos.Add((data1, fim));
		 }
		 else
		 {
			throw new Exception(); // nenhum caso v�lido
		 }
		}

		else
		{
		 // ----------------------------------------
		 // MODO NORMAL: pares data dias
		 // ----------------------------------------
		 if (partes.Length < 2 || partes.Length % 2 != 0) throw new Exception();

		 for (int i = 0; i < partes.Length; i += 2)
		 {
			string strdata = partes[i];
			string strdias = partes[i + 1];

			int dias = int.Parse(strdias);
			DateTime data = parse_data(strdata, agora);
			DateTime fim = data.AddDays(dias - 1);

			periodos.Add((data, fim));
		 }
		}

		// ----------------------------------------
		// AGRUPA PER�ODOS CONFLUENTES
		// ----------------------------------------
		periodos = periodos.OrderBy(p => p.inicio).ToList();
		List<(DateTime ini, DateTime fim)> blocos = new();

		DateTime atual_ini = periodos[0].inicio;
		DateTime atual_fim = periodos[0].fim;

		for (int i = 1; i < periodos.Count; i++)
		{
		 if (periodos[i].inicio <= atual_fim.AddDays(1))
			atual_fim = (periodos[i].fim > atual_fim) ? periodos[i].fim : atual_fim;
		 else
		 {
			blocos.Add((atual_ini, atual_fim));
			atual_ini = periodos[i].inicio;
			atual_fim = periodos[i].fim;
		 }
		}
		blocos.Add((atual_ini, atual_fim));

		// ----------------------------------------
		// PEGA O MAIOR BLOCO
		// ----------------------------------------
		var maior = blocos.OrderByDescending(b => (b.fim - b.ini).TotalDays).First();
		int duracao = (maior.fim - maior.ini).Days + 1;

		tb_resultado.Text = $"Maior prazo: {maior.ini:dd/MM/yy} - {duracao} dias (at� {maior.fim:dd/MM/yy})";
		Clipboard.SetText(maior.ini.ToString("dd/MM/yyyy")); // copia para �rea de transfer�ncia
		finaliza();
	 }
	 catch
	 {
		tb_resultado.Text = "Aperte o bot�o ? para ajuda.";
	 }
	}

	// ----------------------------------------
	// SALVA POSI��O DA JANELA EM JSON
	// ----------------------------------------
	private void finaliza()
	{
	 try
	 {
		var estado = new estado_janela
		{
		 left = this.Left,
		 top = this.Top
		};

		string json = JsonSerializer.Serialize(estado);
		File.WriteAllText("juntatest.cfg", json); // salva no mesmo diret�rio
	 }
	 catch
	 {
		// ignora erro
	 }
	}

	// ----------------------------------------
	// CONVERTE TEXTO DE DATA FLEX�VEL
	// Suporta formatos ddmm, ddmmyy e ddmmyyyy
	// ----------------------------------------
	private DateTime parse_data(string str, DateTime hoje)
	{
	 str = str.Trim();

	 // --- 4 d�gitos (ddmm) ---
	 if (str.Length == 4)
	 {
		int dia = int.Parse(str.Substring(0, 2));
		int mes = int.Parse(str.Substring(2, 2));
		int ano = hoje.Year;

		// se a data estiver no futuro, assume ano anterior
		DateTime data = new DateTime(ano, mes, dia);
		if (data > hoje) data = new DateTime(ano - 1, mes, dia);

		return data;
	 }

	 // --- 6 d�gitos (ddmmyy) ---
	 if (str.Length == 6)
	 {
		return DateTime.ParseExact(str, "ddMMyy", null);
	 }

	 // --- 8 d�gitos (ddmmyyyy) ---
	 if (str.Length == 8)
	 {
		return DateTime.ParseExact(str, "ddMMyyyy", null);
	 }

	 // formato inv�lido
	 throw new FormatException("Formato de data inv�lido.");
	}


	// ----------------------------------------
	// BOT�O AJUDA "?" 
	// Mostra informa��es do programa e instru��es
	// ----------------------------------------
	private void button1_Click(object sender, EventArgs e)
	{
	 string msg =
 @"Nome do programa: Juntatest. Livre distribui��o.
Fun��o: Merger de datas de atestados m�dicos e 
calculo de data de cessa��o do benef�cio.
Autor: Ramiro Ayres Reggiani, Perito M�dico Federal.
CRM 22.820 PR / RQE 25.729.
Contato: (42) 99923-8123 (WhatsApp)
Licen�a: Freeware.
GitHub: https://github.com/Zuabros/juntatest (GNU)

----------------------------------------
O QUE O PROGRAMA FAZ
----------------------------------------
Recebe uma sequ�ncia de atestados contendo data e n�mero de dias de afastamento.

Calcula automaticamente o maior per�odo confluente de afastamento 
(somando atestados que se sobrep�em ou se encostam),
e informa a data de in�cio, a dura��o total em dias corridos e a data final.

----------------------------------------
FORMATO DE ENTRADA (na caixa de cima)
----------------------------------------
Digite os atestados separados por espa�o, no formato:

data dias data dias ...

Exemplo:
10/01/25 5 20/01/25 10 05/02/25 15

Tamb�m � poss�vel digitar apenas duas datas. Nesse caso, 
o programa considera que a primeira � o in�cio e a segunda � o �ltimo dia do afastamento.

Exemplo:
10/01/25 15/01/25

----------------------------------------
BOT�O ""NOVO""
----------------------------------------
Limpa os campos e copia para a �rea de transfer�ncia a data de in�cio 
do maior afastamento, no formato dd/mm/yyyy.";

	 MessageBox.Show(msg, "Ajuda - Juntatest", MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	private bool tenta_parse_data(string entrada, DateTime agora, out DateTime resultado)
	{
	 resultado = DateTime.MinValue;

	 // ddMMyyyy
	 if (DateTime.TryParseExact(entrada, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out resultado))
		return true;

	 // ddMMyy
	 if (DateTime.TryParseExact(entrada, "ddMMyy", null, System.Globalization.DateTimeStyles.None, out resultado))
		return true;

	 // dd/MM/yyyy
	 if (DateTime.TryParseExact(entrada, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out resultado))
		return true;

	 // dd/MM/yy
	 if (DateTime.TryParseExact(entrada, "dd/MM/yy", null, System.Globalization.DateTimeStyles.None, out resultado))
		return true;

	 // ddMM (assume ano atual ou anterior)
	 if (entrada.Length == 4 && int.TryParse(entrada.Substring(0, 2), out int dia) && int.TryParse(entrada.Substring(2, 2), out int mes))
	 {
		int ano = agora.Year;
		try
		{
		 resultado = new DateTime(ano, mes, dia);
		 // Se for futura, assume ano anterior
		 if (resultado > agora)
			resultado = new DateTime(ano - 1, mes, dia);
		 return true;
		}
		catch { }
	 }

	 return false; // nenhuma convers�o deu certo
	}

	// ----------------------------------------
	// BOT�O NOVO
	// Limpa campos e copia a data inicial do �ltimo resultado para a �rea de transfer�ncia
	// ----------------------------------------
	private void button2_Click(object sender, EventArgs e)
	{
	 try
	 {
		string txt = tb_resultado.Text;
		if (txt.StartsWith("Maior prazo: "))
		{
		 // extrai a data inicial no formato dd/mm/yy
		 string data_curtinha = txt.Substring("Maior prazo: ".Length, 8); // pega 8 caracteres da data

		 // converte para datetime com ano completo
		 DateTime data_ini = DateTime.ParseExact(data_curtinha, "dd/MM/yy", null);

		 // copia como dd/MM/yyyy
		 Clipboard.SetText(data_ini.ToString("dd/MM/yyyy"));
		}
	 }
	 catch
	 {
		// ignora erro
	 }

	 // limpa os campos
	 tb_atestados.Text = "";
	 tb_resultado.Text = "";

	 // retorna o foco para o campo de entrada
	 tb_atestados.Focus();
	}

 }
}
