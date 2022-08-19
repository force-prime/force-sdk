(define-constant ERR-UNAUTHORIZED (err u601))

(define-constant CONTRACT-OWNER tx-sender)

(define-data-var admins (list 50 principal) (list CONTRACT-OWNER))

(define-public (set-admins (new-admins (list 50 principal)))
   (begin
       (asserts! (is-eq contract-caller CONTRACT-OWNER) ERR-UNAUTHORIZED)
       (var-set admins new-admins)
       (ok true)
   )
)

(define-read-only (check-admin (caller principal))
    (if (is-some (index-of (var-get admins) caller)) (ok true) ERR-UNAUTHORIZED)
)