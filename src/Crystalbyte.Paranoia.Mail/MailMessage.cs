#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using Crystalbyte.Paranoia.Mail.Mime;
using Crystalbyte.Paranoia.Mail.Mime.Header;
using Crystalbyte.Paranoia.Mail.Mime.Traverse;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    /// <summary>
    ///     This is the root of the email tree structure.<br />
    ///     <see cref="Mime.MessagePart"></see><br />
    ///     <br />
    ///     A Message (this class) contains the headers of an email message such as:
    ///     <code>- To
    ///     - From
    ///     - Subject
    ///     - Content-Type
    ///     - Message-ID</code>
    ///     which are located in the <see cref="Headers" /> property.<br />
    ///     <br />
    ///     Use the <see cref="MailMessage.MessagePart" /> property to find the actual content of the email message.
    /// </summary>
    /// <example>
    ///     Examples are available on the <a href="http://hpop.sourceforge.net/">project homepage</a>.
    /// </example>
    public class MailMessage {
        #region Public properties

        /// <summary>
        ///     Headers of the Message.
        /// </summary>
        public MessageHeader Headers { get; private set; }

        /// <summary>
        ///     This is the body of the email Message.<br />
        ///     <br />
        ///     If the body was parsed for this Message, this property will never be <see langword="null" />.
        /// </summary>
        public MessagePart MessagePart { get; private set; }

        /// <summary>
        ///     The raw content from which this message has been constructed.<br />
        ///     These bytes can be persisted and later used to recreate the Message.
        /// </summary>
        public byte[] RawMessage { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Convenience constructor for <see cref="MailMessage" />.<br />
        ///     <br />
        ///     Creates a message from a byte array. The full message including its body is parsed.
        /// </summary>
        /// <param name="rawMessageContent"> The byte array which is the message contents to parse </param>
        public MailMessage(byte[] rawMessageContent)
            : this(rawMessageContent, true) { }

        /// <summary>
        ///     Constructs a message from a byte array.<br />
        ///     <br />
        ///     The headers are always parsed, but if <paramref name="parseBody" /> is <see langword="false" />, the body is not
        ///     parsed.
        /// </summary>
        /// <param name="rawMessageContent"> The byte array which is the message contents to parse </param>
        /// <param name="parseBody">
        ///     <see langword="true" /> if the body should be parsed, <see langword="false" /> if only headers should be parsed out
        ///     of the
        ///     <paramref
        ///         name="rawMessageContent" />
        ///     byte array
        /// </param>
        public MailMessage(byte[] rawMessageContent, bool parseBody) {
            RawMessage = rawMessageContent;

            // Find the headers and the body parts of the byte array
            MessageHeader headersTemp;
            byte[] body;
            HeaderExtractor.ExtractHeadersAndBody(rawMessageContent, out headersTemp, out body);

            // Set the Headers property
            Headers = headersTemp;

            // Should we also parse the body?
            if (parseBody) {
                // Parse the body into a MessagePart
                MessagePart = new MessagePart(body, Headers);
            }
        }

        #endregion

        #region MessagePart Searching Methods

        /// <summary>
        ///     Finds the first text/plain <see cref="MessagePart" /> in this message.<br />
        ///     This is a convenience method - it simply propagates the call to <see cref="FindFirstMessagePartWithMediaType" />.
        ///     <br />
        ///     <br />
        ///     If no text/plain version is found, <see langword="null" /> is returned.
        /// </summary>
        /// <returns>
        ///     <see cref="MessagePart" /> which has a MediaType of text/plain or <see langword="null" /> if such
        ///     <see
        ///         cref="MessagePart" />
        ///     could not be found.
        /// </returns>
        public MessagePart FindFirstPlainTextVersion() {
            return FindFirstMessagePartWithMediaType("text/plain");
        }

        /// <summary>
        ///     Finds the first text/html <see cref="MessagePart" /> in this message.<br />
        ///     This is a convenience method - it simply propagates the call to <see cref="FindFirstMessagePartWithMediaType" />.
        ///     <br />
        ///     <br />
        ///     If no text/html version is found, <see langword="null" /> is returned.
        /// </summary>
        /// <returns>
        ///     <see cref="MessagePart" /> which has a MediaType of text/html or <see langword="null" /> if such
        ///     <see
        ///         cref="MessagePart" />
        ///     could not be found.
        /// </returns>
        public MessagePart FindFirstHtmlVersion() {
            return FindFirstMessagePartWithMediaType("text/html");
        }

        public List<MessagePart> FindAllTextVersions() {
            return new TextVersionFinder().VisitMessage(this);
        }

        public List<MessagePart> FindAllAttachments() {
            return new AttachmentFinder().VisitMessage(this);
        }

        /// <summary>
        ///     Finds the first <see cref="MessagePart" /> in the <see cref="MailMessage" /> hierarchy with the given MediaType.
        ///     <br />
        ///     <br />
        ///     The search in the hierarchy is a depth-first traversal.
        /// </summary>
        /// <param name="mediaType"> The MediaType to search for. Case is ignored. </param>
        /// <returns>
        ///     A <see cref="MessagePart" /> with the given MediaType or <see langword="null" /> if no such
        ///     <see
        ///         cref="MessagePart" />
        ///     was found
        /// </returns>
        public MessagePart FindFirstMessagePartWithMediaType(string mediaType) {
            return new FindFirstMessagePartWithMediaType().VisitMessage(this, mediaType);
        }

        /// <summary>
        ///     Finds all the <see cref="MessagePart" />s in the <see cref="MailMessage" /> hierarchy with the given MediaType.
        /// </summary>
        /// <param name="mediaType"> The MediaType to search for. Case is ignored. </param>
        /// <returns>
        ///     A List of <see cref="MessagePart" /> s with the given MediaType. <br /> The List might be empty if no such
        ///     <see
        ///         cref="MessagePart" />
        ///     s were found. <br /> The order of the elements in the list is the order which they are found using a depth first
        ///     traversal of the
        ///     <see
        ///         cref="MailMessage" />
        ///     hierarchy.
        /// </returns>
        public List<MessagePart> FindAllMessagePartsWithMediaType(string mediaType) {
            return new FindAllMessagePartsWithMediaType().VisitMessage(this, mediaType);
        }

        #endregion

        #region Message Persistence

        /// <summary>
        ///     Save this <see cref="MailMessage" /> to a file.<br />
        ///     <br />
        ///     Can be loaded at a later time using the <see cref="Load(FileInfo)" /> method.
        /// </summary>
        /// <param name="file"> The File location to save the <see cref="MailMessage" /> to. Existent files will be overwritten. </param>
        /// <exception cref="ArgumentNullException">
        ///     If
        ///     <paramref name="file" />
        ///     is
        ///     <see langword="null" />
        /// </exception>
        /// <exception>
        ///     Other exceptions relevant to using a
        ///     <see cref="FileStream" />
        ///     might be thrown as well
        /// </exception>
        public void Save(FileInfo file) {
            if (file == null)
                throw new ArgumentNullException("file");

            using (var stream = new FileStream(file.FullName, FileMode.Create)) {
                Save(stream);
            }
        }

        /// <summary>
        ///     Save this <see cref="MailMessage" /> to a stream.<br />
        /// </summary>
        /// <param name="messageStream"> The stream to write to </param>
        /// <exception cref="ArgumentNullException">
        ///     If
        ///     <paramref name="messageStream" />
        ///     is
        ///     <see langword="null" />
        /// </exception>
        /// <exception>
        ///     Other exceptions relevant to
        ///     <see cref="Stream.Write" />
        ///     might be thrown as well
        /// </exception>
        public void Save(Stream messageStream) {
            if (messageStream == null)
                throw new ArgumentNullException("messageStream");

            messageStream.Write(RawMessage, 0, RawMessage.Length);
        }

        /// <summary>
        ///     Loads a <see cref="MailMessage" /> from a file containing a raw email.
        /// </summary>
        /// <param name="file"> The File location to load the <see cref="MailMessage" /> from. The file must exist. </param>
        /// <exception cref="ArgumentNullException">
        ///     If
        ///     <paramref name="file" />
        ///     is
        ///     <see langword="null" />
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     If
        ///     <paramref name="file" />
        ///     does not exist
        /// </exception>
        /// <exception>
        ///     Other exceptions relevant to a
        ///     <see cref="FileStream" />
        ///     might be thrown as well
        /// </exception>
        /// <returns> A <see cref="MailMessage" /> with the content loaded from the <paramref name="file" /> </returns>
        public static MailMessage Load(FileInfo file) {
            if (file == null)
                throw new ArgumentNullException("file");

            if (!file.Exists)
                throw new FileNotFoundException("Cannot load message from non-existent file", file.FullName);

            using (var stream = new FileStream(file.FullName, FileMode.Open)) {
                return Load(stream);
            }
        }

        /// <summary>
        ///     Loads a <see cref="MailMessage" /> from a <see cref="Stream" /> containing a raw email.
        /// </summary>
        /// <param name="messageStream"> The <see cref="Stream" /> from which to load the raw <see cref="MailMessage" /> </param>
        /// <exception cref="ArgumentNullException">
        ///     If
        ///     <paramref name="messageStream" />
        ///     is
        ///     <see langword="null" />
        /// </exception>
        /// <exception>
        ///     Other exceptions relevant to
        ///     <see cref="Stream.Read" />
        ///     might be thrown as well
        /// </exception>
        /// <returns> A <see cref="MailMessage" /> with the content loaded from the <paramref name="messageStream" /> </returns>
        public static MailMessage Load(Stream messageStream) {
            if (messageStream == null)
                throw new ArgumentNullException("messageStream");

            using (var outStream = new MemoryStream()) {
                messageStream.CopyTo(outStream);
                var content = outStream.ToArray();
                return new MailMessage(content);
            }
        }

        #endregion
    }
}