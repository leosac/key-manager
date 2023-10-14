# Leosac Key Manager ![Logo](KeyManager/images/leosac_key.png)

 - Stable branch: master [![Build Status](https://github.com/leosac/key-manager/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/leosac/key-manager/actions/workflows/build.yml)
 - Development branch: develop [![Build Status](https://github.com/leosac/key-manager/actions/workflows/build.yml/badge.svg?branch=develop)](https://github.com/leosac/key-manager/actions/workflows/build.yml)

Leosac Key Manager is a standalone application to generate, manage and re-deploy key entries on several kind of Key Store.

## Key Store
 - Local Key Store
 - NXP SAM AV2 / AV3 Key Store
 - HSM PKCS#11 Key Store
 - Leosac Credential Provisioning Server Key Store

## Key Generation
 - Random
 - Password
 - Mnemonics (BIP-39)
 - Key Ceremony (Concat, Xor and Shamir Secret Sharing)
 
## Out of scope
It is not a key store / vault / server but a software to manage them.
You probably need specific hardware or software server in addition to this software.

## Operating System support
Only **Windows** is supported. See [this thread](https://github.com/leosac/key-manager/issues/1) for more information and to vote for Linux support.

## License and Support
The source code of this software is distributed under the GPLv3 license. Dual license may be provided, contact Leosac SAS for more information.

For support, premium services and further updates guarantees, please subscribe to a lifetime or annual plan. See https://leosac.com/key-manager/.

## Plugins
Key Stores, Key Entries and UI Wizards are handled as plugins. Plugins are loaded from `Plugins` folder on the installation directory.

To load a new plugin, simply copy the main plugin dll file and all its dependencies into a subfolder of the Plugins folder. You shouldn't copy the Leosac Key Manager core libraries, only the external/additional ones.
