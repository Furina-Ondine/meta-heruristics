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
       vprolq    zmm0,zmm0,17
       vpsllq    zmm1,zmm7,11
       vpxord    zmm2,zmm6,zmm8
       vpxord    zmm3,zmm7,zmm9
       vpxord    zmm4,zmm7,zmm2
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm5,zmm6,zmm3
       vpxord    zmm2,zmm2,zmm1
       vprolq    zmm3,zmm3,2D
       vpaddq    zmm1,zmm3,zmm5
       vprolq    zmm1,zmm1,17
       vpsllq    zmm16,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm1,zmm5,zmm1
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm16
       vprolq    zmm3,zmm3,2D
       vpaddq    zmm16,zmm3,zmm5
       vprolq    zmm16,zmm16,17
       vpsllq    zmm17,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm16,zmm5,zmm16
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm17
       vprolq    zmm3,zmm3,2D
       vpaddq    zmm17,zmm3,zmm5
       vprolq    zmm17,zmm17,17
       vpsllq    zmm18,zmm4,11
       vpxord    zmm8,zmm2,zmm5
       vpxord    zmm9,zmm3,zmm4
       vpxord    zmm7,zmm4,zmm8
       vpaddq    zmm2,zmm5,zmm17
       vpxord    zmm6,zmm5,zmm9
       vpxord    zmm8,zmm8,zmm18
       vprolq    zmm9,zmm9,2D
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
       call      qword ptr [7FF91F986418]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(UInt64[])
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
       vprolq    zmm0,zmm0,17
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vprolq    zmm9,zmm9,2D
       vmovups   [rcx],zmm0
       add       rcx,40
       inc       eax
       jmp       near ptr M00_L03
M00_L08:
       vpaddq    zmm0,zmm6,zmm9
       vprolq    zmm0,zmm0,17
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vmovups   [rsp+20],zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vprolq    zmm9,zmm9,2D
       lea       rdx,[rsp+20]
       vmovups   [rsp+120],zmm6
       vmovups   [rsp+0E0],zmm7
       vmovups   [rsp+60],zmm9
       vmovups   [rsp+0A0],zmm8
       call      qword ptr [7FF91F9864C0]
       vmovups   zmm6,[rsp+120]
       vmovups   zmm7,[rsp+0E0]
       vmovups   zmm9,[rsp+60]
       vmovups   zmm8,[rsp+0A0]
       jmp       near ptr M00_L04
; Total bytes of code 823
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
       vprolq    zmm0,zmm0,17
       vpsllq    zmm1,zmm7,11
       vpxord    zmm2,zmm6,zmm8
       vpxord    zmm3,zmm7,zmm9
       vpxord    zmm4,zmm7,zmm2
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm5,zmm6,zmm3
       vpxord    zmm2,zmm2,zmm1
       vprolq    zmm3,zmm3,2D
       vpsrlq    zmm0,zmm0,0B
       vcvtuqq2pd zmm0,zmm0
       vbroadcastsd zmm1,qword ptr [7FF91F5CD4C8]
       vmulpd    zmm0,zmm0,zmm1
       vpaddq    zmm16,zmm3,zmm5
       vprolq    zmm16,zmm16,17
       vpsllq    zmm17,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm16,zmm5,zmm16
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm17
       vprolq    zmm3,zmm3,2D
       vpsrlq    zmm16,zmm16,0B
       vcvtuqq2pd zmm16,zmm16
       vmulpd    zmm16,zmm16,zmm1
       vpaddq    zmm17,zmm3,zmm5
       vprolq    zmm17,zmm17,17
       vpsllq    zmm18,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm17,zmm5,zmm17
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm18
       vprolq    zmm3,zmm3,2D
       vpsrlq    zmm17,zmm17,0B
       vcvtuqq2pd zmm17,zmm17
       vmulpd    zmm17,zmm17,zmm1
       vpaddq    zmm18,zmm3,zmm5
       vprolq    zmm18,zmm18,17
       vpsllq    zmm19,zmm4,11
       vpxord    zmm8,zmm2,zmm5
       vpxord    zmm9,zmm3,zmm4
       vpxord    zmm7,zmm4,zmm8
       vpaddq    zmm2,zmm5,zmm18
       vpxord    zmm6,zmm5,zmm9
       vpxord    zmm8,zmm8,zmm19
       vprolq    zmm9,zmm9,2D
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
       call      qword ptr [7FF91F946508]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
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
       vprolq    zmm0,zmm0,17
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vprolq    zmm9,zmm9,2D
       vpsrlq    zmm0,zmm0,0B
       vcvtuqq2pd zmm0,zmm0
       vbroadcastsd zmm1,qword ptr [7FF91F5CD4C8]
       vmulpd    zmm1,zmm0,zmm1
       vmovups   [rcx],zmm1
       add       rcx,40
       inc       eax
       jmp       near ptr M00_L03
