## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.RawFill()
       push      rsi
       push      rbx
       sub       rsp,1C8
       vmovaps   [rsp+1B0],xmm6
       vmovaps   [rsp+1A0],xmm7
       vmovaps   [rsp+190],xmm8
       vmovaps   [rsp+180],xmm9
       mov       rbx,rcx
       mov       rsi,[rbx+10]
       mov       rdx,[rbx+20]
       test      rdx,rdx
       jne       near ptr M00_L06
       xor       ecx,ecx
       xor       r8d,r8d
M00_L00:
       cmp       [rsi],sil
       test      r8d,r8d
       je        near ptr M00_L05
       mov       edx,r8d
       shr       edx,3
       lea       eax,[rdx*8]
       sub       r8d,eax
       vmovups   zmm6,[rsi+28]
       vmovups   zmm7,[rsi+68]
       vmovups   zmm8,[rsi+0A8]
       vmovups   zmm9,[rsi+0E8]
       xor       eax,eax
       jmp       near ptr M00_L02
M00_L01:
       vpaddq    zmm0,zmm6,zmm9
       vpsllq    zmm1,zmm0,17
       vpsrlq    zmm0,zmm0,29
       vpord     zmm0,zmm0,zmm1
       vpsllq    zmm1,zmm7,11
       vpxord    zmm2,zmm6,zmm8
       vpxord    zmm3,zmm7,zmm9
       vpxord    zmm4,zmm7,zmm2
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm5,zmm6,zmm3
       vpxord    zmm2,zmm2,zmm1
       vpsllq    zmm1,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm1
       vpaddq    zmm1,zmm3,zmm5
       vpsllq    zmm16,zmm1,17
       vpsrlq    zmm1,zmm1,29
       vpord     zmm1,zmm1,zmm16
       vpsllq    zmm16,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm1,zmm5,zmm1
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm16
       vpsllq    zmm16,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm16
       vpaddq    zmm16,zmm3,zmm5
       vpsllq    zmm17,zmm16,17
       vpsrlq    zmm16,zmm16,29
       vpord     zmm16,zmm16,zmm17
       vpsllq    zmm17,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm16,zmm5,zmm16
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm17
       vpsllq    zmm17,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm17
       vpaddq    zmm17,zmm3,zmm5
       vpsllq    zmm18,zmm17,17
       vpsrlq    zmm17,zmm17,29
       vpord     zmm17,zmm17,zmm18
       vpsllq    zmm18,zmm4,11
       vpxord    zmm8,zmm2,zmm5
       vpxord    zmm9,zmm3,zmm4
       vpxord    zmm7,zmm4,zmm8
       vpaddq    zmm2,zmm5,zmm17
       vpxord    zmm6,zmm5,zmm9
       vpxord    zmm8,zmm8,zmm18
       vpsllq    zmm3,zmm9,2D
       vpsrlq    zmm4,zmm9,13
       vpord     zmm9,zmm4,zmm3
       vmovups   [rcx],zmm0
       vmovups   [rcx+40],zmm1
       vmovups   [rcx+80],zmm16
       vmovups   [rcx+0C0],zmm2
       add       rcx,100
       mov       eax,r10d
M00_L02:
       lea       r10d,[rax+4]
       cmp       r10d,edx
       jle       near ptr M00_L01
M00_L03:
       cmp       eax,edx
       jl        short M00_L07
       test      r8d,r8d
       jne       near ptr M00_L08
M00_L04:
       vmovups   [rsi+28],zmm6
       vmovups   [rsi+68],zmm7
       vmovups   [rsi+0A8],zmm8
       vmovups   [rsi+0E8],zmm9
M00_L05:
       mov       rcx,[rbx+20]
       call      qword ptr [7FF91F976508]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(UInt64[])
       nop
       vzeroupper
       vmovaps   xmm6,[rsp+1B0]
       vmovaps   xmm7,[rsp+1A0]
       vmovaps   xmm8,[rsp+190]
       vmovaps   xmm9,[rsp+180]
       add       rsp,1C8
       pop       rbx
       pop       rsi
       ret
M00_L06:
       lea       rcx,[rdx+10]
       mov       r8d,[rdx+8]
       jmp       near ptr M00_L00
M00_L07:
       vpaddq    zmm0,zmm6,zmm9
       vpsllq    zmm1,zmm0,17
       vpsrlq    zmm0,zmm0,29
       vpord     zmm0,zmm0,zmm1
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vpsllq    zmm1,zmm9,2D
       vpsrlq    zmm2,zmm9,13
       vpord     zmm9,zmm2,zmm1
       vmovups   [rcx],zmm0
       add       rcx,40
       inc       eax
       jmp       near ptr M00_L03
M00_L08:
       vpaddq    zmm0,zmm6,zmm9
       vpsllq    zmm1,zmm0,17
       vpsrlq    zmm0,zmm0,29
       vpord     zmm0,zmm0,zmm1
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vmovups   [rsp+20],zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vpsllq    zmm0,zmm9,2D
       vpsrlq    zmm1,zmm9,13
       vpord     zmm9,zmm1,zmm0
       lea       rdx,[rsp+20]
       vmovups   [rsp+120],zmm6
       vmovups   [rsp+0E0],zmm7
       vmovups   [rsp+60],zmm9
       vmovups   [rsp+0A0],zmm8
       call      qword ptr [7FF91F9765B0]
       vmovups   zmm6,[rsp+120]
       vmovups   zmm7,[rsp+0E0]
       vmovups   zmm9,[rsp+60]
       vmovups   zmm8,[rsp+0A0]
       jmp       near ptr M00_L04
; Total bytes of code 979
```
```assembly
; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(UInt64[])
       mov       eax,[rcx+8]
       test      eax,eax
       je        short M01_L01
       dec       eax
       mov       rax,[rcx+rax*8+10]
M01_L00:
       vxorps    xmm0,xmm0,xmm0
       vcvtusi2sd xmm0,xmm0,rax
       ret
M01_L01:
       xor       eax,eax
       jmp       short M01_L00
; Total bytes of code 29
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.UnitFill()
       push      rsi
       push      rbx
       sub       rsp,1C8
       vmovaps   [rsp+1B0],xmm6
       vmovaps   [rsp+1A0],xmm7
       vmovaps   [rsp+190],xmm8
       vmovaps   [rsp+180],xmm9
       mov       rbx,rcx
       mov       rsi,[rbx+10]
       mov       rdx,[rbx+28]
       test      rdx,rdx
       je        near ptr M00_L02
       lea       rcx,[rdx+10]
       mov       r8d,[rdx+8]
M00_L00:
       cmp       [rsi],sil
       test      r8d,r8d
       je        near ptr M00_L05
       mov       edx,r8d
       shr       edx,3
       lea       eax,[rdx*8]
       sub       r8d,eax
       vmovups   zmm6,[rsi+28]
       vmovups   zmm7,[rsi+68]
       vmovups   zmm8,[rsi+0A8]
       vmovups   zmm9,[rsi+0E8]
       xor       eax,eax
M00_L01:
       lea       r10d,[rax+4]
       cmp       r10d,edx
       jg        near ptr M00_L03
       vpaddq    zmm0,zmm6,zmm9
       vpsllq    zmm1,zmm0,17
       vpsrlq    zmm0,zmm0,29
       vpord     zmm0,zmm0,zmm1
       vpsllq    zmm1,zmm7,11
       vpxord    zmm2,zmm6,zmm8
       vpxord    zmm3,zmm7,zmm9
       vpxord    zmm4,zmm7,zmm2
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm5,zmm6,zmm3
       vpxord    zmm2,zmm2,zmm1
       vpsllq    zmm1,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm1
       vpsrlq    zmm0,zmm0,0B
       vcvtuqq2pd zmm0,zmm0
       vbroadcastsd zmm1,qword ptr [7FF91F5ED5C8]
       vmulpd    zmm0,zmm0,zmm1
       vpaddq    zmm16,zmm3,zmm5
       vpsllq    zmm17,zmm16,17
       vpsrlq    zmm16,zmm16,29
       vpord     zmm16,zmm16,zmm17
       vpsllq    zmm17,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm16,zmm5,zmm16
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm17
       vpsllq    zmm17,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm17
       vpsrlq    zmm16,zmm16,0B
       vcvtuqq2pd zmm16,zmm16
       vmulpd    zmm16,zmm16,zmm1
       vpaddq    zmm17,zmm3,zmm5
       vpsllq    zmm18,zmm17,17
       vpsrlq    zmm17,zmm17,29
       vpord     zmm17,zmm17,zmm18
       vpsllq    zmm18,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm17,zmm5,zmm17
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm18
       vpsllq    zmm18,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm18
       vpsrlq    zmm17,zmm17,0B
       vcvtuqq2pd zmm17,zmm17
       vmulpd    zmm17,zmm17,zmm1
       vpaddq    zmm18,zmm3,zmm5
       vpsllq    zmm19,zmm18,17
       vpsrlq    zmm18,zmm18,29
       vpord     zmm18,zmm18,zmm19
       vpsllq    zmm19,zmm4,11
       vpxord    zmm8,zmm2,zmm5
       vpxord    zmm9,zmm3,zmm4
       vpxord    zmm7,zmm4,zmm8
       vpaddq    zmm2,zmm5,zmm18
       vpxord    zmm6,zmm5,zmm9
       vpxord    zmm8,zmm8,zmm19
       vpsllq    zmm3,zmm9,2D
       vpsrlq    zmm4,zmm9,13
       vpord     zmm9,zmm4,zmm3
       vpsrlq    zmm2,zmm2,0B
       vcvtuqq2pd zmm2,zmm2
       vmulpd    zmm1,zmm2,zmm1
       vmovups   [rcx],zmm0
       vmovups   [rcx+40],zmm16
       vmovups   [rcx+80],zmm17
       vmovups   [rcx+0C0],zmm1
       add       rcx,100
       mov       eax,r10d
       jmp       near ptr M00_L01
M00_L02:
       xor       ecx,ecx
       xor       r8d,r8d
       jmp       near ptr M00_L00
M00_L03:
       cmp       eax,edx
       jl        short M00_L06
       test      r8d,r8d
       jne       near ptr M00_L07
M00_L04:
       vmovups   [rsi+28],zmm6
       vmovups   [rsi+68],zmm7
       vmovups   [rsi+0A8],zmm8
       vmovups   [rsi+0E8],zmm9
