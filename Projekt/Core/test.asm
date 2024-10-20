section .text
    global main

main:
    mov rax, 60    ; syscall: exit
    xor rdi, rdi   ; status 0
    syscall        ; wywołanie systemowe