M00_L07:
       vpaddq    zmm0,zmm6,zmm9
       vprolq    zmm0,zmm0,17
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vprolq    zmm9,zmm9,2D
       vpsrlq    zmm0,zmm0,0B
       vcvtuqq2pd zmm0,zmm0
       vbroadcastsd zmm1,qword ptr [7FF91F5CD4C8]
       vmulpd    zmm0,zmm0,zmm1
       vmovups   [rsp+20],zmm0
       lea       rdx,[rsp+20]
       vmovups   [rsp+120],zmm6
       vmovups   [rsp+0E0],zmm7
       vmovups   [rsp+60],zmm9
       vmovups   [rsp+0A0],zmm8
       call      qword ptr [7FF91F946610]
       vmovups   zmm6,[rsp+120]
       vmovups   zmm7,[rsp+0E0]
       vmovups   zmm9,[rsp+60]
       vmovups   zmm8,[rsp+0A0]
       jmp       near ptr M00_L04
; Total bytes of code 967
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
       call      qword ptr [7FF91F9763E8]; Anastasya.Metaheuristics.Core.Randomness.RandomSource.Fill(System.Span`1<Int32>, Int32, Int32)
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
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,38
       lea       rbp,[rsp+20]
       xor       eax,eax
       mov       [rbp+8],rax
       mov       rax,0E3A2F0A3659F
       mov       [rbp],rax
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
       mov       [rbp+10],rdx
       test      [rsp],esp
       sub       rsp,40
       lea       rax,[rsp+20]
       vxorps    ymm4,ymm4,ymm4
       vmovdqu32 [rax],zmm4
       xor       ebx,ebx
       cmp       ebx,r11d
       jge       short M01_L04
M01_L02:
       vpaddq    zmm4,zmm0,zmm3
       vprolq    zmm4,zmm4,17
       vpsllq    zmm5,zmm1,11
       vpxord    zmm2,zmm0,zmm2
       vpxord    zmm3,zmm1,zmm3
       vpxord    zmm1,zmm2,zmm1
       vpaddq    zmm4,zmm0,zmm4
       vpxord    zmm0,zmm0,zmm3
       vpxord    zmm2,zmm2,zmm5
       vprolq    zmm3,zmm3,2D
       vmovups   [rax],zmm4
       xor       esi,esi
       cmp       ebx,r11d
       jl        near ptr M01_L10
M01_L03:
       cmp       ebx,r11d
       jl        short M01_L02
M01_L04:
       vmovups   [rcx+28],zmm0
       vmovups   [rcx+68],zmm1
       vmovups   [rcx+0A8],zmm2
       vmovups   [rcx+0E8],zmm3
M01_L05:
       mov       r8,0E3A2F0A3659F
       cmp       [rbp],r8
       je        short M01_L06
       call      CORINFO_HELP_FAIL_FAST
M01_L06:
       nop
       vzeroupper
       lea       rsp,[rbp+18]
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r14
       pop       r15
       pop       rbp
       ret
M01_L07:
       lea       r14,[rbp+8]
       mov       rdx,rdi
       mulx      rdx,r15,r9
       mov       [r14],r15
       mov       r14,rdx
       mov       rdx,[rbp+8]
       mov       rdi,[rbp+10]
       cmp       rdx,rdi
       jae       short M01_L11
       mov       edi,ebx
       jmp       short M01_L12
M01_L08:
       xor       r14d,r14d
       jmp       short M01_L11
M01_L09:
       cmp       edi,r11d
       mov       ebx,edi
       jge       near ptr M01_L03
M01_L10:
       movsxd    rdi,esi
       mov       rdi,[rax+rdi*8]
       cmp       r9,1
       je        short M01_L08
       blsr      r14,r9
       jne       short M01_L07
       xor       r14d,r14d
       tzcnt     r14,r9
       neg       r14d
       add       r14d,40
       shrx      r14,rdi,r14
M01_L11:
       lea       edi,[rbx+1]
       mov       ebx,ebx
       add       r14d,r8d
       mov       [r10+rbx*4],r14d
M01_L12:
       inc       esi
       cmp       esi,8
       jl        short M01_L09
       mov       ebx,edi
       jmp       near ptr M01_L03
M01_L13:
       mov       rcx,offset MT_System.ArgumentException
       call      CORINFO_HELP_NEWSFAST
       mov       rbx,rax
       mov       ecx,0B5
       mov       rdx,7FF91F949E10
       call      qword ptr [7FF91F7A7798]
       mov       rsi,rax
       mov       ecx,5B
       mov       rdx,7FF91F949E10
       call      qword ptr [7FF91F7A7798]
       mov       r8,rax
       mov       rdx,rsi
       mov       rcx,rbx
       call      qword ptr [7FF91F8E5E48]
       mov       rcx,rbx
       call      CORINFO_HELP_THROW
       int       3
; Total bytes of code 539
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
       call      qword ptr [7FF91F976400]; Anastasya.Metaheuristics.Core.Randomness.StandardNormal.Fill(Anastasya.Metaheuristics.Core.Randomness.RandomSource, System.Span`1<Double>)
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F976418]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
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
       mov       rax,0E061A78A8D1C
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
       vbroadcastsd zmm7,qword ptr [7FF91F5F8DA8]
       vbroadcastsd zmm8,qword ptr [7FF91F5F8DB0]
       test      edi,edi
       jle       near ptr M01_L08
