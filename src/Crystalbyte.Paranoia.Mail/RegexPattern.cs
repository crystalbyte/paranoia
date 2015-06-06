#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Mail
// 
// Crystalbyte.Paranoia.Mail is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace Crystalbyte.Paranoia.Mail {
    internal static class RegexPatterns {
        public static readonly string SequenceIdPattern = @"^[aA]\d+ (NO|OK|BAD)";
        public static readonly string ReadWritePattern = @"\[READ-WRITE\]";
        public static readonly string ReadOnlyPattern = @"\[READ-ONLY\]";
        public static readonly string ExamineAndFetchFlagsPattern = @"FLAGS \(.*?\)";
        public static readonly string ExamineSingleFlagOrKeywordPattern = @"([a-zA-Z]+)|(\*)";
        public static readonly string ExamineExistsPattern = @"\d+ EXISTS";
        public static readonly string ExamineRecentPattern = @"\d+ RECENT";
        public static readonly string ExaminePermanentFlagsPattern = @"PERMANENTFLAGS \(.*\)";
        public static readonly string ExamineUidNextPattern = @"UIDNEXT \d+";
        public static readonly string ExamineUnseenPattern = @"UNSEEN \(.*\)";
        public static readonly string ExamineUidValidityPattern = @"UIDVALIDITY \d+";
        public static readonly string NonBase64CharactersPattern = @"[\u007F-\uFFFF\u0000-\u001F]+|&";
        public static readonly string Rfc2060ModifiedBase64Pattern = "&.*?-";
        public static readonly string BodyStructureResponsePattern = "BODYSTRUCTURE";
        public static readonly string InternalDateResponsePattern = "INTERNALDATE \\\".+\\\"";
        public static readonly string QuotedItemsOrNilPattern = "\".*?\"|NIL";
        public static readonly string QuotedItemsPattern = "\".*?\"";
        public static readonly string EnvelopeResponsePattern = "\\(\\(.+?\\)\\)|NIL|\"\"|<.+?>|\".+?\"";
        public static readonly string ParenthesizedItemPattern = @"\(.+?\)";
        public static readonly string CurlyParanthesizedSizePattern = "\\{\\d+\\}";
        public static readonly string BoundaryEnvelope = @"^[\r\n\s]*-+[^\r\n]+|\s*-+[^\r\n]+--(\r\n)*$";
        public static readonly string SingleFlagPattern = @"\\\w+";
        public static readonly string EmailBracketPattern = "<.+>";
        public static readonly string BodyPartCommandPattern = @"BODY[((\d+\.)+)(TEXT|HEADER|\.)]";
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
        public static readonly string EmailAddressPattern = @"[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+(?:[A-Zaa-z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b";
        
    }
}