M00_L05:
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F976508]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
       nop
       vzeroupper
       vmovaps   xmm6,[rsp+1B0]
       vmovaps   xmm7,[rsp+1A0]
       vmovaps   xmm8,[rsp+190]
       vmovaps   xmm9,[rsp+180]
       add       rsp,1C8
       pop       rbx
       pop       rsi
       ret
M00_L06:
       vpaddq    zmm0,zmm6,zmm9
       vpsllq    zmm1,zmm0,17
       vpsrlq    zmm0,zmm0,29
       vpord     zmm0,zmm0,zmm1
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vpsllq    zmm1,zmm9,2D
       vpsrlq    zmm2,zmm9,13
       vpord     zmm9,zmm2,zmm1
       vpsrlq    zmm0,zmm0,0B
       vcvtuqq2pd zmm0,zmm0
       vbroadcastsd zmm1,qword ptr [7FF91F5ED5C8]
       vmulpd    zmm1,zmm0,zmm1
       vmovups   [rcx],zmm1
       add       rcx,40
       inc       eax
       jmp       near ptr M00_L03
M00_L07:
       vpaddq    zmm0,zmm6,zmm9
       vpsllq    zmm1,zmm0,17
       vpsrlq    zmm0,zmm0,29
       vpord     zmm0,zmm0,zmm1
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vpsllq    zmm1,zmm9,2D
       vpsrlq    zmm2,zmm9,13
       vpord     zmm9,zmm2,zmm1
       vpsrlq    zmm0,zmm0,0B
       vcvtuqq2pd zmm0,zmm0
       vbroadcastsd zmm1,qword ptr [7FF91F5ED5C8]
       vmulpd    zmm0,zmm0,zmm1
       vmovups   [rsp+20],zmm0
       lea       rdx,[rsp+20]
       vmovups   [rsp+120],zmm6
       vmovups   [rsp+0E0],zmm7
       vmovups   [rsp+60],zmm9
       vmovups   [rsp+0A0],zmm8
       call      qword ptr [7FF91F976610]
       vmovups   zmm6,[rsp+120]
       vmovups   zmm7,[rsp+0E0]
       vmovups   zmm9,[rsp+60]
       vmovups   zmm8,[rsp+0A0]
       jmp       near ptr M00_L04
; Total bytes of code 1123
```
```assembly
; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
       mov       eax,[rcx+8]
       test      eax,eax
       je        short M01_L00
       dec       eax
       vmovsd    xmm0,qword ptr [rcx+rax*8+10]
       ret
M01_L00:
       vxorps    xmm0,xmm0,xmm0
       ret
; Total bytes of code 21
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.BoundedIntFill()
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       rbx,rcx
       mov       rcx,[rbx+10]
       mov       rdx,[rbx+30]
       test      rdx,rdx
       je        short M00_L02
       lea       r8,[rdx+10]
       mov       edx,[rdx+8]
M00_L00:
       mov       [rsp+20],r8
       mov       [rsp+28],edx
       lea       rdx,[rsp+20]
       mov       r8d,0FFFFFFFB
       mov       r9d,5
       cmp       [rcx],ecx
       call      qword ptr [7FF91F9963E8]; Anastasya.Metaheuristics.Core.Randomness.RandomSource.Fill(System.Span`1<Int32>, Int32, Int32)
       mov       rax,[rbx+30]
       mov       ecx,[rax+8]
       test      ecx,ecx
       je        short M00_L03
       dec       ecx
       mov       eax,[rax+rcx*4+10]
M00_L01:
       vxorps    xmm0,xmm0,xmm0
       vcvtsi2sd xmm0,xmm0,eax
       add       rsp,30
       pop       rbx
       ret
M00_L02:
       xor       r8d,r8d
       xor       edx,edx
       jmp       short M00_L00
M00_L03:
       xor       eax,eax
       jmp       short M00_L01
; Total bytes of code 111
```
```assembly
; Anastasya.Metaheuristics.Core.Randomness.RandomSource.Fill(System.Span`1<Int32>, Int32, Int32)
       push      rbp
       push      r15
       push      r14
       push      r13
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,40
       lea       rbp,[rsp+20]
       xor       eax,eax
       mov       [rbp+10],rax
       mov       rax,0E515BA53EE03
       mov       [rbp+8],rax
       mov       r10,[rdx]
       mov       r11d,[rdx+8]
       cmp       r8d,r9d
       jge       near ptr M01_L13
       test      r11d,r11d
       je        near ptr M01_L05
       movsxd    r9,r9d
       movsxd    rax,r8d
       sub       r9,rax
       vmovups   zmm0,[rcx+28]
       vmovups   zmm1,[rcx+68]
       vmovups   zmm2,[rcx+0A8]
       vmovups   zmm3,[rcx+0E8]
       cmp       r9,1
       jbe       short M01_L00
       blsr      rax,r9
       je        short M01_L00
       mov       rax,r9
       neg       rax
       xor       edx,edx
       div       r9
       jmp       short M01_L01
M01_L00:
       xor       edx,edx
M01_L01:
       mov       [rbp+18],rdx
       test      [rsp],esp
       sub       rsp,40
       lea       rax,[rsp+20]
       vxorps    ymm4,ymm4,ymm4
       vmovdqu32 [rax],zmm4
       mov       rbx,rax
       xor       esi,esi
       cmp       esi,r11d
       jge       short M01_L04
M01_L02:
       vpaddq    zmm4,zmm0,zmm3
       vpsllq    zmm5,zmm4,17
       vpsrlq    zmm4,zmm4,29
       vpord     zmm4,zmm4,zmm5
       vpsllq    zmm5,zmm1,11
       vpxord    zmm2,zmm0,zmm2
       vpxord    zmm3,zmm1,zmm3
       vpxord    zmm1,zmm2,zmm1
       vpaddq    zmm4,zmm0,zmm4
       vpxord    zmm0,zmm3,zmm0
       vpxord    zmm2,zmm2,zmm5
       vpsllq    zmm5,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm5
       vmovups   [rbx],zmm4
       xor       edi,edi
       cmp       esi,r11d
       jl        near ptr M01_L10
M01_L03:
       cmp       esi,r11d
       jl        short M01_L02
M01_L04:
       vmovups   [rcx+28],zmm0
       vmovups   [rcx+68],zmm1
       vmovups   [rcx+0A8],zmm2
       vmovups   [rcx+0E8],zmm3
M01_L05:
       mov       r8,0E515BA53EE03
       cmp       [rbp+8],r8
       je        short M01_L06
       call      CORINFO_HELP_FAIL_FAST
M01_L06:
       nop
       vzeroupper
       lea       rsp,[rbp+20]
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r13
       pop       r14
       pop       r15
       pop       rbp
       ret
M01_L07:
       lea       r15,[rbp+10]
       mov       rdx,r14
       mulx      rdx,r13,r9
       mov       [r15],r13
       mov       r15,rdx
       mov       rdx,[rbp+10]
       mov       r14,[rbp+18]
       cmp       rdx,r14
       jae       short M01_L11
       mov       r14d,esi
       jmp       short M01_L12
M01_L08:
       xor       r15d,r15d
       jmp       short M01_L11
M01_L09:
       cmp       r14d,r11d
       mov       esi,r14d
       jge       near ptr M01_L03
M01_L10:
       movsxd    r14,edi
       mov       r14,[rax+r14*8]
       cmp       r9,1
       je        short M01_L08
       blsr      r15,r9
       jne       short M01_L07
       xor       r15d,r15d
       tzcnt     r15,r9
       neg       r15d
       add       r15d,40
       shrx      r15,r14,r15
M01_L11:
       lea       r14d,[rsi+1]
       mov       esi,esi
       add       r15d,r8d
       mov       [r10+rsi*4],r15d
M01_L12:
       inc       edi
       cmp       edi,8
       jl        short M01_L09
       mov       esi,r14d
       jmp       near ptr M01_L03
M01_L13:
       mov       rcx,offset MT_System.ArgumentException
       call      CORINFO_HELP_NEWSFAST
       mov       rbx,rax
       mov       ecx,0B5
       mov       rdx,7FF91F969E10
       call      qword ptr [7FF91F7B7798]
       mov       rsi,rax
       mov       ecx,5B
       mov       rdx,7FF91F969E10
       call      qword ptr [7FF91F7B7798]
       mov       r8,rax
       mov       rdx,rsi
       mov       rcx,rbx
       call      qword ptr [7FF91F905E48]
       mov       rcx,rbx
       call      CORINFO_HELP_THROW
       int       3
; Total bytes of code 576
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.NormalFill()
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       rbx,rcx
       mov       rcx,[rbx+10]
       mov       rdx,[rbx+28]
       test      rdx,rdx
       je        short M00_L01
       lea       rax,[rdx+10]
       mov       edx,[rdx+8]
M00_L00:
       mov       [rsp+20],rax
       mov       [rsp+28],edx
       lea       rdx,[rsp+20]
       call      qword ptr [7FF91F9764F0]; Anastasya.Metaheuristics.Core.Randomness.StandardNormal.Fill(Anastasya.Metaheuristics.Core.Randomness.RandomSource, System.Span`1<Double>)
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F976508]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
       nop
       add       rsp,30
       pop       rbx
       ret
M00_L01:
       xor       eax,eax
       xor       edx,edx
       jmp       short M00_L00
; Total bytes of code 78
```
```assembly
; Anastasya.Metaheuristics.Core.Randomness.StandardNormal.Fill(Anastasya.Metaheuristics.Core.Randomness.RandomSource, System.Span`1<Double>)
       push      rbp
       push      r15
       push      r14
       push      r13
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,2B0
       vmovaps   [rsp+2A0],xmm6
       vmovaps   [rsp+290],xmm7
       vmovaps   [rsp+280],xmm8
       vmovaps   [rsp+270],xmm9
       lea       rbp,[rsp+20]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu32 [rbp+10],zmm4
       mov       rax,0D82F5C20E73E
       mov       [rbp+8],rax
       mov       rbx,rcx
       mov       rsi,[rdx]
       mov       edi,[rdx+8]
       test      rbx,rbx
       je        near ptr M01_L10
       test      edi,edi
       je        near ptr M01_L08
       test      [rsp],esp
       sub       rsp,40
       lea       rdx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu32 [rdx],zmm0
       mov       r14,rdx
       test      [rsp],esp
       sub       rsp,40
       lea       rdx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu32 [rdx],zmm0
       mov       r15,rdx
       test      [rsp],esp
       sub       rsp,40
       lea       rdx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu32 [rdx],zmm0
       test      [rsp],esp
       sub       rsp,40
       lea       rcx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu32 [rcx],zmm0
       mov       r13,rcx
       xor       ecx,ecx
       jmp       short M01_L02
       nop       dword ptr [rax]
       nop       dword ptr [rax+rax]
