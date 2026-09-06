# JIT 输出

```text
13788742998283706632
; Assembly listing for method JitDiagnostics.CoreCaller:Call(Anastasya.Metaheuristics.Core.Randomness.RandomSource):ulong (FullOpts)
; Emitting BLENDED_CODE for generic ARM64 on Apple
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 3 single block inlinees; 0 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            ldp     x1, x2, [x0, #0x08]
            ldp     x3, x4, [x0, #0x18]
            lsl     x5, x2, #17
            add     x6, x1, x4
            ror     x6, x6, #41
            add     x6, x6, x1
            eor     x3, x3, x1
            eor     x4, x4, x2
            eor     x2, x2, x3
            eor     x1, x1, x4
            eor     x3, x3, x5
            ror     x4, x4, #19
            stp     x1, x2, [x0, #0x08]
            stp     x3, x4, [x0, #0x18]
            mov     x0, x6

G_M000_IG03:                ;; offset=0x0044
            ldp     fp, lr, [sp], #0x10
            ret     lr

; Total bytes of code 76


```
