
import { Clarinet, Tx, Chain, Account, types } from 'https://deno.land/x/clarinet@v0.31.0/index.ts';
import { assertEquals } from 'https://deno.land/std@0.90.0/testing/asserts.ts';

Clarinet.test({
    name: "Test utils",
    async fn(chain: Chain, accounts: Map<string, Account>) {
        const wallet_1 = accounts.get('wallet_1')!;

        let block = chain.mineBlock([
            Tx.contractCall('utils', 'uint-to-string', ["u66553344"], wallet_1.address),
            Tx.contractCall('utils', 'uint-to-string', ["u0"], wallet_1.address),
            Tx.contractCall('utils', 'uint-to-string', ["u987654321"], wallet_1.address),
        ]);

        block.receipts[0].result.expectAscii('66553344')
        block.receipts[1].result.expectAscii('0')
        block.receipts[2].result.expectAscii('987654321')
    },
});
