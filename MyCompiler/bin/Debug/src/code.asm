    .386
    .model flat, stdcall
    option casemap:none
    include kernel32.inc
    includelib kernel32.lib
    include masm32.inc
    includelib masm32.lib
    include data.asm
    .stack 100h
    .code
start:
; WRITE()
; WRITE LITERAL
    invoke StdOut, addr label0p
; READLN()
    mov   eax, 1
    push  eax
; READ CHAR
    push  255
    push  offset @buffer
    call  StdIn
    xor   edx, edx
    mov   dl, @buffer
    pop   edi
    mov   id_array_f[edi], dl
    invoke StdOut, addr clrf
; WRITE()
; WRITE LITERAL
    invoke StdOut, addr label1p
; READLN()
    mov   eax, 2
    push  eax
; READ CHAR
    push  255
    push  offset @buffer
    call  StdIn
    xor   edx, edx
    mov   dl, @buffer
    pop   edi
    mov   id_array_f[edi], dl
    invoke StdOut, addr clrf
; WRITE()
; WRITE LITERAL
    invoke StdOut, addr label2p
; READLN()
; READ INTEGER
    push  255
    push  offset @buffer
    call  StdIn
    xor   eax, eax
    xor   ebx, ebx
    mov   ecx, 10
    xor   esi, esi
    mov   bl, [@buffer]
    sub   bl, '-'
    push  0
    jnz   label3
    push  1
    inc   esi
label3:
    mov   bl, @buffer[esi]
    sub   bl, '0'
    jb    label4
    cmp   bl, 9
    ja    label4
    mul   ecx
    add   eax, ebx
    inc   esi
    jmp   label3
label4:
    pop   ebx
    cmp   ebx, 1
    jnz   label5
    neg   eax
label5:
    push  eax
    pop   eax
    mov   id_x, eax
    invoke StdOut, addr clrf
; WRITE()
; WRITE LITERAL
    invoke StdOut, addr label6p
; READLN()
; READ INTEGER
    push  255
    push  offset @buffer
    call  StdIn
    xor   eax, eax
    xor   ebx, ebx
    mov   ecx, 10
    xor   esi, esi
    mov   bl, [@buffer]
    sub   bl, '-'
    push  0
    jnz   label7
    push  1
    inc   esi
label7:
    mov   bl, @buffer[esi]
    sub   bl, '0'
    jb    label8
    cmp   bl, 9
    ja    label8
    mul   ecx
    add   eax, ebx
    inc   esi
    jmp   label7
label8:
    pop   ebx
    cmp   ebx, 1
    jnz   label9
    neg   eax
label9:
    push  eax
    pop   eax
    mov   id_y, eax
    invoke StdOut, addr clrf
; LET()
    mov   eax, id_x
    push  eax
    mov   eax, 6
    push  eax
    pop   ebx
    pop   eax
    sub   eax, ebx
    push  eax
    mov   eax, 2
    push  eax
    mov   eax, 3
    push  eax
    mov   eax, id_y
    push  eax
    pop   ebx
    pop   eax
    sub   eax, ebx
    push  eax
    pop   ebx
    pop   eax
    mul   ebx
    push  eax
    pop   eax
    pop   edi
    shl   edi, 2
    mov   id_array_mas[edi], eax
; LET()
    mov   eax, id_x
    push  eax
    mov   eax, 6
    push  eax
    pop   ebx
    pop   eax
    sub   eax, ebx
    push  eax
    pop   edi
    shl   edi, 2
    mov   eax, id_array_mas[edi]
    push  eax
    pop   eax
    mov   id_c1, eax
; IF()
    mov   eax, id_x
    push  eax
    mov   eax, id_y
    push  eax
    pop   ebx
    pop   eax
    cmp   eax, ebx
    jb    label10
    push  0
    jmp   label11
label10: 
    push  1
