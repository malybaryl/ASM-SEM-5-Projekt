.code
DeuteranopiaAsm proc

SimulateColorBlindnessASM:
    ; RCX = originalImage (pointer)
    ; RDX = processedImage (pointer)
    ; R8  = pixelCount (number of pixels)
    ; R9  = stride (bytes per row)
    ; R10 = blindnessType (0 = Deuteranopia, 1 = Protanopia, 2 = Tritanopia)

    mov r11, rcx              ; Save pointer to original image
    mov r12, rdx              ; Save pointer to processed image

    xor rbx, rbx              ; Use RBX as pixel counter

PixelLoop:
    cmp rbx, r8               ; Check if all pixels are processed
    jge EndLoop               ; If rbx >= pixelCount, exit

    mov rax, rbx              ; Copy pixel counter to rax
    shl rax, 1                ; rax = rbx * 2 (shift left by 1)
    add rax, rbx              ; rax = rax + rbx = rbx * 3

    ; Load color data (B, G, R)
    mov al, byte ptr [r11 + rax]     ; Load blue channel
    mov cl, byte ptr [r11 + rax + 1] ; Load green channel
    mov dl, byte ptr [r11 + rax + 2] ; Load red channel

    ; Select transformation based on blindnessType
    cmp r10, 0               ; Check if Deuteranopia
    je SimulateDeuteranopia
    cmp r10, 1               ; Check if Protanopia
    je SimulateProtanopia
    cmp r10, 2               ; Check if Tritanopia
    je SimulateTritanopia
    jmp EndPixel             ; Skip if invalid type

SimulateDeuteranopia:
    ; Transform for Deuteranopia
    movzx r8d, dl              ; Convert red channel to 32-bit
    imul r8d, 625              ; R = R * 0.625
    movzx r9d, cl              ; Convert green channel to 32-bit
    imul r9d, 375              ; G = G * 0.375
    add r8d, r9d               ; New Red = (R * 0.625 + G * 0.375)
    shr r8d, 8                 ; Divide by 256 to fit in byte range
    
     ; Green transformation (G = G * 0.7)
    movzx r9d, cl              ; Convert green channel to 32-bit
    imul r9d, 700              ; G = G * 0.7
    shr r9d, 10                ; Divide by 1024 to fit in byte range

    ; Blue transformation (B = B * 0.8)
    movzx r10d, al             ; Convert blue channel to 32-bit
    imul r10d, 800             ; B = B * 0.8
    shr r10d, 10               ; Divide by 1024 to fit in byte range

    ; Store processed pixel back into the image
    mov byte ptr [r12 + rax], al     ; Store blue channel
    mov byte ptr [r12 + rax + 1], cl ; Store green channel
    mov byte ptr [r12 + rax + 2], dl ; Store red channel

    jmp EndPixel

SimulateProtanopia:
    ; Transform for Protanopia
    movzx r8d, dl              ; Convert red channel to 32-bit
    imul r8d, 567              ; R = R * 0.567
    movzx r9d, cl              ; Convert green channel to 32-bit
    imul r9d, 433              ; G = G * 0.433
    add r8d, r9d               ; New Red = (R * 0.567 + G * 0.433)
    shr r8d, 8                 ; Divide by 256
    
     ; Green transformation (G = G * 0.558)
    movzx r9d, cl              ; Convert green channel to 32-bit
    imul r9d, 558              ; G = G * 0.558
    shr r9d, 10                ; Divide by 1024 to fit in byte range

    ; Blue transformation (B = B * 0.0)
    xor r10d, r10d             ; Set blue channel to 0

    ; Store processed pixel back into the image
    mov byte ptr [r12 + rax], al     ; Store blue channel
    mov byte ptr [r12 + rax + 1], cl ; Store green channel
    mov byte ptr [r12 + rax + 2], dl ; Store red channel

    jmp EndPixel

SimulateTritanopia:
    ; Transform for Tritanopia
    movzx r8d, dl              ; Convert red channel to 32-bit
    imul r8d, 950              ; R = R * 0.95
    movzx r9d, cl              ; Convert green channel to 32-bit
    imul r9d, 433              ; G = G * 0.433
    add r8d, r9d               ; New Red = (R * 0.95 + G * 0.433)
    shr r8d, 8                 ; Divide by 256
   
   movzx r9d, cl              ; Convert green channel to 32-bit
    imul r9d, 433              ; G = G * 0.433
    shr r9d, 10                ; Divide by 1024 to fit in byte range

    ; Blue transformation (B = B * 0.567)
    movzx r10d, al             ; Convert blue channel to 32-bit
    imul r10d, 567             ; B = B * 0.567
    shr r10d, 10               ; Divide by 1024 to fit in byte range

    ; Store processed pixel back into the image
    mov byte ptr [r12 + rax], al     ; Store blue channel
    mov byte ptr [r12 + rax + 1], cl ; Store green channel
    mov byte ptr [r12 + rax + 2], dl ; Store red channel

    jmp EndPixel

EndPixel:
    ; Store processed pixel back into the image
    mov byte ptr [r12 + rax], al     ; Store blue channel
    mov byte ptr [r12 + rax + 1], cl ; Store green channel
    mov byte ptr [r12 + rax + 2], dl ; Store red channel

    inc rbx                   ; Increment pixel counter
    jmp PixelLoop             ; Repeat for next pixel

EndLoop:
    ret
DeuteranopiaAsm endp
end