M01_L04:
       vmovups   zmm0,[rbx+28]
       vmovups   zmm1,[rbx+68]
       vmovups   zmm2,[rbx+0A8]
       vmovups   zmm3,[rbx+0E8]
       vpaddq    zmm4,zmm0,zmm3
       vprolq    zmm4,zmm4,17
       vpsllq    zmm5,zmm1,11
       vpxord    zmm2,zmm0,zmm2
       vpxord    zmm3,zmm1,zmm3
       vpxord    zmm1,zmm2,zmm1
       vpaddq    zmm4,zmm0,zmm4
       vpxord    zmm0,zmm0,zmm3
       vpxord    zmm2,zmm2,zmm5
       vprolq    zmm3,zmm3,2D
       vmovups   [rbx+28],zmm0
       vmovups   [rbx+68],zmm1
       vmovups   [rbx+0A8],zmm2
       vmovups   [rbx+0E8],zmm3
       vpsrlq    zmm0,zmm4,0B
       vcvtuqq2pd zmm0,zmm0
       vmulpd    zmm0,zmm0,qword bcst [7FF91F5F8DB8]
       xor       edx,edx
       nop       dword ptr [rax]
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
       vbroadcastsd zmm0,qword ptr [7FF91F5F8DC0]
       vsubpd    zmm0,zmm0,[r14]
       vmovups   [rbp+50],zmm0
       lea       rdx,[rbp+50]
       lea       rcx,[rbp+110]
       vmovups   [rbp+210],zmm6
       vmovups   [rbp+1D0],zmm7
       vmovups   [rbp+190],zmm8
       call      qword ptr [7FF91F976820]; System.Runtime.Intrinsics.VectorMath.LogDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.UInt64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   zmm9,[rbp+110]
       vmovups   zmm8,[rbp+190]
       vmulpd    zmm0,zmm8,[r15]
       vmovups   [rbp+50],zmm0
       lea       rdx,[rbp+50]
       lea       rcx,[rbp+90]
       vmovups   [rbp+150],zmm9
       vmovups   [rbp+190],zmm8
       call      qword ptr [7FF91F976850]; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
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
       nop       word ptr [rax+rax]
M01_L07:
       movsxd    rdx,ecx
       vmovsd    xmm0,qword ptr [r13+rdx*8]
       vmovsd    qword ptr [rsi+rdx*8],xmm0
       inc       ecx
       cmp       ecx,edi
       jl        short M01_L07
M01_L08:
       mov       r8,0E061A78A8D1C
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
       call      qword ptr [7FF91F7A7798]
       mov       rcx,rax
       call      qword ptr [7FF91F976910]
       int       3
M01_L11:
       call      CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION
       int       3
; Total bytes of code 961
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
       vpaddq    zmm2,zmm0,qword bcst [7FF91F5FEE38]
       vpcmpnltuq k1,zmm2,qword bcst [7FF91F5FEE40]
       vpmovm2q  zmm2,k1
       vptestmq  k1,zmm2,zmm2
       kortestb  k1,k1
       jne       near ptr M03_L01
