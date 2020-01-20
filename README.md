# ![](https://github.com/RealityRipple/SecondFactor/raw/master/SecondFactor/Resources/key.png) SecondFactor
Two-Factor authentication on your Windows PC, backed by AES-256 security.

#### Version 1.3
> Author: Andrew Sachen  
> Created: October 20, 2018  
> Updated: January 20, 2020  

Language: Visual Basic.NET  
Compiler: Visual Studio 2019  
Framework: Version 4.0+

##### Involved Technologies:
* OTPAUTH
  * BASE32
  * OTPAUTH URL
* AES (256-bit)
  * Zip file storage using AES-256 (PBKDF2)
    * Optional, non-standard, improved AES-256 ([ZIP-AES-PLUS](https://gist.github.com/RealityRipple/a32f2192501f4775aff36ce143ac6894))
* QR Code Capture and Decoding
* Windows Registry

## Building
This application can be compiled using Visual Studio 2010 or newer, however an Authenticode-based Digital Signature check is integrated into the code to prevent incorrectly-signed or unsigned copies from running. Comment out lines 12-16 of `/SecondFactor/ApplicationEvents.vb` to disable this signature check before compiling if you wish to build your own copy.

This application is *not* designed to support Mono/Xamarin compilation and may not work on Linux or OS X systems. In particular, there are two API calls used by this application: "WinVerifyTrust" and "SendMessage". The former call is used as part of the Authenticode Digital Signature check mentioned above, the latter is used to increase the right margin of the Password Entry Textbox (also used for OTPAUTH secret values), allowing for the Windows-style revealing eye icon to be placed inside the textbox. There may also be internal code which supports Windows UI-drawing methods specifically and may perform poorly or incorrectly on alternate Operating Systems.

## Documentation
SecondFactor supports two command-line operations, used during the Setup and Uninstallation of the application: `-reg` and `-unreg`. These commands respectively add and remove support for the `otpauth:` URL protocol.  
A third command-line operation, `-import`, is called when SecondFactor is initiated by the `otpauth:` protocol, and a standard `totp`-based URL to import should be passed as a parameter. SecondFactor will display a prompt with information about the imported URL, allowing the user to add it to the list of profiles or ignore it.

Profiles are stored in the Windows Registry, under the path `HKEY_CURRENT_USER\Software\RealityRipple Software\SecondFactor`.  
The secret value for each profile is stored in an encrypted format - however, unless the user selects a Password to protect their profiles, the decryption key is also stored in the Registry, meaning the security is little more than obfuscation.  
If a password is chosen, the password is run through PBKDF2 with HMAC-SHA-512, a randomized salt, and however many iterations your computer can manage in 1 second, which should be in the tens or hundreds of thousands of rounds.  
This replaces an older, SHA256-based key derivation function, which would SHA-256 hash the password until a hash beginning and ending with the numeral "2" when displayed in hexadecimal format was found. While no particular security vulnerability was discovered with this method, PBKDF2 provides much higher tiered security, and an incomparable number of rounds.

A "backup" feature, introduced in v1.3, adds the new functionality of being able to save and restore profiles. This feature relies on the widely accepted Zip and JSON formats.  
Each profile gets a uniquely named file, stored without compression using standard AES-256 encryption as defined by the [Zip AEx Specification](https://www.winzip.com/win/en/aes_info.html), which can be opened by a variety of archiving tools.  
This specification, however, limits the implementation of PBKDF2 by setting a constant iteration value and HMAC algorithm. To improve the security of exported profiles, SecondFactor includes the option to use a newly created [addendum](https://gist.github.com/RealityRipple/a32f2192501f4775aff36ce143ac6894) to the specification, as well as some smart code to determine the best number of rounds to use on each device, scaling upward with hardware improvements. Zip files using this altered encryption method can still be accessed and modified by existing archiving software, but the internal JSON files will fail to decrypt.  
For rationale and extra details, please read the above-linked addendum.

## Download
You can grab the latest release from the [Official Web Site](https://realityripple.com/Software/Applications/SecondFactor/).

## License
This is free and unencumbered software released into the public domain, supported by donations, not advertisements. If you find this software useful, [please support it](https://realityripple.com/donate.php?itm=SecondFactor)!
