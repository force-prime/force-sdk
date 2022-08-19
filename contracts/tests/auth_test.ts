
import { Clarinet, Tx, Chain, Account, types } from 'https://deno.land/x/clarinet@v0.31.0/index.ts';
import { assertEquals } from 'https://deno.land/std@0.90.0/testing/asserts.ts';

Clarinet.test({
    name: "Test auth",
    async fn(chain: Chain, accounts: Map<string, Account>) {
        const deployer = accounts.get('deployer')!;
        const wallet_1 = accounts.get('wallet_1')!;
        const wallet_2 = accounts.get('wallet_2')!;

        let block = chain.mineBlock([
            Tx.contractCall('auth', 'set-admins', [types.list([types.principal(deployer.address), types.principal(wallet_1.address)])], wallet_1.address),
            Tx.contractCall('auth', 'check-admin', [types.principal(wallet_1.address)], deployer.address),
            Tx.contractCall('auth', 'check-admin', [types.principal(wallet_1.address)], wallet_1.address),
            Tx.contractCall('auth', 'check-admin', [types.principal(deployer.address)], deployer.address),
            Tx.contractCall('auth', 'set-admins', [types.list([types.principal(wallet_1.address)])], deployer.address),
            Tx.contractCall('auth', 'check-admin', [types.principal(wallet_1.address)], deployer.address),
            Tx.contractCall('auth', 'check-admin', [types.principal(wallet_1.address)], wallet_2.address),
            Tx.contractCall('auth', 'check-admin', [types.principal(wallet_2.address)], deployer.address),
            Tx.contractCall('auth', 'check-admin', [types.principal(wallet_2.address)], wallet_2.address),
            Tx.contractCall('auth', 'set-admins', [types.list([types.principal(wallet_1.address), types.principal(deployer.address)])], deployer.address),
            Tx.contractCall('auth', 'check-admin', [types.principal(wallet_2.address)], wallet_1.address),
        ]);

        block.receipts[0].result.expectErr().expectUint(601); 
        block.receipts[1].result.expectErr().expectUint(601); 
        block.receipts[2].result.expectErr().expectUint(601);
        block.receipts[3].result.expectOk();
        block.receipts[4].result.expectOk();

        block.receipts[5].result.expectOk();
        block.receipts[6].result.expectOk();

        block.receipts[7].result.expectErr().expectUint(601); 
        block.receipts[8].result.expectErr().expectUint(601); 
        block.receipts[9].result.expectOk();
        block.receipts[10].result.expectErr().expectUint(601); 
    },
});
