using ChainAbstractions;
using ChainAbstractions.Stacks;
using StacksForce.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Stacks.WebApi;

IBlockchain chain = StacksAbstractions.TestNet;

var arguments = Environment.GetCommandLineArgs();
if (arguments.Length < 3)
{
    PrintHelp();
    return 1;
}

switch (arguments[1])
{
    case "generate": return GenerateWallet();

    case "wallet": return await ShowWalletInfo();

    case "teststx": return await GetTestStx();

    case "deploy": return await UploadClarityCode();

    default: PrintHelp();
        return 1;
}

void PrintHelp()
{
    Console.WriteLine("Usage:");
    Console.WriteLine($" clitools generate <key file> - generates mnemonic phrase to <key file>");
    Console.WriteLine($" clitools wallet <key file> - shows wallet information for account from <key file>");
    Console.WriteLine($" clitools teststx <key file> - requests test STX for account from <key file>");
    Console.WriteLine($" clitools deploy <key file> <code file> - uploads clarity code from <code file> using account from <key file>");
}

async Task<int> GetTestStx()
{
    var wallet = ReadWallet();
    var stxChain = chain.AsStacksBlockchain();
    var result = await stxChain.GetSTXTestnetTokens(wallet.GetAddress());
    if (result.IsError)
    {
        Console.WriteLine("Error requesting test tokens: " + result.Error);
        return 1;
    }

    var info = await TransactionInfo.ForTxId(chain.AsStacksBlockchain(), result.Data);
    stxChain.GetTransactionMonitor().WatchTransaction(info);
    await WaitForTransaction(info);
    return info.Data.Status == StacksForce.Stacks.TransactionStatus.Success ? 0 : 1;
}

int GenerateWallet()
{
    var keyFile = Environment.GetCommandLineArgs()[2];

    var wallet = chain.CreateNewWallet();
    try
    {
        if (File.Exists(keyFile))
        {
            Console.WriteLine($"{keyFile} already exists!");
            return 1;
        }

        File.WriteAllText(keyFile, wallet.GetMnemonic());
    } catch (Exception) {
        Console.WriteLine("Error writing mnemonic!");
        return 1;
    }

    Console.WriteLine("Mnemonic generated successfully!");
    return 0;
}

IWallet ReadWallet()
{
    var keyFile = Environment.GetCommandLineArgs()[2];

    string mnemonic = null;
    try
    {
        mnemonic = File.ReadAllText(keyFile);
    }
    catch (Exception)
    {
        Console.WriteLine("Cannot read key file: " + keyFile);
        Environment.Exit(-1);
    }

    var wallet = chain.GetWalletForMnemonic(mnemonic);
    if (wallet == null)
    {
        Console.WriteLine("Incorrect mnemonic");
        Environment.Exit(-1);
    }
    return wallet;
}

async Task<int> ShowWalletInfo()
{
    var wallet = ReadWallet();
    var address = wallet.GetAddress();
    var stacksWallet = new StacksWallet(wallet.GetMnemonic());

    Console.WriteLine("Private Key: " + stacksWallet.GetAccount(0).PrivateKey);
    Console.WriteLine("Public Key: " + stacksWallet.GetAccount(0).PublicKey);
    Console.WriteLine("Address: " + address);

    var tokens = await wallet.GetAllTokens();
    if (tokens.IsError)
    {
        Console.WriteLine("Error occured: " + tokens.Error);
        return 1;
    }

    Console.WriteLine("Balances: ");
    foreach (var t in tokens.Data)
        Console.WriteLine(t.BalanceFormatted());

    Console.WriteLine("Active transactions: ");
    var transactions = await TransactionUtils.GetTransactions(chain.AsStacksBlockchain(), address);
    if (transactions.IsError)
    {
        Console.WriteLine("Cannot read active transactions: " + transactions.Error);
        return 1;
    }
    foreach (var t in transactions.Data)
    {
        await t.Refresh();
        if (t.Status != StacksForce.Stacks.TransactionStatus.Pending)
            continue;
        Console.WriteLine(t);
        Console.WriteLine(t.StacksExplorerLink);
    }
    return 0;
}

async Task<int> UploadClarityCode()
{
    var arguments = Environment.GetCommandLineArgs();
    if (arguments.Length < 4)
    {
        PrintHelp();
        return 1;
    }

    var wallet = ReadWallet();

    string fileName = arguments[3];

    string code;
    try
    {
        code = File.ReadAllText(fileName);
    }
    catch (Exception)
    {
        Console.WriteLine($"Cannot read code from: {fileName}");
        return 1;
    }

    code = code.ReplaceLineEndings("\n");

    var contractName = Path.GetFileNameWithoutExtension(fileName);

    var transactionManager = wallet.GetTransactionManager();
    var deployTransaction = await transactionManager.GetContractDeploy(contractName, code);
    if (deployTransaction.IsError)
    {
        Console.WriteLine($"Transaction creation failed: {deployTransaction.Error}");
        return 1;
    }
    var info = await transactionManager.Run(deployTransaction.Data);
    if (info.IsError)
    {
        Console.WriteLine($"Transaction broadcast failed: {info.Error}");
        return 1;
    }
    await WaitForTransaction(info.Data);
    return info.Data.Status == StacksForce.Stacks.TransactionStatus.Success ? 0 : 1;
}

async Task WaitForTransaction(TransactionInfo transaction)
{
    var abstractTransaction = new StacksAbstractions.TransactionWrapper(transaction, null);

    Console.WriteLine("Waiting for transaction to complete...");
    Console.WriteLine("You can also check for transaction status in stacks explorer: ");
    Console.WriteLine(transaction.StacksExplorerLink);

    while (abstractTransaction.State == TransactionState.Pending || abstractTransaction.State == TransactionState.PreApproved)
    {
        await Task.Delay(10000);
    }
    Console.WriteLine("TransactionStatus: " + transaction.Status);
}
