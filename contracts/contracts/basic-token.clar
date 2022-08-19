(define-constant ERR-UNAUTHORIZED (err u601))

(define-fungible-token GAME-CURRENCY)

(define-data-var token-uri (string-ascii 128) "https://urlurl.url/")

(define-read-only (get-name)
   (ok "GAME-CURRENCY")
)

(define-read-only (get-symbol)
   (ok "GACU")
)

(define-read-only (get-decimals)
   (ok u0)
)

(define-read-only (get-balance (user principal))
   (ok (ft-get-balance GAME-CURRENCY user))
)

(define-read-only (get-total-supply)
   (ok (ft-get-supply GAME-CURRENCY))
)

(define-read-only (get-token-uri)
   (ok (var-get token-uri))
)

(define-public (transfer (amount uint) (from principal) (to principal) (memo (optional (buff 34))))
  (begin
    (asserts! (is-eq from tx-sender) ERR-UNAUTHORIZED)
    (if (is-some memo)
       (print memo)
       none
    )

    (ft-transfer? GAME-CURRENCY amount from to)
  )
)

;; admin methods

(define-public (mint (amount uint) (recipient principal))
  (begin
    (unwrap! (contract-call? .auth check-admin contract-caller) ERR-UNAUTHORIZED)
    (ft-mint? GAME-CURRENCY amount recipient)
  )
)

(define-public (set-token-uri (newUri (string-ascii 128)))
  (begin
    (unwrap! (contract-call? .auth check-admin contract-caller) ERR-UNAUTHORIZED)
    (ok (var-set token-uri newUri))
  )
)