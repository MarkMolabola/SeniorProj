using System.Linq;
using System.Text.RegularExpressions;

namespace MDRNJ.Api.Services
{
    /// <summary>
    /// Masks blocked words in chat messages and group names (whole words, any case,
    /// with simple endings like -s/-ed/-ing). Extend <see cref="BlockedWords"/> to
    /// cover more; reports and blocks handle what a word list can't.
    /// </summary>
    public static class ContentFilter
    {
        private static readonly string[] BlockedWords =
        {
            "fuck", "fucker", "motherfucker", "shit", "bullshit", "bitch", "bastard",
            "asshole", "dick", "dickhead", "cock", "cunt", "pussy", "whore", "slut",
            "twat", "wanker", "prick", "douchebag", "jackass",
        };

        private static readonly Regex Pattern = new(
            @"\b(" + string.Join("|", BlockedWords.Select(Regex.Escape)) + @")(s|es|ed|ing|er|ers|y)?\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string Clean(string text) =>
            Pattern.Replace(text, m => new string('*', m.Length));
    }
}
