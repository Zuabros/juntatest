using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Juntador_de_Atestados
{
 public partial class Form1 : Form
 {
	public Form1()
	{
	 InitializeComponent();
	 this.Load += Form1_Load;
	}

	class estado_janela
	{
	 public int left { get; set; }
	 public int top { get; set; }
	}

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
		// ignora erro
	 }
	}

	private void tb_atestados_TextChanged(object sender, EventArgs e)
	{
	 tb_resultado.Text = "";
	 try
	 {
		string input = tb_atestados.Text.Trim();
		if (string.IsNullOrWhiteSpace(input)) return;

		string[] partes = input.Split(' ');
		List<(DateTime inicio, DateTime fim)> periodos = new();
		DateTime agora = DateTime.Today;

		if (partes.Length == 2)
		{
		 string a = partes[0];
		 string b = partes[1];

		 bool a_data = tenta_parse_data(a, agora, false, out DateTime data1);
		 bool b_data = tenta_parse_data(b, agora, true, out DateTime data2);

		 if (a_data && b_data)
		 {
			// Ajusta o ano da segunda data para corresponder à primeira, se necessário
			if (data2 < data1)
			{
			 try
			 {
				data2 = new DateTime(data1.Year, data2.Month, data2.Day);
				if (data2 < data1)
				{
				 data2 = new DateTime(data1.Year + 1, data2.Month, data2.Day);
				}
			 }
			 catch
			 {
				throw new Exception("Datas inválidas ou segunda data anterior à primeira.");
			 }
			}
			periodos.Add((data1, data2));
		 }
		 else if (a_data && int.TryParse(b, out int dias2))
		 {
			DateTime fim = data1.AddDays(dias2 - 1);
			periodos.Add((data1, fim));
		 }
		 else
		 {
			throw new Exception("Formato inválido para a segunda entrada.");
		 }
		}
		else
		{
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

		var maior = blocos
				.OrderByDescending(b => (b.fim - b.ini).TotalDays)
				.ThenByDescending(b => b.ini)
				.First();

		int duracao = (maior.fim - maior.ini).Days + 1;

		tb_resultado.Text = $"Maior prazo: {maior.ini:dd/MM/yy} - {duracao} dias (até {maior.fim:dd/MM/yy})";
		Clipboard.SetText(maior.ini.ToString("dd/MM/yyyy"));
		finaliza();
	 }
	 catch
	 {
		tb_resultado.Text = "Datas inválidas ou formato incorreto. Aperte o botão ? para ajuda.";
	 }
	}

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
		File.WriteAllText("juntatest.cfg", json);
	 }
	 catch
	 {
		// ignora erro
	 }
	}

	private DateTime parse_data(string str, DateTime hoje)
	{
	 str = str.Trim();
	 if (str.Length == 4)
	 {
		int dia = int.Parse(str.Substring(0, 2));
		int mes = int.Parse(str.Substring(2, 2));
		int ano = hoje.Year;
		DateTime data = new DateTime(ano, mes, dia);
		if (data > hoje) data = new DateTime(ano - 1, mes, dia);
		return data;
	 }
	 if (str.Length == 6)
	 {
		return DateTime.ParseExact(str, "ddMMyy", null);
	 }
	 if (str.Length == 8)
	 {
		return DateTime.ParseExact(str, "ddMMyyyy", null);
	 }
	 throw new FormatException("Formato de data inválido.");
	}

	private bool tenta_parse_data(string entrada, DateTime agora, bool allowFuture, out DateTime resultado)
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

	 // ddMM
	 if (entrada.Length == 4 && int.TryParse(entrada.Substring(0, 2), out int dia) && int.TryParse(entrada.Substring(2, 2), out int mes))
	 {
		int ano = agora.Year;
		try
		{
		 resultado = new DateTime(ano, mes, dia);
		 // Se não permitir datas futuras, ajusta para o ano anterior
		 if (!allowFuture && resultado > agora)
			resultado = new DateTime(ano - 1, mes, dia);
		 return true;
		}
		catch { }
	 }

	 return false;
	}

	private void button1_Click(object sender, EventArgs e)
	{
	 string msg =
@"Nome do programa: Juntatest. Livre distribuição.
Função: Merger de datas de atestados médicos e 
calculo de data de cessação do benefício.
Autor: Ramiro Ayres Reggiani, Perito Médico Federal.
CRM 22.820 PR / RQE 25.729.
Contato: (42) 99923-8123 (WhatsApp)
Licença: Freeware.
GitHub: https://github.com/Zuabros/juntatest (GNU)

----------------------------------------
O QUE O PROGRAMA FAZ
----------------------------------------
Recebe uma sequência de atestados contendo data e número de dias de afastamento.

Calcula automaticamente o maior período confluente de afastamento 
(somando atestados que se sobrepõem ou se encostam),
e informa a data de início, a duração total em dias corridos e a data final.

----------------------------------------
FORMATO DE ENTRADA (na caixa de cima)
----------------------------------------
Digite os atestados separados por espaço, no formato:

data dias data dias ...

Exemplo:
10/01/25 5 20/01/25 10 05/02/25 15

Também é possível digitar apenas duas datas. Nesse caso, 
o programa considera que a primeira é o início e a segunda é o último dia do afastamento.

Exemplo:
10/01/25 15/01/25

----------------------------------------
BOTÃO ""NOVO""
----------------------------------------
Limpa os campos e copia para a área de transferência a data de início 
do maior afastamento, no formato dd/mm/yyyy.";
	 MessageBox.Show(msg, "Ajuda - Juntatest", MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	private void button2_Click(object sender, EventArgs e)
	{
	 try
	 {
		string txt = tb_resultado.Text;
		if (txt.StartsWith("Maior prazo: "))
		{
		 string data_curtinha = txt.Substring("Maior prazo: ".Length, 8);
		 DateTime data_ini = DateTime.ParseExact(data_curtinha, "dd/MM/yy", null);
		 Clipboard.SetText(data_ini.ToString("dd/MM/yyyy"));
		}
	 }
	 catch
	 {
		// ignora erro
	 }
	 tb_atestados.Text = "";
	 tb_resultado.Text = "";
	 tb_atestados.Focus();
	}
 }
}