M01_L00:
       xor       r8d,r8d
M01_L01:
       mov       [rax],r8
       inc       ecx
       cmp       ecx,8
       jge       short M01_L03
M01_L02:
       lea       rax,[rdx+rcx*8]
       test      cl,1
       jne       short M01_L00
       mov       r8,0FFFFFFFFFFFFFFFF
       jmp       short M01_L01
M01_L03:
       vmovups   zmm6,[rdx]
       vbroadcastsd zmm7,qword ptr [7FF91F5E8DB8]
       vbroadcastsd zmm8,qword ptr [7FF91F5E8DC0]
       test      edi,edi
       jle       near ptr M01_L08
M01_L04:
       vmovups   zmm0,[rbx+28]
       vmovups   zmm1,[rbx+68]
       vmovups   zmm2,[rbx+0A8]
       vmovups   zmm3,[rbx+0E8]
       vpaddq    zmm4,zmm0,zmm3
       vpsllq    zmm5,zmm4,17
       vpsrlq    zmm4,zmm4,29
       vpord     zmm4,zmm4,zmm5
       vpsllq    zmm5,zmm1,11
       vpxord    zmm2,zmm0,zmm2
       vpxord    zmm3,zmm1,zmm3
       vpxord    zmm1,zmm2,zmm1
       vpaddq    zmm4,zmm0,zmm4
       vpxord    zmm0,zmm3,zmm0
       vpxord    zmm2,zmm2,zmm5
       vpsllq    zmm5,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm5
       vmovups   [rbx+28],zmm0
       vmovups   [rbx+68],zmm1
       vmovups   [rbx+0A8],zmm2
       vmovups   [rbx+0E8],zmm3
       vpsrlq    zmm0,zmm4,0B
       vcvtuqq2pd zmm0,zmm0
       vmulpd    zmm0,zmm0,qword bcst [7FF91F5E8DC8]
       xor       edx,edx
M01_L05:
       lea       rcx,[rdx*8]
       lea       rax,[r14+rcx]
       mov       r8d,edx
       and       r8d,0FFFFFFFE
       cmp       r8d,8
       jae       near ptr M01_L11
       vmovups   [rbp+10],zmm0
       vmovsd    xmm1,qword ptr [rbp+r8*8+10]
       vmovsd    qword ptr [rax],xmm1
       add       rcx,r15
       mov       eax,edx
       or        eax,1
       cmp       eax,8
       jae       near ptr M01_L11
       vmovups   [rbp+10],zmm0
       vmovsd    xmm1,qword ptr [rbp+rax*8+10]
       vmovsd    qword ptr [rcx],xmm1
       inc       edx
       cmp       edx,8
       jl        short M01_L05
       vbroadcastsd zmm0,qword ptr [7FF91F5E8DD0]
       vsubpd    zmm0,zmm0,[r14]
       vmovups   [rbp+50],zmm0
       lea       rdx,[rbp+50]
       lea       rcx,[rbp+110]
       vmovups   [rbp+210],zmm6
       vmovups   [rbp+1D0],zmm7
       vmovups   [rbp+190],zmm8
       call      qword ptr [7FF91F976838]; System.Runtime.Intrinsics.VectorMath.LogDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.UInt64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   zmm9,[rbp+110]
       vmovups   zmm8,[rbp+190]
       vmulpd    zmm0,zmm8,[r15]
       vmovups   [rbp+50],zmm0
       lea       rdx,[rbp+50]
       lea       rcx,[rbp+90]
       vmovups   [rbp+150],zmm9
       vmovups   [rbp+190],zmm8
       call      qword ptr [7FF91F976868]; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   zmm0,[rbp+90]
       vmovups   zmm1,[rbp+0D0]
       vmovups   zmm6,[rbp+210]
       vpternlogq zmm1,zmm0,zmm6,0E4
       vmovups   zmm9,[rbp+150]
       vmovups   zmm7,[rbp+1D0]
       vmulpd    zmm0,zmm9,zmm7
       vsqrtpd   zmm0,zmm0
       vmulpd    zmm0,zmm1,zmm0
       cmp       edi,8
       vmovups   zmm8,[rbp+190]
       jl        short M01_L06
       vmovups   [rsi],zmm0
       add       rsi,40
       sub       edi,8
       test      edi,edi
       jg        near ptr M01_L04
       jmp       short M01_L08
M01_L06:
       vmovups   [r13],zmm0
       xor       ecx,ecx
M01_L07:
       movsxd    rdx,ecx
       vmovsd    xmm0,qword ptr [r13+rdx*8]
       vmovsd    qword ptr [rsi+rdx*8],xmm0
       inc       ecx
       cmp       ecx,edi
       jl        short M01_L07
M01_L08:
       mov       r8,0D82F5C20E73E
       cmp       [rbp+8],r8
       je        short M01_L09
       call      CORINFO_HELP_FAIL_FAST
M01_L09:
       nop
       vzeroupper
       vmovaps   xmm6,[rbp+280]
       vmovaps   xmm7,[rbp+270]
       vmovaps   xmm8,[rbp+260]
       vmovaps   xmm9,[rbp+250]
       lea       rsp,[rbp+290]
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r13
       pop       r14
       pop       r15
       pop       rbp
       ret
M01_L10:
       mov       ecx,12F
       mov       rdx,7FF91F949E10
       call      qword ptr [7FF91F797798]
       mov       rcx,rax
       call      qword ptr [7FF91F976928]
       int       3
M01_L11:
       call      CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION
       int       3
; Total bytes of code 977
```
```assembly
; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
       mov       eax,[rcx+8]
       test      eax,eax
       je        short M02_L00
       dec       eax
       vmovsd    xmm0,qword ptr [rcx+rax*8+10]
       ret
M02_L00:
       vxorps    xmm0,xmm0,xmm0
       ret
; Total bytes of code 21
```
```assembly
; System.Runtime.Intrinsics.VectorMath.LogDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.UInt64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   zmm0,[rdx]
       vmovaps   zmm1,zmm0
       vpaddq    zmm2,zmm0,qword bcst [7FF91F5EEE38]
       vpcmpnltuq k1,zmm2,qword bcst [7FF91F5EEE40]
       vpmovm2q  zmm2,k1
       vptestmq  k1,zmm2,zmm2
       kortestb  k1,k1
       jne       near ptr M03_L01
M03_L00:
       vmovups   zmm0,[rdx]
       vpaddq    zmm0,zmm0,qword bcst [7FF91F5EEE48]
       vpsraq    zmm3,zmm0,34
       vcvtqq2pd zmm3,zmm3
       vpandq    zmm0,zmm0,qword bcst [7FF91F5EEE50]
       vpaddq    zmm0,zmm0,qword bcst [7FF91F5EEE58]
       vsubpd    zmm0,zmm0,qword bcst [7FF91F5EEE60]
       vmulpd    zmm4,zmm0,zmm0
       vmulpd    zmm5,zmm4,zmm4
       vmulpd    zmm16,zmm5,zmm5
       vmulpd    zmm17,zmm16,zmm16
       vbroadcastsd zmm18,qword ptr [7FF91F5EEE68]
       vfmadd213pd zmm18,zmm0,qword bcst [7FF91F5EEE70]
       vbroadcastsd zmm19,qword ptr [7FF91F5EEE78]
       vfmadd213pd zmm19,zmm0,qword bcst [7FF91F5EEE80]
       vfmadd213pd zmm18,zmm4,zmm19
       vfmadd231pd zmm18,zmm5,qword bcst [7FF91F5EEE88]
       vbroadcastsd zmm19,qword ptr [7FF91F5EEE90]
       vfmadd213pd zmm19,zmm0,qword bcst [7FF91F5EEE98]
       vbroadcastsd zmm20,qword ptr [7FF91F5EEEA0]
       vfmadd213pd zmm20,zmm0,qword bcst [7FF91F5EEEA8]
       vfmadd213pd zmm19,zmm4,zmm20
       vbroadcastsd zmm20,qword ptr [7FF91F5EEEB0]
       vfmadd213pd zmm20,zmm0,qword bcst [7FF91F5EEEB8]
       vbroadcastsd zmm21,qword ptr [7FF91F5EEEC0]
       vfmadd213pd zmm21,zmm0,qword bcst [7FF91F5EEEC8]
       vfmadd213pd zmm20,zmm4,zmm21
       vfmadd231pd zmm20,zmm19,zmm5
       vbroadcastsd zmm19,qword ptr [7FF91F5EEED0]
       vfmadd213pd zmm19,zmm0,qword bcst [7FF91F5EEED8]
       vbroadcastsd zmm21,qword ptr [7FF91F5EEEE0]
       vfmadd213pd zmm21,zmm0,qword bcst [7FF91F5EEEE8]
       vfmadd213pd zmm19,zmm4,zmm21
       vbroadcastsd zmm21,qword ptr [7FF91F5EEEF0]
       vfmadd213pd zmm21,zmm0,qword bcst [7FF91F5EEEF8]
       vfmadd213pd zmm4,zmm21,zmm0
       vfmadd213pd zmm5,zmm19,zmm4
       vfmadd213pd zmm16,zmm20,zmm5
       vfmadd213pd zmm17,zmm18,zmm16
       vmovaps   zmm0,zmm3
       vfmadd132pd zmm0,zmm17,qword bcst [7FF91F5EEF00]
       vfmadd132pd zmm3,zmm0,qword bcst [7FF91F5EEF08]
       vpternlogq zmm2,zmm3,zmm1,0AC
       vmovups   [rcx],zmm2
       mov       rax,rcx
       vzeroupper
       ret