label11: 
    mov   eax, id_y
    push  eax
    mov   eax, 5
    push  eax
    pop   ebx
    pop   eax
    cmp   eax, ebx
    ja    label12
    push  0
    jmp   label13
label12: 
    push  1
label13: 
    pop   ebx
    pop   eax
    and   eax, ebx
    push  eax
    pop   eax
    cmp   eax, 0
    jz    label14
; IF THEN
; LET()
    mov   eax, id_x
    push  eax
    mov   eax, id_y
    push  eax
    pop   ebx
    pop   eax
    add   eax, ebx
    push  eax
    mov   eax, 8
    push  eax
    pop   ebx
    pop   eax
    add   eax, ebx
    push  eax
    pop   eax
    mov   id_a, eax
    jmp   label15
label14: 
; IF ELSE 
; LET()
    mov   eax, 5
    push  eax
    mov   eax, id_x
    push  eax
    pop   ebx
    pop   eax
    mul   ebx
    push  eax
    pop   eax
    mov   id_a, eax
label15: 
; IF()
    mov   eax, id_x
    push  eax
    mov   eax, 6
    push  eax
    pop   ebx
    pop   eax
    cmp   eax, ebx
    jne   label16
    push  0
    jmp   label17
label16: 
    push  1
label17: 
    push  1
    pop   ebx
    pop   eax
    or    eax, ebx
    push  eax
    pop   eax
    cmp   eax, 0
    jz    label18
; IF THEN
; LET()
    mov   eax, id_x
    push  eax
    mov   eax, id_c1
    push  eax
    pop   ebx
    pop   eax
    sub   eax, ebx
    push  eax
    pop   eax
    mov   id_b, eax
; WRITELN()
; WRITE LITERAL
    invoke StdOut, addr label20p
    mov   eax, id_b
    push  eax
;WRITE INTEGER (masm32)
    pop   eax
    test  eax, eax
    jns   label21
    neg   eax
    push  eax
    mov   @buffer, '-'
    mov   [@buffer+1], 0
    invoke StdOut, addr @buffer
    pop   eax
label21:
    mov   edi, 11
    mov   bl, 10
label22:
    cdq
    div   ebx
    add   dl, '0'
    dec   edi
    mov   @buffer[edi], dl
    test   eax, eax
    jne    label22
    invoke StdOut, addr @buffer[edi]
    invoke StdOut, addr clrf
    jmp   label19
label18: 
label19: 
; LET()
    mov   eax, 0
    push  eax
    pop   eax
    mov   id_x, eax
; WHILE()
label23:
    mov   eax, id_x
    push  eax
    mov   eax, 25
    push  eax
    pop   ebx
    pop   eax
    cmp   eax, ebx
    jb    label24
    push  0
    jmp   label25
label24: 
    push  1
label25: 
    pop   eax
    cmp   eax, 0
    jz    label26
; LET()
    mov   eax, id_x
    push  eax
    mov   eax, 5
    push  eax
    pop   ebx
    pop   eax
    add   eax, ebx
    push  eax
    pop   eax
    mov   id_x, eax
; LET()
    mov   eax, id_y
    push  eax
    mov   eax, 2
    push  eax
    pop   ebx
    pop   eax
    add   eax, ebx
    push  eax
    mov   eax, 4
    push  eax
    pop   ebx
    pop   eax
    mul   ebx
    push  eax
    pop   eax
    mov   id_y, eax
    jmp   label23
label26: 
; WRITELN()
; WRITE LITERAL
    invoke StdOut, addr label27p
    mov   eax, id_a
    push  eax
;WRITE INTEGER (masm32)
    pop   eax
    test  eax, eax
    jns   label28
    neg   eax
    push  eax
    mov   @buffer, '-'
    mov   [@buffer+1], 0
    invoke StdOut, addr @buffer
    pop   eax
label28:
    mov   edi, 11
    mov   bl, 10
