using NoriCFDI.Utilidades;
using NoriSDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Services.Discovery;
using System.Xml;
using System.Xml.Serialization;

namespace NoriCFDI
{
    public class CFDI
    {
        public class Certificado
        {
            public string cer { get; set; }
            public string key { get; set; }
            public string contraseña { get; set; }
            public string pfx { get; set; }
            public string contraseña_pfx { get; set; }

            public Certificado()
            {
                cer = string.Empty;
                key = string.Empty;
                contraseña = string.Empty;
                pfx = string.Empty;
                contraseña_pfx = string.Empty;
            }
        }
        public int pac { get; set; }
        public bool modo_prueba { get; set; }
        public string directorio_xml { get; set; }
        public string usuario { get; set; }
        public string contraseña { get; set; }
        public Certificado certificado { get; set; }

        Empresa empresa;

        public CFDI()
        {
            try
            {
                certificado = new Certificado();
            }
            catch { throw; }
        }

        public bool Inicializar()
        {
            try
            {
                empresa = Empresa.Obtener();
                var configuracion = Configuracion.Configuraciones().Select(x => new { x.timbrado_modo_prueba, x.directorio_xml, x.pac }).First();
                directorio_xml = configuracion.directorio_xml;
                modo_prueba = configuracion.timbrado_modo_prueba;
                pac = configuracion.pac;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public bool Timbrar(Documento documento, string rfc = null, int documento_electronico_sustitucion_id = 0)
        {
            DocumentoElectronico documento_electronico = documento.DocumentoElectronico();
            string ruta_xml = string.Format(@"{0}\{1}.xml", directorio_xml, documento.id);

            if (documento.EsDocumentoElectronico() && documento_electronico.folio_fiscal.Length == 0)
            {
                if (documento.global)
                    return TimbrarGlobal(documento, rfc, documento_electronico_sustitucion_id);
                try
                {
                    if (documento_electronico.estado != 'A')
                    {
                        Comprobante comprobante = new Comprobante();

                        string numero_certificado, inicio, final, serie;
                        SelloDigital.LeerCER(certificado.cer, out inicio, out final, out serie, out numero_certificado);

                        comprobante.Version = "3.3";

                        comprobante.Serie = Serie.Series().Where(x => x.id == documento.serie_id).Select(x => x.nombre).First();
                        comprobante.Folio = documento.numero_documento.ToString();

                        comprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                        comprobante.NoCertificado = numero_certificado;
                        comprobante.Moneda = Moneda.Monedas().Where(x => x.id == documento.moneda_id).Select(x => x.codigo).First();

                        if (comprobante.Moneda == "MXP" || comprobante.Moneda == "$")
                            comprobante.Moneda = "MXN";

                        if (comprobante.Moneda != "MXN")
                            comprobante.TipoCambio = Math.Round(documento.tipo_cambio, 2);

                        comprobante.TipoDeComprobante = (documento.clase.Equals("FA") || documento.clase.Equals("AC") || documento.clase.Equals("ND")) ? "I" : "E";
                        comprobante.LugarExpedicion = empresa.cp;

                        if (documento.clase.Equals("TS") || documento.clase.Equals("EN"))
                        {
                            comprobante.TipoDeComprobante = "T";
                        }
                        else
                        {
                            var metodo_pago = MetodoPago.MetodosPago().Where(x => x.id == documento.metodo_pago_id).Select(x => x.codigo).First();

                            comprobante.MetodoPago = (metodo_pago.Equals("99") && comprobante.TipoDeComprobante.Equals("I")) ? "PPD" : "PUE";
                            comprobante.FormaPago = metodo_pago;
                        }

                        //Emisor
                        ComprobanteEmisor emisor = new ComprobanteEmisor();
                        emisor.Rfc = empresa.rfc;
                        emisor.Nombre = empresa.nombre_fiscal;
                        emisor.RegimenFiscal = empresa.regimen_fiscal;

                        //Receptor
                        var socio = Socio.Socios().Where(x => x.id == documento.socio_id).Select(x => new { x.rfc, x.nombre }).First();
                        ComprobanteReceptor receptor = new ComprobanteReceptor();
                        receptor.Rfc = (rfc.IsNullOrEmpty()) ? socio.rfc : rfc;
                        receptor.Nombre = socio.nombre;
                        receptor.UsoCFDI = (documento.uso_principal.IsNullOrEmpty()) ? "P01" : documento.uso_principal;

                        //Asignar (Emisor / Receptor)
                        comprobante.Emisor = emisor;
                        comprobante.Receptor = receptor;

                        List<ComprobanteConcepto> conceptos = new List<ComprobanteConcepto>();
                        List<Impuesto.Linea.Impuesto> clases_impuestos = Impuesto.Linea.Impuesto.Impuestos();
                        List<Impuesto.TipoFactor> tipos_factores = Impuesto.TipoFactor.Tipos();
                        foreach (Documento.Partida partida in documento.partidas)
                        {
                            ComprobanteConcepto concepto = new ComprobanteConcepto();
                            var articulo = Articulo.Articulos().Where(x => x.id == partida.articulo_id).Select(x => new { x.codigo_clasificacion, x.clave_unidad }).First();
                            
                            concepto.Importe = Math.Round(partida.cantidad * (Math.Round(partida.precio, 2) * partida.tipo_cambio), 2);
                            concepto.NoIdentificacion = (partida.codigo_barras.IsNullOrEmpty()) ? partida.sku : partida.codigo_barras;
                            concepto.ClaveProdServ = (articulo.codigo_clasificacion.IsNullOrEmpty()) ? "01010101" : articulo.codigo_clasificacion;
                            concepto.Cantidad = Math.Round(partida.cantidad, 4);
                            var unidad_medida = UnidadMedida.UnidadesMedida().Where(x => x.id == partida.unidad_medida_id).First();
                            if (!unidad_medida.IsNullOrEmpty())
                            {
                                if (unidad_medida.codigo.Equals("-1"))
                                    concepto.ClaveUnidad = (articulo.clave_unidad.Length <= 3) ? articulo.clave_unidad : "H87";
                                else
                                    concepto.ClaveUnidad = (unidad_medida.clave_unidad.IsNullOrEmpty()) ? "H87" : unidad_medida.clave_unidad;
                            }
                            else
                            {
                                concepto.ClaveUnidad = (articulo.clave_unidad.Length <= 3) ? articulo.clave_unidad : "H87";
                            }
                            concepto.Descripcion = partida.nombre;
                            concepto.ValorUnitario = Math.Round((partida.precio * partida.tipo_cambio), 2);

                            if (!partida.numero_pedimento.IsNullOrEmpty())
                            {
                                List<ComprobanteConceptoInformacionAduanera> aduanas = new List<ComprobanteConceptoInformacionAduanera>();
                                ComprobanteConceptoInformacionAduanera informacion_aduanera = new ComprobanteConceptoInformacionAduanera();
                                informacion_aduanera.NumeroPedimento = partida.numero_pedimento;
                                aduanas.Add(informacion_aduanera);
                                concepto.InformacionAduanera = aduanas.ToArray();
                            }

                            if (partida.porcentaje_descuento > 0)
                                concepto.Descuento = Math.Round((((partida.porcentaje_descuento) / 100) * concepto.Importe), 2);

                            if (documento.porcentaje_descuento > 0)
                                concepto.Descuento = Math.Round(((documento.porcentaje_descuento) / 100) * (concepto.Importe), 2);

                            if (!comprobante.TipoDeComprobante.Equals("T"))
                            {
                                //Impuestos
                                ComprobanteConceptoImpuestos impuestos = new ComprobanteConceptoImpuestos();
                                List<ComprobanteConceptoImpuestosTraslado> traslados = new List<ComprobanteConceptoImpuestosTraslado>();

                                foreach (Impuesto.Linea impuesto in Impuesto.ObtenerLineas(partida.impuesto_id))
                                {
                                    ComprobanteConceptoImpuestosTraslado traslado = new ComprobanteConceptoImpuestosTraslado();

                                    traslado.TasaOCuota = decimal.Parse((impuesto.porcentaje / 100).ToString("0.000000"));
                                    traslado.Base = Math.Round(concepto.Importe - concepto.Descuento, 2);
                                    traslado.Importe = Math.Round(traslado.Base * traslado.TasaOCuota, 2);
                                    traslado.TipoFactor = tipos_factores.Where(x => x.tipo == Impuesto.Impuestos().Where(i => i.id == partida.impuesto_id).Select(i => i.tipo_factor).First()).Select(x => x.nombre).First();
                                    traslado.Impuesto = clases_impuestos.Where(x => x.nombre == impuesto.impuesto).Select(x => x.codigo).First();

                                    traslados.Add(traslado);
                                }

                                impuestos.Traslados = traslados.ToArray();
                                concepto.Impuestos = impuestos;
                            }

                            //Agrega el concepto
                            conceptos.Add(concepto);
                        }

                        //Asignar conceptos
                        comprobante.Conceptos = conceptos.ToArray();

                        //Asignar impuestos
                        if (comprobante.TipoDeComprobante.Equals("T"))
                        {
                            comprobante.SubTotal = 0;
                            comprobante.Total = 0;
                        }
                        else
                        {
                            //Impuestos comprobante
                            ComprobanteImpuestos comprobante_impuestos = new ComprobanteImpuestos();
                            List<ComprobanteImpuestosTraslado> comprobante_traslados = new List<ComprobanteImpuestosTraslado>();
                            foreach (ComprobanteConceptoImpuestos concepto_impuesto in comprobante.Conceptos.Select(x => x.Impuestos).ToList())
                            {
                                foreach (ComprobanteConceptoImpuestosTraslado traslado in concepto_impuesto.Traslados)
                                {
                                    if (comprobante_traslados.Any(x => x.TasaOCuota == traslado.TasaOCuota))
                                    {
                                        comprobante_traslados.Where(x => x.TasaOCuota == traslado.TasaOCuota).First().Importe += traslado.Importe;
                                    }
                                    else
                                    {
                                        ComprobanteImpuestosTraslado comprobante_traslado = new ComprobanteImpuestosTraslado();

                                        comprobante_traslado.Impuesto = traslado.Impuesto;
                                        comprobante_traslado.TasaOCuota = traslado.TasaOCuota;
                                        comprobante_traslado.TipoFactor = traslado.TipoFactor;
                                        comprobante_traslado.Importe = traslado.Importe;

                                        comprobante_traslados.Add(comprobante_traslado);
                                    }
                                }
                            }


                            //Asignar traslados
                            comprobante_impuestos.Traslados = comprobante_traslados.ToArray();
                            comprobante_impuestos.TotalImpuestosTrasladados = Math.Round(comprobante_traslados.Sum(x => x.Importe), 2);
                            //comprobante_impuestos.TotalImpuestosRetenidos = 0.00M;

                            comprobante.Impuestos = comprobante_impuestos;

                            //Totales
                            decimal descuento_partidas = comprobante.Conceptos.Sum(x => x.Descuento);
                            comprobante.SubTotal = comprobante.Conceptos.Sum(x => x.Importe);

                            //Descuento
                            if (descuento_partidas > 0)
                                comprobante.Descuento = Math.Round(descuento_partidas, 2);

                            comprobante.Total = (comprobante.SubTotal - comprobante.Descuento) + comprobante.Impuestos.TotalImpuestosTrasladados;

                            //CFDI Relacionado
                            ComprobanteCfdiRelacionados comprobante_cfdis_relacionados = new ComprobanteCfdiRelacionados();
                            List<ComprobanteCfdiRelacionadosCfdiRelacionado> comprobante_cfdi_relacionado = new List<ComprobanteCfdiRelacionadosCfdiRelacionado>();

                            if (documento_electronico_sustitucion_id != 0)
                            {
                                DocumentoElectronico sustitucion = DocumentoElectronico.Obtener(documento_electronico_sustitucion_id);
                                ComprobanteCfdiRelacionadosCfdiRelacionado cfdi_relacionado = new ComprobanteCfdiRelacionadosCfdiRelacionado();
                                cfdi_relacionado.UUID = sustitucion.folio_fiscal;
                                comprobante_cfdi_relacionado.Add(cfdi_relacionado);
                                comprobante_cfdis_relacionados.TipoRelacion = "04";
                            }

                            List<Documento.Relacion> relaciones = documento.ObtenerRelaciones();
                            foreach (Documento.Relacion relacion in relaciones)
                            {
                                DataTable documentos = NoriSDK.Utilidades.EjecutarQuery(string.Format("select t0.clase, t1.folio_fiscal, t1.estado, t0.anticipo from documentos t0 inner join documentos_electronicos t1 on t1.documento_id = t0.id where t1.cancelado = 0 and t1.documento_id = {0}", relacion.documento_origen_id));

                                for (int i = 0; i < documentos.Rows.Count; i++)
                                {
                                    ComprobanteCfdiRelacionadosCfdiRelacionado cfdi_relacionado = new ComprobanteCfdiRelacionadosCfdiRelacionado();
                                    cfdi_relacionado.UUID = documentos.Rows[i]["folio_fiscal"].ToString();
                                    comprobante_cfdi_relacionado.Add(cfdi_relacionado);

                                    switch (documentos.Rows[i]["clase"].ToString())
                                    {
                                        case "AC":
                                            comprobante_cfdis_relacionados.TipoRelacion = "07";
                                            break;
                                        case "FA":
                                            comprobante_cfdis_relacionados.TipoRelacion = (comprobante.TipoDeComprobante.Equals("I")) ? "04" : "01";
                                            break;
                                        case "NC":
                                            comprobante_cfdis_relacionados.TipoRelacion = (comprobante.TipoDeComprobante.Equals("E")) ? "04" : "01";
                                            break;
                                        case "ND":
                                            comprobante_cfdis_relacionados.TipoRelacion = (comprobante.TipoDeComprobante.Equals("I")) ? "04" : "02";
                                            break;
                                    }
                                }
                            }

                            //CFDI Relacionado
                            List<Documento.Referencia> referencias = documento.ObtenerReferencias();
                            foreach (Documento.Referencia referencia in referencias)
                            {
                                DataTable documentos = NoriSDK.Utilidades.EjecutarQuery(string.Format("select t0.id, t0.anticipo, t0.clase, t1.folio_fiscal, t1.estado from documentos t0 inner join documentos_electronicos t1 on t1.documento_id = t0.id where t1.documento_id = {0}", referencia.documento_referencia_id));

                                for (int i = 0; i < documentos.Rows.Count; i++)
                                {
                                    ComprobanteCfdiRelacionadosCfdiRelacionado cfdi_relacionado = new ComprobanteCfdiRelacionadosCfdiRelacionado();
                                    cfdi_relacionado.UUID = documentos.Rows[i]["folio_fiscal"].ToString();
                                    comprobante_cfdi_relacionado.Add(cfdi_relacionado);

                                    switch (documentos.Rows[i]["clase"].ToString())
                                    {
                                        case "AC":
                                            comprobante_cfdis_relacionados.TipoRelacion = (comprobante.TipoDeComprobante.Equals("I")) ? "04" : "01";
                                            if (comprobante.TipoDeComprobante.Equals("I") && (bool)documentos.Rows[i]["anticipo"])
                                                comprobante_cfdis_relacionados.TipoRelacion = "07";
                                            else if (comprobante.TipoDeComprobante.Equals("E"))
                                                if (NoriSDK.Utilidades.EjecutarEscalar(string.Format("select count(id) from documentos where anticipo = 1 and id in (select documento_referencia_id from referencias_documentos where documento_id = {0})", (int)documentos.Rows[i]["id"])) > 0)
                                                    comprobante_cfdis_relacionados.TipoRelacion = "07";
                                            break;
                                        case "FA":
                                            comprobante_cfdis_relacionados.TipoRelacion = (comprobante.TipoDeComprobante.Equals("I")) ? "04" : "01";
                                            if (comprobante.TipoDeComprobante.Equals("I") && (bool)documentos.Rows[i]["anticipo"])
                                                comprobante_cfdis_relacionados.TipoRelacion = "07";
                                            else if (comprobante.TipoDeComprobante.Equals("E"))
                                                if (NoriSDK.Utilidades.EjecutarEscalar(string.Format("select count(id) from documentos where (anticipo = 1 or clase = 'AC') and id in (select documento_referencia_id from referencias_documentos where documento_id = {0})", (int)documentos.Rows[i]["id"])) > 0)
                                                    comprobante_cfdis_relacionados.TipoRelacion = "07";
                                            break;
                                        case "NC":
                                            comprobante_cfdis_relacionados.TipoRelacion = (comprobante.TipoDeComprobante.Equals("E")) ? "04" : "01";
                                            break;
                                        case "ND":
                                            comprobante_cfdis_relacionados.TipoRelacion = (comprobante.TipoDeComprobante.Equals("I")) ? "04" : "02";
                                            break;
                                    }
                                }
                            }

                            if (comprobante_cfdi_relacionado.Count > 0)
                            {
                                comprobante_cfdis_relacionados.CfdiRelacionado = comprobante_cfdi_relacionado.ToArray();
                                comprobante.CfdiRelacionados = comprobante_cfdis_relacionados;
                            }
                        }

                        //Complemento carta porte
                        if (comprobante.TipoDeComprobante == "T")
                        {
                            comprobante.Moneda = "XXX";
                            comprobante.Complemento = GenerarComplementoCartaPorte(documento, receptor.Rfc).ToArray();
                        }

                        if (comprobante.TipoDeComprobante == "T" && comprobante.Complemento.Length == 0)
                            return false;

                        //Cadena original y sello
                        string cadena_original = string.Empty;
                        string ruta_xsl = directorio_xml + @"\cadenaoriginal_3_3.xslt";
                        
                        
                        GenerarXML(comprobante, ruta_xml);

                        System.Xml.Xsl.XslCompiledTransform transformador = new System.Xml.Xsl.XslCompiledTransform(true);
                        transformador.Load(ruta_xsl);

                        using (StringWriter sw = new StringWriter())
                        using (XmlWriter xwo = XmlWriter.Create(sw, transformador.OutputSettings))
                        {

                            transformador.Transform(ruta_xml, xwo);
                            cadena_original = sw.ToString();
                        }

                        SelloDigital sello_digital = new SelloDigital();
                        comprobante.Certificado = sello_digital.Certificado(certificado.cer);
                        comprobante.Sello = sello_digital.Sellar(cadena_original, certificado.key, certificado.contraseña);

                        //Generar XML
                        GenerarXML(comprobante, ruta_xml);

                        documento_electronico.sello_CFD = comprobante.Sello;
                        documento_electronico.cadena_original = cadena_original;
                    }

                    var documento_electronico_existente = documento.DocumentoElectronico();

                    if (documento_electronico_existente.folio_fiscal.Length == 0 && documento_electronico_existente.estado != 'A')
                    {
                        if (pac == 0)
                        {
                            PACS.EDICOM servicio = new PACS.EDICOM(usuario, contraseña, modo_prueba);
                            documento_electronico = servicio.Timbrar(documento_electronico, ruta_xml);
                        }
                        else
                        {
                            PACS.SolucionFactible servicio = new PACS.SolucionFactible(usuario, contraseña, modo_prueba);
                            documento_electronico = servicio.Timbrar(documento_electronico, ruta_xml);
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    documento_electronico.estado = 'E';
                    documento_electronico.mensaje = ex.Message;

                    return false;
                }
                finally
                {
                    if (documento_electronico.id == 0)
                        documento_electronico.Agregar();
                    else
                        documento_electronico.Actualizar();
                }
            }

            return false;
        }
        public bool TimbrarGlobal(Documento documento, string rfc = null, int documento_electronico_sustitucion_id = 0)
        {
            DocumentoElectronico documento_electronico = documento.DocumentoElectronico();
            string ruta_xml = string.Format(@"{0}\{1}.xml", directorio_xml, documento.id);

            if (documento.EsDocumentoElectronico() && documento_electronico.folio_fiscal.Length == 0)
            {
                try
                {
                    if (documento_electronico.estado != 'A')
                    {
                        Comprobante comprobante = new Comprobante();

                        string numero_certificado, inicio, final, serie;
                        SelloDigital.LeerCER(certificado.cer, out inicio, out final, out serie, out numero_certificado);

                        comprobante.Version = "3.3";

                        comprobante.Serie = Serie.Series().Where(x => x.id == documento.serie_id).Select(x => x.nombre).First();
                        comprobante.Folio = documento.numero_documento.ToString();

                        comprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                        comprobante.NoCertificado = numero_certificado;
                        comprobante.Moneda = Moneda.Monedas().Where(x => x.id == documento.moneda_id).Select(x => x.codigo).First();

                        if (comprobante.Moneda == "MXP" || comprobante.Moneda == "$")
                            comprobante.Moneda = "MXN";

                        if (comprobante.Moneda != "MXN")
                            comprobante.TipoCambio = Math.Round(documento.tipo_cambio, 2);

                        comprobante.TipoDeComprobante = "I";
                        comprobante.LugarExpedicion = empresa.cp;

                        var metodo_pago = MetodoPago.MetodosPago().Where(x => x.id == documento.metodo_pago_id).Select(x => x.codigo).First();

                        comprobante.MetodoPago = "PUE";
                        comprobante.FormaPago = metodo_pago;
                        //Emisor
                        ComprobanteEmisor emisor = new ComprobanteEmisor();
                        emisor.Rfc = empresa.rfc;
                        emisor.Nombre = empresa.nombre_fiscal;
                        emisor.RegimenFiscal = empresa.regimen_fiscal;

                        //Receptor
                        var socio = Socio.Socios().Where(x => x.id == documento.socio_id).Select(x => new { x.rfc, x.nombre }).First();
                        ComprobanteReceptor receptor = new ComprobanteReceptor();
                        receptor.Rfc = (rfc.IsNullOrEmpty()) ? socio.rfc : rfc;
                        receptor.UsoCFDI = "P01";

                        //Asignar (Emisor / Receptor)
                        comprobante.Emisor = emisor;
                        comprobante.Receptor = receptor;

                        List<ComprobanteConcepto> conceptos = new List<ComprobanteConcepto>();
                        List<Impuesto.Linea.Impuesto> clases_impuestos = Impuesto.Linea.Impuesto.Impuestos();
                        List<Impuesto.TipoFactor> tipos_factores = Impuesto.TipoFactor.Tipos();
                        List<int> relaciones = documento.partidas.Select(x => x.documento_base_id).Distinct().ToList();
                        foreach (int relacion in relaciones)
                        {
                            ComprobanteConcepto concepto = new ComprobanteConcepto();
                            var documento_relacionado = Documento.Documentos().Where(x => x.id == relacion).Select(x => new { x.numero_documento, x.tipo_cambio, x.porcentaje_descuento, x.descuento, x.subtotal, x.impuesto, x.total }).First();

                            concepto.Importe = Math.Round(documento.partidas.Where(x => x.documento_base_id == relacion).Sum(x => x.subtotal), 2);
                            concepto.NoIdentificacion = documento_relacionado.numero_documento.ToString();
                            concepto.ClaveProdServ = "01010101";
                            concepto.Cantidad = 1;
                            concepto.ClaveUnidad = "ACT";
                            concepto.Descripcion = "Venta";
                            concepto.ValorUnitario = concepto.Importe;

                            try
                            {
                                if (documento_relacionado.porcentaje_descuento > 0)
                                    concepto.Descuento = Math.Round((((documento.partidas.Where(x => x.documento_base_id == relacion).Sum(x => x.porcentaje_descuento) / documento.partidas.Where(x => x.documento_base_id == relacion).Count()) / 100) * concepto.Importe), 2);
                            } catch {
                                concepto.Descuento = 0;
                            }

                            if (documento.porcentaje_descuento > 0)
                                concepto.Descuento = concepto.Descuento + Math.Round((((documento.porcentaje_descuento) / 100) * (concepto.Importe - concepto.Descuento)), 2);

                            //Impuestos
                            ComprobanteConceptoImpuestos impuestos = new ComprobanteConceptoImpuestos();
                            List<ComprobanteConceptoImpuestosTraslado> traslados = new List<ComprobanteConceptoImpuestosTraslado>();

                            List<int> tipos_impuestos = documento.partidas.Where(x => x.documento_base_id == relacion).Select(x => x.impuesto_id).Distinct().ToList();
                            foreach(int tipo_impuesto in tipos_impuestos)
                            {
                                foreach (Impuesto.Linea impuesto in Impuesto.ObtenerLineas(tipo_impuesto))
                                {
                                    ComprobanteConceptoImpuestosTraslado traslado = new ComprobanteConceptoImpuestosTraslado();

                                    traslado.TasaOCuota = decimal.Parse((impuesto.porcentaje / 100).ToString("0.000000"));
                                    traslado.Base = Math.Round(documento.partidas.Where(x => x.documento_base_id == relacion && x.impuesto_id == tipo_impuesto).Sum(x => x.subtotal), 2);
                                    traslado.Importe = Math.Round(traslado.Base * traslado.TasaOCuota, 2);
                                    traslado.TipoFactor = tipos_factores.Where(x => x.tipo == Impuesto.Impuestos().Where(i => i.id == tipo_impuesto).Select(i => i.tipo_factor).First()).Select(x => x.nombre).First();
                                    traslado.Impuesto = clases_impuestos.Where(x => x.nombre == impuesto.impuesto).Select(x => x.codigo).First();

                                    traslados.Add(traslado);
                                }
                            }

                            impuestos.Traslados = traslados.ToArray();
                            concepto.Impuestos = impuestos;
                            //Agrega el concepto
                            conceptos.Add(concepto);
                        }

                        //Asignar conceptos
                        comprobante.Conceptos = conceptos.ToArray();

                        //Asignar impuestos
                        //Impuestos comprobante
                        ComprobanteImpuestos comprobante_impuestos = new ComprobanteImpuestos();
                        List<ComprobanteImpuestosTraslado> comprobante_traslados = new List<ComprobanteImpuestosTraslado>();
                        foreach (ComprobanteConceptoImpuestos concepto_impuesto in comprobante.Conceptos.Select(x => x.Impuestos).ToList())
                        {
                            foreach (ComprobanteConceptoImpuestosTraslado traslado in concepto_impuesto.Traslados)
                            {
                                if (comprobante_traslados.Any(x => x.TasaOCuota == traslado.TasaOCuota))
                                {
                                    comprobante_traslados.Where(x => x.TasaOCuota == traslado.TasaOCuota).First().Importe += traslado.Importe;
                                }
                                else
                                {
                                    ComprobanteImpuestosTraslado comprobante_traslado = new ComprobanteImpuestosTraslado();

                                    comprobante_traslado.Impuesto = traslado.Impuesto;
                                    comprobante_traslado.TasaOCuota = traslado.TasaOCuota;
                                    comprobante_traslado.TipoFactor = traslado.TipoFactor;
                                    comprobante_traslado.Importe = traslado.Importe;

                                    comprobante_traslados.Add(comprobante_traslado);
                                }
                            }
                        }

                        //Asignar traslados
                        comprobante_impuestos.Traslados = comprobante_traslados.ToArray();
                        comprobante_impuestos.TotalImpuestosTrasladados = Math.Round(comprobante_traslados.Sum(x => x.Importe), 2);
                        //comprobante_impuestos.TotalImpuestosRetenidos = 0.00M;

                        comprobante.Impuestos = comprobante_impuestos;

                        //Totales
                        decimal descuento_partidas = comprobante.Conceptos.Sum(x => x.Descuento);
                        comprobante.SubTotal = comprobante.Conceptos.Sum(x => x.Importe);

                        //Descuento
                        if (descuento_partidas > 0)
                            comprobante.Descuento = Math.Round(descuento_partidas, 2);

                        comprobante.Total = (comprobante.SubTotal - comprobante.Descuento) + comprobante.Impuestos.TotalImpuestosTrasladados;

                        //CFDI Relacionado
                        ComprobanteCfdiRelacionados comprobante_cfdis_relacionados = new ComprobanteCfdiRelacionados();
                        List<ComprobanteCfdiRelacionadosCfdiRelacionado> comprobante_cfdi_relacionado = new List<ComprobanteCfdiRelacionadosCfdiRelacionado>();

                        if (documento_electronico_sustitucion_id != 0)
                        {
                            DocumentoElectronico sustitucion = DocumentoElectronico.Obtener(documento_electronico_sustitucion_id);
                            ComprobanteCfdiRelacionadosCfdiRelacionado cfdi_relacionado = new ComprobanteCfdiRelacionadosCfdiRelacionado();
                            cfdi_relacionado.UUID = sustitucion.folio_fiscal;
                            comprobante_cfdi_relacionado.Add(cfdi_relacionado);
                            comprobante_cfdis_relacionados.TipoRelacion = "04";
                        }

                        if (comprobante_cfdi_relacionado.Count > 0)
                        {
                            comprobante_cfdis_relacionados.CfdiRelacionado = comprobante_cfdi_relacionado.ToArray();
                            comprobante.CfdiRelacionados = comprobante_cfdis_relacionados;
                        }

                        //Cadena original y sello
                        string cadena_original = string.Empty;
                        string ruta_xsl = directorio_xml + @"\cadenaoriginal_3_3.xslt";

                        GenerarXML(comprobante, ruta_xml);

                        System.Xml.Xsl.XslCompiledTransform transformador = new System.Xml.Xsl.XslCompiledTransform(true);
                        transformador.Load(ruta_xsl);

                        using (StringWriter sw = new StringWriter())
                        using (XmlWriter xwo = XmlWriter.Create(sw, transformador.OutputSettings))
                        {

                            transformador.Transform(ruta_xml, xwo);
                            cadena_original = sw.ToString();
                        }

                        SelloDigital sello_digital = new SelloDigital();
                        comprobante.Certificado = sello_digital.Certificado(certificado.cer);
                        comprobante.Sello = sello_digital.Sellar(cadena_original, certificado.key, certificado.contraseña);

                        //Generar XML
                        GenerarXML(comprobante, ruta_xml);

                        documento_electronico.sello_CFD = comprobante.Sello;
                        documento_electronico.cadena_original = cadena_original;
                    }

                    var documento_electronico_existente = documento.DocumentoElectronico();

                    if (documento_electronico_existente.folio_fiscal.Length == 0 && documento_electronico_existente.estado != 'A')
                    {
                        if (pac == 0)
                        {
                            PACS.EDICOM servicio = new PACS.EDICOM(usuario, contraseña, modo_prueba);
                            documento_electronico = servicio.Timbrar(documento_electronico, ruta_xml);
                        }
                        else
                        {
                            PACS.SolucionFactible servicio = new PACS.SolucionFactible(usuario, contraseña, modo_prueba);
                            documento_electronico = servicio.Timbrar(documento_electronico, ruta_xml);
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    documento_electronico.estado = 'E';
                    documento_electronico.mensaje = ex.Message;

                    return false;
                }
                finally
                {
                    if (documento_electronico.id == 0)
                        documento_electronico.Agregar();
                    else
                        documento_electronico.Actualizar();
                }
            }

            return false;
        }
        public bool Cancelar(DocumentoElectronico documento_electronico)
        {
            if (documento_electronico.estado.Equals('A') && documento_electronico.folio_fiscal.Length > 0)
            {
                try
                {               
                    if (pac == 0)
                    {
                        PACS.EDICOM servicio = new PACS.EDICOM(usuario, contraseña, modo_prueba);
                        documento_electronico = servicio.CancelarUUID(documento_electronico, empresa.rfc, File.ReadAllBytes(certificado.pfx), certificado.contraseña_pfx);
                    }
                    else
                    {
                        PACS.SolucionFactible servicio = new PACS.SolucionFactible(usuario, contraseña, modo_prueba);
                        documento_electronico = servicio.CancelarUUID(documento_electronico, empresa.rfc, File.ReadAllBytes(certificado.cer), File.ReadAllBytes(certificado.key), certificado.contraseña);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    documento_electronico.estado = 'E';
                    documento_electronico.mensaje = ex.Message;

                    return false;
                }
                finally
                {
                    if (documento_electronico.id == 0)
                        documento_electronico.Agregar();
                    else
                        documento_electronico.Actualizar();
                }
            }

            return false;
        }
        public string Cancelar(string uuid, string rfc_receptor, double total)
        {
            try
            {
                if (pac == 0)
                {
                    PACS.EDICOM servicio = new PACS.EDICOM(usuario, contraseña, modo_prueba);
                    return servicio.CancelarUUID(uuid, rfc_receptor, total, empresa.rfc, File.ReadAllBytes(certificado.pfx), certificado.contraseña_pfx);
                }
                else
                {
                    return "Operación no soportada para este PAC.";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string ObtenerEstado(DocumentoElectronico documento_electronico)
        {
            if (documento_electronico.folio_fiscal.Length > 0)
            {
                try
                {
                    if (pac == 0)
                    {
                        PACS.EDICOM servicio = new PACS.EDICOM(usuario, contraseña, modo_prueba);
                        return servicio.ObtenerEstado(documento_electronico, empresa.rfc);
                    }
                    else
                    {
                        PACS.SolucionFactible servicio = new PACS.SolucionFactible(usuario, contraseña, modo_prueba);
                        return servicio.ObtenerEstado(documento_electronico, empresa.rfc);
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

            return string.Empty;
        }
        public bool Timbrar(Pago pago)
        {
            DocumentoElectronico documento_electronico = pago.DocumentoElectronico();

            string ruta_xml = string.Format(@"{0}\{1}.xml", directorio_xml, pago.id);

            try
            {
                if (documento_electronico.estado != 'A')
                {
                    Comprobante comprobante = new Comprobante();
                    comprobante.xsiSchemaLocation += " http://www.sat.gob.mx/Pagos http://www.sat.gob.mx/sitio_internet/cfd/Pagos/Pagos10.xsd";

                    string numero_certificado, inicio, final, serie;
                    SelloDigital.LeerCER(certificado.cer, out inicio, out final, out serie, out numero_certificado);

                    comprobante.Version = "3.3";
                    comprobante.Serie = Serie.Series().Where(x => x.id == pago.serie_id).Select(x => x.nombre).First();
                    comprobante.Folio = pago.numero_documento.ToString();
                    comprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                    comprobante.NoCertificado = numero_certificado;
                    comprobante.Moneda = "XXX";

                    comprobante.TipoDeComprobante = "P";
                    comprobante.LugarExpedicion = empresa.cp;

                    //Emisor
                    ComprobanteEmisor emisor = new ComprobanteEmisor();
                    emisor.Rfc = empresa.rfc;
                    emisor.Nombre = empresa.nombre_fiscal;
                    emisor.RegimenFiscal = empresa.regimen_fiscal;

                    //Receptor
                    var socio = Socio.Socios().Where(x => x.id == pago.socio_id).Select(x => new { x.rfc, x.nombre, x.uso_principal }).First();
                    ComprobanteReceptor receptor = new ComprobanteReceptor();
                    receptor.Rfc = socio.rfc;
                    receptor.Nombre = socio.nombre;
                    receptor.UsoCFDI = "P01";

                    //Asignar (Emisor / Receptor)
                    comprobante.Emisor = emisor;
                    comprobante.Receptor = receptor;

                    List<ComprobanteConcepto> conceptos = new List<ComprobanteConcepto>();
                    List<Impuesto.Linea.Impuesto> clases_impuestos = Impuesto.Linea.Impuesto.Impuestos();
                    List<Impuesto.TipoFactor> tipos_factores = Impuesto.TipoFactor.Tipos();

                    //Concepto pago
                    ComprobanteConcepto concepto = new ComprobanteConcepto();

                    concepto.Importe = 0;
                    concepto.ClaveProdServ = "84111506";
                    concepto.Cantidad = 1;
                    concepto.ClaveUnidad = "ACT";
                    concepto.Descripcion = "Pago";
                    concepto.ValorUnitario = 0;

                    //Agrega el concepto
                    conceptos.Add(concepto);

                    //Asignar conceptos
                    comprobante.Conceptos = conceptos.ToArray();

                    //Totales
                    comprobante.SubTotal = 0;
                    comprobante.Total = 0;

                    //Complemento pago
                    List<ComprobanteComplemento> complementos = new List<ComprobanteComplemento>();
                    ComprobanteComplemento complemento = new ComprobanteComplemento();

                    Pagos complemento_pagos = new Pagos();
                    complemento_pagos.Version = "1.0";

                    PagosPago complemento_pago = new PagosPago();
                    complemento_pago.FechaPago = pago.fecha_contabilizacion.ToString("yyyy-MM-ddTHH:mm:ss");
                    complemento_pago.MonedaP = Moneda.Monedas().Where(x => x.id == pago.moneda_id).Select(x => x.codigo).First();

                    if (complemento_pago.MonedaP == "MXP" || complemento_pago.MonedaP == "$")
                        complemento_pago.MonedaP = "MXN";

                    if (complemento_pago.MonedaP != "MXN")
                    {
                        complemento_pago.TipoCambioP = Math.Round(pago.tipo_cambio, 4);
                        complemento_pago.Monto = Math.Round(pago.flujo.Sum(x => x.importe), 2);
                    }
                    else
                    {
                        complemento_pago.Monto = Math.Round(pago.total, 2);
                    }

                    complemento_pago.FormaDePagoP = MetodoPago.MetodosPago().Where(x => x.id == pago.metodo_pago_id).Select(x => x.codigo).First();   

                    List<PagosPagoDoctoRelacionado> documentos_relacionados = new List<PagosPagoDoctoRelacionado>();
                    foreach (Pago.Partida partida in pago.partidas)
                    {
                        PagosPagoDoctoRelacionado documento_relacionado = new PagosPagoDoctoRelacionado();

                        var folio_fiscal_relacionado = DocumentoElectronico.DocumentosElectronicos().Where(x => x.documento_id == partida.documento_id).Select(x => x.folio_fiscal).First();
                        var documento = Documento.Documentos().Where(x => x.id == partida.documento_id).Select(x => new { x.clase, x.serie_id, x.numero_documento, x.metodo_pago_id, x.moneda_id, x.tipo_cambio, x.total, x.importe_aplicado }).First();

                        if (documento.clase.Equals("FA") || documento.clase.Equals("NC"))
                        {
                            documento_relacionado.IdDocumento = folio_fiscal_relacionado;
                            documento_relacionado.Serie = Serie.Series().Where(x => x.id == documento.serie_id).Select(x => x.nombre).First();
                            documento_relacionado.Folio = documento.numero_documento.ToString();
                            var metodo_pago = MetodoPago.MetodosPago().Where(x => x.id == documento.metodo_pago_id).Select(x => x.codigo).First();
                            documento_relacionado.MetodoDePagoDR = (metodo_pago.Equals("99")) ? "PPD" : "PUE";
                            documento_relacionado.MonedaDR = Moneda.Monedas().Where(x => x.id == documento.moneda_id).Select(x => x.codigo).First();

                            if (documento_relacionado.MonedaDR == "MXP" || documento_relacionado.MonedaDR == "$")
                                documento_relacionado.MonedaDR = "MXN";

                            if (documento_relacionado.MonedaDR != complemento_pago.MonedaP)
                                documento_relacionado.TipoCambioDR = Math.Round(1 / partida.tipo_cambio, 4);

                            if (documento_relacionado.MetodoDePagoDR.Equals("PPD"))
                                documento_relacionado.NumParcialidad = "1";

                            decimal importe_aplicado = documento.importe_aplicado;
                            if (documento.importe_aplicado > documento.total)
                                importe_aplicado = 0;

                            documento_relacionado.ImpSaldoAnt = Math.Round(partida.saldo, 2);
                            documento_relacionado.ImpPagado = Math.Round(partida.importe, 2);
                            documento_relacionado.ImpSaldoInsoluto = Math.Round(documento_relacionado.ImpSaldoAnt - documento_relacionado.ImpPagado, 2);

                            documentos_relacionados.Add(documento_relacionado);
                        }
                    }

                    complemento_pago.DoctoRelacionado = documentos_relacionados.ToArray();
                    complemento_pagos.Pago = new PagosPago[] { complemento_pago };

                    complemento.Any = new XmlElement[] { GenerarComplementoPago(complemento_pagos) };
                    complementos.Add(complemento);
                    comprobante.Complemento = complementos.ToArray();

                    //Cadena original y sello
                    string cadena_original = string.Empty;
                    string ruta_xsl = directorio_xml + @"\cadenaoriginal_3_3.xslt";

                    GenerarXML(comprobante, ruta_xml);

                    System.Xml.Xsl.XslCompiledTransform transformador = new System.Xml.Xsl.XslCompiledTransform(true);
                    transformador.Load(ruta_xsl);

                    using (StringWriter sw = new StringWriter())
                    using (XmlWriter xwo = XmlWriter.Create(sw, transformador.OutputSettings))
                    {

                        transformador.Transform(ruta_xml, xwo);
                        cadena_original = sw.ToString();
                    }

                    SelloDigital sello_digital = new SelloDigital();
                    comprobante.Certificado = sello_digital.Certificado(certificado.cer);
                    comprobante.Sello = sello_digital.Sellar(cadena_original, certificado.key, certificado.contraseña);

                    //Generar XML
                    GenerarXML(comprobante, ruta_xml);

                    documento_electronico.sello_CFD = comprobante.Sello;
                    documento_electronico.cadena_original = cadena_original;
                }

                if (pac == 0)
                {
                    PACS.EDICOM servicio = new PACS.EDICOM(usuario, contraseña, modo_prueba);
                    documento_electronico = servicio.Timbrar(documento_electronico, ruta_xml);
                }
                else
                {
                    PACS.SolucionFactible servicio = new PACS.SolucionFactible(usuario, contraseña, modo_prueba);
                    documento_electronico = servicio.Timbrar(documento_electronico, ruta_xml);
                }

                return true;
            }
            catch (Exception ex)
            {
                documento_electronico.estado = 'E';
                documento_electronico.mensaje = ex.Message;

                return false;
            }
            finally
            {
                if (documento_electronico.id == 0)
                    documento_electronico.Agregar();
                else
                    documento_electronico.Actualizar();
            }
        }
        public List<ComprobanteComplemento> GenerarComplementoCartaPorte(Documento documento, string rfc)
        {
            try
            {
                List<ComprobanteComplemento> complementos = new List<ComprobanteComplemento>();
                ComprobanteComplemento complemento = new ComprobanteComplemento();

                CartaPorte carta_porte = new CartaPorte();
                carta_porte.Version = "2.0";
                carta_porte.TranspInternac = CartaPorteTranspInternac.No;
                carta_porte.TotalDistRec = documento.distancia;

                for (int i = 0; i < 1; i++)
                {
                    carta_porte.Ubicaciones = new CartaPorteUbicacion[2];

                    //Origen
                    CartaPorteUbicacion ubicacion = new CartaPorteUbicacion()
                    {
                        IDUbicacion = string.Format("OR101010", 1),
                        TipoUbicacion = CartaPorteUbicacionTipoUbicacion.Origen,
                        RFCRemitenteDestinatario = empresa.rfc,
                        FechaHoraSalidaLlegada = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                    };

                    Socio.Direccion origen = Socio.Direccion.Obtener(documento.direccion_origen_id);

                    ubicacion.Domicilio = new CartaPorteUbicacionDomicilio()
                    {
                        Calle = origen.calle,
                        Colonia = origen.colonia,
                        Localidad = origen.ciudad,
                        Municipio = origen.municipio,
                        Estado = origen.CodigoEstado(),
                        Pais = c_Pais.MEX,
                        CodigoPostal = origen.cp
                    };

                    carta_porte.Ubicaciones[i] = ubicacion;

                    //Destino
                    ubicacion = new CartaPorteUbicacion()
                    {
                        IDUbicacion = string.Format("DE202020", documento.direccion_envio_id),
                        TipoUbicacion = CartaPorteUbicacionTipoUbicacion.Destino,
                        DistanciaRecorrida = documento.distancia,
                        RFCRemitenteDestinatario = rfc,
                        FechaHoraSalidaLlegada = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                    };

                    Socio.Direccion direccion = Socio.Direccion.Obtener(documento.direccion_envio_id);

                    ubicacion.Domicilio = new CartaPorteUbicacionDomicilio()
                    {
                        Calle = direccion.calle,
                        Colonia = direccion.colonia,
                        Localidad = direccion.ciudad,
                        Municipio = direccion.municipio,
                        Estado = direccion.CodigoEstado(),
                        Pais = c_Pais.MEX,
                        CodigoPostal = direccion.cp
                    };

                    carta_porte.Ubicaciones[i + 1] = ubicacion;
                }

                
                int mercancias = 0;
                decimal peso_bruto = 0;
                carta_porte.Mercancias = new CartaPorteMercancias();
                carta_porte.Mercancias.NumTotalMercancias = documento.partidas.Count;
                carta_porte.Mercancias.Mercancia = new CartaPorteMercanciasMercancia[documento.partidas.Count];
                foreach (Documento.Partida partida in documento.partidas)
                {
                    CartaPorteMercanciasMercancia mercancia = new CartaPorteMercanciasMercancia();

                    var articulo = Articulo.Articulos().Where(x => x.id == partida.articulo_id).Select(x => new { x.codigo_clasificacion, x.clave_unidad, x.peso }).First();

                    mercancia.BienesTransp = articulo.codigo_clasificacion;
                    mercancia.Descripcion = partida.nombre;
                    mercancia.Cantidad = Math.Round(partida.cantidad, 4);

                    var unidad_medida = UnidadMedida.UnidadesMedida().Where(x => x.id == partida.unidad_medida_id).First();

                    if (!unidad_medida.IsNullOrEmpty())
                    {
                        if (unidad_medida.codigo.Equals("-1"))
                            mercancia.ClaveUnidad = (articulo.clave_unidad.Length <= 3) ? articulo.clave_unidad : "H87";
                        else
                            mercancia.ClaveUnidad = (articulo.clave_unidad.IsNullOrEmpty()) ? "H87" : unidad_medida.clave_unidad;
                    }
                    else
                    {
                        mercancia.ClaveUnidad = (partida.clave_unidad.Length <= 3) ? articulo.clave_unidad : "H87";
                    }

                    mercancia.MaterialPeligroso = CartaPorteMercanciasMercanciaMaterialPeligroso.No;
                    mercancia.PesoEnKg = Math.Round(partida.cantidad * articulo.peso, 2);
                    mercancia.ValorMercancia = Math.Round(partida.precio * partida.tipo_cambio, 2);
                    mercancia.Moneda = c_Moneda.MXN;

                    carta_porte.Mercancias.Mercancia[mercancias] = mercancia;
                    peso_bruto += mercancia.PesoEnKg;
                    mercancias++;
                }
                carta_porte.Mercancias.PesoBrutoTotal = peso_bruto;
                carta_porte.Mercancias.UnidadPeso = c_ClaveUnidadPeso.KGM;

                Vehiculo vehiculo = Vehiculo.Obtener(documento.vehiculo_id);
                carta_porte.Mercancias.Autotransporte = new CartaPorteMercanciasAutotransporte()
                {
                    PermSCT = vehiculo.permiso_sct,
                    NumPermisoSCT = vehiculo.numero_permiso_sct,
                    IdentificacionVehicular = new CartaPorteMercanciasAutotransporteIdentificacionVehicular()
                    {
                        ConfigVehicular = vehiculo.configuracion_vehicular,
                        PlacaVM = vehiculo.numero_placas,
                        AnioModeloVM = vehiculo.modelo
                    },
                    Seguros = new CartaPorteMercanciasAutotransporteSeguros()
                    {
                        AseguraRespCivil = vehiculo.aseguradora,
                        PolizaRespCivil = vehiculo.numero_poliza
                    }
                };

                if (documento.remolque_id != 0)
                {
                    Remolque remolque = Remolque.Obtener(documento.remolque_id);
                    carta_porte.Mercancias.Autotransporte.Remolques = new CartaPorteMercanciasAutotransporteRemolque[1];
                    carta_porte.Mercancias.Autotransporte.Remolques[0] = new CartaPorteMercanciasAutotransporteRemolque()
                    {
                        SubTipoRem = remolque.sub_tipo_remolque,
                        Placa = remolque.placa
                    };
                }

                Chofer chofer = Chofer.Obtener(documento.chofer_id);

                carta_porte.FiguraTransporte = new CartaPorteTiposFigura[1];
                carta_porte.FiguraTransporte[0] = new CartaPorteTiposFigura()
                {
                    TipoFigura = chofer.tipo_figura,
                    RFCFigura = chofer.rfc,
                    NumLicencia = chofer.licencia,
                    NombreFigura = chofer.nombre,
                };

                complemento.Any = new XmlElement[] { GenerarComplementoCartaPorte(carta_porte) };
                complementos.Add(complemento);

                return complementos;
            } catch (Exception ex) {
                return new List<ComprobanteComplemento>();
            }
        }
        private void GenerarXML(Comprobante comprobante, string ruta)
        {
            XmlSerializerNamespaces oXmlNameSpace = new XmlSerializerNamespaces();

            oXmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
            oXmlNameSpace.Add("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");
            oXmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            try
            {
                if (comprobante.Complemento.Count() > 0)
                {
                    if (comprobante.TipoDeComprobante == "P")
                        oXmlNameSpace.Add("pago10", "http://www.sat.gob.mx/Pagos");
                    else if (comprobante.TipoDeComprobante == "T")
                        oXmlNameSpace.Add("cartaporte20", "http://www.sat.gob.mx/CartaPorte20");
                }
            } catch { }

            XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));
            string sXml = "";

            using (var sww = new StringWriterWithEncoding(Encoding.UTF8))
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {

                    oXmlSerializar.Serialize(writer, comprobante, oXmlNameSpace);
                    sXml = sww.ToString();
                }
            }

            //Guardamos el string en un archivo
            File.WriteAllText(ruta, sXml);
        }
        public static XmlElement GenerarComplementoPago(Pagos complemento)
        {
            XmlDocument oXmlDocument = new XmlDocument();
            XmlSerializerNamespaces oXmlNameSpace = new XmlSerializerNamespaces();
            oXmlNameSpace.Add("pago10", "http://www.sat.gob.mx/Pagos");

            using (XmlWriter writer = oXmlDocument.CreateNavigator().AppendChild())
            {
                new XmlSerializer(complemento.GetType()).Serialize(writer, complemento, oXmlNameSpace);
            }

            oXmlDocument.DocumentElement.RemoveAttribute("xmlns");

            return oXmlDocument.DocumentElement;
        }

        public static XmlElement GenerarComplementoCartaPorte(CartaPorte complemento)
        {
            XmlDocument oXmlDocument = new XmlDocument();
            XmlSerializerNamespaces oXmlNameSpace = new XmlSerializerNamespaces();
            oXmlNameSpace.Add("cartaporte20", "http://www.sat.gob.mx/CartaPorte20");

            using (XmlWriter writer = oXmlDocument.CreateNavigator().AppendChild())
            {
                new XmlSerializer(complemento.GetType()).Serialize(writer, complemento, oXmlNameSpace);
            }

            oXmlDocument.DocumentElement.RemoveAttribute("xmlns");

            return oXmlDocument.DocumentElement;
        }
    }
}