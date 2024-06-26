# Leosac Key Manager ![Logo](KeyManager/images/leosac_key.png)

-   Stable branch: master [![Build Status](https://github.com/leosac/key-manager/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/leosac/key-manager/actions/workflows/build.yml) [![Codacy Badge](https://app.codacy.com/project/badge/Grade/8b799c8a9e6a4d4bb04b77eb638678ae?branch=master)](https://app.codacy.com/gh/leosac/key-manager/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
-   Development branch: develop [![Build Status](https://github.com/leosac/key-manager/actions/workflows/build.yml/badge.svg?branch=develop)](https://github.com/leosac/key-manager/actions/workflows/build.yml) [![Codacy Badge](https://app.codacy.com/project/badge/Grade/8b799c8a9e6a4d4bb04b77eb638678ae?branch=develop)](https://app.codacy.com/gh/leosac/key-manager/dashboard?branch=develop&utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)

Leosac Key Manager is a standalone application to generate, manage and re-deploy key entries on several kind of Key Store.

## Key Store
-   Local Key Store
-   NXP :tm: SAM AV2 / AV3 Key Store
-   HSM PKCS#11 Key Store
-   Leosac Credential Provisioning Server Key Store
-   ISLOG :tm: SAM Manager template Key Store (read only)
-   Synchronic SAM SE Key Store

## Key Generation
-   Random
-   Password
-   Mnemonics (BIP-39)
-   Key Ceremony (Concat, Xor and Shamir Secret Sharing)
 
## Out of scope
It is not a key store / vault / server but a software to manage them.
You probably need specific hardware or software server in addition to this software.

## Operating System support
Only **Windows** is supported. See [this thread](https://github.com/leosac/key-manager/issues/1) for more information and to vote for Linux support.

## License and Support
The source code of this software is distributed under the **GPLv3** license except the following libraries/folders distributed under the **LGPLv3** and unless another license is specified at a folder level.
*   KeyManager.Library library
*   KeyManager.Library.Plugin library
*   KeyManager.Library.Plugin.UI library
*   KeyManager.Library.UI library
*   KeyManager.Library.KeyStore.File library
*   KeyManager.Library.KeyStore.LCP library

Please contact Leosac SAS (legal@leosac.com) for licensing and legal questions.

For support, private plugins, premium services and further updates guarantees, you should subscribe to a lifetime or annual plan. See https://leosac.com/key-manager/.

## Plugins
Key Stores, Key Entries and UI Wizards are handled as plugins. Plugins are loaded from `Plugins` folder on the installation directory.

To load a new plugin, simply copy the main plugin dll file and all its dependencies into a subfolder of the Plugins folder. You shouldn't copy the Leosac Key Manager core libraries, only the external/additional ones.
