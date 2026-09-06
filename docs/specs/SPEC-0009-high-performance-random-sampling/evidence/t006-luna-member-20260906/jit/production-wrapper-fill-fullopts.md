# JIT 输出

```text
13788742998283706632
; Assembly listing for method Anastasya.Metaheuristics.Core.Randomness.RandomSource:Fill(System.Span`1[ulong]):this (FullOpts)
; Emitting BLENDED_CODE for generic ARM64 on Apple
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 2 single block inlinees; 0 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            cbz     w2, G_M000_IG06

G_M000_IG03:                ;; offset=0x000C
            ldp     x3, x4, [x0, #0x08]
            ldp     x5, x6, [x0, #0x18]
            mov     w7, wzr
            align   [4 bytes for IG04]
            align   [4 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG04:                ;; offset=0x0020
            ubfiz   x8, x7, #3, #32
            add     x8, x1, x8
            lsl     x9, x4, #17
            add     x10, x3, x6
            ror     x10, x10, #41
            add     x10, x10, x3
            eor     x5, x5, x3
            eor     x6, x6, x4
            eor     x4, x4, x5
            eor     x3, x3, x6
            eor     x5, x5, x9
            ror     x6, x6, #19
            str     x10, [x8]
            add     w7, w7, #1
            cmp     w7, w2
            blt     G_M000_IG04

G_M000_IG05:                ;; offset=0x0060
            stp     x3, x4, [x0, #0x08]
            stp     x5, x6, [x0, #0x18]

G_M000_IG06:                ;; offset=0x0068
            ldp     fp, lr, [sp], #0x10
            ret     lr

; Total bytes of code 112


```
