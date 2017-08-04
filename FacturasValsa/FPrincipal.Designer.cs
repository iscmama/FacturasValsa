namespace FacturasValsa
{
    partial class FPrincipal
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FPrincipal));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Ruta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RFC_Emisor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RFC_Receptor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Razon_Social_Receptor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Calle_Receptor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Numero_Exterior_Receptor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Colonia_Receptor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Localidad_Receptor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Municipio_Receptor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Estado_Receptor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Pais_Receptor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Codigo_Postal_Receptor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Serie_Factura = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Folio_Factura = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Lugar_Expedicion_Factura = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Tipo_Comprobante_Factura = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Fecha_Emision_Factura = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Condiciones_Pago_Factura = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Metodo_Pago_Factura = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Moneda_Factura = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Tipo_Cambio_Factura = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Sub_Total_Factura = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IVA_Factura = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Total_Factura = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Id_Empresa = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Id_Cliente = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Correcto = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UUID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RutaNueva = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NumeroCliente = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Contador = new System.Windows.Forms.Timer(this.components);
            this.Asincrono = new System.ComponentModel.BackgroundWorker();
            this.IconoApp = new System.Windows.Forms.NotifyIcon(this.components);
            this.MenuContextual = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.salirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ContadorValidacion = new System.Windows.Forms.Timer(this.components);
            this.AsincronoValidacion = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.MenuContextual.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Ruta,
            this.RFC_Emisor,
            this.RFC_Receptor,
            this.Razon_Social_Receptor,
            this.Calle_Receptor,
            this.Numero_Exterior_Receptor,
            this.Colonia_Receptor,
            this.Localidad_Receptor,
            this.Municipio_Receptor,
            this.Estado_Receptor,
            this.Pais_Receptor,
            this.Codigo_Postal_Receptor,
            this.Serie_Factura,
            this.Folio_Factura,
            this.Lugar_Expedicion_Factura,
            this.Tipo_Comprobante_Factura,
            this.Fecha_Emision_Factura,
            this.Condiciones_Pago_Factura,
            this.Metodo_Pago_Factura,
            this.Moneda_Factura,
            this.Tipo_Cambio_Factura,
            this.Sub_Total_Factura,
            this.IVA_Factura,
            this.Total_Factura,
            this.Id_Empresa,
            this.Id_Cliente,
            this.Correcto,
            this.UUID,
            this.RutaNueva,
            this.NumeroCliente});
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(0, 0);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.Visible = false;
            // 
            // Ruta
            // 
            this.Ruta.HeaderText = "Ruta";
            this.Ruta.Name = "Ruta";
            this.Ruta.ReadOnly = true;
            // 
            // RFC_Emisor
            // 
            this.RFC_Emisor.HeaderText = "RFC_Emisor";
            this.RFC_Emisor.Name = "RFC_Emisor";
            this.RFC_Emisor.ReadOnly = true;
            // 
            // RFC_Receptor
            // 
            this.RFC_Receptor.HeaderText = "RFC_Receptor";
            this.RFC_Receptor.Name = "RFC_Receptor";
            this.RFC_Receptor.ReadOnly = true;
            // 
            // Razon_Social_Receptor
            // 
            this.Razon_Social_Receptor.HeaderText = "Razon_Social_Receptor";
            this.Razon_Social_Receptor.Name = "Razon_Social_Receptor";
            this.Razon_Social_Receptor.ReadOnly = true;
            // 
            // Calle_Receptor
            // 
            this.Calle_Receptor.HeaderText = "Calle_Receptor";
            this.Calle_Receptor.Name = "Calle_Receptor";
            this.Calle_Receptor.ReadOnly = true;
            // 
            // Numero_Exterior_Receptor
            // 
            this.Numero_Exterior_Receptor.HeaderText = "Numero_Exterior_Receptor";
            this.Numero_Exterior_Receptor.Name = "Numero_Exterior_Receptor";
            this.Numero_Exterior_Receptor.ReadOnly = true;
            // 
            // Colonia_Receptor
            // 
            this.Colonia_Receptor.HeaderText = "Colonia_Receptor";
            this.Colonia_Receptor.Name = "Colonia_Receptor";
            this.Colonia_Receptor.ReadOnly = true;
            // 
            // Localidad_Receptor
            // 
            this.Localidad_Receptor.HeaderText = "Localidad_Receptor";
            this.Localidad_Receptor.Name = "Localidad_Receptor";
            this.Localidad_Receptor.ReadOnly = true;
            // 
            // Municipio_Receptor
            // 
            this.Municipio_Receptor.HeaderText = "Municipio_Receptor";
            this.Municipio_Receptor.Name = "Municipio_Receptor";
            this.Municipio_Receptor.ReadOnly = true;
            // 
            // Estado_Receptor
            // 
            this.Estado_Receptor.HeaderText = "Estado_Receptor";
            this.Estado_Receptor.Name = "Estado_Receptor";
            this.Estado_Receptor.ReadOnly = true;
            // 
            // Pais_Receptor
            // 
            this.Pais_Receptor.HeaderText = "Pais_Receptor";
            this.Pais_Receptor.Name = "Pais_Receptor";
            this.Pais_Receptor.ReadOnly = true;
            // 
            // Codigo_Postal_Receptor
            // 
            this.Codigo_Postal_Receptor.HeaderText = "Codigo_Postal_Receptor";
            this.Codigo_Postal_Receptor.Name = "Codigo_Postal_Receptor";
            this.Codigo_Postal_Receptor.ReadOnly = true;
            // 
            // Serie_Factura
            // 
            this.Serie_Factura.HeaderText = "Serie_Factura";
            this.Serie_Factura.Name = "Serie_Factura";
            this.Serie_Factura.ReadOnly = true;
            // 
            // Folio_Factura
            // 
            this.Folio_Factura.HeaderText = "Folio_Factura";
            this.Folio_Factura.Name = "Folio_Factura";
            this.Folio_Factura.ReadOnly = true;
            // 
            // Lugar_Expedicion_Factura
            // 
            this.Lugar_Expedicion_Factura.HeaderText = "Lugar_Expedicion_Factura";
            this.Lugar_Expedicion_Factura.Name = "Lugar_Expedicion_Factura";
            this.Lugar_Expedicion_Factura.ReadOnly = true;
            // 
            // Tipo_Comprobante_Factura
            // 
            this.Tipo_Comprobante_Factura.HeaderText = "Tipo_Comprobante_Factura";
            this.Tipo_Comprobante_Factura.Name = "Tipo_Comprobante_Factura";
            this.Tipo_Comprobante_Factura.ReadOnly = true;
            // 
            // Fecha_Emision_Factura
            // 
            this.Fecha_Emision_Factura.HeaderText = "Fecha_Emision_Factura";
            this.Fecha_Emision_Factura.Name = "Fecha_Emision_Factura";
            this.Fecha_Emision_Factura.ReadOnly = true;
            // 
            // Condiciones_Pago_Factura
            // 
            this.Condiciones_Pago_Factura.HeaderText = "Condiciones_Pago_Factura";
            this.Condiciones_Pago_Factura.Name = "Condiciones_Pago_Factura";
            this.Condiciones_Pago_Factura.ReadOnly = true;
            // 
            // Metodo_Pago_Factura
            // 
            this.Metodo_Pago_Factura.HeaderText = "Metodo_Pago_Factura";
            this.Metodo_Pago_Factura.Name = "Metodo_Pago_Factura";
            this.Metodo_Pago_Factura.ReadOnly = true;
            // 
            // Moneda_Factura
            // 
            this.Moneda_Factura.HeaderText = "Moneda_Factura";
            this.Moneda_Factura.Name = "Moneda_Factura";
            this.Moneda_Factura.ReadOnly = true;
            // 
            // Tipo_Cambio_Factura
            // 
            this.Tipo_Cambio_Factura.HeaderText = "Tipo_Cambio_Factura";
            this.Tipo_Cambio_Factura.Name = "Tipo_Cambio_Factura";
            this.Tipo_Cambio_Factura.ReadOnly = true;
            // 
            // Sub_Total_Factura
            // 
            this.Sub_Total_Factura.HeaderText = "Sub_Total_Factura";
            this.Sub_Total_Factura.Name = "Sub_Total_Factura";
            this.Sub_Total_Factura.ReadOnly = true;
            // 
            // IVA_Factura
            // 
            this.IVA_Factura.HeaderText = "IVA_Factura";
            this.IVA_Factura.Name = "IVA_Factura";
            this.IVA_Factura.ReadOnly = true;
            // 
            // Total_Factura
            // 
            this.Total_Factura.HeaderText = "Total_Factura";
            this.Total_Factura.Name = "Total_Factura";
            this.Total_Factura.ReadOnly = true;
            // 
            // Id_Empresa
            // 
            this.Id_Empresa.HeaderText = "Id_Empresa";
            this.Id_Empresa.Name = "Id_Empresa";
            this.Id_Empresa.ReadOnly = true;
            // 
            // Id_Cliente
            // 
            this.Id_Cliente.HeaderText = "Id_Cliente";
            this.Id_Cliente.Name = "Id_Cliente";
            this.Id_Cliente.ReadOnly = true;
            // 
            // Correcto
            // 
            this.Correcto.HeaderText = "Correcto";
            this.Correcto.Name = "Correcto";
            this.Correcto.ReadOnly = true;
            // 
            // UUID
            // 
            this.UUID.HeaderText = "UUID";
            this.UUID.Name = "UUID";
            this.UUID.ReadOnly = true;
            // 
            // RutaNueva
            // 
            this.RutaNueva.HeaderText = "RutaNueva";
            this.RutaNueva.Name = "RutaNueva";
            this.RutaNueva.ReadOnly = true;
            // 
            // NumeroCliente
            // 
            this.NumeroCliente.HeaderText = "NumeroCliente";
            this.NumeroCliente.Name = "NumeroCliente";
            this.NumeroCliente.ReadOnly = true;
            // 
            // Contador
            // 
            this.Contador.Interval = 600000;
            this.Contador.Tick += new System.EventHandler(this.Contador_Tick);
            // 
            // Asincrono
            // 
            this.Asincrono.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Asincrono_DoWork);
            this.Asincrono.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.Asincrono_RunWorkerCompleted);
            // 
            // IconoApp
            // 
            this.IconoApp.ContextMenuStrip = this.MenuContextual;
            this.IconoApp.Text = "IconoApp";
            this.IconoApp.Visible = true;
            // 
            // MenuContextual
            // 
            this.MenuContextual.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.salirToolStripMenuItem});
            this.MenuContextual.Name = "MenuContextual";
            this.MenuContextual.Size = new System.Drawing.Size(97, 26);
            // 
            // salirToolStripMenuItem
            // 
            this.salirToolStripMenuItem.Name = "salirToolStripMenuItem";
            this.salirToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.salirToolStripMenuItem.Text = "Salir";
            this.salirToolStripMenuItem.Click += new System.EventHandler(this.salirToolStripMenuItem_Click);
            // 
            // ContadorValidacion
            // 
            this.ContadorValidacion.Interval = 600000;
            this.ContadorValidacion.Tick += new System.EventHandler(this.ContadorValidacion_Tick);
            // 
            // AsincronoValidacion
            // 
            this.AsincronoValidacion.DoWork += new System.ComponentModel.DoWorkEventHandler(this.AsincronoValidacion_DoWork);
            this.AsincronoValidacion.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.AsincronoValidacion_RunWorkerCompleted);
            // 
            // FPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.Controls.Add(this.dataGridView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sniffer";
            this.Activated += new System.EventHandler(this.FPrincipal_Activated);
            this.Load += new System.EventHandler(this.FPrincipal_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.MenuContextual.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Ruta;
        private System.Windows.Forms.DataGridViewTextBoxColumn RFC_Emisor;
        private System.Windows.Forms.DataGridViewTextBoxColumn RFC_Receptor;
        private System.Windows.Forms.DataGridViewTextBoxColumn Razon_Social_Receptor;
        private System.Windows.Forms.DataGridViewTextBoxColumn Calle_Receptor;
        private System.Windows.Forms.DataGridViewTextBoxColumn Numero_Exterior_Receptor;
        private System.Windows.Forms.DataGridViewTextBoxColumn Colonia_Receptor;
        private System.Windows.Forms.DataGridViewTextBoxColumn Localidad_Receptor;
        private System.Windows.Forms.DataGridViewTextBoxColumn Municipio_Receptor;
        private System.Windows.Forms.DataGridViewTextBoxColumn Estado_Receptor;
        private System.Windows.Forms.DataGridViewTextBoxColumn Pais_Receptor;
        private System.Windows.Forms.DataGridViewTextBoxColumn Codigo_Postal_Receptor;
        private System.Windows.Forms.DataGridViewTextBoxColumn Serie_Factura;
        private System.Windows.Forms.DataGridViewTextBoxColumn Folio_Factura;
        private System.Windows.Forms.DataGridViewTextBoxColumn Lugar_Expedicion_Factura;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tipo_Comprobante_Factura;
        private System.Windows.Forms.DataGridViewTextBoxColumn Fecha_Emision_Factura;
        private System.Windows.Forms.DataGridViewTextBoxColumn Condiciones_Pago_Factura;
        private System.Windows.Forms.DataGridViewTextBoxColumn Metodo_Pago_Factura;
        private System.Windows.Forms.DataGridViewTextBoxColumn Moneda_Factura;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tipo_Cambio_Factura;
        private System.Windows.Forms.DataGridViewTextBoxColumn Sub_Total_Factura;
        private System.Windows.Forms.DataGridViewTextBoxColumn IVA_Factura;
        private System.Windows.Forms.DataGridViewTextBoxColumn Total_Factura;
        private System.Windows.Forms.DataGridViewTextBoxColumn Id_Empresa;
        private System.Windows.Forms.DataGridViewTextBoxColumn Id_Cliente;
        private System.Windows.Forms.DataGridViewTextBoxColumn Correcto;
        private System.Windows.Forms.DataGridViewTextBoxColumn UUID;
        private System.Windows.Forms.DataGridViewTextBoxColumn RutaNueva;
        private System.Windows.Forms.DataGridViewTextBoxColumn NumeroCliente;
        private System.Windows.Forms.Timer Contador;
        private System.ComponentModel.BackgroundWorker Asincrono;
        private System.Windows.Forms.NotifyIcon IconoApp;
        private System.Windows.Forms.ContextMenuStrip MenuContextual;
        private System.Windows.Forms.ToolStripMenuItem salirToolStripMenuItem;
        private System.Windows.Forms.Timer ContadorValidacion;
        private System.ComponentModel.BackgroundWorker AsincronoValidacion;
    }
}