M03_L00:
       vmovups   zmm0,[rdx]
       vpaddq    zmm0,zmm0,qword bcst [7FF91F5FEE48]
       vpsraq    zmm3,zmm0,34
       vcvtqq2pd zmm3,zmm3
       vpandq    zmm0,zmm0,qword bcst [7FF91F5FEE50]
       vpaddq    zmm0,zmm0,qword bcst [7FF91F5FEE58]
       vsubpd    zmm0,zmm0,qword bcst [7FF91F5FEE60]
       vmulpd    zmm4,zmm0,zmm0
       vmulpd    zmm5,zmm4,zmm4
       vmulpd    zmm16,zmm5,zmm5
       vmulpd    zmm17,zmm16,zmm16
       vbroadcastsd zmm18,qword ptr [7FF91F5FEE68]
       vfmadd213pd zmm18,zmm0,qword bcst [7FF91F5FEE70]
       vbroadcastsd zmm19,qword ptr [7FF91F5FEE78]
       vfmadd213pd zmm19,zmm0,qword bcst [7FF91F5FEE80]
       vfmadd213pd zmm18,zmm4,zmm19
       vfmadd231pd zmm18,zmm5,qword bcst [7FF91F5FEE88]
       vbroadcastsd zmm19,qword ptr [7FF91F5FEE90]
       vfmadd213pd zmm19,zmm0,qword bcst [7FF91F5FEE98]
       vbroadcastsd zmm20,qword ptr [7FF91F5FEEA0]
       vfmadd213pd zmm20,zmm0,qword bcst [7FF91F5FEEA8]
       vfmadd213pd zmm19,zmm4,zmm20
       vbroadcastsd zmm20,qword ptr [7FF91F5FEEB0]
       vfmadd213pd zmm20,zmm0,qword bcst [7FF91F5FEEB8]
       vbroadcastsd zmm21,qword ptr [7FF91F5FEEC0]
       vfmadd213pd zmm21,zmm0,qword bcst [7FF91F5FEEC8]
       vfmadd213pd zmm20,zmm4,zmm21
       vfmadd231pd zmm20,zmm19,zmm5
       vbroadcastsd zmm19,qword ptr [7FF91F5FEED0]
       vfmadd213pd zmm19,zmm0,qword bcst [7FF91F5FEED8]
       vbroadcastsd zmm21,qword ptr [7FF91F5FEEE0]
       vfmadd213pd zmm21,zmm0,qword bcst [7FF91F5FEEE8]
       vfmadd213pd zmm19,zmm4,zmm21
       vbroadcastsd zmm21,qword ptr [7FF91F5FEEF0]
       vfmadd213pd zmm21,zmm0,qword bcst [7FF91F5FEEF8]
       vfmadd213pd zmm4,zmm21,zmm0
       vfmadd213pd zmm5,zmm19,zmm4
       vfmadd213pd zmm16,zmm20,zmm5
       vfmadd213pd zmm17,zmm18,zmm16
       vmovaps   zmm0,zmm3
       vfmadd132pd zmm0,zmm17,qword bcst [7FF91F5FEF00]
       vfmadd132pd zmm3,zmm0,qword bcst [7FF91F5FEF08]
       vpternlogq zmm2,zmm3,zmm1,0AC
       vmovups   [rcx],zmm2
       mov       rax,rcx
       vzeroupper
       ret
M03_L01:
       vxorps    ymm3,ymm3,ymm3
       vpcmpltq  k1,zmm0,zmm3
       vpmovm2q  zmm3,k1
       vpternlogq zmm1,zmm3,qword bcst [7FF91F5FEF10],0B8
       vxorps    ymm4,ymm4,ymm4
       vcmpeqpd  k1,zmm4,zmm0
       vpmovm2q  zmm4,k1
       vpternlogq zmm1,zmm4,qword bcst [7FF91F5FEE38],0B8
       vcmpneqpd k1,zmm0,zmm0
       vpmovm2q  zmm5,k1
       vpternlogq zmm4,zmm5,zmm3,0FE
       vpcmpeqq  k1,zmm0,qword bcst [7FF91F5FEF18]
       vpmovm2q  zmm3,k1
       vorpd     zmm3,zmm3,zmm4
       vandnpd   zmm2,zmm3,zmm2
       vmulpd    zmm4,zmm0,qword bcst [7FF91F5FEF20]
       vpaddq    zmm4,zmm4,qword bcst [7FF91F5FEF28]
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
       vandpd    zmm7,zmm6,qword bcst [7FF91F5FF3A0]
       vpcmpltq  k1,zmm7,qword bcst [7FF91F5FF3A8]
       kortestb  k1,k1
       jb        near ptr M04_L01
       vpcmpltq  k1,zmm7,qword bcst [7FF91F5FF3B0]
       kortestb  k1,k1
       jae       near ptr M04_L03
       vbroadcastsd zmm0,qword ptr [7FF91F5FF3B8]
       vmovaps   zmm1,zmm0
       vfmadd231pd zmm1,zmm7,qword bcst [7FF91F5FF3C0]
       vsubpd    zmm0,zmm1,zmm0
       vmovaps   zmm2,zmm7
       vfmadd231pd zmm2,zmm0,qword bcst [7FF91F5FF3C8]
       vmulpd    zmm3,zmm0,qword bcst [7FF91F5FF3D0]
       vsubpd    zmm4,zmm2,zmm3
       vsubpd    zmm2,zmm2,zmm4
       vsubpd    zmm3,zmm2,zmm3
       vbroadcastsd zmm2,qword ptr [7FF91F5FF3D8]
       vxorpd    zmm3,zmm3,zmm2
       vfmadd132pd zmm0,zmm3,qword bcst [7FF91F5FF3E0]
       vmovaps   zmm3,zmm0
       vsubpd    zmm0,zmm4,zmm3
       vsubpd    zmm4,zmm4,zmm0
       vsubpd    zmm3,zmm4,zmm3
       vmulpd    zmm4,zmm0,zmm0
       vmovaps   zmm5,zmm4
       vmulpd    zmm16,zmm0,zmm5
       vmulpd    zmm17,zmm5,zmm5
       vmovaps   zmm18,zmm17
       vbroadcastsd zmm19,qword ptr [7FF91F5FF3E8]
       vmulpd    zmm20,zmm18,zmm18
       vbroadcastsd zmm21,qword ptr [7FF91F5FF3F0]
       vfmadd213pd zmm21,zmm5,qword bcst [7FF91F5FF3F8]
       vbroadcastsd zmm22,qword ptr [7FF91F5FF400]
       vfmadd213pd zmm22,zmm5,qword bcst [7FF91F5FF408]
       vfmadd213pd zmm18,zmm21,zmm22
       vfmadd231pd zmm18,zmm20,qword bcst [7FF91F5FF410]
       vmulpd    zmm18,zmm18,zmm16
       vxorpd    zmm18,zmm18,zmm2
       vfmadd231pd zmm18,zmm19,zmm3
       vxorpd    zmm21,zmm2,zmm3
       vfmadd213pd zmm5,zmm18,zmm21
       vfmadd231pd zmm5,zmm16,qword bcst [7FF91F5FF418]
       vsubpd    zmm5,zmm0,zmm5
       vmovaps   zmm16,zmm4
       vmulpd    zmm16,zmm19,zmm16
       vbroadcastsd zmm8,qword ptr [7FF91F5FF420]
       vsubpd    zmm18,zmm16,zmm8
       vbroadcastsd zmm19,qword ptr [7FF91F5FF428]
       vfmadd213pd zmm19,zmm4,qword bcst [7FF91F5FF430]
       vbroadcastsd zmm21,qword ptr [7FF91F5FF438]
       vfmadd213pd zmm21,zmm4,qword bcst [7FF91F5FF440]
       vbroadcastsd zmm22,qword ptr [7FF91F5FF448]
       vfmadd213pd zmm4,zmm22,qword bcst [7FF91F5FF450]
       vfmadd231pd zmm4,zmm17,zmm21
       vfmadd213pd zmm19,zmm20,zmm4
       vaddpd    zmm4,zmm8,zmm18
       vsubpd    zmm4,zmm4,zmm16
       vfmadd213pd zmm0,zmm3,zmm4
       vfmadd213pd zmm19,zmm17,zmm0
       vsubpd    zmm0,zmm19,zmm18
       vbroadcastsd zmm3,qword ptr [7FF91F5FF458]
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
       vpandq    zmm0,zmm0,qword bcst [7FF91F5FF460]
       vptestnmq k1,zmm0,zmm0
       vxorpd    zmm0,zmm2,zmm10
       vpblendmq zmm10{k1},zmm0,zmm10
