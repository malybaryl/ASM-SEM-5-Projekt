.code
DeuteranopiaAsm proc

SimulateDeuteranopiaASM:
    ; RCX = originalImage (pointer)
    ; RDX = processedImage (pointer)
    ; R8  = pixelCount (number of pixels)
    ; R9  = stride (bytes per row)

    mov r11, rcx              ; Save pointer to original image
    mov r10, rdx              ; Save pointer to processed image

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

    ; Simulate deuteranopia (adjust color channels)
    ; New Red channel
    movzx r8d, dl              ; Convert red channel to 32-bit
    imul r8d, 625              ; R = R * 0.625
    movzx r9d, cl              ; Convert green channel to 32-bit
    imul r9d, 375              ; G = G * 0.375
    add r8d, r9d               ; New Red = (R * 0.625 + G * 0.375)
    shr r8d, 8                 ; Divide by 256 to fit in byte range

    ; Clamp red channel
    cmp r8d, 255
    jle NoClampRed
    mov r8d, 255
NoClampRed:

    ; Transform green channel
    movzx r9d, cl              ; Reload green channel
    imul r9d, 7                ; G = G * 0.7
    shr r9d, 8                 ; Divide by 256

    ; Clamp green channel
    cmp r9d, 255
    jle NoClampGreen
    mov r9d, 255
NoClampGreen:

    ; Transform blue channel
    movzx rax, al              ; Convert blue to 32-bit
    imul rax, 8                ; B = B * 0.8
    shr rax, 8                 ; Divide by 256

    ; Clamp blue channel
    cmp rax, 255
    jle NoClampBlue
    mov rax, 255
NoClampBlue:

    ; Store processed pixel back into the image
    mov byte ptr [r10 + rax], al     ; Store blue channel
    mov byte ptr [r10 + rax + 1], cl ; Store green channel
    mov byte ptr [r10 + rax + 2], dl ; Store red channel

    inc rbx                   ; Increment pixel counter
    jmp PixelLoop             ; Repeat for next pixel

EndLoop:
    ret
DeuteranopiaAsm endp
end