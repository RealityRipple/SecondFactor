# ![](https://github.com/RealityRipple/SecondFactor/raw/master/SecondFactor/Resources/key.png) SecondFactor
Two-Factor authentication on your Windows PC, backed by AES-256 security.

#### Version 1.2
> Author: Andrew Sachen  
> Created: October 20, 2018  
> Updated: April 14, 2019  

Language: Visual Basic.NET  
Compiler: Visual Studio 2010  
Framework: Version 4.0+

##### Involved Technologies:
* OTPAUTH
  * BASE32
  * OTPAUTH URL
* AES (256-bit)
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
If a password is chosen, the password is SHA-256 hashed until a hash beginning and ending with the numeral "2" when displayed in hexadecimal format is found, and that value is used as the decryption key. While this may limit the possible keys, it also makes creation of the key deterministic while making reversing the password from the key extremely difficult.

## Download
You can grab the latest release from the [Official Web Site](https://realityripple.com/Software/Applications/SecondFactor/).

## License
This is free and unencumbered software released into the public domain, supported by donations, not advertisements. If you find this software useful, [please support it](https://realityripple.com/donate.php?itm=SecondFactor)!
