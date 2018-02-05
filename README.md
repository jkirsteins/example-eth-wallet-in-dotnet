# Demo Ethereum Wallet

This is a barebones Ethereum wallet, which supports:

- generating a new secp256k1 private key
- saving and loading its state from JSON
- synchronizing with Infura's Rinkeby node
- sending transactions

## Dependencies

The main dependency is the Nethereum library, which handles key generation, and Ethereum JSON RPC
function calls.

## Structure

The Wallet is a partial class split across 3 files:

- Wallet.SerializableProperties.cs contains the properties that will be saved to/loaded from JSON
- Wallet.LoadingAndSaving.cs contains code that pertains to wallet generation, loading, and saving
- Wallet.EthereumInteraction.cs contains code that interacts with the Rinkeby node using the Nethereum library

The Wallet - upon creation - will query the Rinkeby node for the latest known block, and store it as the
last known block.

Afterwards, it can be synchronized - every synchronization action will load blocks between the last known block 
of the wallet (stored in the JSON file) and the last known block as reported by the Infura's Rinkeby node. For
every block it will download every transaction, and look if from/to values correspond to our wallet's address. If they
do, the found transactions will be stored in the JSON file.

The application is structured as a CLI application, which presents a menu with the available options:

- listing known transactions (found by the synchronization action, and stored in the local JSON file)
- sending a transaction
- performing a new synchronization action