M03_L01:
       vxorps    ymm3,ymm3,ymm3
       vpcmpltq  k1,zmm0,zmm3
       vpmovm2q  zmm3,k1
       vpternlogq zmm1,zmm3,qword bcst [7FF91F5EEF10],0B8
       vxorps    ymm4,ymm4,ymm4
       vcmpeqpd  k1,zmm4,zmm0
       vpmovm2q  zmm4,k1
       vpternlogq zmm1,zmm4,qword bcst [7FF91F5EEE38],0B8
       vcmpneqpd k1,zmm0,zmm0
       vpmovm2q  zmm5,k1
       vpternlogq zmm4,zmm5,zmm3,0FE
       vpcmpeqq  k1,zmm0,qword bcst [7FF91F5EEF18]
       vpmovm2q  zmm3,k1
       vorpd     zmm3,zmm3,zmm4
       vandnpd   zmm2,zmm3,zmm2
       vmulpd    zmm4,zmm0,qword bcst [7FF91F5EEF20]
       vpaddq    zmm4,zmm4,qword bcst [7FF91F5EEF28]
       vpternlogq zmm2,zmm4,zmm0,0CA
       vmovups   [rdx],zmm2
       vmovaps   zmm2,zmm3
       jmp       near ptr M03_L00
; Total bytes of code 576
```
```assembly
; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       sub       rsp,58
       vmovaps   [rsp+40],xmm6
       vmovaps   [rsp+30],xmm7
       vmovaps   [rsp+20],xmm8
       vmovaps   [rsp+10],xmm9
       vmovaps   [rsp],xmm10
       vmovups   zmm6,[rdx]
       vandpd    zmm7,zmm6,qword bcst [7FF91F5EF3A0]
       vpcmpltq  k1,zmm7,qword bcst [7FF91F5EF3A8]
       kortestb  k1,k1
       jb        near ptr M04_L01
       vpcmpltq  k1,zmm7,qword bcst [7FF91F5EF3B0]
       kortestb  k1,k1
       jae       near ptr M04_L03
       vbroadcastsd zmm0,qword ptr [7FF91F5EF3B8]
       vmovaps   zmm1,zmm0
       vfmadd231pd zmm1,zmm7,qword bcst [7FF91F5EF3C0]
       vsubpd    zmm0,zmm1,zmm0
       vmovaps   zmm2,zmm7
       vfmadd231pd zmm2,zmm0,qword bcst [7FF91F5EF3C8]
       vmulpd    zmm3,zmm0,qword bcst [7FF91F5EF3D0]
       vsubpd    zmm4,zmm2,zmm3
       vsubpd    zmm2,zmm2,zmm4
       vsubpd    zmm3,zmm2,zmm3
       vbroadcastsd zmm2,qword ptr [7FF91F5EF3D8]
       vxorpd    zmm3,zmm3,zmm2
       vfmadd132pd zmm0,zmm3,qword bcst [7FF91F5EF3E0]
       vmovaps   zmm3,zmm0
       vsubpd    zmm0,zmm4,zmm3
       vsubpd    zmm4,zmm4,zmm0
       vsubpd    zmm3,zmm4,zmm3
       vmulpd    zmm4,zmm0,zmm0
       vmovaps   zmm5,zmm4
       vmulpd    zmm16,zmm0,zmm5
       vmulpd    zmm17,zmm5,zmm5
       vmovaps   zmm18,zmm17
       vbroadcastsd zmm19,qword ptr [7FF91F5EF3E8]
       vmulpd    zmm20,zmm18,zmm18
       vbroadcastsd zmm21,qword ptr [7FF91F5EF3F0]
       vfmadd213pd zmm21,zmm5,qword bcst [7FF91F5EF3F8]
       vbroadcastsd zmm22,qword ptr [7FF91F5EF400]
       vfmadd213pd zmm22,zmm5,qword bcst [7FF91F5EF408]
       vfmadd213pd zmm18,zmm21,zmm22
       vfmadd231pd zmm18,zmm20,qword bcst [7FF91F5EF410]
       vmulpd    zmm18,zmm18,zmm16
       vxorpd    zmm18,zmm18,zmm2
       vfmadd231pd zmm18,zmm19,zmm3
       vxorpd    zmm21,zmm2,zmm3
       vfmadd213pd zmm5,zmm18,zmm21
       vfmadd231pd zmm5,zmm16,qword bcst [7FF91F5EF418]
       vsubpd    zmm5,zmm0,zmm5
       vmovaps   zmm16,zmm4
       vmulpd    zmm16,zmm19,zmm16
       vbroadcastsd zmm8,qword ptr [7FF91F5EF420]
       vsubpd    zmm18,zmm16,zmm8
       vbroadcastsd zmm19,qword ptr [7FF91F5EF428]
       vfmadd213pd zmm19,zmm4,qword bcst [7FF91F5EF430]
       vbroadcastsd zmm21,qword ptr [7FF91F5EF438]
       vfmadd213pd zmm21,zmm4,qword bcst [7FF91F5EF440]
       vbroadcastsd zmm22,qword ptr [7FF91F5EF448]
       vfmadd213pd zmm4,zmm22,qword bcst [7FF91F5EF450]
       vfmadd231pd zmm4,zmm17,zmm21
       vfmadd213pd zmm19,zmm20,zmm4
       vaddpd    zmm4,zmm8,zmm18
       vsubpd    zmm4,zmm4,zmm16
       vfmadd213pd zmm0,zmm3,zmm4
       vfmadd213pd zmm19,zmm17,zmm0
       vsubpd    zmm0,zmm19,zmm18
       vbroadcastsd zmm3,qword ptr [7FF91F5EF458]
       vpandd    zmm4,zmm3,zmm1
       vptestnmq k1,zmm4,zmm4
       vpblendmq zmm9{k1},zmm0,zmm5
       vpblendmq zmm10{k1},zmm5,zmm0
       vpsrlq    zmm0,zmm6,3F
       vpsrlq    zmm4,zmm1,1
       vpternlogq zmm5,zmm4,zmm0,11
       vpternlogq zmm5,zmm4,zmm0,0F8
       vpandd    zmm0,zmm5,zmm3
       vptestnmq k1,zmm0,zmm0
       vxorpd    zmm9{k1},zmm2,zmm9
       vpaddq    zmm0,zmm3,zmm1
       vpandq    zmm0,zmm0,qword bcst [7FF91F5EF460]
       vptestnmq k1,zmm0,zmm0
       vxorpd    zmm0,zmm2,zmm10
       vpblendmq zmm10{k1},zmm0,zmm10
M04_L00:
       vpcmpgtq  k1,zmm7,qword bcst [7FF91F5EF468]
       vpblendmq zmm9{k1},zmm6,zmm9
       vpblendmq zmm10{k1},zmm8,zmm10
       vmovups   [rcx],zmm9
       vmovups   [rcx+40],zmm10
       mov       rax,rcx
       vzeroupper
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       ret
M04_L01:
       vmulpd    zmm0,zmm6,zmm6
       vmovaps   zmm10,zmm0
       vpcmpgtq  k1,zmm7,qword bcst [7FF91F5EF470]
       kortestb  k1,k1
       je        near ptr M04_L02
       vmovaps   zmm1,zmm0
       vmulpd    zmm9,zmm6,zmm1
       vmulpd    zmm2,zmm1,zmm1
       vmovaps   zmm3,zmm2
       vbroadcastsd zmm4,qword ptr [7FF91F5EF410]
       vfmadd213pd zmm4,zmm1,qword bcst [7FF91F5EF3F0]
       vmulpd    zmm5,zmm3,zmm3
       vbroadcastsd zmm16,qword ptr [7FF91F5EF3F8]
       vfmadd213pd zmm16,zmm1,qword bcst [7FF91F5EF400]
       vbroadcastsd zmm17,qword ptr [7FF91F5EF408]
       vfmadd213pd zmm1,zmm17,qword bcst [7FF91F5EF478]
       vfmadd213pd zmm3,zmm16,zmm1
       vfmadd213pd zmm4,zmm5,zmm3
       vfmadd213pd zmm9,zmm4,zmm6
       vbroadcastsd zmm1,qword ptr [7FF91F5EF428]
       vfmadd213pd zmm1,zmm0,qword bcst [7FF91F5EF430]
       vbroadcastsd zmm3,qword ptr [7FF91F5EF438]
       vfmadd213pd zmm3,zmm0,qword bcst [7FF91F5EF440]
       vbroadcastsd zmm4,qword ptr [7FF91F5EF448]
       vfmadd213pd zmm0,zmm4,qword bcst [7FF91F5EF450]
       vfmadd213pd zmm2,zmm3,zmm0
       vfmadd213pd zmm1,zmm5,zmm2
       vfmadd213pd zmm1,zmm10,qword bcst [7FF91F5EF480]
       vbroadcastsd zmm8,qword ptr [7FF91F5EF420]
       vfmadd213pd zmm10,zmm1,zmm8
       jmp       near ptr M04_L00
M04_L02:
       vmulpd    zmm0,zmm6,zmm10
       vmovaps   zmm9,zmm6
       vfmadd231pd zmm9,zmm0,qword bcst [7FF91F5EF478]
       vbroadcastsd zmm8,qword ptr [7FF91F5EF420]
       vfmadd132pd zmm10,zmm8,qword bcst [7FF91F5EF480]
       jmp       near ptr M04_L00
M04_L03:
       vzeroupper
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       jmp       qword ptr [7FF91F976AA8]
; Total bytes of code 1016
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.VectorApi()
       mov       rax,[rcx+10]
       vmovups   zmm0,[rax+28]
       vmovups   zmm1,[rax+68]
       vmovups   zmm2,[rax+0A8]
       vmovups   zmm3,[rax+0E8]
       vpaddq    zmm4,zmm0,zmm3
       vpsllq    zmm5,zmm4,17
       vpsrlq    zmm4,zmm4,29
       vpord     zmm4,zmm4,zmm5
       vpsllq    zmm5,zmm1,11
       vpxord    zmm2,zmm0,zmm2
       vpxord    zmm3,zmm1,zmm3
       vpxord    zmm1,zmm2,zmm1
       vpaddq    zmm4,zmm0,zmm4
       vpxord    zmm0,zmm3,zmm0
       vpxord    zmm2,zmm2,zmm5
       vpsllq    zmm5,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm5
       vmovups   [rax+28],zmm0
       vmovups   [rax+68],zmm1
       vmovups   [rax+0A8],zmm2
       vmovups   [rax+0E8],zmm3
       vmovq     rax,xmm4
       vzeroupper
       ret
