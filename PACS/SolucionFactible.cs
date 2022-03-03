using NoriSDK;
using System;
using System.IO;
using System.Linq;

namespace NoriCFDI.PACS
{
    class SolucionFactible
    {
        public NoriCFDI.SolucionFactible.TimbradoPortTypeClient servicio = new NoriCFDI.SolucionFactible.TimbradoPortTypeClient();
        public string usuario { get; set; }
        public string contraseña { get; set; }
        public bool modo_prueba { get; set; }
        string prod_endpoint = "TimbradoEndpoint_PRODUCCION";
        string test_endpoint = "TimbradoEndpoint_TESTING";
        public SolucionFactible(string usuario, string contraseña, bool modo_prueba)
        {
            this.usuario = usuario;
            this.contraseña = contraseña;
            this.modo_prueba = modo_prueba;
            servicio = (modo_prueba) ? new NoriCFDI.SolucionFactible.TimbradoPortTypeClient(test_endpoint) : new NoriCFDI.SolucionFactible.TimbradoPortTypeClient(prod_endpoint);
        }
        public DocumentoElectronico Timbrar(DocumentoElectronico documento_electronico, string ruta_xml)
        {
            try
            {
                byte[] archivo_xml = File.ReadAllBytes(ruta_xml);
                
                NoriCFDI.SolucionFactible.CFDICertificacion respuesta = servicio.timbrar(usuario, contraseña, archivo_xml, false);
                NoriCFDI.SolucionFactible.CFDIResultadoCertificacion[] resultados = respuesta.resultados;

                string UUID = string.Empty;

                if (!resultados[0].uuid.IsNullOrEmpty())
                {
                    archivo_xml = resultados[0].cfdiTimbrado;
                    File.WriteAllBytes(ruta_xml, archivo_xml);


                    UUID = resultados[0].uuid;
                    documento_electronico.sello_SAT = resultados[0].selloSAT;
                }

                if (!UUID.IsNullOrEmpty())
                {
                    documento_electronico.estado = 'A';
                    if (UUID.Length == 36)
                    {
                        documento_electronico.folio_fiscal = UUID;
                        documento_electronico.mensaje = string.Empty;
                    }
                    else
                    {
                        documento_electronico.mensaje = UUID;
                    }

                    try
                    {
                        File.Move(ruta_xml, string.Format(@"{0}\{1}.xml", Path.GetDirectoryName(ruta_xml), UUID));
                    }
                    catch { }
                }
                else
                {
                    documento_electronico.estado = 'E';
                    documento_electronico.mensaje = resultados[0].mensaje;
                }
            }
            catch (CFDiException CFDIe)
            {
                documento_electronico.estado = 'E';
                documento_electronico.mensaje = CFDIe.text;
            }
            catch (Exception ex)
            {
                documento_electronico.estado = 'E';
                documento_electronico.mensaje = ex.Message;
            }

            return documento_electronico;
        }


        public DocumentoElectronico CancelarUUID(DocumentoElectronico documento_electronico, string rfc, byte[] cer, byte[] key, string contraseña_cer)
        {
            try
            {
                string[] uuid = new string[] { documento_electronico.folio_fiscal };
                var documento = (documento_electronico.documento_id != 0) ? Documento.Documentos().Where(x => x.id == documento_electronico.documento_id).Select(x => new { x.socio_id, x.total }).First() : Pago.Pagos().Where(x => x.id == documento_electronico.pago_id).Select(x => new { x.socio_id, x.total }).First();
                string rfc_receptor = Socio.Socios().Where(x => x.id == documento.socio_id).Select(x => x.rfc).First();

                NoriCFDI.SolucionFactible.CFDICancelacion respuesta = servicio.cancelar(usuario, contraseña, uuid, cer, key, contraseña_cer);
                NoriCFDI.SolucionFactible.CFDIResultadoCancelacion[] resultados = respuesta.resultados;

                documento_electronico.estado = 'C';
                documento_electronico.mensaje = string.Format("{0} - ({1}) | {2}", resultados[0].status, resultados[0].statusUUID, resultados[0].mensaje);
            }
            catch (CFDiException CFDIe)
            {
                documento_electronico.estado = 'E';
                documento_electronico.mensaje = CFDIe.text;
            }
            catch (Exception ex)
            {
                documento_electronico.estado = 'E';
                documento_electronico.mensaje = ex.Message;
            }

            return documento_electronico;
        }

        public string ObtenerEstado(DocumentoElectronico documento_electronico, string rfc)
        {
            string estado = string.Empty;
            try
            {
                return "Opción no soportada para el PAC Solución Factible";
            }
            catch (CFDiException CFDIe)
            {
                estado = CFDIe.text;
            }
            catch (Exception ex)
            {
                estado = ex.Message;
            }

            return estado;
        }

        public class CFDiException : Exception
        {
            public int cod;
            public string text;
        }
    }
}
