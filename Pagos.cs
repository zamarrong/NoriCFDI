﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// Este código fuente fue generado automáticamente por xsd, Versión=4.0.30319.1.
// 
namespace NoriCFDI_
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/Pagos")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.sat.gob.mx/Pagos", IsNullable = false)]
    public partial class Pagos
    {

        private PagosPago[] pagoField;

        private string versionField;

        public Pagos()
        {
            versionField = "1.0";
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Pago")]
        public PagosPago[] Pago
        {
            get
            {
                return pagoField;
            }
            set
            {
                pagoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version
        {
            get
            {
                return versionField;
            }
            set
            {
                versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/Pagos")]
    public partial class PagosPago
    {

        private PagosPagoDoctoRelacionado[] doctoRelacionadoField;

        private PagosPagoImpuestos[] impuestosField;

        private string fechaPagoField;

        private string formaDePagoPField;

        private string monedaPField;

        private decimal tipoCambioPField;

        private bool tipoCambioPFieldSpecified;

        private decimal montoField;

        private string numOperacionField;

        private string rfcEmisorCtaOrdField;

        private string nomBancoOrdExtField;

        private string ctaOrdenanteField;

        private string rfcEmisorCtaBenField;

        private string ctaBeneficiarioField;

        private string tipoCadPagoField;

        private bool tipoCadPagoFieldSpecified;

        private byte[] certPagoField;

        private string cadPagoField;

        private byte[] selloPagoField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DoctoRelacionado")]
        public PagosPagoDoctoRelacionado[] DoctoRelacionado
        {
            get
            {
                return doctoRelacionadoField;
            }
            set
            {
                doctoRelacionadoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Impuestos")]
        public PagosPagoImpuestos[] Impuestos
        {
            get
            {
                return impuestosField;
            }
            set
            {
                impuestosField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FechaPago
        {
            get
            {
                return fechaPagoField;
            }
            set
            {
                fechaPagoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FormaDePagoP
        {
            get
            {
                return formaDePagoPField;
            }
            set
            {
                formaDePagoPField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MonedaP
        {
            get
            {
                return monedaPField;
            }
            set
            {
                monedaPField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal TipoCambioP
        {
            get
            {
                return tipoCambioPField;
            }
            set
            {
                tipoCambioPFieldSpecified = true;
                tipoCambioPField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TipoCambioPSpecified
        {
            get
            {
                return tipoCambioPFieldSpecified;
            }
            set
            {
                tipoCambioPFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Monto
        {
            get
            {
                return montoField;
            }
            set
            {
                montoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NumOperacion
        {
            get
            {
                return numOperacionField;
            }
            set
            {
                numOperacionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RfcEmisorCtaOrd
        {
            get
            {
                return rfcEmisorCtaOrdField;
            }
            set
            {
                rfcEmisorCtaOrdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NomBancoOrdExt
        {
            get
            {
                return nomBancoOrdExtField;
            }
            set
            {
                nomBancoOrdExtField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CtaOrdenante
        {
            get
            {
                return ctaOrdenanteField;
            }
            set
            {
                ctaOrdenanteField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RfcEmisorCtaBen
        {
            get
            {
                return rfcEmisorCtaBenField;
            }
            set
            {
                rfcEmisorCtaBenField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CtaBeneficiario
        {
            get
            {
                return ctaBeneficiarioField;
            }
            set
            {
                ctaBeneficiarioField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TipoCadPago
        {
            get
            {
                return tipoCadPagoField;
            }
            set
            {
                tipoCadPagoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TipoCadPagoSpecified
        {
            get
            {
                return tipoCadPagoFieldSpecified;
            }
            set
            {
                tipoCadPagoFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "base64Binary")]
        public byte[] CertPago
        {
            get
            {
                return certPagoField;
            }
            set
            {
                certPagoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CadPago
        {
            get
            {
                return cadPagoField;
            }
            set
            {
                cadPagoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "base64Binary")]
        public byte[] SelloPago
        {
            get
            {
                return selloPagoField;
            }
            set
            {
                selloPagoField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/Pagos")]
    public partial class PagosPagoDoctoRelacionado
    {

        private string idDocumentoField;

        private string serieField;

        private string folioField;

        private string monedaDRField;

        private decimal tipoCambioDRField;

        private bool tipoCambioDRFieldSpecified;

        private string metodoDePagoDRField;

        private string numParcialidadField;

        private decimal impSaldoAntField;

        private bool impSaldoAntFieldSpecified;

        private decimal impPagadoField;

        private bool impPagadoFieldSpecified;

        private decimal impSaldoInsolutoField;

        private bool impSaldoInsolutoFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IdDocumento
        {
            get
            {
                return idDocumentoField;
            }
            set
            {
                idDocumentoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Serie
        {
            get
            {
                return serieField;
            }
            set
            {
                serieField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Folio
        {
            get
            {
                return folioField;
            }
            set
            {
                folioField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MonedaDR
        {
            get
            {
                return monedaDRField;
            }
            set
            {
                monedaDRField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal TipoCambioDR
        {
            get
            {
                return tipoCambioDRField;
            }
            set
            {
                tipoCambioDRFieldSpecified = true;
                tipoCambioDRField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TipoCambioDRSpecified
        {
            get
            {
                return tipoCambioDRFieldSpecified;
            }
            set
            {
                tipoCambioDRFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MetodoDePagoDR
        {
            get
            {
                return metodoDePagoDRField;
            }
            set
            {
                metodoDePagoDRField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string NumParcialidad
        {
            get
            {
                return numParcialidadField;
            }
            set
            {
                numParcialidadField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal ImpSaldoAnt
        {
            get
            {
                return impSaldoAntField;
            }
            set
            {
                impSaldoAntFieldSpecified = true;
                impSaldoAntField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ImpSaldoAntSpecified
        {
            get
            {
                return impSaldoAntFieldSpecified;
            }
            set
            {
                impSaldoAntFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal ImpPagado
        {
            get
            {
                return impPagadoField;
            }
            set
            {
                impPagadoFieldSpecified = true;
                impPagadoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ImpPagadoSpecified
        {
            get
            {
                return impPagadoFieldSpecified;
            }
            set
            {
                impPagadoFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal ImpSaldoInsoluto
        {
            get
            {
                return impSaldoInsolutoField;
            }
            set
            {
                impSaldoInsolutoFieldSpecified = true;
                impSaldoInsolutoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ImpSaldoInsolutoSpecified
        {
            get
            {
                return impSaldoInsolutoFieldSpecified;
            }
            set
            {
                impSaldoInsolutoFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/Pagos")]
    public partial class PagosPagoImpuestos
    {

        private PagosPagoImpuestosRetencion[] retencionesField;

        private PagosPagoImpuestosTraslado[] trasladosField;

        private decimal totalImpuestosRetenidosField;

        private bool totalImpuestosRetenidosFieldSpecified;

        private decimal totalImpuestosTrasladadosField;

        private bool totalImpuestosTrasladadosFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Retencion", IsNullable = false)]
        public PagosPagoImpuestosRetencion[] Retenciones
        {
            get
            {
                return retencionesField;
            }
            set
            {
                retencionesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Traslado", IsNullable = false)]
        public PagosPagoImpuestosTraslado[] Traslados
        {
            get
            {
                return trasladosField;
            }
            set
            {
                trasladosField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal TotalImpuestosRetenidos
        {
            get
            {
                return totalImpuestosRetenidosField;
            }
            set
            {
                totalImpuestosRetenidosField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TotalImpuestosRetenidosSpecified
        {
            get
            {
                return totalImpuestosRetenidosFieldSpecified;
            }
            set
            {
                totalImpuestosRetenidosFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal TotalImpuestosTrasladados
        {
            get
            {
                return totalImpuestosTrasladadosField;
            }
            set
            {
                totalImpuestosTrasladadosField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TotalImpuestosTrasladadosSpecified
        {
            get
            {
                return totalImpuestosTrasladadosFieldSpecified;
            }
            set
            {
                totalImpuestosTrasladadosFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/Pagos")]
    public partial class PagosPagoImpuestosRetencion
    {

        private string impuestoField;

        private decimal importeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Impuesto
        {
            get
            {
                return impuestoField;
            }
            set
            {
                impuestoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Importe
        {
            get
            {
                return importeField;
            }
            set
            {
                importeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/Pagos")]
    public partial class PagosPagoImpuestosTraslado
    {

        private string impuestoField;

        private string tipoFactorField;

        private decimal tasaOCuotaField;

        private decimal importeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Impuesto
        {
            get
            {
                return impuestoField;
            }
            set
            {
                impuestoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TipoFactor
        {
            get
            {
                return tipoFactorField;
            }
            set
            {
                tipoFactorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal TasaOCuota
        {
            get
            {
                return tasaOCuotaField;
            }
            set
            {
                tasaOCuotaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Importe
        {
            get
            {
                return importeField;
            }
            set
            {
                importeField = value;
            }
        }
    }
}
