using FacturasValsa.ValidadorSAT;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace FacturasValsa
{
    public partial class FPrincipal : Form
    {
        private List<String> ListaXML = null;
        //Variable de conexion
        private static String Conexion = ConfigurationManager.ConnectionStrings["dbEnvioFacturas"].ConnectionString;
        //Ruta donde moveremos las facturas (Debe estar dentro de la carpeta de nuestro proyecto)
        private static String RutaMover = ConfigurationManager.AppSettings["rutaDestino"].ToString();
        //Ruta donde el PAC nos deja las facturas
        private static String RutaFacturas = ConfigurationManager.AppSettings["rutaOrigen"].ToString();
        //private static String RutaFacturas = @"N:\Inventiva_WatchDog\FTP\inbound";
        //private static String RutaFacturas = @"C:\Users\Ricardo\Desktop\160301\enviadas";

        public FPrincipal()
        {
            InitializeComponent();
        }

        //Logica general, se ejecutará cada 2 minutos
        private void LogicaGeneral()
        {
            //Lo de los PDF
            ListaXML = ListarArchivos();

            if (ListaXML != null)
            {
                RecorrerXML();
            }

            //Inicializamos conexion
            SqlConnection conn = new SqlConnection(FPrincipal.Conexion);
            //Ahora transaccion
            SqlTransaction Transaccion = null;

            //Iniciamos el trai
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                Transaccion = conn.BeginTransaction();

                //Obtengo la fecha de facturas a buscar
                String Fecha = "";
                String QueryFecha = "SELECT CONCAT((REPLACE(CONVERT(VARCHAR, Fecha, 102), '.', '-')), ' ', (CONVERT(VARCHAR, Fecha, 108))) FROM Fechas;";
                SqlCommand ComandoFecha = new SqlCommand(QueryFecha, conn, Transaccion);
                SqlDataReader LectorFecha = ComandoFecha.ExecuteReader();
                if (LectorFecha.HasRows)
                {
                    while (LectorFecha.Read())
                    {
                        Fecha = LectorFecha.GetString(0);
                    }
                }
                LectorFecha.Close();

                //Obtengo las empresas dadas de alta actualmente
                List<String> ListaEmpresas = new List<String>();
                String QueryEmpresas = "SELECT UPPER(CONCAT(RFC, ',', Id_Empresa)) AS Empresas FROM Empresa;";
                SqlCommand ComandoEmpresas = new SqlCommand(QueryEmpresas, conn, Transaccion);
                SqlDataReader LectorEmpresas = ComandoEmpresas.ExecuteReader();
                if (LectorEmpresas.HasRows)
                {
                    while (LectorEmpresas.Read())
                    {
                        ListaEmpresas.Add(LectorEmpresas.GetString(0));
                    }
                }
                LectorEmpresas.Close();


                //Leemos las facturas (XML)
                //String[] ArchivosXML = Directory.GetFiles(FPrincipal.RutaFacturas, "*.xml", SearchOption.AllDirectories).Where(x => new FileInfo(x).CreationTime.Date >= (Convert.ToDateTime(Fecha))).ToArray();
                String[] ArchivosXML = Directory.GetFiles(FPrincipal.RutaFacturas, "*.xml", SearchOption.AllDirectories);
                //Ahora los PDF
                //String[] ArchivosPDF = Directory.GetFiles(FPrincipal.RutaFacturas, "*.pdf", SearchOption.AllDirectories).Where(x => new FileInfo(x).CreationTime.Date >= (Convert.ToDateTime(Fecha))).ToArray();
                String[] ArchivosPDF = Directory.GetFiles(FPrincipal.RutaFacturas, "*.pdf", SearchOption.AllDirectories);
                //Pasamos los PDF a lista (para hacer mas bajo el for)
                List<String> ListaPDF = new List<String>();
                for (int i = 0; i < ArchivosPDF.Length; i++)
                {
                    ListaPDF.Add(ArchivosPDF[i]);
                }
                ArchivosPDF = null;
                //Como condicion vamos a tener que el PDF debe llamarse igual que el XML
                for (int i = 0; i < ArchivosXML.Length; i++)
                {
                    String NombreArchivoXML = ArchivosXML[i].ToUpper().Replace(FPrincipal.RutaFacturas.ToUpper(), "").Replace(@"\", "").Replace(".XML", "").Replace("/", "");

                    for (int j = 0; j < ListaPDF.Count; j++)
                    {
                        String NombreArchivoPDF = ListaPDF[j].ToUpper().Replace(FPrincipal.RutaFacturas.ToUpper(), "").Replace(@"\", "").Replace(".PDF", "").Replace("/", "");
                        if (NombreArchivoPDF.Equals(NombreArchivoXML))
                        {
                            dataGridView1.Rows.Add(ArchivosXML[i]);
                            dataGridView1.Rows.Add(ListaPDF[j]);
                            ListaPDF.RemoveAt(j);
                        }
                    }
                }
                //Ya que tenemos los archivos que vamos a tener, leo los XML
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    //Se que los XML son los numeros in-pares, pero de igual forma hago validacion
                    String Ruta = dataGridView1.Rows[i].Cells[0].Value.ToString().ToUpper();
                    if (Ruta.EndsWith(".XML"))
                    {
                        //Aquí analizo la factura, obiamente en un try por si es un xml que NO es factura
                        Boolean FueFactura = true;
                        try
                        {
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.Load(dataGridView1.Rows[i].Cells[0].Value.ToString());

                            //Inicializo nodo general (Comprobante)
                            XmlNodeList General = xDoc.GetElementsByTagName("cfdi:Comprobante");

                            //Para obtener datos del emisor
                            XmlNodeList Emisor = ((XmlElement)General[0]).GetElementsByTagName("cfdi:Emisor");
                            foreach (XmlElement Nodo in Emisor)
                            {
                                //RFC emisor es el unico dato que ocupo, pero es obligatorio
                                dataGridView1.Rows[i].Cells[1].Value = Nodo.GetAttribute("rfc");
                            }

                            //Ahora para los datos del receptor
                            XmlNodeList Receptor = ((XmlElement)General[0]).GetElementsByTagName("cfdi:Receptor");
                            foreach (XmlElement Nodo in Receptor)
                            {
                                //RFC y Razon Social son obligatorios, la direccion es opcional
                                dataGridView1.Rows[i].Cells[2].Value = Nodo.GetAttribute("rfc");
                                dataGridView1.Rows[i].Cells[3].Value = Nodo.GetAttribute("nombre");

                                //Nodo de direccion (opcional)
                                XmlNodeList DireccionReceptor = ((XmlElement)Receptor[0]).GetElementsByTagName("cfdi:Domicilio");
                                foreach (XmlElement NodoDireccion in DireccionReceptor)
                                {
                                    //Calle receptor
                                    try
                                    {
                                        dataGridView1.Rows[i].Cells[4].Value = NodoDireccion.GetAttribute("calle");
                                    }
                                    catch
                                    {

                                    }
                                    //Numero exterior
                                    try
                                    {
                                        dataGridView1.Rows[i].Cells[5].Value = NodoDireccion.GetAttribute("noExterior");
                                    }
                                    catch
                                    {

                                    }
                                    //Colonia receptor
                                    try
                                    {
                                        dataGridView1.Rows[i].Cells[6].Value = NodoDireccion.GetAttribute("colonia");
                                    }
                                    catch
                                    {

                                    }
                                    //Localidad receptor
                                    try
                                    {
                                        dataGridView1.Rows[i].Cells[7].Value = NodoDireccion.GetAttribute("localidad");
                                    }
                                    catch
                                    {

                                    }
                                    //Municipio receptor
                                    try
                                    {
                                        dataGridView1.Rows[i].Cells[8].Value = NodoDireccion.GetAttribute("municipio");
                                    }
                                    catch
                                    {

                                    }
                                    //Estado receptor
                                    try
                                    {
                                        dataGridView1.Rows[i].Cells[9].Value = NodoDireccion.GetAttribute("estado");
                                    }
                                    catch
                                    {

                                    }
                                    //País receptor
                                    try
                                    {
                                        dataGridView1.Rows[i].Cells[10].Value = NodoDireccion.GetAttribute("pais");
                                    }
                                    catch
                                    {

                                    }
                                    //Codigo postal del receptor
                                    try
                                    {
                                        dataGridView1.Rows[i].Cells[11].Value = NodoDireccion.GetAttribute("codigoPostal");
                                    }
                                    catch
                                    {

                                    }

                                }
                            }
                            //Ahora obtengom serie y folio, de nuevo, opcional
                            foreach (XmlElement Nodo in General)
                            {
                                try
                                {
                                    dataGridView1.Rows[i].Cells[12].Value = Nodo.GetAttribute("serie");
                                }
                                catch
                                {

                                }
                                try
                                {
                                    dataGridView1.Rows[i].Cells[13].Value = Nodo.GetAttribute("folio");
                                }
                                catch
                                {

                                }
                            }

                            //Lugar de expedicion (opcional)
                            XmlNodeList Expedicion = ((XmlElement)Emisor[0]).GetElementsByTagName("cfdi:ExpedidoEn");
                            foreach (XmlElement Nodo in Expedicion)
                            {
                                try
                                {
                                    //Comentar el formato de lugar de expedición
                                    dataGridView1.Rows[i].Cells[14].Value = Nodo.GetAttribute("calle") + " " + Nodo.GetAttribute("noExterior") + ", " + Nodo.GetAttribute("colonia") + ", C.P. " + Nodo.GetAttribute("codigoPostal") + ", " + Nodo.GetAttribute("municipio") + ", " + Nodo.GetAttribute("estado") + ", " + Nodo.GetAttribute("pais");
                                }
                                catch
                                {

                                }
                            }

                            //Tipo de comprobante (obligatorio)
                            foreach (XmlElement Nodo in General)
                            {
                                dataGridView1.Rows[i].Cells[15].Value = Nodo.GetAttribute("tipoDeComprobante");
                                //Fecha de emision (obligatoria)
                                String FechaTemporal = Nodo.GetAttribute("fecha");
                                String[] ArregloFecha = FechaTemporal.Split('T');
                                dataGridView1.Rows[i].Cells[16].Value = ArregloFecha[0].Replace("-", "") + " " + ArregloFecha[1];
                                //Condiciones de pago (opcional) (NO TENGO EJEMPLO)
                                try
                                {
                                    dataGridView1.Rows[i].Cells[17].Value = Nodo.GetAttribute("condicionesDePago");
                                }
                                catch
                                {

                                }
                                //Metodo de pago (obligatorio)
                                dataGridView1.Rows[i].Cells[18].Value = Nodo.GetAttribute("metodoDePago");
                                //Moneda (opcional)
                                try
                                {
                                    dataGridView1.Rows[i].Cells[19].Value = Nodo.GetAttribute("Moneda");
                                }
                                catch
                                {

                                }
                                //Tipo de cambio (opcional)
                                try
                                {
                                    dataGridView1.Rows[i].Cells[20].Value = Nodo.GetAttribute("TipoCambio");
                                    if (dataGridView1.Rows[i].Cells[20].Value == null)
                                    {
                                        dataGridView1.Rows[i].Cells[20].Value = "1";
                                    }
                                    else if (dataGridView1.Rows[i].Cells[20].Value.ToString().Trim().Length == 0)
                                    {
                                        dataGridView1.Rows[i].Cells[20].Value = "1";
                                    }
                                }
                                catch
                                {

                                }
                                //Sub Total (obligatorio)
                                dataGridView1.Rows[i].Cells[21].Value = Nodo.GetAttribute("subTotal");


                                //Total (obligatorio)
                                dataGridView1.Rows[i].Cells[23].Value = Nodo.GetAttribute("total");
                            }

                            //IVA (obligatorio)
                            try
                            {
                                XmlNodeList Impuestos = ((XmlElement)General[0]).GetElementsByTagName("cfdi:Impuestos");
                                XmlNodeList Traslados = ((XmlElement)Impuestos[0]).GetElementsByTagName("cfdi:Traslados");
                                XmlNodeList Traslado = ((XmlElement)Traslados[0]).GetElementsByTagName("cfdi:Traslado");
                                foreach (XmlElement NodoIVA in Traslado)
                                {
                                    String Temp = NodoIVA.GetAttribute("impuesto");
                                    if (Temp == "IVA")
                                    {
                                        dataGridView1.Rows[i].Cells[22].Value = NodoIVA.GetAttribute("importe");
                                    }
                                }
                            }
                            catch (Exception ex2)
                            {
                                dataGridView1.Rows[i].Cells[22].Value = 0;
                                GuardarLog("Leer facturas IVA", ex2.Message);
                            }


                            //Obtenemos UUID (Obligatorio)
                            //Obtenemos el UUID
                            XmlNodeList NodoComplemento = xDoc.GetElementsByTagName("cfdi:Complemento");
                            XmlNodeList NodoUUID = ((XmlElement)NodoComplemento[0]).GetElementsByTagName("tfd:TimbreFiscalDigital");
                            foreach (XmlElement Nodo in NodoUUID)
                            {
                                dataGridView1.Rows[i].Cells[27].Value = Nodo.GetAttribute("UUID");
                            }

                            //Numero de cliente
                            dataGridView1.Rows[i].Cells[29].Value = "";




                            //Libero memoria, limpio variables
                            General = null;
                            Emisor = null;
                            Receptor = null;
                            Expedicion = null;
                            NodoComplemento = null;
                            NodoUUID = null;
                            xDoc = null;
                            String QueryCliente = string.Empty;
                            string QueryCreaCliente = string.Empty;

                            //Ya que tengo todos los datos de las facturas busco el id de la empresa
                            for (int j = 0; j < ListaEmpresas.Count; j++)
                            {
                                String[] ArregloEmpresas = ListaEmpresas[j].Split(',');

                                if (dataGridView1.Rows[i].Cells[1].Value.ToString().ToUpper().Equals(ArregloEmpresas[0]))
                                {
                                    dataGridView1.Rows[i].Cells[24].Value = ArregloEmpresas[1];
                                }
                            }
                            //Verifico que haya encontrado dato
                            if (dataGridView1.Rows[i].Cells[24].Value == null)
                            {
                                LogErrores.Message(string.Format("No se encontro el RFC {0} en las Empresas Registradas", dataGridView1.Rows[i].Cells[1].Value.ToString().ToUpper()));
                                FueFactura = false;
                            }
                            else
                            {
                                //Si realmente encontro dato, ahora voy por la parte del cliente
                                Boolean HayDatos = false;
                                QueryCliente = "SELECT Id_Cliente FROM Clientes WHERE RFC = '" + DepuraComilla(dataGridView1.Rows[i].Cells[2].Value.ToString()) + "' AND Razon_Social = '" + DepuraComilla(dataGridView1.Rows[i].Cells[3].Value.ToString()) + "';";
                                SqlCommand ComandoCliente = new SqlCommand(QueryCliente, conn, Transaccion);
                                SqlDataReader LectorCliente = ComandoCliente.ExecuteReader();
                                if (LectorCliente.HasRows)
                                {
                                    while (LectorCliente.Read())
                                    {
                                        dataGridView1.Rows[i].Cells[25].Value = LectorCliente.GetInt32(0);
                                        HayDatos = true;
                                    }
                                }
                                LectorCliente.Close();
                                //Si no hubo datos, creo rapiudamente el cliente
                                if (!HayDatos)
                                {
                                    QueryCreaCliente = "INSERT INTO Clientes(RFC, Razon_Social) VALUES('" + DepuraComilla(dataGridView1.Rows[i].Cells[2].Value.ToString()) + "', '" + DepuraComilla(dataGridView1.Rows[i].Cells[3].Value.ToString()) + "');";
                                    SqlCommand ComandoCrearCliente = new SqlCommand(QueryCreaCliente, conn, Transaccion);
                                    ComandoCrearCliente.ExecuteNonQuery();

                                    //Ya que cree el cliente, busco de nuevo el id
                                    LectorCliente = ComandoCliente.ExecuteReader();
                                    if (LectorCliente.HasRows)
                                    {
                                        while (LectorCliente.Read())
                                        {
                                            dataGridView1.Rows[i].Cells[25].Value = LectorCliente.GetInt32(0);
                                            HayDatos = true;
                                        }
                                    }
                                    LectorCliente.Close();
                                    //checo si no se creo (quiza lo deba quitar)
                                    if (!HayDatos)
                                    {
                                        FueFactura = false;
                                    }
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            LogErrores.Write(string.Format("Error al consultar la información del cliente"), ex);
                            FueFactura = false;
                        }

                        if (FueFactura)
                        {
                            dataGridView1.Rows[i].Cells[26].Value = "1";
                            dataGridView1.Rows[i + 1].Cells[26].Value = "1";
                            i = i++;
                        }
                        else
                        {
                            dataGridView1.Rows[i].Cells[26].Value = "0";
                            dataGridView1.Rows[i + 1].Cells[26].Value = "0";
                        }
                    }
                }

                //Ya que analicé todas, les cambio el nombre para la ruta donde van a quedar, LA CARPETA DEBE EXISTIR
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    //Checo si es correcta
                    if (dataGridView1.Rows[i].Cells[26].Value.ToString().Equals("1"))
                    {
                        //Checo si es PDF o XML
                        //Si es PDF, el residuo será diferente a 0
                        if (i % 2 != 0)
                        {
                            dataGridView1.Rows[i].Cells[28].Value = RutaMover + @"\" + dataGridView1.Rows[i - 1].Cells[27].Value + ".pdf";
                        }
                        else
                        {
                            dataGridView1.Rows[i].Cells[28].Value = RutaMover + @"\" + dataGridView1.Rows[i].Cells[27].Value + ".xml";
                        }

                        //Ya que se como se llama su nombre, las muevo
                        //File.Copy(dataGridView1.Rows[i].Cells[0].Value.ToString(), dataGridView1.Rows[i].Cells[28].Value.ToString());
                    }
                }


                //Limpio variables nulas que pudieron haber salido
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        if (dataGridView1.Rows[i].Cells[j].Value == null)
                        {
                            dataGridView1.Rows[i].Cells[j].Value = "";
                        }
                    }
                }

                //Ya que asigne nombres, ahora si muevo            
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    //Checo si es correcta
                    if (dataGridView1.Rows[i].Cells[26].Value.ToString().Equals("1"))
                    {
                        //Checo que sea entrada de XML
                        if (i % 2 == 0)
                        {
                            //Checo que no exista el UUID
                            if (!File.Exists(dataGridView1.Rows[i].Cells[28].Value.ToString()))
                            {
                                //Si no existe guardo datos
                                String QueryInserta = "";
                                QueryInserta += "INSERT INTO Facturas(Id_Empresa, Id_Cliente, Razon_Social, Calle, Numero_Exterior, Colonia, Localidad, Municipio, Estado, País, Codigo_Postal, Serie, Folio, Lugar_Expedicion, Tipo_Comprobante, Fecha_Emision, Condiciones_Pago, Metodo_Pago, Moneda, Tipo_Cambio, Sub_Total, IVA, Total, Ruta_XML, Numero_Cliente, Ruta_PDF) VALUES(";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[24].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[25].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[3].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[4].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[5].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[6].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[7].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[8].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[9].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[10].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[11].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[12].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[13].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[14].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[15].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[16].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[17].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[18].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[19].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[20].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[21].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[22].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[23].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[28].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i].Cells[29].Value.ToString()) + "', ";
                                QueryInserta += "'" + DepuraComilla(dataGridView1.Rows[i + 1].Cells[28].Value.ToString()) + "');";
                                SqlCommand ComandoIserta = new SqlCommand(QueryInserta, conn, Transaccion);
                                ComandoIserta.ExecuteNonQuery();

                                //Ya que guarde muevo el pdf y el xml
                                //MODIFICACION SANDVIK, SOLO COPIO
                                /*File.Copy(dataGridView1.Rows[i].Cells[0].Value.ToString(), dataGridView1.Rows[i].Cells[28].Value.ToString());
                                File.Copy(dataGridView1.Rows[i + 1].Cells[0].Value.ToString(), dataGridView1.Rows[i + 1].Cells[28].Value.ToString());*/
                                //MODIFICACION R.A., MUEVO
                                File.Move(dataGridView1.Rows[i].Cells[0].Value.ToString(), dataGridView1.Rows[i].Cells[28].Value.ToString());
                                File.Move(dataGridView1.Rows[i + 1].Cells[0].Value.ToString(), dataGridView1.Rows[i + 1].Cells[28].Value.ToString());
                            }
                            else
                            {
                                //Si ya existe el archivo, elimino de la carpeta
                                //MODIFICACION SANDVIK
                                //SI YA EXISTE IGNORO


                                File.Delete(dataGridView1.Rows[i].Cells[0].Value.ToString());
                                File.Delete(dataGridView1.Rows[i + 1].Cells[0].Value.ToString());

                            }
                        }
                    }
                }

                //Actualizo la fecha
                String QueryActualizaFecha = "UPDATE Fechas SET Fecha = GETDATE();";
                SqlCommand ComandoActualizaFecha = new SqlCommand(QueryActualizaFecha, conn, Transaccion);
                ComandoActualizaFecha.ExecuteNonQuery();

                //Finalizo transaccion
                Transaccion.Commit();
            }
            catch (Exception ex)
            {
                LogErrores.Write(string.Format("Error en el método LogicaGeneral con la ruta: {0}", FPrincipal.RutaFacturas), ex);

                if (Transaccion != null)
                {
                    Transaccion.Rollback();
                }

                GuardarLog("Guardar Facturas", ex.Message);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
            }
        }
        private String DepuraComilla(String Texto)
        {
            try
            {
                Texto.Replace("'", "''").Trim().ToUpper();
                return Texto;
            }
            catch
            {
                return Texto;
            }
        }
        private void FPrincipal_Load(object sender, EventArgs e)
        {
            
            CheckForIllegalCrossThreadCalls = false;
            //Contador.Start();
            Asincrono.RunWorkerAsync(2000);
            //ContadorValidacion.Start();
        }

        private void Contador_Tick(object sender, EventArgs e)
        {
            if(!Asincrono.IsBusy)
            {
                Asincrono.RunWorkerAsync(2000);
            }
        }

        private void Asincrono_DoWork(object sender, DoWorkEventArgs e)
        {
            //Paramos contador
            Contador.Stop();

            //Borramos residuos
            dataGridView1.Rows.Clear();

            LogicaGeneral();
        }

        private void Asincrono_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Borramos residuos
            dataGridView1.Rows.Clear();
            ListaXML = null;
            //Iniciamos contador
            Contador.Start();
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Ocultar()
        {
            IconoApp.Icon = this.Icon;
            IconoApp.ContextMenuStrip = this.MenuContextual;
            IconoApp.Text = Application.ProductName;
            IconoApp.Visible = true;
            this.Visible = false;

            IconoApp.BalloonTipIcon = ToolTipIcon.Info;
            IconoApp.BalloonTipTitle = Application.ProductName;
            IconoApp.BalloonTipText = "Sniffer Valsa se ejecutará en segundo plano";
            IconoApp.ShowBalloonTip(8);
        }


        private void FPrincipal_Activated(object sender, EventArgs e)
        {
            Ocultar();
        }

        private void ValidarLote()
        {
            SqlConnection conn = new SqlConnection(FPrincipal.Conexion);
            SqlTransaction Transaccion = null;
            List<String> Lista = new List<String>();
            try
            {
                conn.Open();
                Transaccion = conn.BeginTransaction();

                //Checo que ya toque validar
                int Dias = 0;
                String QueryToca = "SELECT CAST((DATEDIFF(MINUTE, Fecha, GETDATE()) / 1440) AS INT) FROM FechasValidacion;";
                SqlCommand ComandoToca = new SqlCommand(QueryToca, conn, Transaccion);
                SqlDataReader LectorToca = ComandoToca.ExecuteReader();
                if(LectorToca.HasRows)
                {
                    while (LectorToca.Read())
                    {
                        Dias = LectorToca.GetInt32(0);
                    }
                }
                LectorToca.Close();

                if(Dias > 0)
                {
                    //Primero checo que mes es (si pasa de mayo, solo valido año en curso)
                    String QueryObtieneMes = "SELECT MONTH(GETDATE());";
                    SqlCommand ComandoObtieneMes = new SqlCommand(QueryObtieneMes, conn, Transaccion);
                    SqlDataReader LectorObtieneMes = ComandoObtieneMes.ExecuteReader();
                    int Mes = 5;
                    if (LectorObtieneMes.HasRows)
                    {
                        while (LectorObtieneMes.Read())
                        {
                            Mes = LectorObtieneMes.GetInt32(0);
                        }
                    }
                    LectorObtieneMes.Close();

                    String QueryObtieneQR = "";
                    if (Mes >= 5)
                    {
                        QueryObtieneQR = "SELECT Facturas.Id_Factura, CONCAT('?re=', Empresa.RFC, '&rr=', Clientes.RFC, '&tt=', Facturas.Total, '&id=', REPLACE(REVERSE(SUBSTRING(REVERSE(Facturas.Ruta_XML), 0, CHARINDEX('\\', REVERSE(Facturas.Ruta_XML)))), '.xml', '')) FROM Facturas INNER JOIN Clientes ON Clientes.Id_Cliente = Facturas.Id_Cliente INNER JOIN Empresa ON Empresa.Id_Empresa = Facturas.Id_Empresa WHERE YEAR(Fecha_Emision) = (YEAR(GETDATE()));";
                    }
                    else
                    {
                        QueryObtieneQR = "SELECT Facturas.Id_Factura, CONCAT('?re=', Empresa.RFC, '&rr=', Clientes.RFC, '&tt=', Facturas.Total, '&id=', REPLACE(REVERSE(SUBSTRING(REVERSE(Facturas.Ruta_XML), 0, CHARINDEX('\\', REVERSE(Facturas.Ruta_XML)))), '.xml', '')) FROM Facturas INNER JOIN Clientes ON Clientes.Id_Cliente = Facturas.Id_Cliente INNER JOIN Empresa ON Empresa.Id_Empresa = Facturas.Id_Empresa WHERE YEAR(Fecha_Emision) = (YEAR(GETDATE()) - 1);";
                    }

                    SqlCommand ComandoObtieneQR = new SqlCommand(QueryObtieneQR, conn, Transaccion);
                    SqlDataReader LectorObtieneQR = ComandoObtieneQR.ExecuteReader();
                    if (LectorObtieneQR.HasRows)
                    {
                        while (LectorObtieneQR.Read())
                        {
                            Lista.Add(LectorObtieneQR.GetInt32(0) + "@" + LectorObtieneQR.GetString(1));
                        }
                    }
                    LectorObtieneQR.Close();
                }
                Transaccion.Commit();
                conn.Close();
            }
            catch(Exception ex4)
            {
                Lista = null;
                try
                {
                    Transaccion.Rollback();
                    conn.Close();
                }
                catch
                {

                }
                GuardarLog("ValidarLote", ex4.Message);
            }

            //Validamos
            if(Lista != null)
            {
                if(Lista.Count > 0)
                {
                    for(int i = 0; i < Lista.Count; i++)
                    {
                        Lista[i] = Lista[i] + "@" + EsValido(Lista[i].Split('@')[1]);
                    }
                }
            }

            //Guardamos en base de datos
            if (Lista != null)
            {
                if (Lista.Count > 0)
                {
                    Transaccion = null;
                    try
                    {
                        conn.Open();
                        Transaccion = conn.BeginTransaction();

                        for (int i = 0; i < Lista.Count; i++)
                        {
                            String[] ArregloTemporal = Lista[i].Split('@');
                            String Query = "UPDATE Facturas SET Status = '" + ArregloTemporal[2].ToUpper() + "' WHERE Id_Factura = '" + ArregloTemporal[0] + "';";
                            SqlCommand Comando = new SqlCommand(Query, conn, Transaccion);
                            Comando.ExecuteNonQuery();
                        }

                        //Colocamos como actualizado
                        String QueryActualiza = "UPDATE FechasValidacion SET Fecha = CONCAT(CONVERT(VARCHAR, GETDATE(), 112), ' 23:00:00'), VecesModificada = VecesModificada + 1;";
                        SqlCommand ComandoActualiza = new SqlCommand(QueryActualiza, conn, Transaccion);
                        ComandoActualiza.ExecuteNonQuery();

                        Transaccion.Commit();
                        conn.Close();
                    }
                    catch(Exception ex5)
                    {
                        try
                        {
                            Transaccion.Rollback();
                            conn.Close();
                        }
                        catch
                        {
                            
                        }
                        GuardarLog("Validar Lote", ex5.Message);
                    }
                }
            }

            //Limpio variable 
            Lista = null;
        }

        private String EsValido(String DatosQR)
        {
            String Retorno = "";
            try
            {
                ConsultaCFDIService Servicio = new ConsultaCFDIService();
                Acuse Resultados = Servicio.Consulta(DatosQR);
                Retorno = Resultados.Estado;
            }
            catch
            {
                Retorno = "Error";
            }
            if (String.IsNullOrEmpty(Retorno))
            {
                Retorno = "Error";
            }
            return Retorno;
        }

        private void ContadorValidacion_Tick(object sender, EventArgs e)
        {
            AsincronoValidacion.RunWorkerAsync(2000);
        }

        private void AsincronoValidacion_DoWork(object sender, DoWorkEventArgs e)
        {
            ContadorValidacion.Stop();

            //Checamos si corre
            ValidarLote();
        }

        private void AsincronoValidacion_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ContadorValidacion.Start();
        }

        private void GuardarLog(String Modulo, String Error)
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(FPrincipal.Conexion);

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                String Query = "INSERT INTO RegistroLogs(Modulo, Error) VALUES('" + DepuraComilla(Modulo) + "', '" + DepuraComilla(Error) + "');";
                SqlCommand Comando = new SqlCommand(Query, conn);
                Comando.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                LogErrores.Write(string.Format("Error en el método GuardarLog con el Modulo {0} y el Error {1}", Modulo, Error), ex);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
            }
        }

        private void DescargarFTP(String FtpUrl, String FileNameToDownload, String userName, String password)
        {

            try
            {
                string ResponseDescription = "";
                string PureFileName = new FileInfo(FileNameToDownload).Name;
                string DownloadedFilePath = RutaFacturas + "/" + PureFileName;
                string downloadUrl = String.Format("{0}/{1}", FtpUrl, FileNameToDownload);
                FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(downloadUrl);
                req.Method = WebRequestMethods.Ftp.DownloadFile;
                req.Credentials = new NetworkCredential(userName, password);
                req.UseBinary = true;
                req.Proxy = null;
                FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                Stream stream = response.GetResponseStream();
                byte[] buffer = new byte[2048];
                FileStream fs = new FileStream(DownloadedFilePath, FileMode.Create);
                int ReadCount = stream.Read(buffer, 0, buffer.Length);
                while (ReadCount > 0)
                {
                    fs.Write(buffer, 0, ReadCount);
                    ReadCount = stream.Read(buffer, 0, buffer.Length);
                }
                ResponseDescription = response.StatusDescription;
                fs.Close();
                stream.Close();
            }
            catch(Exception ex8)
            {
                GuardarLog("Descargar FTP", ex8.Message);
            }
        }

        private String ObtenerArchivo(String Nombre)
        {
            if (Nombre != null)
            {
                return Path.GetFileName(Nombre);
            }
            else
            {
                return "";
            }
        }


        /*Aqui empieza lo del PDF*/
        private DatosFactura LeerFactura(String Path)
        {
            DatosFactura DatFac = new DatosFactura();
            try
            {
                //Leemos XML
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(Path);

                //Inicializo nodo general (Comprobante)
                XmlNodeList General = xDoc.GetElementsByTagName("cfdi:Comprobante");

                //Para obtener datos del emisor
                XmlNodeList Emisor = ((XmlElement)General[0]).GetElementsByTagName("cfdi:Emisor");
                foreach (XmlElement Nodo in Emisor)
                {
                    DatFac.RFCEmisor = Nodo.GetAttribute("rfc");
                    DatFac.NombreEmisor = Nodo.GetAttribute("nombre");
                }

                //Domicilio fiscal del emisor
                XmlNodeList DomicilioEmisor = ((XmlElement)Emisor[0]).GetElementsByTagName("cfdi:DomicilioFiscal");
                if (DomicilioEmisor != null)
                {
                    foreach (XmlElement Nodo in DomicilioEmisor)
                    {
                        DatFac.CalleEmisor = Nodo.GetAttribute("calle");
                        DatFac.NumeroExteriorEmisor = Nodo.GetAttribute("noExterior");
                        DatFac.NumeroInteriorEmisor = Nodo.GetAttribute("noInterior");
                        DatFac.ColoniaEmisor = Nodo.GetAttribute("colonia");
                        DatFac.MunicipioEmisor = Nodo.GetAttribute("municipio");
                        DatFac.EstadoEmisor = Nodo.GetAttribute("estado");
                        DatFac.PaisEmisor = Nodo.GetAttribute("pais");
                    }
                }

                //Domicilio del emisor
                String DomicilioFiscalEmisor = "";
                if (Depurar(DatFac.CalleEmisor).Length != 0)
                {
                    DomicilioFiscalEmisor = Depurar(DatFac.CalleEmisor);
                }
                if (Depurar(DatFac.NumeroExteriorEmisor).Length != 0)
                {
                    if (DomicilioFiscalEmisor.Length != 0)
                    {
                        DomicilioFiscalEmisor += " #" + Depurar(DatFac.NumeroExteriorEmisor);
                    }
                    else
                    {
                        DomicilioFiscalEmisor = Depurar(DatFac.NumeroExteriorEmisor);
                    }
                }
                if (Depurar(DatFac.NumeroInteriorEmisor).Length != 0)
                {
                    if (DomicilioFiscalEmisor.Length != 0)
                    {
                        DomicilioFiscalEmisor += " INT. " + Depurar(DatFac.NumeroInteriorEmisor);
                    }
                    else
                    {
                        DomicilioFiscalEmisor = Depurar(DatFac.NumeroInteriorEmisor);
                    }
                }
                if (Depurar(DatFac.ColoniaEmisor).Length != 0)
                {
                    if (DomicilioFiscalEmisor.Length != 0)
                    {
                        DomicilioFiscalEmisor += ", " + Depurar(DatFac.ColoniaEmisor);
                    }
                    else
                    {
                        DomicilioFiscalEmisor = Depurar(DatFac.ColoniaEmisor);
                    }
                }
                DatFac.DomicilioEmisor = DomicilioFiscalEmisor;

                DomicilioFiscalEmisor = "";
                if (Depurar(DatFac.MunicipioEmisor).Length != 0)
                {
                    DomicilioFiscalEmisor = Depurar(DatFac.MunicipioEmisor);
                }
                if (Depurar(DatFac.EstadoEmisor).Length != 0)
                {
                    if (DomicilioFiscalEmisor.Length != 0)
                    {
                        DomicilioFiscalEmisor += ", " + Depurar(DatFac.EstadoEmisor);
                    }
                    else
                    {
                        DomicilioFiscalEmisor = Depurar(DatFac.EstadoEmisor);
                    }
                }
                if (Depurar(DatFac.PaisEmisor).Length != 0)
                {
                    if (DomicilioFiscalEmisor.Length != 0)
                    {
                        DomicilioFiscalEmisor += ", " + Depurar(DatFac.PaisEmisor);
                    }
                    else
                    {
                        DomicilioFiscalEmisor = Depurar(DatFac.PaisEmisor);
                    }
                }
                DatFac.DomicilioEmisorDos = DomicilioFiscalEmisor;

                //Regimen Fiscal
                XmlNodeList RegimenFiscal = ((XmlElement)Emisor[0]).GetElementsByTagName("cfdi:RegimenFiscal");
                if (RegimenFiscal != null)
                {
                    foreach (XmlElement Nodo in RegimenFiscal)
                    {
                        DatFac.RegimenFiscal = Nodo.GetAttribute("Regimen");
                    }
                }


                //Numero de factura y lugar de expedicion
                foreach (XmlElement Nodo in General)
                {
                    DatFac.Serie = Nodo.GetAttribute("serie");
                    DatFac.Folio = Nodo.GetAttribute("folio");
                    DatFac.LugarExpedicion = Nodo.GetAttribute("LugarExpedicion");
                    DatFac.FormaPago = Nodo.GetAttribute("formaDePago");
                    DatFac.MetodoPago = Nodo.GetAttribute("metodoDePago");
                    DatFac.NumeroCuenta = Nodo.GetAttribute("NumCtaPago");
                    DatFac.Total = Nodo.GetAttribute("total");
                    DatFac.SubTotal = Nodo.GetAttribute("subTotal");
                    DatFac.Moneda = Nodo.GetAttribute("moneda");
                    DatFac.SelloEmisor = Nodo.GetAttribute("sello");
                    DatFac.CertificadoEmisor = Nodo.GetAttribute("noCertificado");
                    DatFac.Version = Nodo.GetAttribute("version");
                }

                if (Depurar(DatFac.LugarExpedicion).Length != 0)
                {
                    DatFac.LugarExpedicion = "\nEXPEDIDO EN " + DatFac.LugarExpedicion;
                }

                //Numero de certificado y fecha de facturacion
                XmlNodeList Complemento = ((XmlElement)General[0]).GetElementsByTagName("cfdi:Complemento");
                XmlNodeList DatosSAT = ((XmlElement)Complemento[0]).GetElementsByTagName("tfd:TimbreFiscalDigital");
                if (DatosSAT != null)
                {
                    foreach (XmlElement Nodo in DatosSAT)
                    {
                        DatFac.NumeroCertificado = Nodo.GetAttribute("noCertificadoSAT");
                        DatFac.FechaTimbrado = Nodo.GetAttribute("FechaTimbrado");
                        DatFac.UUID = Nodo.GetAttribute("UUID");
                        DatFac.SelloSAT = Nodo.GetAttribute("selloCFD");

                    }
                }

                //Ahora los datos del receptor
                XmlNodeList Receptor = ((XmlElement)General[0]).GetElementsByTagName("cfdi:Receptor");
                foreach (XmlElement Nodo in Receptor)
                {
                    DatFac.RFCReceptor = Nodo.GetAttribute("rfc");
                    DatFac.NombreReceptor = Nodo.GetAttribute("nombre");
                }

                //Domicilio fiscal del receptor
                XmlNodeList DomicilioReceptor = ((XmlElement)Receptor[0]).GetElementsByTagName("cfdi:Domicilio");
                if (DomicilioReceptor != null)
                {
                    foreach (XmlElement Nodo in DomicilioReceptor)
                    {
                        DatFac.CalleReceptor = Nodo.GetAttribute("calle");
                        DatFac.NumeroExteriorReceptor = Nodo.GetAttribute("noExterior");
                        DatFac.NumeroInteriorReceptor = Nodo.GetAttribute("noInterior");
                        DatFac.ColoniaReceptor = Nodo.GetAttribute("colonia");
                        DatFac.MunicipioReceptor = Nodo.GetAttribute("municipio");
                        DatFac.EstadoReceptor = Nodo.GetAttribute("estado");
                        DatFac.PaisReceptor = Nodo.GetAttribute("pais");
                        DatFac.CPReceptor = Nodo.GetAttribute("codigoPostal");
                    }
                }

                String DomicilioFiscalReceptor = "";
                if (Depurar(DatFac.CalleReceptor).Length != 0)
                {
                    DomicilioFiscalReceptor = Depurar(DatFac.CalleReceptor);
                }
                if (Depurar(DatFac.NumeroExteriorReceptor).Length != 0)
                {
                    if (DomicilioFiscalReceptor.Length != 0)
                    {
                        DomicilioFiscalReceptor += " #" + Depurar(DatFac.NumeroExteriorReceptor);
                    }
                    else
                    {
                        DomicilioFiscalReceptor = Depurar(DatFac.NumeroExteriorReceptor);
                    }
                }
                if (Depurar(DatFac.NumeroInteriorReceptor).Length != 0)
                {
                    if (DomicilioFiscalReceptor.Length != 0)
                    {
                        DomicilioFiscalReceptor += " INT. " + Depurar(DatFac.NumeroInteriorReceptor);
                    }
                    else
                    {
                        DomicilioFiscalReceptor = Depurar(DatFac.NumeroInteriorReceptor);
                    }
                }
                DatFac.DomicilioReceptor = DomicilioFiscalReceptor;
                DomicilioFiscalReceptor = "";

                if (Depurar(DatFac.ColoniaReceptor).Length != 0)
                {
                    DomicilioFiscalReceptor = "COLONIA " + Depurar(DatFac.ColoniaReceptor);
                }
                if (Depurar(DatFac.MunicipioReceptor).Length != 0)
                {
                    if (DomicilioFiscalReceptor.Length != 0)
                    {
                        DomicilioFiscalReceptor += ", " + Depurar(DatFac.MunicipioReceptor);
                    }
                    else
                    {
                        DomicilioFiscalReceptor = Depurar(DatFac.MunicipioReceptor);
                    }
                }
                DatFac.DomicilioReceptorDos = DomicilioFiscalReceptor;
                DomicilioFiscalReceptor = "";

                if (Depurar(DatFac.CPReceptor).Length != 0)
                {
                    DomicilioFiscalReceptor = "CP " + Depurar(DatFac.CPReceptor);
                }
                DatFac.DomicilioReceptorTres = DomicilioFiscalReceptor;
                DomicilioFiscalReceptor = "";

                if (Depurar(DatFac.EstadoReceptor).Length != 0)
                {
                    DomicilioFiscalReceptor = Depurar(DatFac.EstadoReceptor);
                }
                if (Depurar(DatFac.PaisReceptor).Length != 0)
                {
                    if (DomicilioFiscalReceptor.Length != 0)
                    {
                        DomicilioFiscalReceptor += ", " + Depurar(DatFac.PaisReceptor);
                    }
                    else
                    {
                        DomicilioFiscalReceptor = Depurar(DatFac.PaisReceptor);
                    }
                }
                DatFac.DomicilioReceptorCuatro = DomicilioFiscalReceptor;


                //Las partidas
                XmlNodeList Partidas = ((XmlElement)General[0]).GetElementsByTagName("cfdi:Conceptos");
                XmlNodeList Producto = ((XmlElement)Partidas[0]).GetElementsByTagName("cfdi:Concepto");
                List<Partidas> ListaProductos = new List<Partidas>();
                foreach (XmlElement Nodo in Producto)
                {
                    Partidas Part = new Partidas();
                    Part.PrecioUnitario = Nodo.GetAttribute("valorUnitario");
                    Part.Total = Nodo.GetAttribute("importe");
                    Part.Descripcion = Nodo.GetAttribute("descripcion");
                    Part.Unidad = Nodo.GetAttribute("unidad");
                    Part.Cantidad = Nodo.GetAttribute("cantidad");
                    ListaProductos.Add(Part);
                }
                DatFac.Productos = ListaProductos;

                //El IVA
                XmlNodeList Impuestos = ((XmlElement)General[0]).GetElementsByTagName("cfdi:Impuestos");
                XmlNodeList Traslados = ((XmlElement)Impuestos[0]).GetElementsByTagName("cfdi:Traslados");
                XmlNodeList IVA = ((XmlElement)Traslados[0]).GetElementsByTagName("cfdi:Traslado");
                foreach (XmlElement Nodo in IVA)
                {
                    if (Nodo.GetAttribute("impuesto").Trim().ToUpper().Equals("IVA"))
                    {
                        DatFac.IVA = Nodo.GetAttribute("importe");
                    }
                }

                CadenaOriginal CadOri = new CadenaOriginal();
                DatFac.CadenaOriginal = CadOri.GeneradorCadenas(Path, DatFac.Version);
            }
            catch(Exception ex)
            {
                LogErrores.Write(string.Format("Error en el método LeerFactura con la ruta: {0}", Path), ex);
                DatFac = null;
            }

            return DatFac;
        }

        private String Depurar(String Texto)
        {
            if (Texto == null)
            {
                return "";
            }
            else
            {
                return Texto.ToUpper();
            }
        }

        private void Crear(String PathXML)
        {
            try
            {
                String PathPDF = PathXML.Substring(0, PathXML.LastIndexOf('.')) + ".pdf";

                if (!File.Exists(PathPDF))
                {
                    DatosFactura DatFac = LeerFactura(PathXML);

                    if (DatFac != null)
                    {
                        Document documento = new Document(PageSize.LETTER);
                        documento.SetPageSize(PageSize.LETTER);
                        documento.SetMargins(10, 10, 30, 30);

                        PdfWriter writer = PdfWriter.GetInstance(documento, new FileStream(PathPDF, FileMode.Create));
                        documento.Open();

                        //Añadimos logo de PROAS


                        // Insertamos la imagen en el documento
                        documento.Add(TablaCabezal(DatFac));
                        documento.Add(new Paragraph("\n"));
                        documento.Add(TablaReceptor(DatFac));
                        documento.Add(TablaExpedidoEn(DatFac));
                        documento.Add(TablaProductos(DatFac));

                        documento.Add(TablaFormaDePagoYOtros(DatFac));
                        documento.Add(TablaTotales(DatFac));
                        documento.Add(new Paragraph("\n"));
                        documento.Add(TablaBottom(DatFac));
                        documento.Close();

                        writer.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                LogErrores.Write(string.Format("Error en el método Crear con la ruta: {0}", PathXML), ex);
            }
        }

        private PdfPTable TablaCabezal(DatosFactura DatFac)
        {
            try
            {
                //Creamos una tabla de 3 celdas
                PdfPTable tabla = new PdfPTable(3);
                //Damos fuente
                iTextSharp.text.Font fuente = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 5, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                iTextSharp.text.Font fuenteNegrita = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 5, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                //Damos dimensiones
                tabla.SetTotalWidth((new float[] { 90, 265, 115 }));
                tabla.LockedWidth = true;
                iTextSharp.text.Image LogoPROAS = iTextSharp.text.Image.GetInstance("Logo.png");
                LogoPROAS.ScalePercent(20f);
                //Invocamos al atributo celda para usarlo en general
                PdfPCell celda;
                //celdas que contienen el logo de proas
                celda = new PdfPCell(LogoPROAS);
                celda.Rowspan = 8;
                celda.VerticalAlignment = Element.ALIGN_MIDDLE;
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Nombre de razon social
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.NombreEmisor), fuente));
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Factura número titulo
                celda = new PdfPCell(new Paragraph("FACTURA NÚMERO:", fuenteNegrita));
                celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Calle y colonia de emisor
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.DomicilioEmisor), fuente));
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Número de factura
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.Serie) + " " + Depurar(DatFac.Folio), fuente));
                celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Municipio y estado
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.DomicilioEmisorDos), fuente));
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Espacio en blanco entre el folio fiscal
                celda = new PdfPCell(new Paragraph("", fuente));
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Espacio en blanco entre el municipio
                celda = new PdfPCell(new Paragraph("", fuente));
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Numero de serie del certificado del sat título
                celda = new PdfPCell(new Paragraph("NO. DE SERIE DEL CERTIFICADO DEL SAT:", fuenteNegrita));
                celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //País
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.RFCEmisor), fuente));
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Numero de serie del certificado del sat
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.NumeroCertificado), fuente));
                celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Espaciado entre ambas celdas
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.RegimenFiscal), fuente));
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                celda = new PdfPCell(new Paragraph("\n", fuente));
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //RFC de erit
                celda = new PdfPCell(new Paragraph("\n", fuente));
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Fecha y hora de facturación título
                celda = new PdfPCell(new Paragraph("FECHA Y HORA DE FACTURACIÓN:", fuenteNegrita));
                celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Regimen fiscal
                celda = new PdfPCell(new Paragraph("", fuente));
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                //Fecha y hora de facturación
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.FechaTimbrado), fuente));
                celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);
                return tabla;
            }
            catch
            {
                return null;
            }
        }

        private PdfPTable TablaReceptor(DatosFactura DatFac)
        {
            try
            {
                PdfPTable tabla = new PdfPTable(2);
                tabla.SetTotalWidth((new float[] { 50, 500 }));
                tabla.LockedWidth = true;
                //Damos fuente
                iTextSharp.text.Font fuente = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 5, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                iTextSharp.text.Font fuenteNegrita = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 5, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                PdfPCell celda;

                celda = new PdfPCell(new Paragraph("CLIENTE:", fuenteNegrita));
                celda.VerticalAlignment = Element.ALIGN_TOP;
                celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                celda.Border = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph(Depurar(DatFac.NombreReceptor), fuente));
                celda.VerticalAlignment = Element.ALIGN_TOP;
                celda.Border = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph("RFC:", fuenteNegrita));
                celda.VerticalAlignment = Element.ALIGN_TOP;
                celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                celda.Border = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph(Depurar(DatFac.RFCReceptor), fuente));
                celda.VerticalAlignment = Element.ALIGN_TOP;
                celda.Border = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph("DIRECCIÓN:", fuenteNegrita));
                celda.VerticalAlignment = Element.ALIGN_TOP;
                celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                celda.Border = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph(Depurar(DatFac.DomicilioReceptor), fuente));
                celda.VerticalAlignment = Element.ALIGN_TOP;
                celda.Border = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph("", fuenteNegrita));
                celda.VerticalAlignment = Element.ALIGN_TOP;
                celda.Border = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph(Depurar(DatFac.DomicilioReceptorDos), fuente));
                celda.VerticalAlignment = Element.ALIGN_TOP;
                celda.Border = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph("", fuenteNegrita));
                celda.VerticalAlignment = Element.ALIGN_TOP;
                celda.Border = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph(Depurar(DatFac.DomicilioReceptorTres), fuente));
                celda.VerticalAlignment = Element.ALIGN_TOP;
                celda.Border = 0;
                tabla.AddCell(celda);


                celda = new PdfPCell(new Paragraph("", fuenteNegrita));
                celda.VerticalAlignment = Element.ALIGN_TOP;
                celda.Border = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph(Depurar(DatFac.DomicilioReceptorCuatro), fuente));
                celda.VerticalAlignment = Element.ALIGN_TOP;
                celda.Border = 0;
                tabla.AddCell(celda);

                return tabla;
            }
            catch
            {
                return null;
            }
        }

        private PdfPTable TablaExpedidoEn(DatosFactura DatFac)
        {
            try
            {
                PdfPTable tabla = new PdfPTable(1);
                tabla.SetTotalWidth((new float[] { 550 }));
                tabla.LockedWidth = true;
                iTextSharp.text.Font fuente = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 5, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                PdfPCell celda;

                celda = new PdfPCell(new Paragraph(Depurar(DatFac.LugarExpedicion), fuente));
                celda.BorderWidthBottom = 1;
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthTop = 1;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);

                return tabla;
            }
            catch
            {
                return null;
            }
        }

        private PdfPTable TablaProductos(DatosFactura DatFac)
        {
            try
            {
                //Creamos una tabla de 3 celdas
                PdfPTable tabla = new PdfPTable(5);
                iTextSharp.text.Font fuente = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 5, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                iTextSharp.text.Font fuenteTitulos = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 5, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
                tabla.SetTotalWidth((new float[] { 40, 40, 390, 40, 40 }));
                tabla.LockedWidth = true;
                PdfPCell celda;
                celda = new PdfPCell(new Paragraph("CANTIDAD", fuenteTitulos));
                celda.BackgroundColor = BaseColor.BLACK;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 0;
                celda.BorderColorLeft = BaseColor.BLACK;
                celda.BorderColorRight = BaseColor.WHITE;
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                tabla.AddCell(celda);
                celda = new PdfPCell(new Paragraph("UNIDAD", fuenteTitulos));
                celda.BackgroundColor = BaseColor.BLACK;
                celda.BorderColorLeft = BaseColor.WHITE;
                celda.BorderColorRight = BaseColor.WHITE;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 0;
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                tabla.AddCell(celda);
                celda = new PdfPCell(new Paragraph("DESCRIPCIÓN", fuenteTitulos));
                celda.BackgroundColor = BaseColor.BLACK;
                celda.BorderColorLeft = BaseColor.WHITE;
                celda.BorderColorRight = BaseColor.WHITE;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 0;
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                tabla.AddCell(celda);
                celda = new PdfPCell(new Paragraph("P. UNITARIO", fuenteTitulos));
                celda.BackgroundColor = BaseColor.BLACK;
                celda.BorderColorLeft = BaseColor.WHITE;
                celda.BorderColorRight = BaseColor.WHITE;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 0;
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                tabla.AddCell(celda);
                celda = new PdfPCell(new Paragraph("IMPORTE", fuenteTitulos));
                celda.BackgroundColor = BaseColor.BLACK;
                celda.BorderColorLeft = BaseColor.WHITE;
                celda.BorderColorRight = BaseColor.BLACK;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                tabla.AddCell(celda);


                int i = 0;
                foreach (Partidas Part in DatFac.Productos)
                {
                    i = i + 1;
                    celda = new PdfPCell(new Paragraph(Depurar(Part.Cantidad), fuente));
                    celda.BorderWidthBottom = 0;
                    celda.BorderWidthTop = 0;
                    celda.BorderWidthLeft = 1;
                    celda.BorderWidthRight = 0;
                    celda.HorizontalAlignment = Element.ALIGN_CENTER;
                    tabla.AddCell(celda);
                    celda = new PdfPCell(new Paragraph(Depurar(Part.Unidad), fuente));
                    celda.BorderWidthBottom = 0;
                    celda.BorderWidthTop = 0;
                    celda.BorderWidthLeft = 1;
                    celda.BorderWidthRight = 0;
                    celda.HorizontalAlignment = Element.ALIGN_CENTER;
                    tabla.AddCell(celda);
                    celda = new PdfPCell(new Paragraph(Depurar(Part.Descripcion), fuente));
                    celda.BorderWidthBottom = 0;
                    celda.BorderWidthTop = 0;
                    celda.BorderWidthLeft = 1;
                    celda.BorderWidthRight = 0;
                    celda.HorizontalAlignment = Element.ALIGN_LEFT;
                    tabla.AddCell(celda);
                    celda = new PdfPCell(new Paragraph(Depurar(Part.PrecioUnitario), fuente));
                    celda.BorderWidthBottom = 0;
                    celda.BorderWidthTop = 0;
                    celda.BorderWidthLeft = 1;
                    celda.BorderWidthRight = 0;
                    celda.HorizontalAlignment = Element.ALIGN_CENTER;
                    tabla.AddCell(celda);
                    celda = new PdfPCell(new Paragraph(Depurar(Part.Total), fuente));
                    celda.BorderWidthBottom = 0;
                    celda.BorderWidthTop = 0;
                    celda.BorderWidthLeft = 1;
                    celda.BorderWidthRight = 1;
                    celda.HorizontalAlignment = Element.ALIGN_CENTER;
                    tabla.AddCell(celda);
                }
                if (i < 34)
                {
                    for (int j = i; j < 38; j++)
                    {
                        celda = new PdfPCell(new Paragraph("\n", fuente));
                        if (j == 37)
                        {
                            celda.BorderWidthBottom = 1;
                        }
                        else
                        {
                            celda.BorderWidthBottom = 0;
                        }
                        celda.BorderWidthTop = 0;
                        celda.BorderWidthLeft = 1;
                        celda.BorderWidthRight = 0;
                        celda.HorizontalAlignment = Element.ALIGN_CENTER;
                        tabla.AddCell(celda);

                        celda = new PdfPCell(new Paragraph("", fuente));
                        if (j == 37)
                        {
                            celda.BorderWidthBottom = 1;
                        }
                        else
                        {
                            celda.BorderWidthBottom = 0;
                        }
                        celda.BorderWidthTop = 0;
                        celda.BorderWidthLeft = 1;
                        celda.BorderWidthRight = 0;
                        celda.HorizontalAlignment = Element.ALIGN_CENTER;
                        tabla.AddCell(celda);

                        celda = new PdfPCell(new Paragraph("", fuente));
                        if (j == 37)
                        {
                            celda.BorderWidthBottom = 1;
                        }
                        else
                        {
                            celda.BorderWidthBottom = 0;
                        }
                        celda.BorderWidthTop = 0;
                        celda.BorderWidthLeft = 1;
                        celda.BorderWidthRight = 0;
                        celda.HorizontalAlignment = Element.ALIGN_LEFT;
                        tabla.AddCell(celda);

                        celda = new PdfPCell(new Paragraph("", fuente));
                        if (j == 37)
                        {
                            celda.BorderWidthBottom = 1;
                        }
                        else
                        {
                            celda.BorderWidthBottom = 0;
                        }
                        celda.BorderWidthTop = 0;
                        celda.BorderWidthLeft = 1;
                        celda.BorderWidthRight = 0;
                        celda.HorizontalAlignment = Element.ALIGN_CENTER;
                        tabla.AddCell(celda);

                        celda = new PdfPCell(new Paragraph("", fuente));
                        if (j == 37)
                        {
                            celda.BorderWidthBottom = 1;
                        }
                        else
                        {
                            celda.BorderWidthBottom = 0;
                        }
                        celda.BorderWidthTop = 0;
                        celda.BorderWidthLeft = 1;
                        celda.BorderWidthRight = 1;
                        celda.HorizontalAlignment = Element.ALIGN_CENTER;
                        tabla.AddCell(celda);
                    }
                }

                return tabla;
            }
            catch
            {
                return null;
            }
        }

        private PdfPTable TablaFormaDePagoYOtros(DatosFactura DatFac)
        {
            try
            {
                PdfPTable tabla = new PdfPTable(2);
                tabla.SetTotalWidth((new float[] { 275, 275 }));
                tabla.LockedWidth = true;
                iTextSharp.text.Font fuente = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 5, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                PdfPCell celda;
                String Metodo = Depurar(DatFac.MetodoPago);

                if (Metodo.Equals("01"))
                {
                    Metodo += " - EFECTIVO";
                }
                else if (Metodo.Equals("02"))
                {
                    Metodo += " - CHEQUE NOMINATIVO";
                }
                else if (Metodo.Equals("03"))
                {
                    Metodo += " - TRANSFERENCIA ELECTRÓNICA DE FONDOS";
                }
                else if (Metodo.Equals("04"))
                {
                    Metodo += " - TARJETA DE CRÉDITO";
                }
                else if (Metodo.Equals("05"))
                {
                    Metodo += " - MONEDERO ELECTRÓNICO";
                }
                else if (Metodo.Equals("06"))
                {
                    Metodo += " DINERO ELECTRÓNICO";
                }
                else if (Metodo.Equals("08"))
                {
                    Metodo += " - VALES DE DESPENSA";
                }
                else if (Metodo.Equals("28"))
                {
                    Metodo += " - TARJETA DE DÉBITO";
                }
                else if (Metodo.Equals("29"))
                {
                    Metodo += " - TARJETA DE SERVICIO";
                }
                else if (Metodo.Equals("99"))
                {
                    Metodo += " - OTROS";
                }

                celda = new PdfPCell(new Paragraph("\nMÉTODO DE PAGO: " + Metodo, fuente));
                celda.BorderWidthBottom = 1;
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthTop = 1;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);

                if (Metodo.Equals("TARJETA DE DÉBITO") || Metodo.Equals("TARJETA DE CRÉDITO"))
                {
                    celda = new PdfPCell(new Paragraph("\nNÚMERO DE CUENTA DE PAGO: " + Depurar(DatFac.NumeroCuenta), fuente));
                    celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celda.BorderWidthBottom = 1;
                    celda.BorderColor = BaseColor.WHITE;
                    celda.BorderWidthTop = 1;
                    celda.BorderWidthLeft = 1;
                    celda.BorderWidthRight = 1;
                    tabla.AddCell(celda);
                }
                else
                {
                    celda = new PdfPCell(new Paragraph("\n", fuente));
                    celda.BorderWidthBottom = 1;
                    celda.BorderColor = BaseColor.WHITE;
                    celda.BorderWidthTop = 1;
                    celda.BorderWidthLeft = 1;
                    celda.BorderWidthRight = 1;
                    tabla.AddCell(celda);
                }

                celda = new PdfPCell(new Paragraph(Depurar(DatFac.FormaPago) + "\n ", fuente));
                celda.Colspan = 2;
                celda.BorderWidthBottom = 1;
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthTop = 1;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);

                return tabla;
            }
            catch
            {
                return null;
            }
        }

        private PdfPTable TablaTotales(DatosFactura DatFac)
        {
            try
            {
                PdfPTable tabla = new PdfPTable(3);
                tabla.SetTotalWidth((new float[] { 470, 40, 40 }));
                tabla.LockedWidth = true;
                iTextSharp.text.Font fuente = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 5, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                iTextSharp.text.Font fuenteCuadroNegro = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 5, iTextSharp.text.Font.NORMAL, BaseColor.WHITE);
                PdfPCell celda;

                celda = new PdfPCell(new Paragraph("IMPORTE CON LETRA", fuente));
                celda.BorderWidthBottom = 0;
                celda.BorderColorBottom = BaseColor.WHITE;
                celda.BorderWidthTop = 1;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph("SUBTOTAL", fuenteCuadroNegro));
                celda.BackgroundColor = BaseColor.BLACK;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 1;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph(Math.Round(Convert.ToDecimal(Depurar(DatFac.SubTotal)), 2).ToString("C"), fuente));
                celda.BorderColorBottom = BaseColor.WHITE;
                celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 1;
                celda.BorderWidthLeft = 0;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);

                NumeroALetra NumLet = new NumeroALetra();
                celda = new PdfPCell(new Paragraph(NumLet.enletras(Depurar(DatFac.Total)) + " " + Depurar(DatFac.Moneda), fuente));
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph("IVA", fuenteCuadroNegro));
                celda.BackgroundColor = BaseColor.BLACK;
                celda.BorderColorTop = BaseColor.WHITE;
                celda.BorderWidthBottom = 1;
                celda.BorderWidthTop = 1;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph(Math.Round(Convert.ToDecimal(Depurar(DatFac.IVA)), 2).ToString("C"), fuente));
                celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                celda.BorderColorBottom = BaseColor.WHITE;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 1;
                celda.BorderWidthLeft = 0;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph("", fuente));
                celda.BorderWidthBottom = 1;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph("TOTAL", fuenteCuadroNegro));
                celda.BackgroundColor = BaseColor.BLACK;
                celda.BorderColorTop = BaseColor.WHITE;
                celda.BorderWidthBottom = 1;
                celda.BorderWidthTop = 1;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 0;
                tabla.AddCell(celda);

                celda = new PdfPCell(new Paragraph(Math.Round(Convert.ToDecimal(Depurar(DatFac.Total)), 2).ToString("C"), fuente));
                celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                celda.BorderWidthBottom = 1;
                celda.BorderWidthTop = 1;
                celda.BorderWidthLeft = 0;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);

                return tabla;
            }
            catch
            {
                return null;
            }
        }

        private PdfPTable TablaBottom(DatosFactura DatFac)
        {
            try
            {
                PdfPTable tabla = new PdfPTable(2);
                //Doy dimensiones a la tabla
                tabla.SetTotalWidth((new float[] { 150, 400 }));
                tabla.LockedWidth = true;
                //Asigno fuentes
                iTextSharp.text.Font fuente = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 4, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                iTextSharp.text.Font fuenteNegrita = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 5, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                //Invocamos al atributo celda para usarlo en general
                PdfPCell celda;
                //Agrego el codigo QR
                var val = "?re=" + Depurar(DatFac.RFCEmisor) + "&rr=" + Depurar(DatFac.RFCReceptor) + "&tt=" + ModificarTotal(Depurar(DatFac.Total)) + "&id=" + Depurar(DatFac.UUID);
                var barcode = new BarcodeQRCode(val, 100, 100, null);
                var image = barcode.GetImage();
                image.ScalePercent(160f);

                celda = new PdfPCell(image);
                celda.Rowspan = 11;
                celda.PaddingLeft = -27f;
                celda.PaddingTop = -17f;
                celda.PaddingBottom = -20f;
                celda.VerticalAlignment = Element.ALIGN_MIDDLE;
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                celda.BorderColor = BaseColor.WHITE;
                tabla.AddCell(celda);

                //Folio UUID
                celda = new PdfPCell(new Paragraph("FOLIO UUID", fuenteNegrita));
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 1;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);
                //Folio UUID
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.UUID), fuente));
                celda.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);
                //Celda Cadena Original titulo
                celda = new PdfPCell(new Paragraph("CADENA ORIGINAL DE COMPLEMENTO DE CERTIFICACIÓN DEL SAT", fuenteNegrita));
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 1;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);
                //Celda cadena original
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.CadenaOriginal), fuente));
                celda.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);
                //Sello SAT titulo
                celda = new PdfPCell(new Paragraph("SELLO DIGITAL DEL SAT", fuenteNegrita));
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);
                //Sello SAT
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.SelloSAT), fuente));
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthBottom = 1; ;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);
                //Sello emisor titulo
                celda = new PdfPCell(new Paragraph("NÚMERO DE CERTIFICADO DEL EMISOR", fuenteNegrita));
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);
                //Sello emisor
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.CertificadoEmisor), fuente));
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthBottom = 1;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);
                //Sello emisor titulo
                celda = new PdfPCell(new Paragraph("SELLO DIGITAL DEL EMISOR", fuenteNegrita));
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthBottom = 0;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);
                //Sello emisor
                celda = new PdfPCell(new Paragraph(Depurar(DatFac.SelloEmisor), fuente));
                celda.BorderColor = BaseColor.WHITE;
                celda.BorderWidthBottom = 1;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);
                //Estacio en blanco
                celda = new PdfPCell(new Paragraph("\nESTE DOCUMENTO ES UNA REPRESENTACIÓN IMPRESA DE UN CFDI", fuente));
                celda.BorderColor = BaseColor.WHITE;
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                celda.BorderWidthBottom = 1;
                celda.BorderWidthTop = 0;
                celda.BorderWidthLeft = 1;
                celda.BorderWidthRight = 1;
                tabla.AddCell(celda);
                return tabla;
            }
            catch
            {
                return null;
            }
        }

        private String ModificarTotal(String Total)
        {

            try
            {
                String Temporal = Total;
                String Enteros = Temporal.Substring(0, Temporal.IndexOf("."));
                String Decimales = Temporal.Substring(Temporal.IndexOf(".") + 1);

                String Ceros = "";
                if (Enteros.Length < 10)
                {
                    int NumeroCeros = 10 - Enteros.Length;
                    for (int i = 0; i < NumeroCeros; i++)
                    {
                        Ceros = Ceros + "0";
                    }
                }
                Enteros = Ceros + Enteros;
                Ceros = "";
                if (Decimales.Length < 6)
                {
                    int NumeroCeros = 6 - Decimales.Length;
                    for (int i = 0; i < NumeroCeros; i++)
                    {
                        Ceros = Ceros + "0";
                    }
                }
                Decimales = Decimales + Ceros;
                Temporal = Enteros + "." + Decimales;
                return Temporal;
            }
            catch
            {
                return Total;
            }
        }

        private List<String> ListarArchivos()
        {
            List<String> Lista = new List<String>();

            try
            {
                String[] ArchivosXML = Directory.GetFiles(FPrincipal.RutaFacturas, "*.xml", SearchOption.AllDirectories);

                if (ArchivosXML != null)
                {
                    for (int i = 0; i < ArchivosXML.Length; i++)
                    {
                        Lista.Add(ArchivosXML[i]);
                    }
                }
                else
                {
                    Lista = null;
                }
            }
            catch(Exception ex)
            {
                LogErrores.Write(string.Format("Error en el método ListarArchivos con la ruta: {0}", FPrincipal.RutaFacturas), ex);
                Lista = null;
            }

            return Lista;
        }

        private void RecorrerXML()
        {
            foreach (String PathXML in ListaXML)
            {
                Crear(PathXML);
            }
        }
    }
}
