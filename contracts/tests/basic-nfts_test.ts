
import { Clarinet, Tx, Chain, Account, types } from 'https://deno.land/x/clarinet@v0.31.0/index.ts';
import { assertEquals } from 'https://deno.land/std@0.90.0/testing/asserts.ts';

Clarinet.test({
    name: "Test basic-nfts",
    async fn(chain: Chain, accounts: Map<string, Account>) {
        
        const deployer = accounts.get('deployer')!;

        const wallet_1 = accounts.get('wallet_1')!;
        const wallet_2 = accounts.get('wallet_2')!;

        
        const contract = deployer.address + ".basic-nfts";

        let block = chain.mineBlock([
            Tx.contractCall('basic-nfts', 'mint', [types.principal(wallet_1.address), "u201"], deployer.address),
            Tx.contractCall('basic-nfts', 'mint', [types.principal(wallet_2.address), "u201"], deployer.address),
            Tx.contractCall('basic-nfts', 'mint', [types.principal(wallet_1.address), "u203"], deployer.address),
            Tx.contractCall('basic-nfts', 'set-base-uri', [types.ascii('https://test.test/')], deployer.address),
            Tx.contractCall('basic-nfts', 'get-token-uri', ["{id: u2, type: u201}"], wallet_1.address),
        ]);

        block.receipts[0].events.expectNonFungibleTokenMintEvent("{id: u1, type: u201}", wallet_1.address, contract, "GAME-NFT");
        block.receipts[1].events.expectNonFungibleTokenMintEvent("{id: u2, type: u201}", wallet_2.address, contract, "GAME-NFT");
        block.receipts[2].events.expectNonFungibleTokenMintEvent("{id: u3, type: u203}", wallet_1.address, contract, "GAME-NFT");
        block.receipts[3].result.expectOk();
        block.receipts[4].result.expectOk().expectAscii("https://test.test/201.json");
    },
});
