.code
DeuteranopiaAsm proc
    ; Save callee-saved registers
    push rbx
    push rsi
    push rdi
    
    ; Save parameters
    mov rsi, rcx        ; originalImage pointer
    mov rdi, rdx        ; processedImage pointer
    mov ecx, r8d        ; pixelCount
    mov r10d, r9d       ; stride - u¿ywamy do przesuniêcia miêdzy wierszami
    mov ebx, [rsp+40h]  ; blindnessType (5th parameter)

    ; Przygotuj liczniki
    mov r8, 0          ; current row offset
    mov r9, 0          ; processed pixels counter
    mov r11d, 255      ; maksymalna wartoœæ koloru

    ; SprawdŸ typ œlepoty barw
    cmp ebx, 0
    je ProcessPixelLoopDeuteranopia
    cmp ebx, 1
    je ProcessPixelLoopProtanopia
    cmp ebx, 2
    je ProcessPixelLoopTritanopia
    
;
; DEUTERANOPIA
; Wzory:
; newR = (originalR * 0.625 + originalG * 0.375)
; newG = (originalG * 0.7)
; newB = (originalB * 0.??
;
ProcessPixelLoopDeuteranopia:
    xor rax, rax
ProcessNextPixelDeut:
    cmp r9, rcx
    jge EndLoop

    ; Za³aduj kolory RGB
    movzx eax, BYTE PTR [rsi]     ; Blue
    movzx ebx, BYTE PTR [rsi+1]   ; Green
    movzx edx, BYTE PTR [rsi+2]   ; Red

    ; Oblicz nowe wartoœci
    push rax            ; Zachowaj Blue
    
    ; newR = R * 0.625 + G * 0.375
    imul edx, 625      ; R * 0.625
    imul ebx, 375      ; G * 0.375
    add edx, ebx
    shr edx, 10        ; Dzielenie przez 1024

    ; newG = G * 0.7
    movzx ebx, BYTE PTR [rsi+1]
    imul ebx, 700
    shr ebx, 10

    ; newB = B * 0.8
    pop rax             ; Przywróæ Blue
    imul eax, 800
    shr eax, 10

    jmp StoreColors

;
; PROTANOPIA
; Wzory:
; newR = (originalR * 0.567 + originalG * 0.433)
; newG = (originalG * 0.558)
; newB = (originalB * 0.0)
;
ProcessPixelLoopProtanopia:
    xor rax, rax
ProcessNextPixelProt:
    cmp r9, rcx
    jge EndLoop

    ; Za³aduj kolory RGB
    movzx eax, BYTE PTR [rsi]     ; Blue
    movzx ebx, BYTE PTR [rsi+1]   ; Green
    movzx edx, BYTE PTR [rsi+2]   ; Red

    ; Oblicz nowe wartoœci
    ; newR = R * 0.567 + G * 0.433
    imul edx, 567      ; R * 0.567
    imul ebx, 433      ; G * 0.433
    add edx, ebx
    shr edx, 10        ; Dzielenie przez 1024

    ; newG = G * 0.558
    movzx ebx, BYTE PTR [rsi+1]
    imul ebx, 558
    shr ebx, 10

    ; newB = B * 0.0
    xor eax, eax       ; B = 0

    jmp StoreColors

;
; TRITANOPIA
; Wzory:
; newR = (originalR * 0.95)
; newG = (originalG * 0.433)
; newB = (originalB * 0.567)
;
ProcessPixelLoopTritanopia:
    xor rax, rax
ProcessNextPixelTrit:
    cmp r9, rcx
    jge EndLoop

    ; Za³aduj kolory RGB
    movzx eax, BYTE PTR [rsi]     ; Blue
    movzx ebx, BYTE PTR [rsi+1]   ; Green
    movzx edx, BYTE PTR [rsi+2]   ; Red

    ; Oblicz nowe wartoœci
    push rax            ; Zachowaj Blue

    ; newR = R * 0.95
    imul edx, 950
    shr edx, 10

    ; newG = G * 0.433
    imul ebx, 433
    shr ebx, 10

    ; newB = B * 0.567
    pop rax
    imul eax, 567
    shr eax, 10

    ; Kontynuuj do zapisywania kolorów

StoreColors:
    ; Ogranicz wartoœci do 0-255
    cmp edx, 255
    cmova edx, r11d
    cmp ebx, 255
    cmova ebx, r11d
    cmp eax, 255
    cmova eax, r11d

    ; Zapisz przetworzone kolory
    mov BYTE PTR [rdi], al      ; Blue
    mov BYTE PTR [rdi+1], bl    ; Green
    mov BYTE PTR [rdi+2], dl    ; Red

    ; PrzejdŸ do nastêpnego piksela
    add rsi, 3
    add rdi, 3
    inc r9
    
    ; SprawdŸ czy trzeba przejœæ do nastêpnego wiersza
    mov rax, r9
    xor rdx, rdx
    div r10
    cmp rdx, 0
    jne ContinueProcessing
    
    ; Dodaj padding na koñcu wiersza
    mov rax, rsi
    and rax, 3
    jz ContinueProcessing
    add rsi, rax
    add rdi, rax

ContinueProcessing:
    ; Kontynuuj odpowiedni¹ pêtlê w zale¿noœci od typu œlepoty barw
    cmp DWORD PTR [rsp+40h], 0
    je ProcessNextPixelDeut
    cmp DWORD PTR [rsp+40h], 1
    je ProcessNextPixelProt
    jmp ProcessNextPixelTrit

EndLoop:
    ; Restore registers
=======
.data
ALIGN 16
deuteranopia_coeffs:
    dword 625, 375, 700, 800    ; Coefficients for deuteranopia * 1000
protanopia_coeffs:
    dword 567, 433, 558, 0      ; Coefficients for protanopia * 1000
tritanopia_coeffs:
    dword 950, 433, 567, 0      ; Coefficients for tritanopia * 1000
max_color:
    dword 255, 255, 255, 255    ; Maximum values for RGB channels
zero:
    dword 0, 0, 0, 0            ; Zero values used for unpacking

.code
DeuteranopiaAsm proc
    ; Preserve registers to avoid overwriting their values
    push rbx
    push rsi
    push rdi
    
    ; Assign parameters to registers
    mov rsi, rcx        ; `rsi` points to the original image (originalImage pointer)
    mov rdi, rdx        ; `rdi` points to the processed image (processedImage pointer)
    mov ecx, r8d        ; `ecx` holds the number of pixels to process (pixelCount)
    mov r10d, r9d       ; `r10d` holds the stride (number of bytes per row)
    mov ebx, [rsp+40h]  ; `ebx` stores the type of color blindness (blindnessType)

    ; Load the appropriate coefficients based on blindnessType
    lea rax, deuteranopia_coeffs
    cmp ebx, 0          ; Check if blindnessType == 0 (deuteranopia)
    je load_coeffs       ; If true, jump to load_coeffs
    lea rax, protanopia_coeffs
    cmp ebx, 1          ; Check if blindnessType == 1 (protanopia)
    je load_coeffs       ; If true, jump to load_coeffs
    lea rax, tritanopia_coeffs  ; Default to tritanopia coefficients

load_coeffs:
    ; Load coefficients and constants
    movdqu xmm4, [rax]          ; Load coefficients into xmm4
    lea rax, max_color          ; Load the address of max_color into rax
    movdqu xmm5, [rax]          ; Load maximum RGB values into xmm5
    pxor xmm7, xmm7             ; Clear xmm7 (used as a zero register for unpacking)
    
    ; Calculate the number of full 4-pixel vectors
    mov eax, ecx                ; Copy pixelCount to eax
    shr eax, 2                  ; Divide pixelCount by 4
    mov r9d, eax                ; Store the number of full vector iterations in r9d
    
    ; If there are no full vectors, process remaining pixels
    test r9d, r9d
    jz process_remaining

process_vectors:
vector_loop:
    ; Load 4 pixels (12 bytes) from the original image
    movq xmm0, qword ptr [rsi]     ; Load 8 bytes (first 2 pixels)
    movd xmm1, dword ptr [rsi+8]   ; Load the next 4 bytes (3rd pixel)
    
    ; Unpack data into 32-bit integers
    punpcklbw xmm0, xmm7           ; Unpack low bytes to words
    punpcklwd xmm0, xmm7           ; Unpack low words to dwords
    
    punpcklbw xmm1, xmm7           ; Unpack low bytes to words
    punpcklwd xmm1, xmm7           ; Unpack low words to dwords
    
    ; Perform calculations for all channels
    pmulld xmm0, xmm4              ; Multiply by coefficients
    psrld xmm0, 10                 ; Divide by 1024 (shift right by 10 bits)
    
    pmulld xmm1, xmm4              ; Multiply by coefficients
    psrld xmm1, 10                 ; Divide by 1024 (shift right by 10 bits)
    
    ; Clamp values to 255
    pminud xmm0, xmm5              ; Clamp xmm0 values to 255
    pminud xmm1, xmm5              ; Clamp xmm1 values to 255
    
    ; Pack back into bytes
    packusdw xmm0, xmm1            ; Pack into 16-bit integers
    packuswb xmm0, xmm0            ; Pack into 8-bit integers
    
    ; Store processed pixels in the output image
    movq qword ptr [rdi], xmm0     ; Store the first 8 bytes (2 pixels)
    movd dword ptr [rdi+8], xmm0   ; Store the next 4 bytes (3rd pixel)
    
    ; Move to the next set of pixels
    add rsi, 12                    ; Move source pointer (4 pixels * 3 bytes)
    add rdi, 12                    ; Move destination pointer
    dec r9d                        ; Decrement vector loop counter
    jnz vector_loop                ; Repeat if more vectors to process

process_remaining:
    ; Process remaining pixels (less than 4)
    mov eax, ecx
    and eax, 3                     ; Remaining pixels = pixelCount % 4
    jz cleanup                     ; If no remaining pixels, jump to cleanup

remaining_loop:
    ; Load one pixel
    movzx r8d, BYTE PTR [rsi]      ; Load blue channel
    movzx r9d, BYTE PTR [rsi+1]    ; Load green channel
    movzx r11d, BYTE PTR [rsi+2]   ; Load red channel
    
    ; Perform calculations for the single pixel
    movd xmm0, r11d                ; Load red channel into xmm0
    movd xmm1, r9d                 ; Load green channel into xmm1
    movd xmm2, r8d                 ; Load blue channel into xmm2
    
    pmulld xmm0, xmm4              ; Multiply red by coefficients
    psrld xmm0, 10                 ; Divide by 1024
    
    pmulld xmm1, xmm4              ; Multiply green by coefficients
    psrld xmm1, 10                 ; Divide by 1024
    
    pmulld xmm2, xmm4              ; Multiply blue by coefficients
    psrld xmm2, 10                 ; Divide by 1024
    
    ; Clamp values to 255
    pminud xmm0, xmm5
    pminud xmm1, xmm5
    pminud xmm2, xmm5
    
    ; Extract results
    movd edx, xmm0                 ; New red channel
    movd ebx, xmm1                 ; New green channel
    movd ecx, xmm2                 ; New blue channel
    
    ; Store the processed pixel
    mov BYTE PTR [rdi], cl         ; Store blue channel
    mov BYTE PTR [rdi+1], bl       ; Store green channel
    mov BYTE PTR [rdi+2], dl       ; Store red channel
    
    ; Move to the next pixel
    add rsi, 3
    add rdi, 3
    dec eax
    jnz remaining_loop             ; Repeat if more remaining pixels

cleanup:
    ; Restore registers and return
    pop rdi
    pop rsi
    pop rbx
    ret

DeuteranopiaAsm endp
end