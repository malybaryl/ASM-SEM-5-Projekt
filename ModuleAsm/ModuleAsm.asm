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
    mov r10d, r9d       ; stride - używamy do przesunięcia między wierszami
    mov ebx, [rsp+40h]  ; blindnessType (5th parameter)

    ; Przygotuj liczniki
    mov r8, 0          ; current row offset
    mov r9, 0          ; processed pixels counter
    mov r11d, 255      ; maksymalna wartość koloru

    ; Sprawdź typ ślepoty barw
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
; newB = (originalB * 0.0)
;
ProcessPixelLoopDeuteranopia:
    xor rax, rax
ProcessNextPixelDeut:
    cmp r9, rcx
    jge EndLoop

    ; Załaduj kolory RGB
    movzx eax, BYTE PTR [rsi]     ; Blue
    movzx ebx, BYTE PTR [rsi+1]   ; Green
    movzx edx, BYTE PTR [rsi+2]   ; Red

    ; Oblicz nowe wartości
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
    pop rax             ; Przywróć Blue
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

    ; Załaduj kolory RGB
    movzx eax, BYTE PTR [rsi]     ; Blue
    movzx ebx, BYTE PTR [rsi+1]   ; Green
    movzx edx, BYTE PTR [rsi+2]   ; Red

    ; Oblicz nowe wartości
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

    ; Załaduj kolory RGB
    movzx eax, BYTE PTR [rsi]     ; Blue
    movzx ebx, BYTE PTR [rsi+1]   ; Green
    movzx edx, BYTE PTR [rsi+2]   ; Red

    ; Oblicz nowe wartości
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
    ; Ogranicz wartości do 0-255
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

    ; Przejdź do następnego piksela
    add rsi, 3
    add rdi, 3
    inc r9
    
    ; Sprawdź czy trzeba przejść do następnego wiersza
    mov rax, r9
    xor rdx, rdx
    div r10
    cmp rdx, 0
    jne ContinueProcessing
    
    ; Dodaj padding na końcu wiersza
    mov rax, rsi
    and rax, 3
    jz ContinueProcessing
    add rsi, rax
    add rdi, rax

ContinueProcessing:
    ; Kontynuuj odpowiednią pętlę w zależności od typu ślepoty barw
    cmp DWORD PTR [rsp+40h], 0
    je ProcessNextPixelDeut
    cmp DWORD PTR [rsp+40h], 1
    je ProcessNextPixelProt
    jmp ProcessNextPixelTrit

EndLoop:
    ; Restore registers
    pop rdi
    pop rsi
    pop rbx
    ret

DeuteranopiaAsm endp
end