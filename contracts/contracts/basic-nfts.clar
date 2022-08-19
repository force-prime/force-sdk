
(define-constant ERR-UNAUTHORIZED (err u601))

(define-constant NUM_TO_CHAR (list    
    "0" "1" "2" "3" "4" "5" "6" "7" "8" "9"
))
(define-constant DIGIT_INDEX (list    
    u1 u2 u3 u4 u5 u6 u7 u8 u9 u10 u11 u12 u13 u14 u15 u16 u17 u18 u19 u20
))

(define-non-fungible-token GAME-NFT {id: uint, type: uint})

(define-data-var base-uri (string-ascii 60) "https://urlurl.url/")
(define-data-var token-counter uint u0)


(define-read-only (get-owner (token-id {id: uint, type: uint}))
	(ok (nft-get-owner? GAME-NFT token-id))
)

(define-read-only (get-token-uri (token-id {id: uint, type: uint}))
    (let 
    (
       (id (get id token-id))
       (type (get type token-id))
    )    
       (ok (concat (var-get base-uri) (concat (uint-to-string type) ".json")))
    )
)

(define-public (mint (recipient principal) (type uint))
    (begin
        (unwrap! (contract-call? .auth check-admin contract-caller) ERR-UNAUTHORIZED)

        (let 
            (
                (token-id-new (+ (var-get token-counter) u1))
                (token-data {id: token-id-new, type: type})
            )    
            (try! (nft-mint? GAME-NFT token-data recipient))
	    	(var-set token-counter token-id-new)
		    (ok token-data)
        )
    )
)

(define-public (set-base-uri (uri (string-ascii 60)))
    (begin
        (unwrap! (contract-call? .auth check-admin contract-caller) ERR-UNAUTHORIZED)
 
        (var-set base-uri uri)

        (ok true)
    ) 
)

(define-private (uint-to-string (num uint))    
    (if (is-eq num u0)        
        "0"
        (get str (fold concat-uint DIGIT_INDEX { number: num, str: ""}))    
    )
)

(define-private (concat-uint (index uint) (data { number: uint, str: (string-ascii 20) }))
    (let 
        (            
            (number (get number data))        
            (str (get str data))        
            (digit (mod number u10))
            (newNumber (/ number u10))
            (digitStr (unwrap-panic (element-at NUM_TO_CHAR digit)))
        )        
        (if (is-eq number u0)            
            data
            (merge data {number : newNumber, str: (unwrap-panic (as-max-len? (concat digitStr str) u20))})
        )    
    )
)

