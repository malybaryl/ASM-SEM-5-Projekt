.code
DeuteranopiaAsm proc

SimulateColorBlindnessASM:
    ; RCX = originalImage (pointer)
    ; RDX = processedImage (pointer)
    ; R8  = pixelCount (number of pixels)
    ; R9 = stride
    ; [RSP + 8] = blindnessType
    
    ; Saving at the stack...
    push [RSP + 8]
    push RCX
    push RDX
    push R8
    push R9

    ; STACK: 
    ; blindnessType
    ; originalImage
    ; processedImage
    ; pixelCount
    ; stride  

    ; Checking blindness Type
    xor RAX, RAX
    mov RCX, [rsp]
    add RAX, RCX
    
    ; if RAX == 0 - jump to DeuteranopiaLoop
    cmp RAX, 0              
    je DeuteranopiaLoop     

    ; if RAX == 1 - jump to ProtanopiaLoop
    cmp RAX, 1              
    je ProtanopiaLoop      

    ; if RAX == 2 - jump to ProtanopiaLoop
    cmp RAX, 2              
    je TritanopiaLoop       

    ; else end
    jmp EndLoop

DeuteranopiaLoop:
    xor eax, eax
    inc eax
    jmp EndLoop

ProtanopiaLoop:
    xor eax, eax
    inc eax
    inc eax
    jmp EndLoop

TritanopiaLoop:
    xor eax, eax
    inc eax
    inc eax
    inc eax
    jmp EndLoop

EndLoop:
    ; end protocol
    ret                      
DeuteranopiaAsm endp
end
