using System.Reflection;

namespace JapaneseVerbConjugation.SharedResources.Constants
{
    public static class VersionInfo
    {
        /// <summary>
        /// Gets the application version as a string (e.g., "1.0.0")
        /// </summary>
        public static string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version == null)
                return "Unknown";

            // Return Major.Minor.Patch format
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        /// <summary>
        /// Gets the full application version string for display (e.g., "v1.0.0")
        /// </summary>
        public static string GetVersionString()
        {
            return $"v{GetVersion()}";
        }

        /// <summary>
        /// Gets the application title with version (e.g., "Japanese Verb Conjugation v1.0.0")
        /// </summary>
        public static string GetApplicationTitle()
        {
            return $"Japanese Verb Conjugation {GetVersionString()}";
        }
    }
}
