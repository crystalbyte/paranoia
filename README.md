# Paranoia

Paranoia is an email client for Windows, based on the research paper **"Security and usability engineering with particular attention to electronic mail"** published in 
2005 by Volker Roth, Tobias Straub, and Kai Richter.

You can read the entire paper [here](http://www.volkerroth.com/download/Roth2005c.pdf).

While PGP has been around since 1991 and has seen a rise in usages after the documents revealed by Edward Snowden, adoption is still very scarce.
One of the reasons, if not the reason, why adoption has failed is the fact that the tools available for the average user are inconvenient at best.
Most of today's agents like Outlook and Thunderbird do already offer encryption in one form or another, however all of them treat it as a second class citizen.
Users need to manually enable, configure and maintain their keychains and or certificate stores, which can be cumbersome, too cumbersome for most people, including myself.

The goal of Paranoia is to hide the encryption process from the user while sacrificing as little usability as possible.
While a user is made aware when encryption is being used, he or she does not have to deal with any idiosyncrasies that come with it.
Usual necessities, such as key generation, key exchange, key renewal, keychain management, decryption and encryption are fully automated and do not require any user interaction.

The integration into the current IMAP/SMTP infrastructure comes with several restrictions.
Since IMAP and SMTP rely on open MIME structures, the metadata is not and cannot be encrypted without sacrificing backward compatibility.
There are new protocols addressing this issue such as [DarkMail](https://darkmail.info/) which may be implemented at a later stage.

## Building

While the code should run out of the box, it is necessary to choose x86 or x64 as the platform target, building with "Any CPU" will fail miserably.

## License

The source code is currently licensed under the [GPLv3](http://www.gnu.org/licenses/gpl.html).
