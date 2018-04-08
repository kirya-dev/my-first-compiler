.data
; SUPPORTING VARIABLES
    @buffer   db      6
    blength   db      ?
    @buf      db      256 DUP (?)
    clrf      db      0Dh, 0Ah, 0
    output    db      6 DUP (?), 0
    err_msg   db      "Input error, try again", 0Dh, 0Ah, 0
    @true     db      "true"
    @@true    db      "true"
    @false    db      "false"
    @@false   db      "false"
; USING VARIABLES
id_array_f db 3 DUP (?)
id_array_mas dd 10 DUP (?)
id_x dd ?
id_y dd ?
id_a dd ?
id_b dd ?
id_c1 dd ?
id_i dd ?
label0p db "Enter the array of char element: ", 0
label1p db "Enter the array of char element2: ", 0
label2p db "Enter x: ", 0
label6p db "Enter y: ", 0
label20p db "b = ", 0
label27p db "a = ", 0
label30p db "x = ", 0
label33p db "y = ", 0
label36p db "Cycle FOR type Basic, step 3 :", 0
label41p db " ", 0