label29:
    cdq
    div   ebx
    add   dl, '0'
    dec   edi
    mov   @buffer[edi], dl
    test   eax, eax
    jne    label29
    invoke StdOut, addr @buffer[edi]
    invoke StdOut, addr clrf
; WRITELN()
; WRITE LITERAL
    invoke StdOut, addr label30p
    mov   eax, id_x
    push  eax
;WRITE INTEGER (masm32)
    pop   eax
    test  eax, eax
    jns   label31
    neg   eax
    push  eax
    mov   @buffer, '-'
    mov   [@buffer+1], 0
    invoke StdOut, addr @buffer
    pop   eax
label31:
    mov   edi, 11
    mov   bl, 10
label32:
    cdq
    div   ebx
    add   dl, '0'
    dec   edi
    mov   @buffer[edi], dl
    test   eax, eax
    jne    label32
    invoke StdOut, addr @buffer[edi]
    invoke StdOut, addr clrf
; WRITELN()
; WRITE LITERAL
    invoke StdOut, addr label33p
    mov   eax, id_y
    push  eax
;WRITE INTEGER (masm32)
    pop   eax
    test  eax, eax
    jns   label34
    neg   eax
    push  eax
    mov   @buffer, '-'
    mov   [@buffer+1], 0
    invoke StdOut, addr @buffer
    pop   eax
label34:
    mov   edi, 11
    mov   bl, 10
label35:
    cdq
    div   ebx
    add   dl, '0'
    dec   edi
    mov   @buffer[edi], dl
    test   eax, eax
    jne    label35
    invoke StdOut, addr @buffer[edi]
    invoke StdOut, addr clrf
; WRITELN()
; WRITE LITERAL
    invoke StdOut, addr label36p
    invoke StdOut, addr clrf
; LET()
    mov   eax, 0
    push  eax
    pop   eax
    mov   id_x, eax
; FORBASIC()
    mov   eax, id_x
    push  eax
    mov   eax, 1
    push  eax
    pop   edi
    shl   edi, 2
    mov   eax, id_array_mas[edi]
    push  eax
label38:
    mov   eax, 22
    push  eax
    pop   eax
    pop   edi
    mov   ebx, id_array_mas[edi]
    push  edi
    cmp   ebx, eax
    jg    label37
    push  eax
    mov   eax, 3
    push  eax
    pop   ecx
    pop   eax
    push  ecx
; LET()
    mov   eax, id_x
    push  eax
    mov   eax, id_x
    push  eax
    pop   edi
    shl   edi, 2
    mov   eax, id_array_mas[edi]
    push  eax
    mov   eax, 1
    push  eax
    pop   ebx
    pop   eax
    sub   eax, ebx
    push  eax
    pop   eax
    pop   edi
    shl   edi, 2
    mov   id_array_mas[edi], eax
; WRITE()
    mov   eax, id_x
    push  eax
    pop   edi
    shl   edi, 2
    mov   eax, id_array_mas[edi]
    push  eax
;WRITE INTEGER (masm32)
    pop   eax
    test  eax, eax
    jns   label39
    neg   eax
    push  eax
    mov   @buffer, '-'
    mov   [@buffer+1], 0
    invoke StdOut, addr @buffer
    pop   eax
label39:
    mov   edi, 11
    mov   bl, 10
label40:
    cdq
    div   ebx
    add   dl, '0'
    dec   edi
    mov   @buffer[edi], dl
    test   eax, eax
    jne    label40
    invoke StdOut, addr @buffer[edi]
; WRITE LITERAL
    invoke StdOut, addr label41p
    pop   ecx
    pop   edi
    mov   eax, id_array_mas[edi]
    add   eax, ecx
    mov   id_array_mas[edi], eax
    push  edi
    jmp   label38
label37:
    mov   eax, id_x
    push  eax
error:
    push 5000d ;delay 5 seconds
    call Sleep
    invoke ExitProcess, 0
end   start
