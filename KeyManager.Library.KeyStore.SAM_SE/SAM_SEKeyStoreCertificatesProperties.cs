/*
** File Name: SAM_SEKeyStoreCertificatesProperties.cs
** Author: s_eva
** Creation date: September 2024
** Description: This file store certificates properties of a SAM-SE Key Store.
** Licence: LGPLv3
** Copyright (c) 2023-Present Synchronic
*/

using Leosac.KeyManager.Library.KeyStore.SAM_SE.DLL;
using Leosac.KeyManager.Library.KeyStore.SAM_SE.Properties;
using System.ComponentModel;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SEKeyStoreCertificatesProperties : KeyStoreProperties, IEquatable<SAM_SEKeyStoreCertificatesProperties>
    {
        public SAM_SEKeyStoreCertificatesProperties(uint id, string path, string? commonName, bool validCertificate, SAM_SECertificatesType type, bool real)
        {
            Id = id;
            Path = path;
            CommonName = commonName;
            ValidCertificate = validCertificate;
            ValidKey = false;
            Type = type;
            Real = real;
            UpdateToolTipContent();
        }

        public SAM_SEKeyStoreCertificatesProperties(uint id, string path, string? commonName, bool validCertificate, bool validKey, SAM_SECertificatesType type, bool real)
        {
            Id = id;
            Path = path;
            CommonName = commonName;
            ValidCertificate = validCertificate;
            ValidKey = validKey;
            Type = type;
            Real = real;
            UpdateToolTipContent();
        }

        public static Dictionary<SAM_SECertificatesType, string> CertificatesNames { get; } =
            new Dictionary<SAM_SECertificatesType, string>()
            {
                {SAM_SECertificatesType.CERT_TYPE_CRT_TLS, Properties.Resources.TLSModule},
                {SAM_SECertificatesType.CERT_TYPE_CRT_RADIUS, Properties.Resources.RADIUSModule},
                {SAM_SECertificatesType.CERT_TYPE_CRT_WEB, Properties.Resources.WEBModule},
                {SAM_SECertificatesType.CERT_TYPE_CRT_JANUS, Properties.Resources.JANUSModule},
            };

        public uint Id { get; set; } = 0;
        public String Path { get; set; } = String.Empty;
        public String? CommonName { get; set; } = String.Empty;

        private bool _ValidCertificate = false;
        public bool ValidCertificate
        { 
            get => _ValidCertificate; 
            set
            {
                SetProperty(ref _ValidCertificate, value);
                UpdateToolTipContent();
            }
        }

        private bool _ValidKey = false;
        public bool ValidKey
        { 
            get => _ValidKey; 
            set
            {
                SetProperty(ref _ValidKey, value);
                UpdateToolTipContent();
            }
        }

        private bool _Real = false;
        public bool Real
        {
            get => _Real;
            set
            {
                SetProperty(ref _Real, value);
            }
        }

        private String _ToolTip = String.Empty;
        public String ToolTip
        {
            get => _ToolTip;
            set 
            {
                SetProperty(ref _ToolTip, value);
            }
        }

        private SAM_SECertificatesType _Type = SAM_SECertificatesType.CERT_TYPE_CRT_CA;
        public SAM_SECertificatesType Type
        { 
            get => _Type;
            set
            {
                _Type = value;
                TypeName = CertificatesNames.GetValueOrDefault(_Type);

            }
        }
        public String TypeName { get; set; } = String.Empty;

        private void UpdateToolTipContent()
        {
            switch(Type)
            {
                case SAM_SECertificatesType.CERT_TYPE_CRT_RCA:
                    if(ValidCertificate)
                        ToolTip = Resources.ToolTipOpenCert;
                    else
                        ToolTip = Resources.ToolTipDepositCert;
                    break;
                case SAM_SECertificatesType.CERT_TYPE_CRT_CA:
                    ToolTip = Resources.ToolTipOpenCert;
                    break;
                case SAM_SECertificatesType.CERT_TYPE_CRT_RADIUS:
                case SAM_SECertificatesType.CERT_TYPE_KEY_RADIUS:
                case SAM_SECertificatesType.CERT_TYPE_CRT_TLS:
                case SAM_SECertificatesType.CERT_TYPE_KEY_TLS:
                case SAM_SECertificatesType.CERT_TYPE_CRT_WEB:
                case SAM_SECertificatesType.CERT_TYPE_KEY_WEB:
                case SAM_SECertificatesType.CERT_TYPE_CRT_JANUS:
                case SAM_SECertificatesType.CERT_TYPE_KEY_JANUS:
                    if (ValidCertificate)
                    {
                        if (ValidKey)
                        {
                            ToolTip = Resources.ToolTipOpenCert;
                        }
                        else
                        {
                            ToolTip = Resources.ToolTipOpenCert + "\n\r" + Resources.ToolTipDepositKey;
                        }
                    }
                    else
                    {
                        if (ValidKey)
                        {
                            ToolTip = Resources.ToolTipDepositCert;
                        }
                        else
                        {
                            ToolTip = Resources.ToolTipDepositFile;
                        }
                    }
                    break;
            }
        }
        public override bool Equals(object? obj)
        {
            return this.Equals(obj as SAM_SEKeyStoreCertificatesProperties);
        }

        public bool Equals(SAM_SEKeyStoreCertificatesProperties? p)
        {
            if (p is null)
                return false;

            if (Object.ReferenceEquals(this, p))
                return true;

            if (this.GetType() != p.GetType())
                return false;

            return (CommonName == p.CommonName && Type == p.Type);
        }

        public override int GetHashCode() => (CommonName).GetHashCode();

        public static bool operator ==(SAM_SEKeyStoreCertificatesProperties? lhs, SAM_SEKeyStoreCertificatesProperties? rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                    return true;

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(SAM_SEKeyStoreCertificatesProperties? lhs, SAM_SEKeyStoreCertificatesProperties? rhs) => !(lhs == rhs);
    }
}
