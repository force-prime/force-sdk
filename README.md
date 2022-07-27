# force-sdk
C# SDK for Stacks blockchain (https://www.stacks.co). 
NOTE: WIP and is going to be actively modified.
#### Parts
- [`read chain state`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/StacksApi/Stacks/WebApi) Wrapper for Hiro web API to read blockchain state (e.g. balances, transactions, etc...)
- [`read tokens metadata`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/StacksApi/Stacks/Metadata) Convient methods to read token metadata
- [`handle wallet`](https://github.com/stacks-force/force-sdk/blob/main/csharpsdk/StacksApi/Stacks/Wallet.cs) Wrapper for working with Stacks HD wallet
- [`unit tests`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/Test) Unit tests for all parts of the project
- [`high level abstractions`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/StacksApi/Abstractions) High-level Stacks agnostic abstractions to use blockchain without any specific knowledge
#### Upcoming
- Chain transactions support - build and launch Stacks transactions (e.g. deploy smart contract, call smart contract)
- Basic Clarity (smart contract language for Stacks) templates for token and NFT handling
- Examples and documentation
- Unity package for seamless integration
- Nuget package for .NET (Xamarin, MAUI developers)