; Total bytes of code 182
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.RawFill()
       push      rsi
       push      rbx
       sub       rsp,1C8
       vmovaps   [rsp+1B0],xmm6
       vmovaps   [rsp+1A0],xmm7
       vmovaps   [rsp+190],xmm8
       vmovaps   [rsp+180],xmm9
       mov       rbx,rcx
       mov       rsi,[rbx+10]
       mov       rdx,[rbx+20]
       test      rdx,rdx
       jne       near ptr M00_L06
       xor       ecx,ecx
       xor       r8d,r8d
M00_L00:
       cmp       [rsi],sil
       test      r8d,r8d
       je        near ptr M00_L05
       mov       edx,r8d
       shr       edx,3
       lea       eax,[rdx*8]
       sub       r8d,eax
       vmovups   zmm6,[rsi+28]
       vmovups   zmm7,[rsi+68]
       vmovups   zmm8,[rsi+0A8]
       vmovups   zmm9,[rsi+0E8]
       xor       eax,eax
       jmp       near ptr M00_L02
M00_L01:
       vpaddq    zmm0,zmm6,zmm9
       vpsllq    zmm1,zmm0,17
       vpsrlq    zmm0,zmm0,29
       vpord     zmm0,zmm0,zmm1
       vpsllq    zmm1,zmm7,11
       vpxord    zmm2,zmm6,zmm8
       vpxord    zmm3,zmm7,zmm9
       vpxord    zmm4,zmm7,zmm2
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm5,zmm3,zmm6
       vpxord    zmm2,zmm2,zmm1
       vpsllq    zmm1,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm1
       vpaddq    zmm1,zmm3,zmm5
       vpsllq    zmm16,zmm1,17
       vpsrlq    zmm1,zmm1,29
       vpord     zmm1,zmm1,zmm16
       vpsllq    zmm16,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm1,zmm5,zmm1
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm16
       vpsllq    zmm16,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm16
       vpaddq    zmm16,zmm3,zmm5
       vpsllq    zmm17,zmm16,17
       vpsrlq    zmm16,zmm16,29
       vpord     zmm16,zmm16,zmm17
       vpsllq    zmm17,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm16,zmm5,zmm16
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm17
       vpsllq    zmm17,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm17
       vpaddq    zmm17,zmm3,zmm5
       vpsllq    zmm18,zmm17,17
       vpsrlq    zmm17,zmm17,29
       vpord     zmm17,zmm17,zmm18
       vpsllq    zmm18,zmm4,11
       vpxord    zmm8,zmm2,zmm5
       vpxord    zmm9,zmm3,zmm4
       vpxord    zmm7,zmm4,zmm8
       vpaddq    zmm2,zmm5,zmm17
       vpxord    zmm6,zmm5,zmm9
       vpxord    zmm8,zmm8,zmm18
       vpsllq    zmm3,zmm9,2D
       vpsrlq    zmm4,zmm9,13
       vpord     zmm9,zmm4,zmm3
       vmovups   [rcx],zmm0
       vmovups   [rcx+40],zmm1
       vmovups   [rcx+80],zmm16
       vmovups   [rcx+0C0],zmm2
       add       rcx,100
       mov       eax,r10d
M00_L02:
       lea       r10d,[rax+4]
       cmp       r10d,edx
       jle       near ptr M00_L01
M00_L03:
       cmp       eax,edx
       jl        short M00_L07
       test      r8d,r8d
       jne       near ptr M00_L08
M00_L04:
       vmovups   [rsi+28],zmm6
       vmovups   [rsi+68],zmm7
       vmovups   [rsi+0A8],zmm8
       vmovups   [rsi+0E8],zmm9
M00_L05:
       mov       rcx,[rbx+20]
       call      qword ptr [7FF91F9664F0]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(UInt64[])
       nop
       vzeroupper
       vmovaps   xmm6,[rsp+1B0]
       vmovaps   xmm7,[rsp+1A0]
       vmovaps   xmm8,[rsp+190]
       vmovaps   xmm9,[rsp+180]
       add       rsp,1C8
       pop       rbx
       pop       rsi
       ret
M00_L06:
       lea       rcx,[rdx+10]
       mov       r8d,[rdx+8]
       jmp       near ptr M00_L00
M00_L07:
       vpaddq    zmm0,zmm6,zmm9
       vpsllq    zmm1,zmm0,17
       vpsrlq    zmm0,zmm0,29
       vpord     zmm0,zmm0,zmm1
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vpsllq    zmm1,zmm9,2D
       vpsrlq    zmm2,zmm9,13
       vpord     zmm9,zmm2,zmm1
       vmovups   [rcx],zmm0
       add       rcx,40
       inc       eax
       jmp       near ptr M00_L03
M00_L08:
       vpaddq    zmm0,zmm6,zmm9
       vpsllq    zmm1,zmm0,17
       vpsrlq    zmm0,zmm0,29
       vpord     zmm0,zmm0,zmm1
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vmovups   [rsp+20],zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vpsllq    zmm0,zmm9,2D
       vpsrlq    zmm1,zmm9,13
       vpord     zmm9,zmm1,zmm0
       lea       rdx,[rsp+20]
       vmovups   [rsp+120],zmm6
       vmovups   [rsp+0E0],zmm7
       vmovups   [rsp+60],zmm9
       vmovups   [rsp+0A0],zmm8
       call      qword ptr [7FF91F966598]
       vmovups   zmm6,[rsp+120]
       vmovups   zmm7,[rsp+0E0]
       vmovups   zmm9,[rsp+60]
       vmovups   zmm8,[rsp+0A0]
       jmp       near ptr M00_L04
; Total bytes of code 979
```
```assembly
; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(UInt64[])
       mov       eax,[rcx+8]
       test      eax,eax
       je        short M01_L01
       dec       eax
       mov       rax,[rcx+rax*8+10]
M01_L00:
       vxorps    xmm0,xmm0,xmm0
       vcvtusi2sd xmm0,xmm0,rax
       ret
M01_L01:
       xor       eax,eax
       jmp       short M01_L00
; Total bytes of code 29
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.UnitFill()
       push      rsi
       push      rbx
       sub       rsp,1C8
       vmovaps   [rsp+1B0],xmm6
       vmovaps   [rsp+1A0],xmm7
       vmovaps   [rsp+190],xmm8
       vmovaps   [rsp+180],xmm9
       mov       rbx,rcx
       mov       rsi,[rbx+10]
       mov       rdx,[rbx+28]
       test      rdx,rdx
       je        near ptr M00_L02
       lea       rcx,[rdx+10]
       mov       r8d,[rdx+8]
M00_L00:
       cmp       [rsi],sil
       test      r8d,r8d
       je        near ptr M00_L05
       mov       edx,r8d
       shr       edx,3
       lea       eax,[rdx*8]
       sub       r8d,eax
       vmovups   zmm6,[rsi+28]
       vmovups   zmm7,[rsi+68]
       vmovups   zmm8,[rsi+0A8]
       vmovups   zmm9,[rsi+0E8]
       xor       eax,eax
M00_L01:
       lea       r10d,[rax+4]
       cmp       r10d,edx
       jg        near ptr M00_L03
       vpaddq    zmm0,zmm6,zmm9
       vpsllq    zmm1,zmm0,17
       vpsrlq    zmm0,zmm0,29
       vpord     zmm0,zmm0,zmm1
       vpsllq    zmm1,zmm7,11
       vpxord    zmm2,zmm6,zmm8
       vpxord    zmm3,zmm7,zmm9
       vpxord    zmm4,zmm7,zmm2
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm5,zmm3,zmm6
       vpxord    zmm2,zmm2,zmm1
       vpsllq    zmm1,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm1
       vpsrlq    zmm0,zmm0,0B
       vcvtuqq2pd zmm0,zmm0
       vbroadcastsd zmm1,qword ptr [7FF91F5FD5C8]
       vmulpd    zmm0,zmm0,zmm1
       vpaddq    zmm16,zmm3,zmm5
       vpsllq    zmm17,zmm16,17
       vpsrlq    zmm16,zmm16,29
       vpord     zmm16,zmm16,zmm17
       vpsllq    zmm17,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm16,zmm5,zmm16
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm17
       vpsllq    zmm17,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm17
       vpsrlq    zmm16,zmm16,0B
       vcvtuqq2pd zmm16,zmm16
       vmulpd    zmm16,zmm16,zmm1
       vpaddq    zmm17,zmm3,zmm5
       vpsllq    zmm18,zmm17,17
       vpsrlq    zmm17,zmm17,29
       vpord     zmm17,zmm17,zmm18
       vpsllq    zmm18,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm17,zmm5,zmm17
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm18
       vpsllq    zmm18,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm18
       vpsrlq    zmm17,zmm17,0B
       vcvtuqq2pd zmm17,zmm17
       vmulpd    zmm17,zmm17,zmm1
       vpaddq    zmm18,zmm3,zmm5
       vpsllq    zmm19,zmm18,17
       vpsrlq    zmm18,zmm18,29
       vpord     zmm18,zmm18,zmm19
       vpsllq    zmm19,zmm4,11
       vpxord    zmm8,zmm2,zmm5
       vpxord    zmm9,zmm3,zmm4
       vpxord    zmm7,zmm4,zmm8
       vpaddq    zmm2,zmm5,zmm18
       vpxord    zmm6,zmm5,zmm9
       vpxord    zmm8,zmm8,zmm19
       vpsllq    zmm3,zmm9,2D
       vpsrlq    zmm4,zmm9,13
       vpord     zmm9,zmm4,zmm3
       vpsrlq    zmm2,zmm2,0B
       vcvtuqq2pd zmm2,zmm2
       vmulpd    zmm1,zmm2,zmm1
       vmovups   [rcx],zmm0
       vmovups   [rcx+40],zmm16
       vmovups   [rcx+80],zmm17
       vmovups   [rcx+0C0],zmm1
       add       rcx,100
       mov       eax,r10d
       jmp       near ptr M00_L01
M00_L02:
       xor       ecx,ecx
       xor       r8d,r8d
       jmp       near ptr M00_L00
M00_L03:
       cmp       eax,edx
       jl        short M00_L06
       test      r8d,r8d
       jne       near ptr M00_L07
M00_L04:
       vmovups   [rsi+28],zmm6
       vmovups   [rsi+68],zmm7
       vmovups   [rsi+0A8],zmm8
       vmovups   [rsi+0E8],zmm9
