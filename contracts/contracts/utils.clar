(define-constant NUM_TO_CHAR (list    
    "0" "1" "2" "3" "4" "5" "6" "7" "8" "9"
))
(define-constant DIGIT_INDEX (list    
    u1 u2 u3 u4 u5 u6 u7 u8 u9 u10 u11 u12 u13 u14 u15 u16 u17 u18 u19 u20
))

(define-read-only (uint-to-string (num uint))    
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

