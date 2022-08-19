
import { Clarinet, Tx, Chain, Account, types } from 'https://deno.land/x/clarinet@v0.31.0/index.ts';
import { assertEquals } from 'https://deno.land/std@0.90.0/testing/asserts.ts';

Clarinet.test({
    name: "Test shop",
    async fn(chain: Chain, accounts: Map<string, Account>) {
        const deployer = accounts.get('deployer')!;

        const wallet_1 = accounts.get('wallet_1')!;
        
        const contract = deployer.address + ".shop";

        let block = chain.mineBlock([
            Tx.contractCall('auth', 'set-admins', [types.list([types.principal(contract), types.principal(deployer.address)])], deployer.address),
            Tx.contractCall('shop', 'update-nft-offers', [types.list([types.tuple({"type": "u1", "token-price": "u2000", "stx-price": "u5000000"})])], deployer.address),
            Tx.contractCall('shop', 'set-token-offers', [types.list([types.tuple({count: "u100", price: "u100"}), types.tuple({count: "u2000", price: "u200"})])], deployer.address),
            Tx.contractCall('shop', 'buy-nft-stx', ["u2", "u5000000"], wallet_1.address),
            Tx.contractCall('shop', 'buy-nft-stx', ["u1", "u4000000"], wallet_1.address),
            Tx.contractCall('shop', 'buy-nft-stx', ["u1", "u5000000"], wallet_1.address),
            Tx.contractCall('shop', 'buy-nft-tokens', ["u1", "u2000"], wallet_1.address),
            Tx.contractCall('shop', 'buy-tokens-stx', ["u100", "u2000"], wallet_1.address),
            Tx.contractCall('shop', 'buy-tokens-stx', ["u200", "u2000"], wallet_1.address),
            Tx.contractCall('shop', 'buy-nft-tokens', ["u1", "u2000"], wallet_1.address),
        ]);

        block.receipts[0].result.expectOk();
        block.receipts[1].result.expectOk(); 
        block.receipts[2].result.expectOk();
        block.receipts[3].result.expectErr().expectUint(503); 
        block.receipts[4].result.expectErr().expectUint(401); 
        block.receipts[5].result.expectOk(); 
        block.receipts[6].result.expectErr().expectUint(301); 
        block.receipts[7].result.expectErr().expectUint(503); 
        block.receipts[8].result.expectOk();
        block.receipts[9].result.expectOk();
    },
});
