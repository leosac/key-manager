using System.ComponentModel.DataAnnotations;

namespace Leosac.KeyManager.Library.UI
{
    public enum KeyCeremonyType
    {
        [Display(Name = "Concatenation")]
        Concat,
        [Display(Name = "Xor")]
        Xor,
        [Display(Name = "Shamir Secret Sharing")]
        ShamirSecretSharing
    }
}