M04_L00:
       vpcmpgtq  k1,zmm7,qword bcst [7FF91F5FF468]
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
       vpcmpgtq  k1,zmm7,qword bcst [7FF91F5FF470]
       kortestb  k1,k1
       je        near ptr M04_L02
       vmovaps   zmm1,zmm0
       vmulpd    zmm9,zmm6,zmm1
       vmulpd    zmm2,zmm1,zmm1
       vmovaps   zmm3,zmm2
       vbroadcastsd zmm4,qword ptr [7FF91F5FF410]
       vfmadd213pd zmm4,zmm1,qword bcst [7FF91F5FF3F0]
       vmulpd    zmm5,zmm3,zmm3
       vbroadcastsd zmm16,qword ptr [7FF91F5FF3F8]
       vfmadd213pd zmm16,zmm1,qword bcst [7FF91F5FF400]
       vbroadcastsd zmm17,qword ptr [7FF91F5FF408]
       vfmadd213pd zmm1,zmm17,qword bcst [7FF91F5FF478]
       vfmadd213pd zmm3,zmm16,zmm1
       vfmadd213pd zmm4,zmm5,zmm3
       vfmadd213pd zmm9,zmm4,zmm6
       vbroadcastsd zmm1,qword ptr [7FF91F5FF428]
       vfmadd213pd zmm1,zmm0,qword bcst [7FF91F5FF430]
       vbroadcastsd zmm3,qword ptr [7FF91F5FF438]
       vfmadd213pd zmm3,zmm0,qword bcst [7FF91F5FF440]
       vbroadcastsd zmm4,qword ptr [7FF91F5FF448]
       vfmadd213pd zmm0,zmm4,qword bcst [7FF91F5FF450]
       vfmadd213pd zmm2,zmm3,zmm0
       vfmadd213pd zmm1,zmm5,zmm2
       vfmadd213pd zmm1,zmm10,qword bcst [7FF91F5FF480]
       vbroadcastsd zmm8,qword ptr [7FF91F5FF420]
       vfmadd213pd zmm10,zmm1,zmm8
       jmp       near ptr M04_L00
M04_L02:
       vmulpd    zmm0,zmm6,zmm10
       vmovaps   zmm9,zmm6
       vfmadd231pd zmm9,zmm0,qword bcst [7FF91F5FF478]
       vbroadcastsd zmm8,qword ptr [7FF91F5FF420]
       vfmadd132pd zmm10,zmm8,qword bcst [7FF91F5FF480]
       jmp       near ptr M04_L00