M00_L05:
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F986508]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
       nop
       vzeroupper
       vmovaps   xmm6,[rsp+1B0]
       vmovaps   xmm7,[rsp+1A0]
       vmovaps   xmm8,[rsp+190]
       vmovaps   xmm9,[rsp+180]
       add       rsp,1C8
       pop       rbx
       pop       rsi
       ret
M00_L06:
       vpaddq    zmm0,zmm6,zmm9
       vpsllq    zmm1,zmm0,17
       vpsrlq    zmm0,zmm0,29
       vpord     zmm0,zmm0,zmm1
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vpsllq    zmm1,zmm9,2D
       vpsrlq    zmm2,zmm9,13
       vpord     zmm9,zmm2,zmm1
       vpsrlq    zmm0,zmm0,0B
       vcvtuqq2pd zmm0,zmm0
       vbroadcastsd zmm1,qword ptr [7FF91F5FD5C8]
       vmulpd    zmm1,zmm0,zmm1
       vmovups   [rcx],zmm1
       add       rcx,40
       inc       eax
       jmp       near ptr M00_L03
M00_L07:
       vpaddq    zmm0,zmm6,zmm9
       vpsllq    zmm1,zmm0,17
       vpsrlq    zmm0,zmm0,29
       vpord     zmm0,zmm0,zmm1
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vpsllq    zmm1,zmm9,2D
       vpsrlq    zmm2,zmm9,13
       vpord     zmm9,zmm2,zmm1
       vpsrlq    zmm0,zmm0,0B
       vcvtuqq2pd zmm0,zmm0
       vbroadcastsd zmm1,qword ptr [7FF91F5FD5C8]
       vmulpd    zmm0,zmm0,zmm1
       vmovups   [rsp+20],zmm0
       lea       rdx,[rsp+20]
       vmovups   [rsp+120],zmm6
       vmovups   [rsp+0E0],zmm7
       vmovups   [rsp+60],zmm9
       vmovups   [rsp+0A0],zmm8
       call      qword ptr [7FF91F986610]
       vmovups   zmm6,[rsp+120]
       vmovups   zmm7,[rsp+0E0]
       vmovups   zmm9,[rsp+60]
       vmovups   zmm8,[rsp+0A0]
       jmp       near ptr M00_L04
; Total bytes of code 1123
```
```assembly
; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
       mov       eax,[rcx+8]
       test      eax,eax
       je        short M01_L00
       dec       eax
       vmovsd    xmm0,qword ptr [rcx+rax*8+10]
       ret
M01_L00:
       vxorps    xmm0,xmm0,xmm0
       ret
; Total bytes of code 21
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.BoundedIntFill()
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       rbx,rcx
       mov       rcx,[rbx+10]
       mov       rdx,[rbx+30]
       test      rdx,rdx
       je        short M00_L02
       lea       r8,[rdx+10]
       mov       edx,[rdx+8]
M00_L00:
       mov       [rsp+20],r8
       mov       [rsp+28],edx
       lea       rdx,[rsp+20]
       mov       r8d,0FFFFFFFB
       mov       r9d,5
       cmp       [rcx],ecx
       call      qword ptr [7FF91F9663E8]; Anastasya.Metaheuristics.Core.Randomness.RandomSource.Fill(System.Span`1<Int32>, Int32, Int32)
       mov       rax,[rbx+30]
       mov       ecx,[rax+8]
       test      ecx,ecx
       je        short M00_L03
       dec       ecx
       mov       eax,[rax+rcx*4+10]
M00_L01:
       vxorps    xmm0,xmm0,xmm0
       vcvtsi2sd xmm0,xmm0,eax
       add       rsp,30
       pop       rbx
       ret
M00_L02:
       xor       r8d,r8d
       xor       edx,edx
       jmp       short M00_L00
M00_L03:
       xor       eax,eax
       jmp       short M00_L01
; Total bytes of code 111
```
```assembly
; Anastasya.Metaheuristics.Core.Randomness.RandomSource.Fill(System.Span`1<Int32>, Int32, Int32)
       push      rbp
       push      r15
       push      r14
       push      r13
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,40
       lea       rbp,[rsp+20]
       xor       eax,eax
       mov       [rbp+10],rax
       mov       rax,59A2DC27C2EC
       mov       [rbp+8],rax
       mov       r10,[rdx]
       mov       r11d,[rdx+8]
       cmp       r8d,r9d
       jge       near ptr M01_L13
       test      r11d,r11d
       je        near ptr M01_L05
       movsxd    r9,r9d
       movsxd    rax,r8d
       sub       r9,rax
       vmovups   zmm0,[rcx+28]
       vmovups   zmm1,[rcx+68]
       vmovups   zmm2,[rcx+0A8]
       vmovups   zmm3,[rcx+0E8]
       cmp       r9,1
       jbe       short M01_L00
       blsr      rax,r9
       je        short M01_L00
       mov       rax,r9
       neg       rax
       xor       edx,edx
       div       r9
       jmp       short M01_L01
M01_L00:
       xor       edx,edx
M01_L01:
       mov       [rbp+18],rdx
       test      [rsp],esp
       sub       rsp,40
       lea       rax,[rsp+20]
       vxorps    ymm4,ymm4,ymm4
       vmovdqu32 [rax],zmm4
       mov       rbx,rax
       xor       esi,esi
       cmp       esi,r11d
       jge       short M01_L04
M01_L02:
       vpaddq    zmm4,zmm0,zmm3
       vpsllq    zmm5,zmm4,17
       vpsrlq    zmm4,zmm4,29
       vpord     zmm4,zmm4,zmm5
       vpsllq    zmm5,zmm1,11
       vpxord    zmm2,zmm0,zmm2
       vpxord    zmm3,zmm1,zmm3
       vpxord    zmm1,zmm2,zmm1
       vpaddq    zmm4,zmm0,zmm4
       vpxord    zmm0,zmm3,zmm0
       vpxord    zmm2,zmm2,zmm5
       vpsllq    zmm5,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm5
       vmovups   [rbx],zmm4
       xor       edi,edi
       cmp       esi,r11d
       jl        near ptr M01_L10
M01_L03:
       cmp       esi,r11d
       jl        short M01_L02
M01_L04:
       vmovups   [rcx+28],zmm0
       vmovups   [rcx+68],zmm1
       vmovups   [rcx+0A8],zmm2
       vmovups   [rcx+0E8],zmm3
M01_L05:
       mov       r8,59A2DC27C2EC
       cmp       [rbp+8],r8
       je        short M01_L06
       call      CORINFO_HELP_FAIL_FAST
M01_L06:
       nop
       vzeroupper
       lea       rsp,[rbp+20]
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r13
       pop       r14
       pop       r15
       pop       rbp
       ret
M01_L07:
       lea       r15,[rbp+10]
       mov       rdx,r14
       mulx      rdx,r13,r9
       mov       [r15],r13
       mov       r15,rdx
       mov       rdx,[rbp+10]
       mov       r14,[rbp+18]
       cmp       rdx,r14
       jae       short M01_L11
       mov       r14d,esi
       jmp       short M01_L12
M01_L08:
       xor       r15d,r15d
       jmp       short M01_L11
M01_L09:
       cmp       r14d,r11d
       mov       esi,r14d
       jge       near ptr M01_L03
M01_L10:
       movsxd    r14,edi
       mov       r14,[rax+r14*8]
       cmp       r9,1
       je        short M01_L08
       blsr      r15,r9
       jne       short M01_L07
       xor       r15d,r15d
       tzcnt     r15,r9
       neg       r15d
       add       r15d,40
       shrx      r15,r14,r15
M01_L11:
       lea       r14d,[rsi+1]
       mov       esi,esi
       add       r15d,r8d
       mov       [r10+rsi*4],r15d
M01_L12:
       inc       edi
       cmp       edi,8
       jl        short M01_L09
       mov       esi,r14d
       jmp       near ptr M01_L03
M01_L13:
       mov       rcx,offset MT_System.ArgumentException
       call      CORINFO_HELP_NEWSFAST
       mov       rbx,rax
       mov       ecx,0B5
       mov       rdx,7FF91F939E10
       call      qword ptr [7FF91F787798]
       mov       rsi,rax
       mov       ecx,5B
       mov       rdx,7FF91F939E10
       call      qword ptr [7FF91F787798]
       mov       r8,rax
       mov       rdx,rsi
       mov       rcx,rbx
       call      qword ptr [7FF91F8D5E48]
       mov       rcx,rbx
       call      CORINFO_HELP_THROW
       int       3
; Total bytes of code 576
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.NormalFill()
       push      rbx
       sub       rsp,30
       xor       eax,eax
       mov       [rsp+20],rax
       mov       rbx,rcx
       mov       rcx,[rbx+10]
       mov       rdx,[rbx+28]
       test      rdx,rdx
       je        short M00_L01
       lea       rax,[rdx+10]
       mov       edx,[rdx+8]
M00_L00:
       mov       [rsp+20],rax
       mov       [rsp+28],edx
       lea       rdx,[rsp+20]
       call      qword ptr [7FF91F966400]; Anastasya.Metaheuristics.Core.Randomness.StandardNormal.Fill(Anastasya.Metaheuristics.Core.Randomness.RandomSource, System.Span`1<Double>)
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F966418]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
       nop
       add       rsp,30
       pop       rbx
       ret
M00_L01:
       xor       eax,eax
       xor       edx,edx
       jmp       short M00_L00
; Total bytes of code 78
```
```assembly
; Anastasya.Metaheuristics.Core.Randomness.StandardNormal.Fill(Anastasya.Metaheuristics.Core.Randomness.RandomSource, System.Span`1<Double>)
       push      rbp
       push      r15
       push      r14
       push      r13
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,2B0
       vmovaps   [rsp+2A0],xmm6
       vmovaps   [rsp+290],xmm7
       vmovaps   [rsp+280],xmm8
       vmovaps   [rsp+270],xmm9
       lea       rbp,[rsp+20]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu32 [rbp+10],zmm4
       mov       rax,13024531BF1F
       mov       [rbp+8],rax
       mov       rbx,rcx
       mov       rsi,[rdx]
       mov       edi,[rdx+8]
       test      rbx,rbx
       je        near ptr M01_L10
       test      edi,edi
       je        near ptr M01_L08
       test      [rsp],esp
       sub       rsp,40
       lea       rdx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu32 [rdx],zmm0
       mov       r14,rdx
       test      [rsp],esp
       sub       rsp,40
       lea       rdx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu32 [rdx],zmm0
       mov       r15,rdx
       test      [rsp],esp
       sub       rsp,40
       lea       rdx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu32 [rdx],zmm0
       test      [rsp],esp
       sub       rsp,40
       lea       rcx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu32 [rcx],zmm0
       mov       r13,rcx
       xor       ecx,ecx
       jmp       short M01_L02
       nop       dword ptr [rax]
       nop       dword ptr [rax+rax]
