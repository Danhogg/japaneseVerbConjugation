using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseVerbConjugation.Models
{
    public sealed class DictionaryEntry
    {
        public string Kanji { get; init; } = null!;
        public string Reading { get; init; } = null!;
    }
}
