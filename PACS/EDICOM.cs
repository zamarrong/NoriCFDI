using Ionic.Zip;
using NoriSDK;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NoriCFDI.PACS
{
    class EDICOM
    {
        public Edicom.CFDiService servicio { get; private set; }
        public string usuario { get; set; }
        public string contraseña { get; set; }
        public bool modo_prueba { get; set; }
        public EDICOM(string usuario, string contraseña, bool modo_prueba)
        {
            servicio = new Edicom.CFDiService();
            this.usuario = usuario;
            this.contraseña = contraseña;
            this.modo_prueba = modo_prueba;
        }
        public DocumentoElectronico Timbrar(DocumentoElectronico documento_electronico, string ruta_xml)
        {
            try
            {
                byte[] archivo_xml = File.ReadAllBytes(ruta_xml);
                archivo_xml = (modo_prueba) ? servicio.getCfdiTest(usuario, contraseña, archivo_xml) : servicio.getCfdi(usuario, contraseña, archivo_xml);
                ZipFile zip = ZipFile.Read(archivo_xml);
                ZipEntry zip_xml = zip.Entries.First(x => x.FileName.EndsWith(".xml"));
                MemoryStream stream_xml = new MemoryStream();
                zip_xml.Extract(stream_xml);

                archivo_xml = stream_xml.ToArray();

                File.WriteAllBytes(ruta_xml, archivo_xml);

                string UUID = string.Empty;
                //UUID = ObtenerUUID(archivo_xml);

                try
                {
                    XNamespace cfdi = @"http://www.sat.gob.mx/cfd/4";
                    XNamespace tfd = @"http://www.sat.gob.mx/TimbreFiscalDigital";

                    var xdoc = XDocument.Load(ruta_xml);
                    var elt = xdoc.Element(cfdi + "Comprobante").Element(cfdi + "Complemento").Element(tfd + "TimbreFiscalDigital");

                    UUID = (string)elt.Attribute("UUID");
                    documento_electronico.sello_SAT = (string)elt.Attribute("SelloSAT");
                }
                catch
                {
                    UUID = ObtenerUUID(archivo_xml);
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
                    documento_electronico.estado = 'P';
                    documento_electronico.mensaje = "El comprobante aún no ha sido timbrado.";
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
            if (documento_electronico.mensaje.Length >= 254)
                documento_electronico.mensaje = documento_electronico.mensaje.Substring(0, 253);

            return documento_electronico;
        }

        public string ObtenerUUID(byte[] archivo_xml)
        {
            string UUID = string.Empty;
            try
            {
                UUID = (modo_prueba) ? servicio.getUUIDTest(usuario, contraseña, archivo_xml) : servicio.getUUID(usuario, contraseña, archivo_xml);
            }
            catch (CFDiException CFDIe)
            {
                UUID = CFDIe.text;
            }
            catch (Exception ex)
            {
                UUID = ex.Message;
            }
            return UUID;
        }

        public DocumentoElectronico CancelarUUID(DocumentoElectronico documento_electronico, string rfc, byte[] pfx, string contraseña_pfx)
        {
            try
            {
                string[] uuid = new string[] { documento_electronico.folio_fiscal };
                servicio.cancelaCFDiAsync(usuario, contraseña, rfc, uuid, pfx, contraseña_pfx, documento_electronico.motivo, documento_electronico.folio_fiscal_sustitucion);
                var documento = (documento_electronico.documento_id != 0) ? Documento.Documentos().Where(x => x.id == documento_electronico.documento_id).Select(x => new { x.socio_id, x.total }).First() : Pago.Pagos().Where(x => x.id == documento_electronico.pago_id).Select(x => new { x.socio_id, x.total }).First();
                string rfc_receptor = Socio.Socios().Where(x => x.id == documento.socio_id).Select(x => x.rfc).First();
                Edicom.CancelData respuesta = servicio.cancelCFDiAsync(usuario, contraseña, rfc, rfc_receptor, documento_electronico.folio_fiscal, Math.Round(Convert.ToDouble(documento.total), 2), pfx, contraseña_pfx, documento_electronico.motivo, documento_electronico.folio_fiscal_sustitucion, modo_prueba);
                documento_electronico.estado = 'C';
                documento_electronico.mensaje = string.Format("{0} - ({1}) | {2} - {3}", respuesta.status, respuesta.statusCode, respuesta.cancelQueryData.cancelStatus, respuesta.cancelQueryData.isCancelable);
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

        public string CancelarUUID(string uuid, string rfc_receptor, double total, string motivo, string sustitucion, string rfc, byte[] pfx, string contraseña_pfx)
        {
            try
            {
                string[] uuids = new string[] { uuid };
                servicio.cancelaCFDiAsync(usuario, contraseña, rfc, uuids, pfx, contraseña_pfx, motivo, sustitucion);
                Edicom.CancelData respuesta = servicio.cancelCFDiAsync(usuario, contraseña, rfc, rfc_receptor, uuid, total, pfx, contraseña_pfx, motivo, sustitucion, modo_prueba);
                return string.Format("{0} - ({1}) | {2} - {3}", respuesta.status, respuesta.statusCode, respuesta.cancelQueryData.cancelStatus, respuesta.cancelQueryData.isCancelable);
            }
            catch (CFDiException CFDIe)
            {
                return CFDIe.text;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string ObtenerEstado(DocumentoElectronico documento_electronico, string rfc)
        {
            string estado = string.Empty;
            try
            {
                string[] uuid = new string[] { documento_electronico.folio_fiscal };
                var documento = (documento_electronico.documento_id != 0) ? Documento.Documentos().Where(x => x.id == documento_electronico.documento_id).Select(x => new { x.socio_id, x.total }).First() : Pago.Pagos().Where(x => x.id == documento_electronico.pago_id).Select(x => new { x.socio_id, x.total }).First();
                string rfc_receptor = Socio.Socios().Where(x => x.id == documento.socio_id).Select(x => x.rfc).First();
                Edicom.CancelQueryData respuesta = servicio.getCFDiStatus(usuario, contraseña, rfc, rfc_receptor, documento_electronico.folio_fiscal, Math.Round(Convert.ToDouble(documento.total), 2), modo_prueba);
                estado = string.Format("{0} - ({1}) | {2} - {3}", respuesta.status, respuesta.statusCode, respuesta.cancelStatus, respuesta.isCancelable);
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
