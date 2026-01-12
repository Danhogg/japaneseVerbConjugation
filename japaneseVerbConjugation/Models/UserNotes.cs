using System;
using System.Collections.Generic;
using System.Text;

namespace JapaneseVerbConjugation.Models
{
    public sealed class UserNote
    {
        public string Text { get; set; } = null!;

        // Optional example sentences the user adds
        public List<UserExample> Examples { get; set; } = new();

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedUtc { get; set; }
    }
}
