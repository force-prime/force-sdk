
import { Clarinet, Tx, Chain, Account, types } from 'https://deno.land/x/clarinet@v0.31.0/index.ts';
import { assertEquals } from 'https://deno.land/std@0.90.0/testing/asserts.ts';

Clarinet.test({
    name: "Test basic-token",
    async fn(chain: Chain, accounts: Map<string, Account>) {

        const deployer = accounts.get('deployer')!;

        const wallet_1 = accounts.get('wallet_1')!;
        const wallet_2 = accounts.get('wallet_2')!;

        let block = chain.mineBlock([
            Tx.contractCall('basic-token', 'mint', ["u5000", types.principal(wallet_1.address)], deployer.address),
            Tx.contractCall('basic-token', 'get-balance', [types.principal(wallet_1.address)], wallet_1.address),
            Tx.contractCall('basic-token', 'mint', ["u5000", types.principal(wallet_1.address)], wallet_1.address),
            Tx.contractCall('basic-token', 'transfer', ["u500", types.principal(wallet_1.address), types.principal(wallet_2.address), "none"], wallet_2.address),
            Tx.contractCall('basic-token', 'transfer', ["u500", types.principal(wallet_1.address), types.principal(wallet_2.address), "none"], wallet_1.address),
            Tx.contractCall('basic-token', 'get-balance', [types.principal(wallet_1.address)], wallet_1.address),
            Tx.contractCall('basic-token', 'get-balance', [types.principal(wallet_2.address)], wallet_1.address),
        ]);

        block.receipts[0].result.expectOk();
        block.receipts[1].result.expectOk().expectUint(5000);
        block.receipts[2].result.expectErr().expectUint(601);
        block.receipts[3].result.expectErr().expectUint(601);
        block.receipts[4].result.expectOk();
        block.receipts[5].result.expectOk().expectUint(4500);
        block.receipts[6].result.expectOk().expectUint(500);
    },
});
