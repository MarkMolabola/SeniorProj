using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MDRNJ.Api.Services
{
    /// <summary>
    /// Masks blocked words in chat messages and group names, and keeps them out of usernames.
    /// Matches whole words in any case, with simple endings (-s/-ed/-ing...), common
    /// look-alike characters (sh1t, $hit, f@ck, sh*t) and stretched letters (fuuuck). Extend
    /// <see cref="BlockedWords"/> to cover more; reports and blocks handle what a word list can't.
    /// </summary>
    public static class ContentFilter
    {
        private static readonly string[] BlockedWords =
        {
            // Profanity
            "fuck", "fucker", "motherfucker", "fck", "fuk", "shit", "bullshit", "bitch", "bastard",
            "asshole", "dick", "dickhead", "cock", "cunt", "pussy", "whore", "slut",
            "twat", "wanker", "prick", "douchebag", "jackass", "dildo", "blowjob", "jizz",
            // Slurs
            "nigger", "nigga", "niga", "faggot", "fag", "retard", "chink", "spic", "kike",
            "tranny", "dyke", "wetback", "gook", "beaner", "raghead", "towelhead", "paki", "coon",
            // Harassment
            "kys", "killyourself",
        };

        /// <summary>Real words the endings would otherwise catch.</summary>
        private static readonly HashSet<string> Allowed = new() { "spicy", "cocky", "dicky" };

        /// <summary>
        /// Checked even inside other letters in usernames ("xxfuckxx"). Only words no ordinary
        /// name contains: "cunt", "dick" or "shit" would reject Scunthorpe, Dickson or Ashita.
        /// </summary>
        private static readonly string[] NeverInUsernames =
            { "fuck", "nigger", "nigga", "faggot", "bitch", "whore", "pussy", "retard" };

        /// <summary>Characters people swap in for a letter. Any vowel can also be hidden as * or @.</summary>
        private static readonly Dictionary<char, string> LookAlikes = new()
        {
            ['a'] = "a4@", ['b'] = "b8", ['e'] = "e3", ['g'] = "g9", ['i'] = "i1!|",
            ['o'] = "o0", ['s'] = "s5$", ['t'] = "t7+", ['u'] = "uv",
        };

        private static readonly Regex Words = Build(BlockedWords, wholeWord: true);
        private static readonly Regex Embedded = Build(NeverInUsernames, wholeWord: false);

        public static string Clean(string text)
        {
            var chars = text.ToCharArray();
            foreach (var m in Hits(Words, text))
                for (var i = m.Index; i < m.Index + m.Length; i++)
                    chars[i] = '*';
            return new string(chars);
        }

        /// <summary>True if the text has a blocked word, including run together as in a username.</summary>
        public static bool IsOffensive(string text)
        {
            // "SlayerShit99" → "Slayer Shit99" so each part is a word of its own
            var split = Regex.Replace(text, "(?<=[a-z])(?=[A-Z])", " ");
            return Hits(Words, split).Any() || Embedded.IsMatch(text);
        }

        private static IEnumerable<Match> Hits(Regex pattern, string text) =>
            pattern.Matches(text).Where(m => !Allowed.Contains(m.Value.ToLowerInvariant()));

        /// <summary>
        /// Each letter matches itself or a look-alike, repeated ("fuuuck"). Whole words can't
        /// touch other letters, so "class" and "Scunthorpe" pass.
        /// </summary>
        private static Regex Build(IEnumerable<string> words, bool wholeWord)
        {
            var alternatives = string.Join("|", words.Select(w => string.Concat(w.Select(Letter))));
            var pattern = wholeWord
                ? @"(?<![a-z])(" + alternatives + @")(s|es|ed|ing|er|ers|y)?(?![a-z])"
                : "(" + alternatives + ")";
            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        private static string Letter(char c)
        {
            var options = LookAlikes.TryGetValue(c, out var alikes) ? alikes : c.ToString();
            if ("aeiou".Contains(c)) options += "*@";
            return "[" + Regex.Escape(new string(options.Distinct().ToArray())).Replace("]", @"\]") + "]+";
        }
    }
}
