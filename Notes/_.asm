section .text
global SimulateDeuteranopiaASM

SimulateDeuteranopiaASM:
    ; Save registers
    push rbp
    mov rbp, rsp
    sub rsp, 32

    mov r9, rdx             ; pixelCount
    mov r8d, dword [rbp+40] ; threadCount
    mov rdx, rcx            ; originalImage
    mov rcx, rdi            ; processedImage

    ; Calculate partition size for each thread
    xor rax, rax
    mov eax, r9d
    div r8d
    mov r10d, eax           ; partitionSize

    ; Parallel processing
    mov eax, r10d
    mov r11, rdx
    mov rdx, rcx

ProcessLoop:
    cmp r9d, 0
    je Done

    ; Load the original pixel data
    movzx eax, byte [r11]
    movzx ecx, byte [r11 + 1]
    movzx edx, byte [r11 + 2]

    ; Simulate deuteranopia (green-blindness)
    imul eax, eax, 625
    imul ecx, ecx, 375
    add eax, ecx
    shr eax, 10              ; divide by 1024
    mov [rdx + 2], al       ; Red channel

    imul ecx, ecx, 7
    shr ecx, 3               ; divide by 8
    mov [rdx + 1], cl       ; Green channel

    imul edx, edx, 8
    shr edx, 3               ; divide by 8
    mov [rdx], dl           ; Blue channel

    ; Move to the next pixel
    add r11, 3
    add rdx, 3
    dec r9d
    jmp ProcessLoop

Done:
    ; Restore registers
    add rsp, 32
    pop rbp
    ret