M01_L00:
       xor       r8d,r8d
M01_L01:
       mov       [rax],r8
       inc       ecx
       cmp       ecx,8
       jge       short M01_L03
M01_L02:
       lea       rax,[rdx+rcx*8]
       test      cl,1
       jne       short M01_L00
       mov       r8,0FFFFFFFFFFFFFFFF
       jmp       short M01_L01
M01_L03:
       vmovups   zmm6,[rdx]
       vbroadcastsd zmm7,qword ptr [7FF91F5D8DB8]
       vbroadcastsd zmm8,qword ptr [7FF91F5D8DC0]
       test      edi,edi
       jle       near ptr M01_L08
M01_L04:
       vmovups   zmm0,[rbx+28]
       vmovups   zmm1,[rbx+68]
       vmovups   zmm2,[rbx+0A8]
       vmovups   zmm3,[rbx+0E8]
       vpaddq    zmm4,zmm0,zmm3
       vpsllq    zmm5,zmm4,17
       vpsrlq    zmm4,zmm4,29
       vpord     zmm4,zmm4,zmm5
       vpsllq    zmm5,zmm1,11
       vpxord    zmm2,zmm0,zmm2
       vpxord    zmm3,zmm1,zmm3
       vpxord    zmm1,zmm2,zmm1
       vpaddq    zmm4,zmm0,zmm4
       vpxord    zmm0,zmm3,zmm0
       vpxord    zmm2,zmm2,zmm5
       vpsllq    zmm5,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm5
       vmovups   [rbx+28],zmm0
       vmovups   [rbx+68],zmm1
       vmovups   [rbx+0A8],zmm2
       vmovups   [rbx+0E8],zmm3
       vpsrlq    zmm0,zmm4,0B
       vcvtuqq2pd zmm0,zmm0
       vmulpd    zmm0,zmm0,qword bcst [7FF91F5D8DC8]
       xor       edx,edx
M01_L05:
       lea       rcx,[rdx*8]
       lea       rax,[r14+rcx]
       mov       r8d,edx
       and       r8d,0FFFFFFFE
       cmp       r8d,8
       jae       near ptr M01_L11
       vmovups   [rbp+10],zmm0
       vmovsd    xmm1,qword ptr [rbp+r8*8+10]
       vmovsd    qword ptr [rax],xmm1
       add       rcx,r15
       mov       eax,edx
       or        eax,1
       cmp       eax,8
       jae       near ptr M01_L11
       vmovups   [rbp+10],zmm0
       vmovsd    xmm1,qword ptr [rbp+rax*8+10]
       vmovsd    qword ptr [rcx],xmm1
       inc       edx
       cmp       edx,8
       jl        short M01_L05
       vbroadcastsd zmm0,qword ptr [7FF91F5D8DD0]
       vsubpd    zmm0,zmm0,[r14]
       vmovups   [rbp+50],zmm0
       lea       rdx,[rbp+50]
       lea       rcx,[rbp+110]
       vmovups   [rbp+210],zmm6
       vmovups   [rbp+1D0],zmm7
       vmovups   [rbp+190],zmm8
       call      qword ptr [7FF91F966748]; System.Runtime.Intrinsics.VectorMath.LogDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.UInt64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   zmm9,[rbp+110]
       vmovups   zmm8,[rbp+190]
       vmulpd    zmm0,zmm8,[r15]
       vmovups   [rbp+50],zmm0
       lea       rdx,[rbp+50]
       lea       rcx,[rbp+90]
       vmovups   [rbp+150],zmm9
       vmovups   [rbp+190],zmm8
       call      qword ptr [7FF91F966778]; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   zmm0,[rbp+90]
       vmovups   zmm1,[rbp+0D0]
       vmovups   zmm6,[rbp+210]
       vpternlogq zmm1,zmm0,zmm6,0E4
       vmovups   zmm9,[rbp+150]
       vmovups   zmm7,[rbp+1D0]
       vmulpd    zmm0,zmm9,zmm7
       vsqrtpd   zmm0,zmm0
       vmulpd    zmm0,zmm1,zmm0
       cmp       edi,8
       vmovups   zmm8,[rbp+190]
       jl        short M01_L06
       vmovups   [rsi],zmm0
       add       rsi,40
       sub       edi,8
       test      edi,edi
       jg        near ptr M01_L04
       jmp       short M01_L08
M01_L06:
       vmovups   [r13],zmm0
       xor       ecx,ecx
M01_L07:
       movsxd    rdx,ecx
       vmovsd    xmm0,qword ptr [r13+rdx*8]
       vmovsd    qword ptr [rsi+rdx*8],xmm0
       inc       ecx
       cmp       ecx,edi
       jl        short M01_L07
M01_L08:
       mov       r8,13024531BF1F
       cmp       [rbp+8],r8
       je        short M01_L09
       call      CORINFO_HELP_FAIL_FAST
M01_L09:
       nop
       vzeroupper
       vmovaps   xmm6,[rbp+280]
       vmovaps   xmm7,[rbp+270]
       vmovaps   xmm8,[rbp+260]
       vmovaps   xmm9,[rbp+250]
       lea       rsp,[rbp+290]
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r13
       pop       r14
       pop       r15
       pop       rbp
       ret
M01_L10:
       mov       ecx,12F
       mov       rdx,7FF91F939E10
       call      qword ptr [7FF91F787798]
       mov       rcx,rax
       call      qword ptr [7FF91F966838]
       int       3
M01_L11:
       call      CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION
       int       3
; Total bytes of code 977
```
```assembly
; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
       mov       eax,[rcx+8]
       test      eax,eax
       je        short M02_L00
       dec       eax
       vmovsd    xmm0,qword ptr [rcx+rax*8+10]
       ret
M02_L00:
       vxorps    xmm0,xmm0,xmm0
       ret
; Total bytes of code 21
```
```assembly
; System.Runtime.Intrinsics.VectorMath.LogDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.UInt64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   zmm0,[rdx]
       vmovaps   zmm1,zmm0
       vpaddq    zmm2,zmm0,qword bcst [7FF91F5DEE38]
       vpcmpnltuq k1,zmm2,qword bcst [7FF91F5DEE40]
       vpmovm2q  zmm2,k1
       vptestmq  k1,zmm2,zmm2
       kortestb  k1,k1
       jne       near ptr M03_L01
M03_L00:
       vmovups   zmm0,[rdx]
       vpaddq    zmm0,zmm0,qword bcst [7FF91F5DEE48]
       vpsraq    zmm3,zmm0,34
       vcvtqq2pd zmm3,zmm3
       vpandq    zmm0,zmm0,qword bcst [7FF91F5DEE50]
       vpaddq    zmm0,zmm0,qword bcst [7FF91F5DEE58]
       vsubpd    zmm0,zmm0,qword bcst [7FF91F5DEE60]
       vmulpd    zmm4,zmm0,zmm0
       vmulpd    zmm5,zmm4,zmm4
       vmulpd    zmm16,zmm5,zmm5
       vmulpd    zmm17,zmm16,zmm16
       vbroadcastsd zmm18,qword ptr [7FF91F5DEE68]
       vfmadd213pd zmm18,zmm0,qword bcst [7FF91F5DEE70]
       vbroadcastsd zmm19,qword ptr [7FF91F5DEE78]
       vfmadd213pd zmm19,zmm0,qword bcst [7FF91F5DEE80]
       vfmadd213pd zmm18,zmm4,zmm19
       vfmadd231pd zmm18,zmm5,qword bcst [7FF91F5DEE88]
       vbroadcastsd zmm19,qword ptr [7FF91F5DEE90]
       vfmadd213pd zmm19,zmm0,qword bcst [7FF91F5DEE98]
       vbroadcastsd zmm20,qword ptr [7FF91F5DEEA0]
       vfmadd213pd zmm20,zmm0,qword bcst [7FF91F5DEEA8]
       vfmadd213pd zmm19,zmm4,zmm20
       vbroadcastsd zmm20,qword ptr [7FF91F5DEEB0]
       vfmadd213pd zmm20,zmm0,qword bcst [7FF91F5DEEB8]
       vbroadcastsd zmm21,qword ptr [7FF91F5DEEC0]
       vfmadd213pd zmm21,zmm0,qword bcst [7FF91F5DEEC8]
       vfmadd213pd zmm20,zmm4,zmm21
       vfmadd231pd zmm20,zmm19,zmm5
       vbroadcastsd zmm19,qword ptr [7FF91F5DEED0]
       vfmadd213pd zmm19,zmm0,qword bcst [7FF91F5DEED8]
       vbroadcastsd zmm21,qword ptr [7FF91F5DEEE0]
       vfmadd213pd zmm21,zmm0,qword bcst [7FF91F5DEEE8]
       vfmadd213pd zmm19,zmm4,zmm21
       vbroadcastsd zmm21,qword ptr [7FF91F5DEEF0]
       vfmadd213pd zmm21,zmm0,qword bcst [7FF91F5DEEF8]
       vfmadd213pd zmm4,zmm21,zmm0
       vfmadd213pd zmm5,zmm19,zmm4
       vfmadd213pd zmm16,zmm20,zmm5
       vfmadd213pd zmm17,zmm18,zmm16
       vmovaps   zmm0,zmm3
       vfmadd132pd zmm0,zmm17,qword bcst [7FF91F5DEF00]
       vfmadd132pd zmm3,zmm0,qword bcst [7FF91F5DEF08]
       vpternlogq zmm2,zmm3,zmm1,0AC
       vmovups   [rcx],zmm2
       mov       rax,rcx
       vzeroupper
       ret
