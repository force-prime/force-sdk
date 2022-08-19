# force-sdk
C# SDK for Stacks blockchain (https://www.stacks.co). 
NOTE: WIP and is going to be actively modified.
#### Parts
- [`read chain state`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/StacksApi/Stacks/WebApi) Wrapper for Hiro web API to read blockchain state (e.g. balances, transactions, etc...)
- [`launch transactions`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/StacksApi/Stacks/ChainTransactions) Build, launch and track transactions in Stacks blockchain
- [`read tokens metadata`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/StacksApi/Stacks/Metadata) Convient methods to read token metadata
- [`handle wallet`](https://github.com/stacks-force/force-sdk/blob/main/csharpsdk/StacksApi/Stacks/Wallet.cs) Wrapper for working with Stacks HD wallet
- [`unit tests`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/Test) Unit tests for all parts of the project
- [`high level abstractions`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/ChainAbstractions) High-level Stacks agnostic abstractions to use blockchain and smart contracts without any specific knowledge
- [`simple smart contracts`](https://github.com/stacks-force/force-sdk/tree/main/contracts) Basic contracts for dealing with tokens, NFTs and in-game shop
#### Upcoming
- Examples and documentation
- Unity package for seamless integration
- Nuget package for .NET (Xamarin, MAUI developers)

