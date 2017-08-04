using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacturasValsa
{
    class DatosFactura
    {
        public String RFCEmisor;
        public String NombreEmisor;
        public String CalleEmisor;
        public String ColoniaEmisor;
        public String NumeroInteriorEmisor;
        public String NumeroExteriorEmisor;
        public String MunicipioEmisor;
        public String EstadoEmisor;
        public String PaisEmisor;
        public String RegimenFiscal;
        public String Serie;
        public String Folio;
        public String NumeroCertificado;
        public String FechaTimbrado;
        public String DomicilioEmisor;
        public String DomicilioEmisorDos;
        public String RFCReceptor;
        public String NombreReceptor;
        public String CalleReceptor;
        public String ColoniaReceptor;
        public String NumeroInteriorReceptor;
        public String NumeroExteriorReceptor;
        public String MunicipioReceptor;
        public String EstadoReceptor;
        public String PaisReceptor;
        public String CPReceptor;
        public String DomicilioReceptor;
        public String DomicilioReceptorDos;
        public String DomicilioReceptorTres;
        public String DomicilioReceptorCuatro;
        public String LugarExpedicion;
        public List<Partidas> Productos;
        public String MetodoPago;
        public String NumeroCuenta;
        public String FormaPago;
        public String SubTotal;
        public String Total;
        public String IVA;
        public String Moneda;

        public String SelloSAT;
        public String SelloEmisor;
        public String CadenaOriginal;
        public String UUID;
        public String CertificadoEmisor;

        public String Version;
    }
}
