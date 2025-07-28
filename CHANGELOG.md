# v1.22.0 - 28/07/2025
 - Add feature to print list of key entries (for summaries)
 - Save key entries ordering user choice
 - Fix Div Input on Key Links

# v1.21.1 - 06/19/2025
 - Fix LCP key entry name definition

# v1.21.0 - 06/11/2025
 - Add new CryptoNG Key Store
 - Add an option on NXP SAM key store properties to define the authentication mode (Unlock / AuthenticateHost)
 - Fix LCP key store opening
 - Enhance Synchronic SAM-SE key store (details on a06dedc9734942b7e3e36a8deada9465d9a1e3e6)

# v1.20.0 - 01/08/2025
 - Add random number generator feature to Key Store and allow to use an existing favorite as a random number generator for local key generation
 - Add support for numeric sort on key entry id
 - Improve key versions listing UI
 - Fix "import now" on key link
 - Fix uncatched exception during key ceremony generation
 - Fix potential crash during key printing

# v1.19.0 - 10/26/2024
 - Add KCV variants
 - Add KCV selection auto saving
 - Add key entries Sort feature
 - Add secret key change on File key store
 - Add support for secrets encryption per machine
 - Add optional Elevated/Restricted user feature
 - Add Identifier to Favorite and use it instead of Name for the favorite links
 - Fix DESFire reading test on SAM AV2/AV3 key store

# v1.18.1 - 09/06/2024

 - Fix possible race condition on initialization cleanup when opening a key store

# v1.18.0 - 08/27/2024

 - Add Key Value export as text file
 - Improve secret cleanup on publish to avoid asking several times the same secret
 - Clear clipboard on application exit
 - Add key speech on key action buttons
 - Add QrCode display of key value on key action buttons
 - Add Favorite Name to Key Store secret prompt
 - Fix favorites scrolling
 - Fix Key Entry clone

# v1.17.0 - 06/30/2024
 
 - Add Import from another key store feature
 - Add default generic Key Generation logic on Store operation (publish / import)
 - Clear favorites filter on favorite key store opening
 - Clear temporary key store secret from memory when closing key store
 - Fix freeze on Key Entry edition when using key generation feature (snackbar issue)
 - Fix key generation binding on key store opening

# v1.16.1 - 05/23/2024
 
 - Prompt for optional user elevation when editing global settings
 - Enhance Snackbar messages / errors
 - Fix Favorites file location settings usage

# v1.16.0 - 05/21/2024
 
 - Add Search input box on Favorites view
 - New Per User / Per Machine settings logic

# v1.15.0 - 04/11/2024
 
 - Add Synchronic SAM-SE Key Store
 - Add an option on PKCS#11 key store to enforce Label use
 - Add hexpubvar variable
 - Add Resolve Variables option on Publish
 - Add a new Key Store diff feature
 - Add prompt for linked Key Store secret during publish if required
 - Add key generation dialog on secret favorite opening
 - Add a button to copy a key entry
 - Add DESFire authentication tool with SAM AV
 - Add Deep Listing option on File Key Store to check all key entries on key store opening and retrieve labels
 - Fix key store close on error during key link test
 - Fix unexpected confirmation message on Key Store publish/diff cancellation
 - Fix File Key Store export to only add *.leok files to zip archive

# v1.14.0 - 03/07/2024
 
 - Add Dry Run option on publish
 - Add an optional variable (%{pubvar}) on publish
 - Add Sensitive property to PKCS#11 key entry
 - Add Favorites Import/Export buttons
 - Add Settings window, to optionally change the default file path for Favorites
 - Fix Key Store closing when an error occurs on Publish
 - Fix .NET prerequisite version error message on MSI installation
 - Move some logic from KeyManager.Library to KeyManager.Library.KeyGen to reduce dependencies list of the core library

# v1.13.1 - 02/13/2024
 
 - Add "Save" option to PKCS#11 key store user's password
 - Add attributes variables to KeyEntry Id during publish/links resolution
 - Add an option to enable/disable key links during publish
 - Add key store property to automatically hide/show key entry label if supported
 - Fix MIFARE and AES256 key variant definition

