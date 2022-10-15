## Stacks Force SDK.
The C# SDK allows interaction with the [`Stacks blockchain`](https://www.stacks.co/)
\
\
Main SDK project: [`StacksApi project`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/StacksApi)
\
Easy-to-use blockchain abstractions: [`ChainAbstractions project`](https://github.com/stacks-force/force-sdk/blob/main/csharpsdk/ChainAbstractions) (based on StacksApi project)
\
At the moment SDK projects target .NET Standard 2.1 (to be compatible with Unity, MAUI/Xamarin).
\
\
Major repo parts:

* High level abstractions (ChainAbstractions project) to start experimenting and coding right away :)
\
See [`Abstractions.cs`](https://github.com/stacks-force/force-sdk/blob/main/csharpsdk/ChainAbstractions/Abstractions.cs)

* Read blockchain state - e.g. check balances, read account's NFTs and so on (mostly read-only operations that do not create new blockchain records)
\
See [`WebApi`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/StacksApi/Stacks/WebApi) and check https://docs.hiro.so/api.
\
See [`NFT metadata`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/StacksApi/Stacks/Metadata)

* Update blockchain state - e.g. transfer funds and execute smart contracts (that's more advanced part)
\
See [`Handle wallet`](https://github.com/stacks-force/force-sdk/blob/main/csharpsdk/StacksApi/Stacks/Wallet.cs)
\
See [`Transactions`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/StacksApi/Stacks/ChainTransactions)
\
For details you should dive into:
\
https://github.com/stacksgov/sips/blob/main/sips/sip-005/sip-005-blocks-and-transactions.md

* Unit tests for Abstractions and StacksApi projects
\
See [`Test project`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/Test)

* Basic smart contract for dealing with tokens, NFTs and in-game shop
\
See [`Smart contracts`](https://github.com/stacks-force/force-sdk/tree/main/contracts)

* Examples 
\
[`ShortDemos`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/ShortDemos)
\
[`MauiSample`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/MauiSample)
\
[`CliTools`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/CliTools)
\
[`ShopSample`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/ShopSample)

### Quick-start:
0) If you are not familiar with blockchains or Stacks read [`Introduction`](https://github.com/stacks-force/force-sdk/tree/main/docs/introduction.md)
1) Clone the repo
2) Launch ShortDemos (.NET 6.0 console app) or MAUISample (.NET 6.0 MAUI app)
\
MAUISample can be installed on Android/iOS devices and more.
\
Read more about [`MAUI`](https://docs.microsoft.com/en-us/dotnet/maui/what-is-maui)
3) Explore ShopSample project, read [`instructions`](https://github.com/stacks-force/force-sdk/tree/main/docs/shop.md)
4) Explore simple but addictive [`Unity game`](https://github.com/stacks-force/force-sdk/tree/main/csharpsdk/UnitySample), read [`instructions`](https://github.com/stacks-force/force-sdk/tree/main/docs/how-to-unity.md)

### Our discord channel dedicated to .NET and Stacks development:
https://discord.gg/2hmpTGCu6y

### Next steps:
* Explore stacks ecosystem: https://www.stacks.co/
* Get acquainted with wallet software
 https://wallet.hiro.so/wallet/faq
 https://www.xverse.app/
* Learn more about Stacks, NFTs and more:
 https://gamma.io/learn
* Learn nuances of blockchain transactions:
 https://wallet.hiro.so/wallet/faq#transactions
 https://support.gamma.io/hc/en-us/articles/6011057713427-What-are-network-fees-
* Learn more about smart contracts or write your own:
 https://docs.hiro.so/
