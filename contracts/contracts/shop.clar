(define-constant ERR-UNAUTHORIZED (err u601))
(define-constant ERR-INTERNAL-ERROR (err u602))

(define-constant ERR-MISSING-PURCHASE (err u503))
(define-constant ERR-INCORRECT-DATA (err u401))

(define-constant ERR-NOT-ENOUGH-BALANCE (err u301))

(define-constant CONTRACT-OWNER tx-sender)

(define-map nft-offers
    uint
    { 
        token-price: uint,
        stx-price: uint,
    }
) 

(define-data-var token-offers (list 30 {price: uint, count: uint}) (list))

(define-read-only (get-nft-cost-stx (type uint))
    (default-to u0 (get stx-price (map-get? nft-offers type)))
)

(define-read-only (get-nft-cost-token (type uint))
    (default-to u0 (get token-price (map-get? nft-offers type)))
)

(define-public (update-nft-offers (offers (list 50 {type: uint, token-price: uint, stx-price: uint})))
   (begin
       (unwrap! (contract-call? .auth check-admin contract-caller) ERR-UNAUTHORIZED)

       (map set-nft-offer-internal offers)

       (ok true)
    ) 
)

(define-public (remove-nft-offers (offers (list 50 uint)))
   (begin
       (unwrap! (contract-call? .auth check-admin contract-caller) ERR-UNAUTHORIZED)

       (map remove-nft-offer-internal offers)

       (ok true)
    ) 
)

(define-public (set-token-offers (offers (list 30 {price: uint, count: uint})))
   (begin
       (unwrap! (contract-call? .auth check-admin contract-caller) ERR-UNAUTHORIZED)

       (var-set token-offers offers)

       (ok true)
    ) 
)

(define-private (set-nft-offer-internal (p {type: uint, token-price: uint, stx-price: uint}))
    (let
    (
        (type (get type p))
        (token-price (get token-price p))
        (stx-price (get stx-price p))
    )  
        (map-set nft-offers type {token-price: token-price, stx-price: stx-price})
    )
)

(define-private (remove-nft-offer-internal (type uint))
    (map-delete nft-offers type)
)

(define-public (buy-nft-stx (type uint) (cost uint))
    (let
    (
        (balance (stx-get-balance tx-sender))
        (stx-price (get-nft-cost-stx type))
    )   
        (asserts! (> stx-price u0) ERR-MISSING-PURCHASE) 
        (asserts! (>= cost stx-price) ERR-INCORRECT-DATA) 
        (asserts! (>= balance stx-price) ERR-NOT-ENOUGH-BALANCE) 

        (unwrap! (stx-transfer? stx-price tx-sender CONTRACT-OWNER) ERR-INTERNAL-ERROR) 
        (unwrap! (contract-call? .basic-nfts mint tx-sender type) ERR-INTERNAL-ERROR) 

        (ok true)
    )
)

(define-public (buy-nft-tokens (type uint) (cost uint))
    (let
    (
        (balance (unwrap! (contract-call? .basic-token get-balance tx-sender) ERR-INTERNAL-ERROR))
        (token-price (get-nft-cost-token type))
    )   
        (asserts! (> cost u0) ERR-MISSING-PURCHASE) 
        (asserts! (>= cost token-price) ERR-INCORRECT-DATA) 
        (asserts! (>= balance token-price) ERR-NOT-ENOUGH-BALANCE) 

        (unwrap! (contract-call? .basic-token transfer token-price tx-sender CONTRACT-OWNER none) ERR-INTERNAL-ERROR) 
        (unwrap! (contract-call? .basic-nfts mint tx-sender type) ERR-INTERNAL-ERROR) 

        (ok true)
    )
)

(define-public (buy-tokens-stx (price uint) (count uint))
    (let
    (
        (balance (stx-get-balance tx-sender))
    )   
        (asserts! (is-some (index-of (var-get token-offers) {price: price, count: count})) ERR-MISSING-PURCHASE) 
        (asserts! (>= balance price) ERR-NOT-ENOUGH-BALANCE) 

        (unwrap! (stx-transfer? price tx-sender CONTRACT-OWNER) ERR-INTERNAL-ERROR) 
        (unwrap! (contract-call? .basic-token mint count tx-sender) ERR-INTERNAL-ERROR) 
        (ok true)
    )
)
