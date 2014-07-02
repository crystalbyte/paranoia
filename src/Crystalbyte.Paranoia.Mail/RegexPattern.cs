namespace Crystalbyte.Paranoia.Mail {
    internal static class RegexPatterns {
        public const string SequenceIdPattern = @"^[aA]\d+ (NO|OK|BAD)";
        public const string ReadWritePattern = @"\[READ-WRITE\]";
        public const string ReadOnlyPattern = @"\[READ-ONLY\]";
        public const string ExamineAndFetchFlagsPattern = @"FLAGS \(.*?\)";
        public const string ExamineSingleFlagOrKeywordPattern = @"([a-zA-Z]+)|(\*)";
        public const string ExamineExistsPattern = @"\d+ EXISTS";
        public const string ExamineRecentPattern = @"\d+ RECENT";
        public const string ExaminePermanentFlagsPattern = @"PERMANENTFLAGS \(.*\)";
        public const string ExamineUidNextPattern = @"UIDNEXT \d+";
        public const string ExamineUnseenPattern = @"UNSEEN \(.*\)";
        public const string ExamineUidValidityPattern = @"UIDVALIDITY \d+";
        public const string NonBase64CharactersPattern = @"[\u007F-\uFFFF\u0000-\u001F]+|&";
        public const string Rfc2060ModifiedBase64Pattern = "&.*?-";
        public const string BodyStructureResponsePattern = "BODYSTRUCTURE";
        public const string InternalDateResponsePattern = "INTERNALDATE \\\".+\\\"";
        public const string QuotedItemsOrNilPattern = "\".*?\"|NIL";
        public const string QuotedItemsPattern = "\".*?\"";
        public const string EnvelopeResponsePattern = "\\(\\(.+?\\)\\)|NIL|\"\"|<.+?>|\".+?\"";
        public const string ParenthesizedItemPattern = @"\(.+?\)";
        public const string CurlyParanthesizedSizePattern = "\\{\\d+\\}";
        public const string BoundaryEnvelope = @"^[\r\n\s]*-+[^\r\n]+|\s*-+[^\r\n]+--(\r\n)*$";
        public const string SingleFlagPattern = @"\\\w+";
        public const string EmailBracketPattern = "<.+>";
        public const string BodyPartCommandPattern = @"BODY[((\d+\.)+)(TEXT|HEADER|\.)]";
        public static readonly string HeaderNamePattern = @"^.[^\s(\cA-\cZ)]*:";
        public static readonly string HeaderContinuationPattern = @"^\s";
        public static readonly string HeaderInlineCommentPattern = @"\(.+\)";
        public static readonly string QuotedExpressionPattern = "\"[^\"]+\"";
        public static readonly string SplitAttributeNamePattern = @"\*[0-9]+=";
        public static readonly string ContainedInInequalitySymbolsPattern = @"<.+>";
        public static readonly string QuotedPrintableOutOfBounceCharacterPattern = @"[\u007F-\u00FF=\f]{1}";
        public static readonly string QuotedPrintableTrailingWhitespacePattern = @"[\t ]+$";
        public static readonly string QuotedPrintableCroppedEncodedItemPattern = "(=|=[A-F0-9])$";
        public static readonly string QuotedPrintableEncodedItemPattern = "(=[A-F0-9\r\n]{2})+";
    }
}