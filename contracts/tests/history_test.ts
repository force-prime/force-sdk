import { Clarinet, Tx, Chain, Account, types } from 'https://deno.land/x/clarinet@v1.0.2/index.ts';
import { assertEquals } from 'https://deno.land/std@0.90.0/testing/asserts.ts';

Clarinet.test({
    name: "Test history",
    async fn(chain: Chain, accounts: Map<string, Account>) {
        const deployer = accounts.get('deployer')!;

        const wallet_1 = accounts.get('wallet_1')!;
        const wallet_2 = accounts.get('wallet_2')!;

        let block = chain.mineBlock([
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u11"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_2.address), "u11"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u12"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u13"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u14"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u15"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u16"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u17"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u18"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u19"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u20"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u21"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u22"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u23"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u24"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u25"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u26"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u27"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u28"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u29"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u30"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u31"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_1.address), "u32"], deployer.address),
           Tx.contractCall('history', 'add-entry', [types.principal(wallet_2.address), "u32"], deployer.address),
           Tx.contractCall('history', 'get-entries', [types.principal(wallet_1.address), "u0"], wallet_2.address),
           Tx.contractCall('history', 'get-entries', [types.principal(wallet_2.address), "u0"], wallet_1.address),
           Tx.contractCall('history', 'get-entries', [types.principal(wallet_1.address), "u20"], wallet_2.address),
        ]);

        assertEquals(["u11", "u12", "u13", "u14", "u15", "u16", "u17", "u18", "u19", "u20", "u21", "u22", "u23", "u24", "u25", "u26", "u27", "u28", "u29", "u30"], block.receipts[24].result.expectList());
        assertEquals(["u11", "u32"], block.receipts[25].result.expectList());
        assertEquals(["u31", "u32"], block.receipts[26].result.expectList());
    },
});
