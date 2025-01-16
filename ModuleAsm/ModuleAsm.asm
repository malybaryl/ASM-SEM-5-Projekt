.code
DeuteranopiaAsm proc

;
;   INITIALIZATION
;

    ; Save callee-saved registers
    movd xmm3, rcx ; IntPtr originalImage  
    movd xmm4, rdx ; IntPtr processedImage
    movd xmm5, r8  ; int pixelCount
    movd xmm6, r9  ; int stride
    movd xmm7, rbx ; int blindnessType
   
    ; Checking blindnessType
    movd eax, xmm7                      ; Load blindnessType(int) to eax

    cmp eax, 0                          ; if blindnessType == 0
    je ProcessPixelLoopDeuteranopia     ; process Deuteranopia

    cmp eax, 1                          ; if blindnessType == 1
    je ProcessPixelLoopProtanopia       ; process Protanopia

    cmp eax, 2                          ; if blindnessType == 2
    je ProcessPixelLoopTritanopia       ; process Tritanopia

    ; Default process ( if blindnessType != 0,1,2 ) is Deuteranopia

   
;
;   DEUTERANOPIA
;
; newR = (originalR * 0.625 + originalG * 0.375);                          
; newG = (originalG * 0.7);                           
; newB = (originalB * 0.8);
;
;

ProcessPixelLoopDeuteranopia:
    jmp ProcessPixelLoopDeuteranopia 

    
;
;   PROTANOPIA
;
;   newR = (originalR * 0.567 + originalG * 0.433);
;   newG = (originalG * 0.558);
;   newB = (originalB * 0.0);
;
;

ProcessPixelLoopProtanopia:
    jmp ProcessPixelLoopProtanopia           

;
;   TRITANOPIA
;
; newR = (originalR * 0.95);
; newG = (originalG * 0.433);
; newB = (originalB * 0.567);

ProcessPixelLoopTritanopia: 
    jmp ProcessPixelLoopTritanopia          


;
;   END LOOP
;

EndLoop:
    ret                           

DeuteranopiaAsm endp

end
