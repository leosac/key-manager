﻿using Leosac.KeyManager.Library.DivInput;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public abstract class KeyStore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public abstract string Name { get; }

        public abstract bool CanCreateKeyEntries { get; }

        public abstract bool CanDeleteKeyEntries { get; }

        public KeyStoreProperties? Properties { get; set; }

        public bool CreateIfMissing { get; set; } = false;

        public bool CheckKeyEntryExists(KeyEntry keyEntry)
        {
            return CheckKeyEntryExists(keyEntry.Identifier);
        }

        public abstract void Open();

        public abstract void Close();

        public abstract bool CheckKeyEntryExists(string identifier);

        public abstract IList<string> GetAll();

        public abstract void Create(IChangeKeyEntry keyEntry);

        public abstract KeyEntry? Get(string identifier);

        public abstract void Update(IChangeKeyEntry keyEntry, bool ignoreIfMissing = false);

        public abstract void Delete(string identifier, bool ignoreIfMissing = false);

        public virtual void Store(IChangeKeyEntry change)
        {
            Store(new List<IChangeKeyEntry>
            {
                change
            });
        }

        public abstract void Store(IList<IChangeKeyEntry> changes);

        public virtual void Publish(KeyStore store, Func<string, KeyStore?> getFavoriteKeyStore, Action<KeyStore, int>? initCallback = null)
        {
            var changes = new List<IChangeKeyEntry>();
            var ids = GetAll();
            if (initCallback != null)
            {
                initCallback(this, ids.Count);
            }
            foreach (var id in ids)
            {
                var entry = Get(id);
                if (entry != null)
                {
                    if (entry.Link != null && !string.IsNullOrEmpty(entry.Link.KeyIdentifier) && !string.IsNullOrEmpty(entry.Link.KeyStoreFavorite))
                    {
                        var cryptogram = new KeyEntryCryptogram();
                        cryptogram.Identifier = id;

                        var ks = getFavoriteKeyStore(entry.Link.KeyStoreFavorite);
                        if (ks != null)
                        {
                            ks.Open();
                            var divContext = new DivInput.DivInputContext()
                            {
                                KeyStore = ks,
                                KeyEntry = entry
                            };
                            cryptogram.Value = ks.ResolveKeyEntryLink(entry.Link.KeyIdentifier, ComputeDivInput(divContext, entry.Link.DivInput), entry.Link.WrappingKeyId, entry.Link.WrappingKeyVersion);
                            ks.Close();
                        }
                    }
                    else
                    {
                        if (entry.Variant != null)
                        {
                            foreach (var kv in entry.Variant.KeyVersions)
                            {
                                if (kv.Key.Link != null && !string.IsNullOrEmpty(kv.Key.Link.KeyIdentifier) && !string.IsNullOrEmpty(kv.Key.Link.KeyStoreFavorite))
                                {
                                    var ks = getFavoriteKeyStore(kv.Key.Link.KeyStoreFavorite);
                                    if (ks != null)
                                    {
                                        ks.Open();
                                        var divContext = new DivInput.DivInputContext()
                                        {
                                            KeyStore = ks,
                                            KeyEntry = entry,
                                            KeyVersion = kv
                                        };
                                        kv.Key.Value = ks.ResolveKeyLink(kv.Key.Link.KeyIdentifier, kv.Key.Link.KeyVersion, ComputeDivInput(divContext, kv.Key.Link.DivInput));
                                        ks.Close();
                                    }
                                }
                            }
                        }
                        changes.Add(entry);
                    }
                }
            }

            store.Open();
            store.Store(changes);
            store.Close();
        }

        private string? ComputeDivInput(DivInputContext divContext, IList<DivInputFragment> divInput)
        {
            throw new NotImplementedException();
        }

        protected void OnKeyEntryRetrieved(KeyEntry keyEntry)
        {
            if (KeyEntryRetrieved != null)
            {
                KeyEntryRetrieved(this, keyEntry);
            }
        }

        protected void OnKeyEntryUpdated(IChangeKeyEntry keyEntry)
        {
            if (KeyEntryUpdated != null)
            {
                KeyEntryUpdated(this, keyEntry);
            }
        }

        public Key? GetKey(string keyIdentifier, byte keyVersion)
        {
            log.Info(String.Format("Getting key with Key Entry Identifier `{0}` and Key Version `{1}`...", keyIdentifier, keyVersion));
            var keyEntry = Get(keyIdentifier);
            if (keyEntry != null)
            {
                if (keyEntry.Variant != null)
                {
                    var kv = keyEntry.Variant.KeyVersions.Where(kv => kv.Version == keyVersion).FirstOrDefault();
                    if (kv != null)
                    {
                        return kv.Key;
                    }
                    else
                    {
                        log.Error(String.Format("Cannot found the key version `{0}` on key entry `{1}`", keyVersion, keyIdentifier));
                    }
                }
                else
                {
                    log.Error("No key variant set on the Key Entry.");
                }
            }
            else
            {
                log.Error(String.Format("Key Entry Identifier `{0}` cannot be found.", keyIdentifier));
            }

            return null;
        }

        /// <summary>
        /// Return the change key entry cryptogram.
        /// </summary>
        /// <param name="keyIdentifier"></param>
        /// <param name="divInput"></param>
        /// <param name="wrappingKeyId"></param>
        /// <param name="wrappingKeyVersion"></param>
        /// <returns></returns>
        /// <exception cref="KeyStoreException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public abstract string? ResolveKeyEntryLink(string keyIdentifier, string? divInput = null, string? wrappingKeyId = null, byte wrappingKeyVersion = 0);

        /// <summary>
        /// Return the key value.
        /// </summary>
        /// <param name="keyIdentifier"></param>
        /// <param name="keyVersion"></param>
        /// <param name="divInput"></param>
        /// <returns></returns>
        /// <exception cref="KeyStoreException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public abstract string? ResolveKeyLink(string keyIdentifier, byte keyVersion, string? divInput = null);

        public event EventHandler<KeyEntry>? KeyEntryRetrieved;

        public event EventHandler<IChangeKeyEntry>? KeyEntryUpdated;
    }
}
