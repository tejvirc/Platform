namespace Aristocrat.Monaco.G2S
{
    using System;
    using Gaming.Contracts;

    public enum G2SGameTag
    {
        G2S_newGame
    }

    public static class GameTagHelper
    {
        public static string ToInternalGameTagString(this string tag)
        {
            return tag != null && Enum.TryParse<G2SGameTag>(tag, true, out var result)
                ? result.ToGameTag().ToString()
                : string.Empty;
        }

        public static GameTag? ToGameTag(this G2SGameTag tag)
        {
            switch (tag)
            {
                case G2SGameTag.G2S_newGame:
                    return GameTag.NewGame;
            }

            return null;
        }
        public static string ToG2SGameTagString(this string tag)
        {
            return tag != null && Enum.TryParse<GameTag>(tag, true, out var result)
                ? result.ToG2SGameTag().ToString()
                : string.Empty;
        }

        public static G2SGameTag? ToG2SGameTag(this GameTag tag)
        {
            switch (tag)
            {
                case GameTag.NewGame:
                    return G2SGameTag.G2S_newGame;
            }

            return null;
        }
    }
}
