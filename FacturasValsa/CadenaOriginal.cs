using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml;
using System.IO;

namespace FacturasValsa
{
    class CadenaOriginal
    {
        public String GeneradorCadenas(String Path, String Version)
        {
            try
            {
                //Cargar el XML
                StreamReader reader = new StreamReader(Path);
                XPathDocument myXPathDoc = new XPathDocument(reader);

                //Cargando el XSLT
                XslCompiledTransform myXslTrans = new XslCompiledTransform();
                if (Version.Trim().Equals("3.2"))
                {
                    myXslTrans.Load("cadenaoriginal_3_2.xslt");
                }
                else if (Version.Trim().Equals("3.3"))
                {
                    myXslTrans.Load("cadenaoriginal_3_3");
                }
                else if (Version.Trim().Equals("3.0"))
                {
                    myXslTrans.Load("cadenaoriginal_3_0");
                }

                StringWriter str = new StringWriter();
                XmlTextWriter myWriter = new XmlTextWriter(str);

                //Aplicando transformacion
                myXslTrans.Transform(myXPathDoc, null, myWriter);

                //Resultado
                return str.ToString();

                //Fin del programa.
            }
            catch
            {
                return null;
            }
        }
    }
}
