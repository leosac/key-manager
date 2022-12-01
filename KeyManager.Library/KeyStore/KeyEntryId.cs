﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyEntryId : KMObject, IEquatable<KeyEntryId>
    {
        public KeyEntryId()
        {
            _id = Guid.NewGuid().ToString();
        }

        private string? _id;
        private string? _label;

        public string? Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string? Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public bool IsConfigured()
        {
            return (!string.IsNullOrEmpty(Id) || !string.IsNullOrEmpty(Label));
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as KeyEntryId);
        }

        public bool Equals(KeyEntryId? p)
        {
            if (p is null)
                return false;

            if (Object.ReferenceEquals(this, p))
                return true;

            if (this.GetType() != p.GetType())
                return false;

            return (Id == p.Id && Label == p.Label);
        }

        public override int GetHashCode() => (Id, Label).GetHashCode();

        public static bool operator ==(KeyEntryId? lhs, KeyEntryId? rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                    return true;

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(KeyEntryId? lhs, KeyEntryId? rhs) => !(lhs == rhs);
    }
}