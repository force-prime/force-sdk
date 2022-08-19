(define-constant ERR-UNAUTHORIZED (err u601))

(define-constant CONTRACT-OWNER tx-sender)

(define-constant DIGIT_INDEX (list    
    u0 u1 u2 u3 u4 u5 u6 u7 u8 u9 u10 u11 u12 u13 u14 u15 u16 u17 u18 u19
))

(define-map player2count
    principal
    uint
)

(define-map history
    {
        player: principal,
        index: uint
	}
    uint
)

(define-public (add-entry (player principal) (data uint))
    (let
    (
        (player-entry-count-current (default-to u0 (map-get? player2count player)))
        (player-entry-count-new (+ player-entry-count-current u1))
    )
        (asserts! (is-eq contract-caller CONTRACT-OWNER) ERR-UNAUTHORIZED)

        (map-set player2count player player-entry-count-new)
        (map-set history {player: player, index: player-entry-count-current} data)
        (ok true)
    ) 
)

(define-read-only (get-entries (player principal) (offset uint))
    (get result (fold get-entries-internal DIGIT_INDEX {result: (list), offset: offset, player: player}))    
)

(define-private (get-entries-internal (index uint) (data {result: (list 20 uint), offset: uint, player: principal}))
    (let
    (
        (result (get result data))        
        (offset (get offset data))
        (player (get player data))
        (real-index (+ offset index))        
        (player-entry-data (default-to u0 (map-get? history {player: player, index: real-index})))
    )
        (if (> player-entry-data u0)
            (merge data {result: (unwrap-panic (as-max-len? (append result player-entry-data) u20))})
            data)
    )
)
 