M04_L03:
       vzeroupper
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       jmp       qword ptr [7FF91F976A90]
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
       vprolq    zmm4,zmm4,17
       vpsllq    zmm5,zmm1,11
       vpxord    zmm2,zmm0,zmm2
       vpxord    zmm3,zmm1,zmm3
       vpxord    zmm1,zmm2,zmm1
       vpaddq    zmm4,zmm0,zmm4
       vpxord    zmm0,zmm0,zmm3
       vpxord    zmm2,zmm2,zmm5
       vprolq    zmm3,zmm3,2D
       vmovups   [rax+28],zmm0
       vmovups   [rax+68],zmm1
       vmovups   [rax+0A8],zmm2
       vmovups   [rax+0E8],zmm3
       vmovq     rax,xmm4
       vzeroupper
       ret
; Total bytes of code 156
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
       vprolq    zmm0,zmm0,17
       vpsllq    zmm1,zmm7,11
       vpxord    zmm2,zmm6,zmm8
       vpxord    zmm3,zmm7,zmm9
       vpxord    zmm4,zmm7,zmm2
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm5,zmm6,zmm3
       vpxord    zmm2,zmm2,zmm1
       vprolq    zmm3,zmm3,2D
       vpaddq    zmm1,zmm3,zmm5
       vprolq    zmm1,zmm1,17
       vpsllq    zmm16,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm1,zmm5,zmm1
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm16
       vprolq    zmm3,zmm3,2D
       vpaddq    zmm16,zmm3,zmm5
       vprolq    zmm16,zmm16,17
       vpsllq    zmm17,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm16,zmm5,zmm16
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm17
       vprolq    zmm3,zmm3,2D
       vpaddq    zmm17,zmm3,zmm5
       vprolq    zmm17,zmm17,17
       vpsllq    zmm18,zmm4,11
       vpxord    zmm8,zmm2,zmm5
       vpxord    zmm9,zmm3,zmm4
       vpxord    zmm7,zmm4,zmm8
       vpaddq    zmm2,zmm5,zmm17
       vpxord    zmm6,zmm5,zmm9
       vpxord    zmm8,zmm8,zmm18
       vprolq    zmm9,zmm9,2D
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
       call      qword ptr [7FF91F986418]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(UInt64[])
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
       vprolq    zmm0,zmm0,17
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vprolq    zmm9,zmm9,2D
       vmovups   [rcx],zmm0
       add       rcx,40
       inc       eax
       jmp       near ptr M00_L03
M00_L08:
       vpaddq    zmm0,zmm6,zmm9
       vprolq    zmm0,zmm0,17
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vmovups   [rsp+20],zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vprolq    zmm9,zmm9,2D
       lea       rdx,[rsp+20]
       vmovups   [rsp+120],zmm6
       vmovups   [rsp+0E0],zmm7
       vmovups   [rsp+60],zmm9
       vmovups   [rsp+0A0],zmm8
       call      qword ptr [7FF91F9864C0]
       vmovups   zmm6,[rsp+120]
       vmovups   zmm7,[rsp+0E0]
       vmovups   zmm9,[rsp+60]
       vmovups   zmm8,[rsp+0A0]
       jmp       near ptr M00_L04
; Total bytes of code 823
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
       vprolq    zmm0,zmm0,17
       vpsllq    zmm1,zmm7,11
       vpxord    zmm2,zmm6,zmm8
       vpxord    zmm3,zmm7,zmm9
       vpxord    zmm4,zmm7,zmm2
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm5,zmm6,zmm3
       vpxord    zmm2,zmm2,zmm1
       vprolq    zmm3,zmm3,2D
       vpsrlq    zmm0,zmm0,0B
       vcvtuqq2pd zmm0,zmm0
       vbroadcastsd zmm1,qword ptr [7FF91F5ED4C8]
       vmulpd    zmm0,zmm0,zmm1
       vpaddq    zmm16,zmm3,zmm5
       vprolq    zmm16,zmm16,17
       vpsllq    zmm17,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm16,zmm5,zmm16
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm17
       vprolq    zmm3,zmm3,2D
       vpsrlq    zmm16,zmm16,0B
       vcvtuqq2pd zmm16,zmm16
       vmulpd    zmm16,zmm16,zmm1
       vpaddq    zmm17,zmm3,zmm5
       vprolq    zmm17,zmm17,17
       vpsllq    zmm18,zmm4,11
       vpxord    zmm2,zmm2,zmm5
       vpxord    zmm3,zmm3,zmm4
       vpxord    zmm4,zmm2,zmm4
       vpaddq    zmm17,zmm5,zmm17
       vpxord    zmm5,zmm3,zmm5
       vpxord    zmm2,zmm2,zmm18
       vprolq    zmm3,zmm3,2D
       vpsrlq    zmm17,zmm17,0B
       vcvtuqq2pd zmm17,zmm17
       vmulpd    zmm17,zmm17,zmm1
       vpaddq    zmm18,zmm3,zmm5
       vprolq    zmm18,zmm18,17
       vpsllq    zmm19,zmm4,11
       vpxord    zmm8,zmm2,zmm5
       vpxord    zmm9,zmm3,zmm4
       vpxord    zmm7,zmm4,zmm8
       vpaddq    zmm2,zmm5,zmm18
       vpxord    zmm6,zmm5,zmm9
       vpxord    zmm8,zmm8,zmm19
       vprolq    zmm9,zmm9,2D
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
       call      qword ptr [7FF91F966418]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
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
       vprolq    zmm0,zmm0,17
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vprolq    zmm9,zmm9,2D
       vpsrlq    zmm0,zmm0,0B
       vcvtuqq2pd zmm0,zmm0
       vbroadcastsd zmm1,qword ptr [7FF91F5ED4C8]
       vmulpd    zmm1,zmm0,zmm1
       vmovups   [rcx],zmm1
       add       rcx,40
       inc       eax
       jmp       near ptr M00_L03
