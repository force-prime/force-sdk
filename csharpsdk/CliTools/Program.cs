using ChainAbstractions;
using ChainAbstractions.Stacks;
using StacksForce.Stacks.ChainTransactions;
using StacksForce.Stacks.WebApi;

IBlockchain chain = StacksAbstractions.TestNet;

var arguments = Environment.GetCommandLineArgs();
if (arguments.Length < 3)
{
    PrintHelp();
    return;
}

switch (arguments[1])
{
    case "generate": GenerateWallet();
        return;

    case "wallet": await ShowWalletInfo();
        return;

    case "teststx": await GetTestStx();
        return;

    case "deploy": await UploadClarityCode();
        return;

    default: PrintHelp();
        return;
}

void PrintHelp()
{
    Console.WriteLine("Usage:");
    Console.WriteLine($" clitools generate <key file> - generates mnemonic phrase to <key file>");
    Console.WriteLine($" clitools wallet <key file> - shows wallet information for account from <key file>");
    Console.WriteLine($" clitools teststx <key file> - requests test STX for account from <key file>");
    Console.WriteLine($" clitools deploy <key file> <code file> - uploads clarity code from <code file> using account from <key file>");
}

async Task GetTestStx()
{
    var wallet = ReadWallet();
    var stxChain = chain.AsStacksBlockchain();
    var result = await stxChain.GetSTXTestnetTokens(wallet.GetAddress());
    if (result.IsError)
    {
        Console.WriteLine("Error requesting test tokens: " + result.Error);
        return;
    }

    var info = await TransactionInfo.ForTxId(chain.AsStacksBlockchain(), result.Data);
    stxChain.GetTransactionMonitor().WatchTransaction(info);
    await WaitForTransaction(info);
}

void GenerateWallet()
{
    var keyFile = Environment.GetCommandLineArgs()[2];

    var wallet = chain.CreateNewWallet();
    try
    {
        if (File.Exists(keyFile))
        {
            Console.WriteLine($"{keyFile} already exists!");
            return;
        }

        File.WriteAllText(keyFile, wallet.GetMnemonic());
    } catch (Exception) {
        Console.WriteLine("Error writing mnemonic!");
        return;
    }

    Console.WriteLine("Mnemonic generated successfully!");
}

IBasicWallet ReadWallet()
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

async Task ShowWalletInfo()
{
    var wallet = ReadWallet();
    var address = wallet.GetAddress();

    Console.WriteLine("Address: " + address);

    var tokens = await wallet.GetAllTokens();
    Console.WriteLine("Balances: ");
    foreach (var t in tokens)
        Console.WriteLine(t.BalanceFormatted());

    Console.WriteLine("Active transactions: ");
    var transactions = await TransactionUtils.GetTransactions(chain.AsStacksBlockchain(), address);
    if (transactions.IsError)
    {
        Console.WriteLine("Cannot read active transactions: " + transactions.Error);
        return;
    }
    foreach (var t in transactions.Data)
    {
        await t.Refresh();
        if (t.Status != StacksForce.Stacks.TransactionStatus.Pending)
            continue;
        Console.WriteLine(t);
        Console.WriteLine(t.StacksExplorerLink);
    }
}

async Task UploadClarityCode()
{
    var arguments = Environment.GetCommandLineArgs();
    if (arguments.Length < 4)
    {
        PrintHelp();
        return;
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
        return;
    }

    code = code.ReplaceLineEndings("\n");

    var contractName = Path.GetFileNameWithoutExtension(fileName);

    var transactionManager = wallet.GetTransactionManager();
    var deployTransaction = await transactionManager.GetContractDeploy(contractName, code);
    if (deployTransaction.IsError)
    {
        Console.WriteLine($"Transaction creation failed: {deployTransaction.Error}");
        return;
    }
    var info = await transactionManager.Run(deployTransaction.Data);
    if (info.IsError)
    {
        Console.WriteLine($"Transaction broadcast failed: {info.Error}");
        return;
    }
    await WaitForTransaction(info.Data);
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
