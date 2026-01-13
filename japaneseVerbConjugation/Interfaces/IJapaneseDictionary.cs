using JapaneseVerbConjugation.Enums;
using System.Text;

namespace JapaneseVerbConjugation.Interfaces
{
    public interface IJapaneseDictionary
    {
        bool TryGetReading(string dictionaryForm, out string reading);
        bool TryGetVerbGroup(string dictionaryForm, out VerbGroupEnum group);
    }
}