M03_L01:
       vxorps    ymm3,ymm3,ymm3
       vpcmpltq  k1,zmm0,zmm3
       vpmovm2q  zmm3,k1
       vpternlogq zmm1,zmm3,qword bcst [7FF91F5DEF10],0B8
       vxorps    ymm4,ymm4,ymm4
       vcmpeqpd  k1,zmm4,zmm0
       vpmovm2q  zmm4,k1
       vpternlogq zmm1,zmm4,qword bcst [7FF91F5DEE38],0B8
       vcmpneqpd k1,zmm0,zmm0
       vpmovm2q  zmm5,k1
       vpternlogq zmm4,zmm5,zmm3,0FE
       vpcmpeqq  k1,zmm0,qword bcst [7FF91F5DEF18]
       vpmovm2q  zmm3,k1
       vorpd     zmm3,zmm3,zmm4
       vandnpd   zmm2,zmm3,zmm2
       vmulpd    zmm4,zmm0,qword bcst [7FF91F5DEF20]
       vpaddq    zmm4,zmm4,qword bcst [7FF91F5DEF28]
       vpternlogq zmm2,zmm4,zmm0,0CA
       vmovups   [rdx],zmm2
       vmovaps   zmm2,zmm3
       jmp       near ptr M03_L00
; Total bytes of code 576
```
```assembly
; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       sub       rsp,58
       vmovaps   [rsp+40],xmm6
       vmovaps   [rsp+30],xmm7
       vmovaps   [rsp+20],xmm8
       vmovaps   [rsp+10],xmm9
       vmovaps   [rsp],xmm10
       vmovups   zmm6,[rdx]
       vandpd    zmm7,zmm6,qword bcst [7FF91F5DF3A0]
       vpcmpltq  k1,zmm7,qword bcst [7FF91F5DF3A8]
       kortestb  k1,k1
       jb        near ptr M04_L01
       vpcmpltq  k1,zmm7,qword bcst [7FF91F5DF3B0]
       kortestb  k1,k1
       jae       near ptr M04_L03
       vbroadcastsd zmm0,qword ptr [7FF91F5DF3B8]
       vmovaps   zmm1,zmm0
       vfmadd231pd zmm1,zmm7,qword bcst [7FF91F5DF3C0]
       vsubpd    zmm0,zmm1,zmm0
       vmovaps   zmm2,zmm7
       vfmadd231pd zmm2,zmm0,qword bcst [7FF91F5DF3C8]
       vmulpd    zmm3,zmm0,qword bcst [7FF91F5DF3D0]
       vsubpd    zmm4,zmm2,zmm3
       vsubpd    zmm2,zmm2,zmm4
       vsubpd    zmm3,zmm2,zmm3
       vbroadcastsd zmm2,qword ptr [7FF91F5DF3D8]
       vxorpd    zmm3,zmm3,zmm2
       vfmadd132pd zmm0,zmm3,qword bcst [7FF91F5DF3E0]
       vmovaps   zmm3,zmm0
       vsubpd    zmm0,zmm4,zmm3
       vsubpd    zmm4,zmm4,zmm0
       vsubpd    zmm3,zmm4,zmm3
       vmulpd    zmm4,zmm0,zmm0
       vmovaps   zmm5,zmm4
       vmulpd    zmm16,zmm0,zmm5
       vmulpd    zmm17,zmm5,zmm5
       vmovaps   zmm18,zmm17
       vbroadcastsd zmm19,qword ptr [7FF91F5DF3E8]
       vmulpd    zmm20,zmm18,zmm18
       vbroadcastsd zmm21,qword ptr [7FF91F5DF3F0]
       vfmadd213pd zmm21,zmm5,qword bcst [7FF91F5DF3F8]
       vbroadcastsd zmm22,qword ptr [7FF91F5DF400]
       vfmadd213pd zmm22,zmm5,qword bcst [7FF91F5DF408]
       vfmadd213pd zmm18,zmm21,zmm22
       vfmadd231pd zmm18,zmm20,qword bcst [7FF91F5DF410]
       vmulpd    zmm18,zmm18,zmm16
       vxorpd    zmm18,zmm18,zmm2
       vfmadd231pd zmm18,zmm19,zmm3
       vxorpd    zmm21,zmm2,zmm3
       vfmadd213pd zmm5,zmm18,zmm21
       vfmadd231pd zmm5,zmm16,qword bcst [7FF91F5DF418]
       vsubpd    zmm5,zmm0,zmm5
       vmovaps   zmm16,zmm4
       vmulpd    zmm16,zmm19,zmm16
       vbroadcastsd zmm8,qword ptr [7FF91F5DF420]
       vsubpd    zmm18,zmm16,zmm8
       vbroadcastsd zmm19,qword ptr [7FF91F5DF428]
       vfmadd213pd zmm19,zmm4,qword bcst [7FF91F5DF430]
       vbroadcastsd zmm21,qword ptr [7FF91F5DF438]
       vfmadd213pd zmm21,zmm4,qword bcst [7FF91F5DF440]
       vbroadcastsd zmm22,qword ptr [7FF91F5DF448]
       vfmadd213pd zmm4,zmm22,qword bcst [7FF91F5DF450]
       vfmadd231pd zmm4,zmm17,zmm21
       vfmadd213pd zmm19,zmm20,zmm4
       vaddpd    zmm4,zmm8,zmm18
       vsubpd    zmm4,zmm4,zmm16
       vfmadd213pd zmm0,zmm3,zmm4
       vfmadd213pd zmm19,zmm17,zmm0
       vsubpd    zmm0,zmm19,zmm18
       vbroadcastsd zmm3,qword ptr [7FF91F5DF458]
       vpandd    zmm4,zmm3,zmm1
       vptestnmq k1,zmm4,zmm4
       vpblendmq zmm9{k1},zmm0,zmm5
       vpblendmq zmm10{k1},zmm5,zmm0
       vpsrlq    zmm0,zmm6,3F
       vpsrlq    zmm4,zmm1,1
       vpternlogq zmm5,zmm4,zmm0,11
       vpternlogq zmm5,zmm4,zmm0,0F8
       vpandd    zmm0,zmm5,zmm3
       vptestnmq k1,zmm0,zmm0
       vxorpd    zmm9{k1},zmm2,zmm9
       vpaddq    zmm0,zmm3,zmm1
       vpandq    zmm0,zmm0,qword bcst [7FF91F5DF460]
       vptestnmq k1,zmm0,zmm0
       vxorpd    zmm0,zmm2,zmm10
       vpblendmq zmm10{k1},zmm0,zmm10
M04_L00:
       vpcmpgtq  k1,zmm7,qword bcst [7FF91F5DF468]
       vpblendmq zmm9{k1},zmm6,zmm9
       vpblendmq zmm10{k1},zmm8,zmm10
       vmovups   [rcx],zmm9
       vmovups   [rcx+40],zmm10
       mov       rax,rcx
       vzeroupper
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       ret
M04_L01:
       vmulpd    zmm0,zmm6,zmm6
       vmovaps   zmm10,zmm0
       vpcmpgtq  k1,zmm7,qword bcst [7FF91F5DF470]
       kortestb  k1,k1
       je        near ptr M04_L02
       vmovaps   zmm1,zmm0
       vmulpd    zmm9,zmm6,zmm1
       vmulpd    zmm2,zmm1,zmm1
       vmovaps   zmm3,zmm2
       vbroadcastsd zmm4,qword ptr [7FF91F5DF410]
       vfmadd213pd zmm4,zmm1,qword bcst [7FF91F5DF3F0]
       vmulpd    zmm5,zmm3,zmm3
       vbroadcastsd zmm16,qword ptr [7FF91F5DF3F8]
       vfmadd213pd zmm16,zmm1,qword bcst [7FF91F5DF400]
       vbroadcastsd zmm17,qword ptr [7FF91F5DF408]
       vfmadd213pd zmm1,zmm17,qword bcst [7FF91F5DF478]
       vfmadd213pd zmm3,zmm16,zmm1
       vfmadd213pd zmm4,zmm5,zmm3
       vfmadd213pd zmm9,zmm4,zmm6
       vbroadcastsd zmm1,qword ptr [7FF91F5DF428]
       vfmadd213pd zmm1,zmm0,qword bcst [7FF91F5DF430]
       vbroadcastsd zmm3,qword ptr [7FF91F5DF438]
       vfmadd213pd zmm3,zmm0,qword bcst [7FF91F5DF440]
       vbroadcastsd zmm4,qword ptr [7FF91F5DF448]
       vfmadd213pd zmm0,zmm4,qword bcst [7FF91F5DF450]
       vfmadd213pd zmm2,zmm3,zmm0
       vfmadd213pd zmm1,zmm5,zmm2
       vfmadd213pd zmm1,zmm10,qword bcst [7FF91F5DF480]
       vbroadcastsd zmm8,qword ptr [7FF91F5DF420]
       vfmadd213pd zmm10,zmm1,zmm8
       jmp       near ptr M04_L00
M04_L02:
       vmulpd    zmm0,zmm6,zmm10
       vmovaps   zmm9,zmm6
       vfmadd231pd zmm9,zmm0,qword bcst [7FF91F5DF478]
       vbroadcastsd zmm8,qword ptr [7FF91F5DF420]
       vfmadd132pd zmm10,zmm8,qword bcst [7FF91F5DF480]
       jmp       near ptr M04_L00
M04_L03:
       vzeroupper
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       jmp       qword ptr [7FF91F9669B8]
; Total bytes of code 1016
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.VectorApi()
       mov       rax,[rcx+10]
       vmovups   zmm0,[rax+28]
       vmovups   zmm1,[rax+68]
       vmovups   zmm2,[rax+0A8]
       vmovups   zmm3,[rax+0E8]
       vpaddq    zmm4,zmm0,zmm3
       vpsllq    zmm5,zmm4,17
       vpsrlq    zmm4,zmm4,29
       vpord     zmm4,zmm4,zmm5
       vpsllq    zmm5,zmm1,11
       vpxord    zmm2,zmm0,zmm2
       vpxord    zmm3,zmm1,zmm3
       vpxord    zmm1,zmm2,zmm1
       vpaddq    zmm4,zmm0,zmm4
       vpxord    zmm0,zmm3,zmm0
       vpxord    zmm2,zmm2,zmm5
       vpsllq    zmm5,zmm3,2D
       vpsrlq    zmm3,zmm3,13
       vpord     zmm3,zmm3,zmm5
       vmovups   [rax+28],zmm0
       vmovups   [rax+68],zmm1
       vmovups   [rax+0A8],zmm2
       vmovups   [rax+0E8],zmm3
       vmovq     rax,xmm4
       vzeroupper
       ret
; Total bytes of code 182
```

