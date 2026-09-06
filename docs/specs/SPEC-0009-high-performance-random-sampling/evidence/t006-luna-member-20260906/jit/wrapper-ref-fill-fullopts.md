# JIT 输出

```text
13788742998283706632
; Assembly listing for method JitDiagnostics.WrapperState:Fill(System.Span`1[ulong]):this (FullOpts)
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
            cmp     w2, #0
            ble     G_M000_IG05

G_M000_IG03:                ;; offset=0x0010
            mov     x3, xzr
            align   [0 bytes for IG04]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG04:                ;; offset=0x0014
            add     x4, x1, x3
            add     x3, x3, #8
            ldp     x5, x6, [x0, #0x08]
            ldp     x7, x8, [x0, #0x18]
            lsl     x9, x6, #17
            add     x10, x5, x8
            ror     x10, x10, #41
            add     x10, x10, x5
            eor     x7, x7, x5
            eor     x8, x8, x6
            eor     x6, x6, x7
            eor     x5, x5, x8
            eor     x7, x7, x9
            ror     x8, x8, #19
            stp     x5, x6, [x0, #0x08]
            stp     x7, x8, [x0, #0x18]
            str     x10, [x4]
            sub     w2, w2, #1
            cbnz    w2, G_M000_IG04

G_M000_IG05:                ;; offset=0x0060
            ldp     fp, lr, [sp], #0x10
            ret     lr

; Total bytes of code 104


```