M00_L07:
       vpaddq    zmm0,zmm6,zmm9
       vprolq    zmm0,zmm0,17
       vpsllq    zmm1,zmm7,11
       vpxord    zmm8,zmm6,zmm8
       vpxord    zmm9,zmm7,zmm9
       vpxord    zmm7,zmm7,zmm8
       vpaddq    zmm0,zmm6,zmm0
       vpxord    zmm6,zmm6,zmm9
       vpxord    zmm8,zmm8,zmm1
       vprolq    zmm9,zmm9,2D
       vpsrlq    zmm0,zmm0,0B
       vcvtuqq2pd zmm0,zmm0
       vbroadcastsd zmm1,qword ptr [7FF91F5ED4C8]
       vmulpd    zmm0,zmm0,zmm1
       vmovups   [rsp+20],zmm0
       lea       rdx,[rsp+20]
       vmovups   [rsp+120],zmm6
       vmovups   [rsp+0E0],zmm7
       vmovups   [rsp+60],zmm9
       vmovups   [rsp+0A0],zmm8
       call      qword ptr [7FF91F966520]
       vmovups   zmm6,[rsp+120]
       vmovups   zmm7,[rsp+0E0]
       vmovups   zmm9,[rsp+60]
       vmovups   zmm8,[rsp+0A0]
       jmp       near ptr M00_L04
; Total bytes of code 967
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
       call      qword ptr [7FF91F9564D8]; Anastasya.Metaheuristics.Core.Randomness.RandomSource.Fill(System.Span`1<Int32>, Int32, Int32)
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
       push      rdi
       push      rsi
       push      rbx
       sub       rsp,38
       lea       rbp,[rsp+20]
       xor       eax,eax
       mov       [rbp+8],rax
       mov       rax,0BA5308645341
       mov       [rbp],rax
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
       mov       [rbp+10],rdx
       test      [rsp],esp
       sub       rsp,40
       lea       rax,[rsp+20]
       vxorps    ymm4,ymm4,ymm4
       vmovdqu32 [rax],zmm4
       xor       ebx,ebx
       cmp       ebx,r11d
       jge       short M01_L04
M01_L02:
       vpaddq    zmm4,zmm0,zmm3
       vprolq    zmm4,zmm4,17
       vpsllq    zmm5,zmm1,11
       vpxord    zmm2,zmm0,zmm2
       vpxord    zmm3,zmm1,zmm3
       vpxord    zmm1,zmm2,zmm1
       vpaddq    zmm4,zmm0,zmm4
       vpxord    zmm0,zmm0,zmm3
       vpxord    zmm2,zmm2,zmm5
       vprolq    zmm3,zmm3,2D
       vmovups   [rax],zmm4
       xor       esi,esi
       cmp       ebx,r11d
       jl        near ptr M01_L10
M01_L03:
       cmp       ebx,r11d
       jl        short M01_L02
M01_L04:
       vmovups   [rcx+28],zmm0
       vmovups   [rcx+68],zmm1
       vmovups   [rcx+0A8],zmm2
       vmovups   [rcx+0E8],zmm3
M01_L05:
       mov       r8,0BA5308645341
       cmp       [rbp],r8
       je        short M01_L06
       call      CORINFO_HELP_FAIL_FAST
M01_L06:
       nop
       vzeroupper
       lea       rsp,[rbp+18]
       pop       rbx
       pop       rsi
       pop       rdi
       pop       r14
       pop       r15
       pop       rbp
       ret
M01_L07:
       lea       r14,[rbp+8]
       mov       rdx,rdi
       mulx      rdx,r15,r9
       mov       [r14],r15
       mov       r14,rdx
       mov       rdx,[rbp+8]
       mov       rdi,[rbp+10]
       cmp       rdx,rdi
       jae       short M01_L11
       mov       edi,ebx
       jmp       short M01_L12
M01_L08:
       xor       r14d,r14d
       jmp       short M01_L11
M01_L09:
       cmp       edi,r11d
       mov       ebx,edi
       jge       near ptr M01_L03
M01_L10:
       movsxd    rdi,esi
       mov       rdi,[rax+rdi*8]
       cmp       r9,1
       je        short M01_L08
       blsr      r14,r9
       jne       short M01_L07
       xor       r14d,r14d
       tzcnt     r14,r9
       neg       r14d
       add       r14d,40
       shrx      r14,rdi,r14
M01_L11:
       lea       edi,[rbx+1]
       mov       ebx,ebx
       add       r14d,r8d
       mov       [r10+rbx*4],r14d
M01_L12:
       inc       esi
       cmp       esi,8
       jl        short M01_L09
       mov       ebx,edi
       jmp       near ptr M01_L03
M01_L13:
       mov       rcx,offset MT_System.ArgumentException
       call      CORINFO_HELP_NEWSFAST
       mov       rbx,rax
       mov       ecx,0B5
       mov       rdx,7FF91F929E10
       call      qword ptr [7FF91F787798]
       mov       rsi,rax
       mov       ecx,5B
       mov       rdx,7FF91F929E10
       call      qword ptr [7FF91F787798]
       mov       r8,rax
       mov       rdx,rsi
       mov       rcx,rbx
       call      qword ptr [7FF91F8C5E48]
       mov       rcx,rbx
       call      CORINFO_HELP_THROW
       int       3
; Total bytes of code 539
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
       call      qword ptr [7FF91F9664F0]; Anastasya.Metaheuristics.Core.Randomness.StandardNormal.Fill(Anastasya.Metaheuristics.Core.Randomness.RandomSource, System.Span`1<Double>)
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F966508]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
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
       mov       rax,1FC80394A96F
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
       vbroadcastsd zmm7,qword ptr [7FF91F5E8DA8]
       vbroadcastsd zmm8,qword ptr [7FF91F5E8DB0]
       test      edi,edi
       jle       near ptr M01_L08