# v1.12.0 - 01/29/2024
 
 - Fix default Key Entry configuration
 - Upgrade project from .NET7 to .NET8
 - Add support for some NXP SAM AV3 features: new AES 192/256 symmetric key entries, Offline Upload and Offline Perso key entry classes and add a Reserved for Offline Perso properties. Bump dependency version to LibLogicalAccess v3.

# v1.11.2 - 11/28/2023
 
 - Fix SAM Key Entry update when only Key A is defined
 
# v1.11.1 - 11/07/2023
 
 - Fix File Key Store Properties UI display

# v1.11.0 - 11/06/2023
 
 - Update key entries properties without updating key values if unchanged, for NXP SAM and HSM PKCS#11 key store
 - Add Dump options support on ISLOG SAM Manager template migration
 - Add default key entry properties configuration, per key store
 - Add Key Entry generation button on key store (mainly for PKCS#11 key store)
 - Remove Create Key Store action on Home screen
 - Key Ceremony has evolved to two actions, Key Sharing Ceremony and Key Reunification Ceremony
 - Add fragment Print/QRCode export on Key Ceremony generation
 - Add UI for saved secret master encryption key configuration
 - Add Div Input configuration UI on Key Links
 - Fix SAM key entries push ordering
 - Fix 3K3DES SAM key entry update

# v1.10.0 - 10/19/2023
 
 - Add ISLOG SAM Manager template support as a new key store
 - Add basic File Key Store Import/Export feature (as zip)
 - Add missing confirmation message on Key Link test
 - Fix exception handling on SAM Switch To AV2 tool
 - Fix Key Link - This fix is considered as critical: keys could have been published with default value instead of expected key materials with v1.8 and v1.9 when using a Key Link because of a silent error since the move to async

# v1.9.0 - 10/16/2023
 
 - Sort SAM Key Entries on Store operation
 - Add SAM UID to log file and Get Version tool
 - Add Leosac Credential Provisioning Server key store
 - Add badge counter to all key entries selection toggle
 - Add Label to key entries list if available
 - Enhance File Key Store secret UI definition
 - Enhance Key Store secret save UI checkbox
 - Remove Key (Entry) Link Identifier default value

# v1.8.0 - 09/29/2023
 
 - Add character count to key store secret inputs
 - Add button to Select/Unselect all key entries
 - Add specific button action for key link import
 - Add SAM Get Version tool
 - Add SAM Lock/Unlock tool
 - Add a button to refresh list of key entries and add some visual feedback when loading the list
 - Sort key store favorites by name
 - Enhance key derivation from password default parameters
 - Fix key entry search
 - Fix DESFire AID input on SAM key entry

# v1.7.0 - 08/18/2023
 
 - Add confirmation message on wizard completion
 - Add Activate Mifare SAM features tool for NXP SAM AV3
 - Upgrade from WiX v3 to v4
 - Add French MSI translation

# v1.6.0 - 08/07/2023
 
 - Move from .NET6 to .NET7

# v1.5.0 - 07/10/2023
 
 - Add multiple KeyEntry selection for grouped operations
 - Fix SAM for Access Control wizard default key entries properties

# v1.4.0 - 07/05/2023
 
 - Add option to store key store Secret
 - Add option to force SAM card type

# v1.3.0 - 06/13/2023
 
 - Use WpfApp library for the general Application Layout
 - Use LibLogicalAccess MSM packages on MSI installer
 - Add confirmation message on successful Key Entries publish
 - Add button to edit key store favorite directly from Favorites list
 - Fix random issue with DialogHost when previous dialog wasn't properly closed

# v1.2.0 - 04/03/2023
 
 - Load plugins from the same sub-folder on the same context (fix enum value assignment from UI component)
 - Fix Key Ceremony start button French translation

# v1.1.0 - 03/24/2023
 
 - Key stores/entries/wizards are now independent plugins dll files
 - Add support for NXP SAM AV3 as AV2
 - Disable some UI controls for better user feedback
 - Fix Access Control Wizard
 - Fix SAM exception messages
 - Fix Key Link to skip div input computation (not implemented)

# v1.0.0 - 01/24/2023
 
 - First release of Leosac Key Manager.