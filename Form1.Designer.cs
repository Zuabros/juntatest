namespace Juntador_de_Atestados
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

	#region Windows Form Designer generated code

	/// <summary>
	///  Required method for Designer support - do not modify
	///  the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
	 tb_atestados = new TextBox();
	 tb_resultado = new TextBox();
	 button1 = new Button();
	 button2 = new Button();
	 SuspendLayout();
	 // 
	 // tb_atestados
	 // 
	 tb_atestados.Dock = DockStyle.Fill;
	 tb_atestados.Location = new Point(0, 0);
	 tb_atestados.Multiline = true;
	 tb_atestados.Name = "tb_atestados";
	 tb_atestados.Size = new Size(340, 174);
	 tb_atestados.TabIndex = 0;
	 tb_atestados.TextChanged += tb_atestados_TextChanged;
	 // 
	 // tb_resultado
	 // 
	 tb_resultado.Location = new Point(0, 151);
	 tb_resultado.Name = "tb_resultado";
	 tb_resultado.Size = new Size(251, 23);
	 tb_resultado.TabIndex = 1;
	 // 
	 // button1
	 // 
	 button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
	 button1.Location = new Point(315, 151);
	 button1.Name = "button1";
	 button1.Size = new Size(25, 23);
	 button1.TabIndex = 2;
	 button1.Text = "?";
	 button1.UseVisualStyleBackColor = true;
	 button1.Click += button1_Click;
	 // 
	 // button2
	 // 
	 button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
	 button2.Location = new Point(257, 151);
	 button2.Name = "button2";
	 button2.Size = new Size(59, 23);
	 button2.TabIndex = 3;
	 button2.Text = "Novo";
	 button2.UseVisualStyleBackColor = true;
	 button2.Click += button2_Click;
	 // 
	 // Form1
	 // 
	 AutoScaleDimensions = new SizeF(7F, 15F);
	 AutoScaleMode = AutoScaleMode.Font;
	 ClientSize = new Size(340, 174);
	 Controls.Add(button2);
	 Controls.Add(button1);
	 Controls.Add(tb_resultado);
	 Controls.Add(tb_atestados);
	 Name = "Form1";
	 Text = "Juntatest 1.31";
	 ResumeLayout(false);
	 PerformLayout();
	}

	#endregion

	private TextBox tb_atestados;
	private TextBox tb_resultado;
	private Button button1;
	private Button button2;
 }
}