M01_L04:
       vmovups   zmm0,[rbx+28]
       vmovups   zmm1,[rbx+68]
       vmovups   zmm2,[rbx+0A8]
       vmovups   zmm3,[rbx+0E8]
       vpaddq    zmm4,zmm0,zmm3
       vprolq    zmm4,zmm4,17
       vpsllq    zmm5,zmm1,11
       vpxord    zmm2,zmm0,zmm2
       vpxord    zmm3,zmm1,zmm3
       vpxord    zmm1,zmm2,zmm1
       vpaddq    zmm4,zmm0,zmm4
       vpxord    zmm0,zmm0,zmm3
       vpxord    zmm2,zmm2,zmm5
       vprolq    zmm3,zmm3,2D
       vmovups   [rbx+28],zmm0
       vmovups   [rbx+68],zmm1
       vmovups   [rbx+0A8],zmm2
       vmovups   [rbx+0E8],zmm3
       vpsrlq    zmm0,zmm4,0B
       vcvtuqq2pd zmm0,zmm0
       vmulpd    zmm0,zmm0,qword bcst [7FF91F5E8DB8]
       xor       edx,edx
       nop       dword ptr [rax]
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
       vbroadcastsd zmm0,qword ptr [7FF91F5E8DC0]
       vsubpd    zmm0,zmm0,[r14]
       vmovups   [rbp+50],zmm0
       lea       rdx,[rbp+50]
       lea       rcx,[rbp+110]
       vmovups   [rbp+210],zmm6
       vmovups   [rbp+1D0],zmm7
       vmovups   [rbp+190],zmm8
       call      qword ptr [7FF91F966910]; System.Runtime.Intrinsics.VectorMath.LogDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.UInt64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   zmm9,[rbp+110]
       vmovups   zmm8,[rbp+190]
       vmulpd    zmm0,zmm8,[r15]
       vmovups   [rbp+50],zmm0
       lea       rdx,[rbp+50]
       lea       rcx,[rbp+90]
       vmovups   [rbp+150],zmm9
       vmovups   [rbp+190],zmm8
       call      qword ptr [7FF91F966940]; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
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
       nop       word ptr [rax+rax]
M01_L07:
       movsxd    rdx,ecx
       vmovsd    xmm0,qword ptr [r13+rdx*8]
       vmovsd    qword ptr [rsi+rdx*8],xmm0
       inc       ecx
       cmp       ecx,edi
       jl        short M01_L07
M01_L08:
       mov       r8,1FC80394A96F
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
       call      qword ptr [7FF91F797798]
       mov       rcx,rax
       call      qword ptr [7FF91F966A00]
       int       3
M01_L11:
       call      CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION
       int       3
; Total bytes of code 961
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
       jmp       qword ptr [7FF91F966B80]
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
       vprolq    zmm4,zmm4,17
       vpsllq    zmm5,zmm1,11
       vpxord    zmm2,zmm0,zmm2
       vpxord    zmm3,zmm1,zmm3
       vpxord    zmm1,zmm2,zmm1
       vpaddq    zmm4,zmm0,zmm4
       vpxord    zmm0,zmm0,zmm3
       vpxord    zmm2,zmm2,zmm5
       vprolq    zmm3,zmm3,2D
       vmovups   [rax+28],zmm0
       vmovups   [rax+68],zmm1
       vmovups   [rax+0A8],zmm2
       vmovups   [rax+0E8],zmm3
       vmovq     rax,xmm4
       vzeroupper
       ret
; Total bytes of code 156
```

