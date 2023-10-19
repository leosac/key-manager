using System.Security.Cryptography;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.ISLOG
{
    public class ISLOGKeyStore : KeyStore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IDictionary<int, SAMSymmetricKeyEntry> _keyEntries;

        public ISLOGKeyStore()
        {
            _keyEntries = new Dictionary<int, SAMSymmetricKeyEntry>();
        }

        public override string Name => "ISLOG SAM Manager Template";

        public override bool CanCreateKeyEntries => false;

        public override bool CanDeleteKeyEntries => false;

        public override bool CanUpdateKeyEntries => false;

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => new KeyEntryClass[] { KeyEntryClass.Symmetric };
        }

        public override Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            var id = GetId(identifier, keClass);
            return Task.FromResult(_keyEntries.ContainsKey(id));
        }

        public override Task Close()
        {
            _keyEntries.Clear();
            return Task.CompletedTask;
        }

        public override Task Create(IChangeKeyEntry keyEntry)
        {
            throw new NotImplementedException();
        }

        public override Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing)
        {
            throw new NotImplementedException();
        }

        public override Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            KeyEntry? ke = null;
            var id = GetId(identifier, keClass);
            if (_keyEntries.ContainsKey(id))
            {
                ke = _keyEntries[id];
            }

            return Task.FromResult(ke);
        }

        public override Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass)
        {
            if (keClass != null)
            {
                CheckKeyEntryClassIsSupported(keClass.Value);
            }

            return Task.FromResult((IList<KeyEntryId>)_keyEntries.Values.Select(ke => ke.Identifier).ToList());
        }

        public override async Task Open()
        {
            var fileName = GetISLOGProperties().TemplateFile;
            if (!string.IsNullOrEmpty(fileName) && System.IO.File.Exists(fileName))
            {
                Stream stream = System.IO.File.OpenRead(fileName);
                if (!string.IsNullOrEmpty(Properties?.Secret))
                {
                    using var aes = Aes.Create();
                    aes.Key = Convert.FromHexString(Properties.Secret);
                    aes.IV = new byte[16];
                    using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
                    var memstream = new MemoryStream();
                    await cryptoStream.CopyToAsync(memstream);
                    stream.Dispose();
                    memstream.Position = 0;
                    stream = memstream;
                }
                var xdoc = await XDocument.LoadAsync(stream, LoadOptions.None, new CancellationToken());
                var entries = xdoc.XPathSelectElements("/XMLSAMConfiguration/Keyentrys/item");
                foreach (var entry in entries)
                {
                    var ke = new SAMSymmetricKeyEntry();
                    var idstr = entry.XPathSelectElement("./key/int")?.Value;
                    if (!string.IsNullOrEmpty(idstr))
                    {
                        if (int.TryParse(idstr, out int id))
                        {
                            ke.Identifier.Id = id.ToString();
                            var keUIEl = entry.XPathSelectElement("./value/KeyEntryUI/KeyEntryUI");
                            if (keUIEl != null)
                            {
                                var keyType = keUIEl.Attribute("KeyType")?.Value ?? "DES";
                                // We adjust the key type if required
                                if (keyType == "AES")
                                {
                                    keyType = "AES128";
                                }
                                else if (keyType == "T3KDES")
                                {
                                    keyType = "TK3DES";
                                }
                                ke.Variant = ke.GetAllVariants().Where(v => v.Name == keyType).FirstOrDefault();
                                var keEl = keUIEl.Element("KeyEntry");
                                if (keEl != null)
                                {
                                    if (ke.Variant != null)
                                    {
                                        var keyEl = keEl.Element("Key");
                                        var versionEl = keEl.Element("KeyVersion");
                                        if (versionEl != null && keyEl != null)
                                        {
                                            KeyVersion? keyc = null;
                                            if (ke.Variant.KeyContainers.Count > 2)
                                            {
                                                keyc = ke.Variant.KeyContainers[2] as KeyVersion;
                                            }

                                            if (ke.Variant.KeyContainers[0] is KeyVersion keya)
                                            {
                                                keya.Version = byte.Parse(versionEl.Attribute("vera")?.Value ?? "0");
                                                keya.Key.SetAggregatedValueString(keyEl.Attribute("keya")?.Value ?? string.Empty);
                                            }
                                            if (ke.Variant.KeyContainers[1] is KeyVersion keyb)
                                            {
                                                keyb.Version = byte.Parse(versionEl.Attribute("verb")?.Value ?? "0");
                                                keyb.Key.SetAggregatedValueString(keyEl.Attribute("keyb")?.Value ?? string.Empty);
                                            }
                                            if (keyc != null)
                                            {
                                                keyc.Version = byte.Parse(versionEl.Attribute("verc")?.Value ?? "0");
                                                keyc.Key.SetAggregatedValueString(keyEl.Attribute("keyc")?.Value ?? string.Empty);
                                            }
                                        }
                                    }

                                    var setEl = keEl.Element("SET");
                                    if (setEl != null)
                                    {
                                        var extsetEl = keEl.Element("ExtSET");
                                        // We create LLA objects to be able to use Key Entry helpers for SET/ExtSET decoding
                                        var llaKeInfo = new LibLogicalAccess.Card.KeyEntryAV2Information
                                        {
                                            set = new byte[]
                                            {
                                                byte.Parse(setEl.Attribute("set0")?.Value ?? "0"),
                                                byte.Parse(setEl.Attribute("set1")?.Value ?? "0")
                                            },
                                            kuc = 0xff,
                                            ExtSET = byte.Parse(extsetEl?.Attribute("bitmask")?.Value ?? "0")
                                        };
                                        var llaKe = new LibLogicalAccess.Card.AV2SAMKeyEntry();
                                        llaKe.setKeyEntryInformation(llaKeInfo);
                                        SAMKeyStore.ParseKeyEntryProperties(llaKeInfo, llaKe.getSETStruct(), ke.SAMProperties);
                                    }

                                    var keInfoEl = keEl.Element("KeyEntryInformation");
                                    if (keInfoEl != null)
                                    {
                                        ke.SAMProperties!.ChangeKeyRefId = byte.Parse(keInfoEl.Attribute("cekno")?.Value ?? "0");
                                        ke.SAMProperties.ChangeKeyRefVersion = byte.Parse(keInfoEl.Attribute("cekv")?.Value ?? "0");
                                    }

                                    var desfireEl = keEl.Element("DESFire");
                                    if (desfireEl != null)
                                    {
                                        ke.SAMProperties!.DESFireAID[0] = byte.Parse(desfireEl.Attribute("desfireAid0")?.Value ?? "0");
                                        ke.SAMProperties.DESFireAID[1] = byte.Parse(desfireEl.Attribute("desfireAid1")?.Value ?? "0");
                                        ke.SAMProperties.DESFireAID[2] = byte.Parse(desfireEl.Attribute("desfireAid2")?.Value ?? "0");
                                        ke.SAMProperties.DESFireKeyNum = byte.Parse(desfireEl.Attribute("desfirekeyno")?.Value ?? "0");
                                    }

                                    var kucEl = keEl.Element("KeyUsageCounter");
                                    if (kucEl != null)
                                    {
                                        var counter = byte.Parse(kucEl.Attribute("kuc")?.Value ?? "255");
                                        if (counter != 255)
                                        {
                                            ke.SAMProperties!.KeyUsageCounter = counter;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                log.Error("Cannot parse /value/KeyEntryUI/KeyEntryUI node for the key entry.");
                            }
                            _keyEntries.Add(id, ke);
                        }
                        else
                        {
                            log.Error("Cannot parse /key/int node for the key entry.");
                        }
                    }
                    else
                    {
                        log.Error("Missing /key/int node for the key entry.");
                    }
                }
                stream.Dispose();
            }
        }

        public override Task Store(IList<IChangeKeyEntry> changes)
        {
            throw new NotImplementedException();
        }

        public override Task Update(IChangeKeyEntry keyEntry, bool ignoreIfMissing)
        {
            throw new NotImplementedException();
        }

        public ISLOGKeyStoreProperties GetISLOGProperties()
        {
            var p = Properties as ISLOGKeyStoreProperties;
            return p ?? throw new KeyStoreException("Missing ISLOG key store properties.");
        }

        private void CheckKeyEntryClassIsSupported(KeyEntryClass keClass)
        {
            if (!SupportedClasses.Contains(keClass))
            {
                throw new Exception("Unexpected Key Entry class.");
            }
        }

        private int GetId(KeyEntryId identifier, KeyEntryClass keClass)
        {
            CheckKeyEntryClassIsSupported(keClass);
            if (!int.TryParse(identifier.Id, out int id))
            {
                throw new Exception("Unexpected Key Entry identifier format.");
            }

            return id;
        }
    }
}
