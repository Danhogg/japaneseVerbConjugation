namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public enum NoteSaveAction
    {
        None,
        Clear,
        Save
    }

    public readonly record struct NoteSaveDecision(NoteSaveAction Action, string? NormalizedText);

    public static class NoteSavePolicy
    {
        public static NoteSaveDecision Evaluate(string? existingNotes, string? newInput)
        {
            if (string.IsNullOrWhiteSpace(newInput))
            {
                if (string.IsNullOrWhiteSpace(existingNotes))
                    return new NoteSaveDecision(NoteSaveAction.None, null);

                return new NoteSaveDecision(NoteSaveAction.Clear, null);
            }

            return new NoteSaveDecision(NoteSaveAction.Save, newInput.Trim());
        }
    }
}
