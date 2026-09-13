## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.RawFill()
       push      rsi
       push      rbx
       sub       rsp,78
       vmovaps   [rsp+60],xmm6
       vmovaps   [rsp+50],xmm7
       vmovaps   [rsp+40],xmm8
       vmovaps   [rsp+30],xmm9
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
       shr       edx,1
       lea       eax,[rdx+rdx]
       sub       r8d,eax
       vmovups   xmm6,[rsi+28]
       vmovups   xmm7,[rsi+38]
       vmovups   xmm8,[rsi+48]
       vmovups   xmm9,[rsi+58]
       xor       eax,eax
       jmp       near ptr M00_L02
M00_L01:
       vpaddq    xmm0,xmm6,xmm9
       vprolq    xmm0,xmm0,17
       vpsllq    xmm1,xmm7,11
       vpxor     xmm2,xmm6,xmm8
       vpxor     xmm3,xmm7,xmm9
       vpxor     xmm4,xmm7,xmm2
       vpaddq    xmm0,xmm6,xmm0
       vpxor     xmm5,xmm6,xmm3
       vpxor     xmm2,xmm2,xmm1
       vprolq    xmm3,xmm3,2D
       vpaddq    xmm1,xmm3,xmm5
       vprolq    xmm1,xmm1,17
       vpsllq    xmm16,xmm4,11
       vpxor     xmm2,xmm2,xmm5
       vpxor     xmm3,xmm3,xmm4
       vpxor     xmm4,xmm2,xmm4
       vpaddq    xmm1,xmm5,xmm1
       vpxor     xmm5,xmm3,xmm5
       vpxord    xmm2,xmm2,xmm16
       vprolq    xmm3,xmm3,2D
       vpaddq    xmm16,xmm3,xmm5
       vprolq    xmm16,xmm16,17
       vpsllq    xmm17,xmm4,11
       vpxor     xmm2,xmm2,xmm5
       vpxor     xmm3,xmm3,xmm4
       vpxor     xmm4,xmm2,xmm4
       vpaddq    xmm16,xmm5,xmm16
       vpxor     xmm5,xmm3,xmm5
       vpxord    xmm2,xmm2,xmm17
       vprolq    xmm3,xmm3,2D
       vpaddq    xmm17,xmm3,xmm5
       vprolq    xmm17,xmm17,17
       vpsllq    xmm18,xmm4,11
       vpxor     xmm8,xmm2,xmm5
       vpxor     xmm9,xmm3,xmm4
       vpxor     xmm7,xmm4,xmm8
       vpaddq    xmm2,xmm5,xmm17
       vpxor     xmm6,xmm5,xmm9
       vpxord    xmm8,xmm8,xmm18
       vprolq    xmm9,xmm9,2D
       vmovups   [rcx],xmm0
       vmovups   [rcx+10],xmm1
       vmovups   [rcx+20],xmm16
       vmovups   [rcx+30],xmm2
       add       rcx,40
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
       vmovups   [rsi+28],xmm6
       vmovups   [rsi+38],xmm7
       vmovups   [rsi+48],xmm8
       vmovups   [rsi+58],xmm9
M00_L05:
       mov       rcx,[rbx+20]
       call      qword ptr [7FF91F976448]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(UInt64[])
       nop
       vmovaps   xmm6,[rsp+60]
       vmovaps   xmm7,[rsp+50]
       vmovaps   xmm8,[rsp+40]
       vmovaps   xmm9,[rsp+30]
       add       rsp,78
       pop       rbx
       pop       rsi
       ret
M00_L06:
       lea       rcx,[rdx+10]
       mov       r8d,[rdx+8]
       jmp       near ptr M00_L00
M00_L07:
       vpaddq    xmm0,xmm6,xmm9
       vprolq    xmm0,xmm0,17
       vpsllq    xmm1,xmm7,11
       vpxor     xmm8,xmm6,xmm8
       vpxor     xmm9,xmm7,xmm9
       vpxor     xmm7,xmm7,xmm8
       vpaddq    xmm0,xmm6,xmm0
       vpxor     xmm6,xmm6,xmm9
       vpxor     xmm8,xmm8,xmm1
       vprolq    xmm9,xmm9,2D
       vmovups   [rcx],xmm0
       add       rcx,10
       inc       eax
       jmp       near ptr M00_L03
M00_L08:
       vpaddq    xmm0,xmm6,xmm9
       vprolq    xmm0,xmm0,17
       vpsllq    xmm1,xmm7,11
       vpxor     xmm8,xmm6,xmm8
       vpxor     xmm9,xmm7,xmm9
       vpxor     xmm7,xmm7,xmm8
       vpaddq    xmm0,xmm6,xmm0
       vmovaps   [rsp+20],xmm0
       vpxor     xmm6,xmm6,xmm9
       vpxor     xmm8,xmm8,xmm1
       vprolq    xmm9,xmm9,2D
       lea       rdx,[rsp+20]
       call      qword ptr [7FF91F9764F0]
       jmp       near ptr M00_L04
; Total bytes of code 588
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
       sub       rsp,78
       vmovaps   [rsp+60],xmm6
       vmovaps   [rsp+50],xmm7
       vmovaps   [rsp+40],xmm8
       vmovaps   [rsp+30],xmm9
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
       shr       edx,1
       lea       eax,[rdx+rdx]
       sub       r8d,eax
       vmovups   xmm6,[rsi+28]
       vmovups   xmm7,[rsi+38]
       vmovups   xmm8,[rsi+48]
       vmovups   xmm9,[rsi+58]
       xor       eax,eax
M00_L01:
       lea       r10d,[rax+4]
       cmp       r10d,edx
       jg        near ptr M00_L03
       vpaddq    xmm0,xmm6,xmm9
       vprolq    xmm0,xmm0,17
       vpsllq    xmm1,xmm7,11
       vpxor     xmm2,xmm6,xmm8
       vpxor     xmm3,xmm7,xmm9
       vpxor     xmm4,xmm7,xmm2
       vpaddq    xmm0,xmm6,xmm0
       vpxor     xmm5,xmm6,xmm3
       vpxor     xmm2,xmm2,xmm1
       vprolq    xmm3,xmm3,2D
       vpsrlq    xmm0,xmm0,0B
       vcvtuqq2pd xmm0,xmm0
       vmovddup  xmm1,qword ptr [7FF91F60CAF0]
       vmulpd    xmm0,xmm0,xmm1
       vpaddq    xmm16,xmm3,xmm5
       vprolq    xmm16,xmm16,17
       vpsllq    xmm17,xmm4,11
       vpxor     xmm2,xmm2,xmm5
       vpxor     xmm3,xmm3,xmm4
       vpxor     xmm4,xmm2,xmm4
       vpaddq    xmm16,xmm5,xmm16
       vpxor     xmm5,xmm3,xmm5
       vpxord    xmm2,xmm2,xmm17
       vprolq    xmm3,xmm3,2D
       vpsrlq    xmm16,xmm16,0B
       vcvtuqq2pd xmm16,xmm16
       vmulpd    xmm16,xmm16,xmm1
       vpaddq    xmm17,xmm3,xmm5
       vprolq    xmm17,xmm17,17
       vpsllq    xmm18,xmm4,11
       vpxor     xmm2,xmm2,xmm5
       vpxor     xmm3,xmm3,xmm4
       vpxor     xmm4,xmm2,xmm4
       vpaddq    xmm17,xmm5,xmm17
       vpxor     xmm5,xmm3,xmm5
       vpxord    xmm2,xmm2,xmm18
       vprolq    xmm3,xmm3,2D
       vpsrlq    xmm17,xmm17,0B
       vcvtuqq2pd xmm17,xmm17
       vmulpd    xmm17,xmm17,xmm1
       vpaddq    xmm18,xmm3,xmm5
       vprolq    xmm18,xmm18,17
       vpsllq    xmm19,xmm4,11
       vpxor     xmm8,xmm2,xmm5
       vpxor     xmm9,xmm3,xmm4
       vpxor     xmm7,xmm4,xmm8
       vpaddq    xmm2,xmm5,xmm18
       vpxor     xmm6,xmm5,xmm9
       vpxord    xmm8,xmm8,xmm19
       vprolq    xmm9,xmm9,2D
       vpsrlq    xmm2,xmm2,0B
       vcvtuqq2pd xmm2,xmm2
       vmulpd    xmm1,xmm2,xmm1
       vmovups   [rcx],xmm0
       vmovups   [rcx+10],xmm16
       vmovups   [rcx+20],xmm17
       vmovups   [rcx+30],xmm1
       add       rcx,40
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
       vmovups   [rsi+28],xmm6
       vmovups   [rsi+38],xmm7
       vmovups   [rsi+48],xmm8
       vmovups   [rsi+58],xmm9
M00_L05:
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F986460]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
       nop
       vmovaps   xmm6,[rsp+60]
       vmovaps   xmm7,[rsp+50]
       vmovaps   xmm8,[rsp+40]
       vmovaps   xmm9,[rsp+30]
       add       rsp,78
       pop       rbx
       pop       rsi
       ret
M00_L06:
       vpaddq    xmm0,xmm6,xmm9
       vprolq    xmm0,xmm0,17
       vpsllq    xmm1,xmm7,11
       vpxor     xmm8,xmm6,xmm8
       vpxor     xmm9,xmm7,xmm9
       vpxor     xmm7,xmm7,xmm8
       vpaddq    xmm0,xmm6,xmm0
       vpxor     xmm6,xmm6,xmm9
       vpxor     xmm8,xmm8,xmm1
       vprolq    xmm9,xmm9,2D
       vpsrlq    xmm0,xmm0,0B
       vcvtuqq2pd xmm0,xmm0
       vmovddup  xmm1,qword ptr [7FF91F60CAF0]
       vmulpd    xmm1,xmm0,xmm1
       vmovups   [rcx],xmm1
       add       rcx,10
       inc       eax
       jmp       near ptr M00_L03
M00_L07:
       vpaddq    xmm0,xmm6,xmm9
       vprolq    xmm0,xmm0,17
       vpsllq    xmm1,xmm7,11
       vpxor     xmm8,xmm6,xmm8
       vpxor     xmm9,xmm7,xmm9
       vpxor     xmm7,xmm7,xmm8
       vpaddq    xmm0,xmm6,xmm0
       vpxor     xmm6,xmm6,xmm9
       vpxor     xmm8,xmm8,xmm1
       vprolq    xmm9,xmm9,2D
       vpsrlq    xmm0,xmm0,0B
       vcvtuqq2pd xmm0,xmm0
       vmovddup  xmm1,qword ptr [7FF91F60CAF0]
       vmulpd    xmm0,xmm0,xmm1
       vmovaps   [rsp+20],xmm0
       lea       rdx,[rsp+20]
       call      qword ptr [7FF91F986568]
       jmp       near ptr M00_L04
; Total bytes of code 716
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
       call      qword ptr [7FF91F986418]; Anastasya.Metaheuristics.Core.Randomness.RandomSource.Fill(System.Span`1<Int32>, Int32, Int32)
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
       mov       rax,0EF8344543615
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
       vmovups   xmm0,[rcx+28]
       vmovups   xmm1,[rcx+38]
       vmovups   xmm2,[rcx+48]
       vmovups   xmm3,[rcx+58]
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
       sub       rsp,10
       lea       rax,[rsp+20]
       vxorps    xmm4,xmm4,xmm4
       vmovups   [rax],xmm4
       xor       ebx,ebx
       cmp       ebx,r11d
       jge       short M01_L04
M01_L02:
       vpaddq    xmm4,xmm0,xmm3
       vprolq    xmm4,xmm4,17
       vpsllq    xmm5,xmm1,11
       vpxor     xmm2,xmm0,xmm2
       vpxor     xmm3,xmm1,xmm3
       vpxor     xmm1,xmm2,xmm1
       vpaddq    xmm4,xmm0,xmm4
       vpxor     xmm0,xmm0,xmm3
       vpxor     xmm2,xmm2,xmm5
       vprolq    xmm3,xmm3,2D
       vmovups   [rax],xmm4
       xor       esi,esi
       cmp       ebx,r11d
       jl        short M01_L10
M01_L03:
       cmp       ebx,r11d
       jl        short M01_L02
M01_L04:
       vmovups   [rcx+28],xmm0
       vmovups   [rcx+38],xmm1
       vmovups   [rcx+48],xmm2
       vmovups   [rcx+58],xmm3
M01_L05:
       mov       r8,0EF8344543615
       cmp       [rbp],r8
       je        short M01_L06
       call      CORINFO_HELP_FAIL_FAST
M01_L06:
       nop
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
       jge       short M01_L03
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
       cmp       esi,2
       jl        short M01_L09
       mov       ebx,edi
       jmp       near ptr M01_L03
M01_L13:
       mov       rcx,offset MT_System.ArgumentException
       call      CORINFO_HELP_NEWSFAST
       mov       rbx,rax
       mov       ecx,0B5
       mov       rdx,7FF91F959F90
       call      qword ptr [7FF91F7B7798]
       mov       rsi,rax
       mov       ecx,5B
       mov       rdx,7FF91F959F90
       call      qword ptr [7FF91F7B7798]
       mov       r8,rax
       mov       rdx,rsi
       mov       rcx,rbx
       call      qword ptr [7FF91F8F5E48]
       mov       rcx,rbx
       call      CORINFO_HELP_THROW
       int       3
; Total bytes of code 468
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
       call      qword ptr [7FF91F966358]; Anastasya.Metaheuristics.Core.Randomness.StandardNormal.Fill(Anastasya.Metaheuristics.Core.Randomness.RandomSource, System.Span`1<Double>)
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F966370]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
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
       sub       rsp,0C0
       vmovaps   [rsp+0B0],xmm6
       vmovaps   [rsp+0A0],xmm7
       vmovaps   [rsp+90],xmm8
       vmovaps   [rsp+80],xmm9
       lea       rbp,[rsp+20]
       xor       eax,eax
       mov       [rbp+10],rax
       mov       [rbp+18],rax
       mov       rax,16493EF389DB
       mov       [rbp+8],rax
       mov       rbx,rcx
       mov       rsi,[rdx]
       mov       edi,[rdx+8]
       test      rbx,rbx
       je        near ptr M01_L10
       test      edi,edi
       je        near ptr M01_L08
       test      [rsp],esp
       sub       rsp,10
       lea       rdx,[rsp+20]
       vxorps    xmm0,xmm0,xmm0
       vmovups   [rdx],xmm0
       mov       r14,rdx
       test      [rsp],esp
       sub       rsp,10
       lea       rdx,[rsp+20]
       vmovups   [rdx],xmm0
       mov       r15,rdx
       test      [rsp],esp
       sub       rsp,10
       lea       rdx,[rsp+20]
       vmovups   [rdx],xmm0
       test      [rsp],esp
       sub       rsp,10
       lea       rcx,[rsp+20]
       vmovups   [rcx],xmm0
       mov       r13,rcx
       xor       ecx,ecx
       jmp       short M01_L02
       nop       dword ptr [rax]
M01_L00:
       xor       r8d,r8d
M01_L01:
       mov       [rax],r8
       inc       ecx
       cmp       ecx,2
       jge       short M01_L03
M01_L02:
       lea       rax,[rdx+rcx*8]
       test      cl,1
       jne       short M01_L00
       mov       r8,0FFFFFFFFFFFFFFFF
       jmp       short M01_L01
M01_L03:
       vmovups   xmm6,[rdx]
       vmovddup  xmm7,qword ptr [7FF91F5E87C0]
       vmovddup  xmm8,qword ptr [7FF91F5E87C8]
       test      edi,edi
       jle       near ptr M01_L08
M01_L04:
       vmovups   xmm0,[rbx+28]
       vmovups   xmm1,[rbx+38]
       vmovups   xmm2,[rbx+48]
       vmovups   xmm3,[rbx+58]
       vpaddq    xmm4,xmm0,xmm3
       vprolq    xmm4,xmm4,17
       vpsllq    xmm5,xmm1,11
       vpxor     xmm2,xmm0,xmm2
       vpxor     xmm3,xmm1,xmm3
       vpxor     xmm1,xmm2,xmm1
       vpaddq    xmm4,xmm0,xmm4
       vpxor     xmm0,xmm0,xmm3
       vpxor     xmm2,xmm2,xmm5
       vprolq    xmm3,xmm3,2D
       vmovups   [rbx+28],xmm0
       vmovups   [rbx+38],xmm1
       vmovups   [rbx+48],xmm2
       vmovups   [rbx+58],xmm3
       vpsrlq    xmm0,xmm4,0B
       vcvtuqq2pd xmm0,xmm0
       vmulpd    xmm0,xmm0,qword bcst [7FF91F5E87D0]
       xor       edx,edx
M01_L05:
       lea       rcx,[rdx*8]
       lea       rax,[r14+rcx]
       mov       r8d,edx
       and       r8d,0FFFFFFFE
       cmp       r8d,2
       jae       near ptr M01_L11
       vmovaps   [rbp+10],xmm0
       vmovsd    xmm1,qword ptr [rbp+r8*8+10]
       vmovsd    qword ptr [rax],xmm1
       add       rcx,r15
       mov       eax,edx
       or        eax,1
       cmp       eax,2
       jae       near ptr M01_L11
       vmovaps   [rbp+10],xmm0
       vmovsd    xmm1,qword ptr [rbp+rax*8+10]
       vmovsd    qword ptr [rcx],xmm1
       inc       edx
       cmp       edx,2
       jl        short M01_L05
       vmovddup  xmm0,qword ptr [7FF91F5E87D8]
       vsubpd    xmm0,xmm0,[r14]
       vmovaps   [rbp+20],xmm0
       lea       rdx,[rbp+20]
       lea       rcx,[rbp+50]
       call      qword ptr [7FF91F966760]; System.Runtime.Intrinsics.VectorMath.LogDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.UInt64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovaps   xmm9,[rbp+50]
       vmulpd    xmm0,xmm8,[r15]
       vmovaps   [rbp+20],xmm0
       lea       rdx,[rbp+20]
       lea       rcx,[rbp+30]
       call      qword ptr [7FF91F966790]; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   xmm0,[rbp+30]
       vmovups   xmm1,[rbp+40]
       vpternlogq xmm1,xmm0,xmm6,0E4
       vmulpd    xmm0,xmm9,xmm7
       vsqrtpd   xmm0,xmm0
       vmulpd    xmm0,xmm1,xmm0
       cmp       edi,2
       jl        short M01_L06
       vmovups   [rsi],xmm0
       add       rsi,10
       sub       edi,2
       test      edi,edi
       jg        near ptr M01_L04
       jmp       short M01_L08
M01_L06:
       vmovups   [r13],xmm0
       xor       ecx,ecx
       nop       dword ptr [rax]
M01_L07:
       movsxd    rdx,ecx
       vmovsd    xmm0,qword ptr [r13+rdx*8]
       vmovsd    qword ptr [rsi+rdx*8],xmm0
       inc       ecx
       cmp       ecx,edi
       jl        short M01_L07
M01_L08:
       mov       r8,16493EF389DB
       cmp       [rbp+8],r8
       je        short M01_L09
       call      CORINFO_HELP_FAIL_FAST
M01_L09:
       nop
       vmovaps   xmm6,[rbp+90]
       vmovaps   xmm7,[rbp+80]
       vmovaps   xmm8,[rbp+70]
       vmovaps   xmm9,[rbp+60]
       lea       rsp,[rbp+0A0]
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
       mov       rdx,7FF91F939F90
       call      qword ptr [7FF91F797798]
       mov       rcx,rax
       call      qword ptr [7FF91F966850]
       int       3
M01_L11:
       call      CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION
       int       3
; Total bytes of code 698
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
       vmovups   xmm0,[rdx]
       vmovaps   xmm1,xmm0
       vpaddq    xmm2,xmm0,qword bcst [7FF91F5EC6B0]
       vpcmpnltuq k1,xmm2,qword bcst [7FF91F5EC6B8]
       vpmovm2q  xmm2,k1
       vptest    xmm2,xmm2
       jne       near ptr M03_L01
M03_L00:
       vmovups   xmm0,[rdx]
       vpaddq    xmm0,xmm0,qword bcst [7FF91F5EC6C0]
       vpsraq    xmm3,xmm0,34
       vcvtqq2pd xmm3,xmm3
       vpandq    xmm0,xmm0,qword bcst [7FF91F5EC6C8]
       vpaddq    xmm0,xmm0,qword bcst [7FF91F5EC6D0]
       vsubpd    xmm0,xmm0,qword bcst [7FF91F5EC6D8]
       vmulpd    xmm4,xmm0,xmm0
       vmulpd    xmm5,xmm4,xmm4
       vmulpd    xmm16,xmm5,xmm5
       vmulpd    xmm17,xmm16,xmm16
       vmovddup  xmm18,qword ptr [7FF91F5EC6E0]
       vfmadd213pd xmm18,xmm0,qword bcst [7FF91F5EC6E8]
       vmovddup  xmm19,qword ptr [7FF91F5EC6F0]
       vfmadd213pd xmm19,xmm0,qword bcst [7FF91F5EC6F8]
       vfmadd213pd xmm18,xmm4,xmm19
       vfmadd231pd xmm18,xmm5,qword bcst [7FF91F5EC700]
       vmovddup  xmm19,qword ptr [7FF91F5EC708]
       vfmadd213pd xmm19,xmm0,qword bcst [7FF91F5EC710]
       vmovddup  xmm20,qword ptr [7FF91F5EC718]
       vfmadd213pd xmm20,xmm0,qword bcst [7FF91F5EC720]
       vfmadd213pd xmm19,xmm4,xmm20
       vmovddup  xmm20,qword ptr [7FF91F5EC728]
       vfmadd213pd xmm20,xmm0,qword bcst [7FF91F5EC730]
       vmovddup  xmm21,qword ptr [7FF91F5EC738]
       vfmadd213pd xmm21,xmm0,qword bcst [7FF91F5EC740]
       vfmadd213pd xmm20,xmm4,xmm21
       vfmadd231pd xmm20,xmm19,xmm5
       vmovddup  xmm19,qword ptr [7FF91F5EC748]
       vfmadd213pd xmm19,xmm0,qword bcst [7FF91F5EC750]
       vmovddup  xmm21,qword ptr [7FF91F5EC758]
       vfmadd213pd xmm21,xmm0,qword bcst [7FF91F5EC760]
       vfmadd213pd xmm19,xmm4,xmm21
       vmovddup  xmm21,qword ptr [7FF91F5EC768]
       vfmadd213pd xmm21,xmm0,qword bcst [7FF91F5EC770]
       vfmadd213pd xmm4,xmm21,xmm0
       vfmadd213pd xmm5,xmm19,xmm4
       vfmadd213pd xmm16,xmm20,xmm5
       vfmadd213pd xmm17,xmm18,xmm16
       vmovaps   xmm0,xmm3
       vfmadd132pd xmm0,xmm17,qword bcst [7FF91F5EC778]
       vfmadd132pd xmm3,xmm0,qword bcst [7FF91F5EC780]
       vpternlogq xmm2,xmm3,xmm1,0AC
       vmovups   [rcx],xmm2
       mov       rax,rcx
       ret
M03_L01:
       vxorps    xmm3,xmm3,xmm3
       vpcmpgtq  xmm3,xmm3,xmm0
       vpternlogq xmm1,xmm3,qword bcst [7FF91F5EC788],0B8
       vxorps    xmm4,xmm4,xmm4
       vcmpeqpd  xmm4,xmm4,xmm0
       vpternlogq xmm1,xmm4,qword bcst [7FF91F5EC6B0],0B8
       vcmpneqpd xmm5,xmm0,xmm0
       vpternlogq xmm4,xmm5,xmm3,0FE
       vpcmpeqq  xmm3,xmm0,[7FF91F5EC790]
       vorpd     xmm3,xmm3,xmm4
       vandnpd   xmm2,xmm3,xmm2
       vmulpd    xmm4,xmm0,qword bcst [7FF91F5EC7A0]
       vpaddq    xmm4,xmm4,qword bcst [7FF91F5EC7A8]
       vpternlogq xmm2,xmm4,xmm0,0CA
       vmovups   [rdx],xmm2
       vmovaps   xmm2,xmm3
       jmp       near ptr M03_L00
; Total bytes of code 515
```
```assembly
; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       sub       rsp,58
       vmovaps   [rsp+40],xmm6
       vmovaps   [rsp+30],xmm7
       vmovaps   [rsp+20],xmm8
       vmovaps   [rsp+10],xmm9
       vmovaps   [rsp],xmm10
       vmovups   xmm6,[rdx]
       vandpd    xmm7,xmm6,qword bcst [7FF91F5ECB80]
       vmovddup  xmm0,qword ptr [7FF91F5ECB88]
       vpcmpgtq  xmm0,xmm0,xmm7
       vpcmpeqd  xmm1,xmm1,xmm1
       vptest    xmm0,xmm1
       jb        near ptr M04_L01
       vmovddup  xmm0,qword ptr [7FF91F5ECB90]
       vpcmpgtq  xmm0,xmm0,xmm7
       vpcmpeqd  xmm1,xmm1,xmm1
       vptest    xmm0,xmm1
       jae       near ptr M04_L03
       vmovddup  xmm0,qword ptr [7FF91F5ECB98]
       vmovaps   xmm1,xmm0
       vfmadd231pd xmm1,xmm7,qword bcst [7FF91F5ECBA0]
       vsubpd    xmm0,xmm1,xmm0
       vmovaps   xmm2,xmm7
       vfmadd231pd xmm2,xmm0,qword bcst [7FF91F5ECBA8]
       vmulpd    xmm3,xmm0,qword bcst [7FF91F5ECBB0]
       vsubpd    xmm4,xmm2,xmm3
       vsubpd    xmm2,xmm2,xmm4
       vsubpd    xmm3,xmm2,xmm3
       vmovddup  xmm2,qword ptr [7FF91F5ECBB8]
       vxorpd    xmm3,xmm3,xmm2
       vfmadd132pd xmm0,xmm3,qword bcst [7FF91F5ECBC0]
       vmovaps   xmm3,xmm0
       vsubpd    xmm0,xmm4,xmm3
       vsubpd    xmm4,xmm4,xmm0
       vsubpd    xmm3,xmm4,xmm3
       vmulpd    xmm4,xmm0,xmm0
       vmovaps   xmm5,xmm4
       vmulpd    xmm16,xmm0,xmm5
       vmulpd    xmm17,xmm5,xmm5
       vmovaps   xmm18,xmm17
       vmovddup  xmm19,qword ptr [7FF91F5ECBC8]
       vmulpd    xmm20,xmm18,xmm18
       vmovddup  xmm21,qword ptr [7FF91F5ECBD0]
       vfmadd213pd xmm21,xmm5,qword bcst [7FF91F5ECBD8]
       vmovddup  xmm22,qword ptr [7FF91F5ECBE0]
       vfmadd213pd xmm22,xmm5,qword bcst [7FF91F5ECBE8]
       vfmadd213pd xmm18,xmm21,xmm22
       vfmadd231pd xmm18,xmm20,qword bcst [7FF91F5ECBF0]
       vmulpd    xmm18,xmm18,xmm16
       vxorpd    xmm18,xmm18,xmm2
       vfmadd231pd xmm18,xmm19,xmm3
       vxorpd    xmm21,xmm2,xmm3
       vfmadd213pd xmm5,xmm18,xmm21
       vfmadd231pd xmm5,xmm16,qword bcst [7FF91F5ECBF8]
       vsubpd    xmm5,xmm0,xmm5
       vmovaps   xmm16,xmm4
       vmulpd    xmm16,xmm19,xmm16
       vmovddup  xmm8,qword ptr [7FF91F5ECC00]
       vsubpd    xmm18,xmm16,xmm8
       vmovddup  xmm19,qword ptr [7FF91F5ECC08]
       vfmadd213pd xmm19,xmm4,qword bcst [7FF91F5ECC10]
       vmovddup  xmm21,qword ptr [7FF91F5ECC18]
       vfmadd213pd xmm21,xmm4,qword bcst [7FF91F5ECC20]
       vmovddup  xmm22,qword ptr [7FF91F5ECC28]
       vfmadd213pd xmm4,xmm22,qword bcst [7FF91F5ECC30]
       vfmadd231pd xmm4,xmm17,xmm21
       vfmadd213pd xmm19,xmm20,xmm4
       vaddpd    xmm4,xmm8,xmm18
       vsubpd    xmm4,xmm4,xmm16
       vfmadd213pd xmm0,xmm3,xmm4
       vfmadd213pd xmm19,xmm17,xmm0
       vsubpd    xmm0,xmm19,xmm18
       vmovddup  xmm3,qword ptr [7FF91F5ECC38]
       vpand     xmm4,xmm3,xmm1
       vptestnmq k1,xmm4,xmm4
       vpblendmq xmm9{k1},xmm0,xmm5
       vpblendmq xmm10{k1},xmm5,xmm0
       vpsrlq    xmm0,xmm6,3F
       vpsrlq    xmm4,xmm1,1
       vpternlogq xmm5,xmm4,xmm0,11
       vpternlogq xmm5,xmm4,xmm0,0F8
       vpand     xmm0,xmm5,xmm3
       vxorps    xmm4,xmm4,xmm4
       vpcmpeqq  xmm0,xmm4,xmm0
       vxorpd    xmm4,xmm2,xmm9
       vblendvpd xmm9,xmm9,xmm4,xmm0
       vpaddq    xmm0,xmm3,xmm1
       vpandq    xmm0,xmm0,qword bcst [7FF91F5ECC40]
       vxorps    xmm1,xmm1,xmm1
       vpcmpeqq  xmm0,xmm1,xmm0
       vxorpd    xmm1,xmm2,xmm10
       vblendvpd xmm10,xmm1,xmm10,xmm0
M04_L00:
       vpcmpgtq  k1,xmm7,qword bcst [7FF91F5ECC48]
       vpblendmq xmm9{k1},xmm6,xmm9
       vpblendmq xmm10{k1},xmm8,xmm10
       vmovups   [rcx],xmm9
       vmovups   [rcx+10],xmm10
       mov       rax,rcx
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       ret
M04_L01:
       vmulpd    xmm0,xmm6,xmm6
       vmovaps   xmm10,xmm0
       vpcmpgtq  xmm1,xmm7,[7FF91F5ECC50]
       vptest    xmm1,xmm1
       je        near ptr M04_L02
       vmovaps   xmm1,xmm0
       vmulpd    xmm9,xmm6,xmm1
       vmulpd    xmm2,xmm1,xmm1
       vmovaps   xmm3,xmm2
       vmovddup  xmm4,qword ptr [7FF91F5ECBF0]
       vfmadd213pd xmm4,xmm1,qword bcst [7FF91F5ECBD0]
       vmulpd    xmm5,xmm3,xmm3
       vmovddup  xmm16,qword ptr [7FF91F5ECBD8]
       vfmadd213pd xmm16,xmm1,qword bcst [7FF91F5ECBE0]
       vmovddup  xmm17,qword ptr [7FF91F5ECBE8]
       vfmadd213pd xmm1,xmm17,qword bcst [7FF91F5ECC60]
       vfmadd213pd xmm3,xmm16,xmm1
       vfmadd213pd xmm4,xmm5,xmm3
       vfmadd213pd xmm9,xmm4,xmm6
       vmovddup  xmm1,qword ptr [7FF91F5ECC08]
       vfmadd213pd xmm1,xmm0,qword bcst [7FF91F5ECC10]
       vmovddup  xmm3,qword ptr [7FF91F5ECC18]
       vfmadd213pd xmm3,xmm0,qword bcst [7FF91F5ECC20]
       vmovddup  xmm4,qword ptr [7FF91F5ECC28]
       vfmadd213pd xmm0,xmm4,qword bcst [7FF91F5ECC30]
       vfmadd213pd xmm2,xmm3,xmm0
       vfmadd213pd xmm1,xmm5,xmm2
       vfmadd213pd xmm1,xmm10,qword bcst [7FF91F5ECC68]
       vmovddup  xmm8,qword ptr [7FF91F5ECC00]
       vfmadd213pd xmm10,xmm1,xmm8
       jmp       near ptr M04_L00
M04_L02:
       vmulpd    xmm0,xmm6,xmm10
       vmovaps   xmm9,xmm6
       vfmadd231pd xmm9,xmm0,qword bcst [7FF91F5ECC60]
       vmovddup  xmm8,qword ptr [7FF91F5ECC00]
       vfmadd132pd xmm10,xmm8,qword bcst [7FF91F5ECC68]
       jmp       near ptr M04_L00
M04_L03:
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       jmp       qword ptr [7FF91F96CE28]
; Total bytes of code 947
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.VectorApi()
       mov       rax,[rcx+10]
       vmovups   xmm0,[rax+28]
       vmovups   xmm1,[rax+38]
       vmovups   xmm2,[rax+48]
       vmovups   xmm3,[rax+58]
       vpaddq    xmm4,xmm0,xmm3
       vprolq    xmm4,xmm4,17
       vpsllq    xmm5,xmm1,11
       vpxor     xmm2,xmm0,xmm2
       vpxor     xmm3,xmm1,xmm3
       vpxor     xmm1,xmm2,xmm1
       vpaddq    xmm4,xmm0,xmm4
       vpxor     xmm0,xmm0,xmm3
       vpxor     xmm2,xmm2,xmm5
       vprolq    xmm3,xmm3,2D
       vmovups   [rax+28],xmm0
       vmovups   [rax+38],xmm1
       vmovups   [rax+48],xmm2
       vmovups   [rax+58],xmm3
       vmovq     rax,xmm4
       ret
; Total bytes of code 97
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.RawFill()
       push      rsi
       push      rbx
       sub       rsp,78
       vmovaps   [rsp+60],xmm6
       vmovaps   [rsp+50],xmm7
       vmovaps   [rsp+40],xmm8
       vmovaps   [rsp+30],xmm9
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
       shr       edx,1
       lea       eax,[rdx+rdx]
       sub       r8d,eax
       vmovups   xmm6,[rsi+28]
       vmovups   xmm7,[rsi+38]
       vmovups   xmm8,[rsi+48]
       vmovups   xmm9,[rsi+58]
       xor       eax,eax
       jmp       near ptr M00_L02
M00_L01:
       vpaddq    xmm0,xmm6,xmm9
       vprolq    xmm0,xmm0,17
       vpsllq    xmm1,xmm7,11
       vpxor     xmm2,xmm6,xmm8
       vpxor     xmm3,xmm7,xmm9
       vpxor     xmm4,xmm7,xmm2
       vpaddq    xmm0,xmm6,xmm0
       vpxor     xmm5,xmm6,xmm3
       vpxor     xmm2,xmm2,xmm1
       vprolq    xmm3,xmm3,2D
       vpaddq    xmm1,xmm3,xmm5
       vprolq    xmm1,xmm1,17
       vpsllq    xmm16,xmm4,11
       vpxor     xmm2,xmm2,xmm5
       vpxor     xmm3,xmm3,xmm4
       vpxor     xmm4,xmm2,xmm4
       vpaddq    xmm1,xmm5,xmm1
       vpxor     xmm5,xmm3,xmm5
       vpxord    xmm2,xmm2,xmm16
       vprolq    xmm3,xmm3,2D
       vpaddq    xmm16,xmm3,xmm5
       vprolq    xmm16,xmm16,17
       vpsllq    xmm17,xmm4,11
       vpxor     xmm2,xmm2,xmm5
       vpxor     xmm3,xmm3,xmm4
       vpxor     xmm4,xmm2,xmm4
       vpaddq    xmm16,xmm5,xmm16
       vpxor     xmm5,xmm3,xmm5
       vpxord    xmm2,xmm2,xmm17
       vprolq    xmm3,xmm3,2D
       vpaddq    xmm17,xmm3,xmm5
       vprolq    xmm17,xmm17,17
       vpsllq    xmm18,xmm4,11
       vpxor     xmm8,xmm2,xmm5
       vpxor     xmm9,xmm3,xmm4
       vpxor     xmm7,xmm4,xmm8
       vpaddq    xmm2,xmm5,xmm17
       vpxor     xmm6,xmm5,xmm9
       vpxord    xmm8,xmm8,xmm18
       vprolq    xmm9,xmm9,2D
       vmovups   [rcx],xmm0
       vmovups   [rcx+10],xmm1
       vmovups   [rcx+20],xmm16
       vmovups   [rcx+30],xmm2
       add       rcx,40
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
       vmovups   [rsi+28],xmm6
       vmovups   [rsi+38],xmm7
       vmovups   [rsi+48],xmm8
       vmovups   [rsi+58],xmm9
M00_L05:
       mov       rcx,[rbx+20]
       call      qword ptr [7FF91F986460]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(UInt64[])
       nop
       vmovaps   xmm6,[rsp+60]
       vmovaps   xmm7,[rsp+50]
       vmovaps   xmm8,[rsp+40]
       vmovaps   xmm9,[rsp+30]
       add       rsp,78
       pop       rbx
       pop       rsi
       ret
M00_L06:
       lea       rcx,[rdx+10]
       mov       r8d,[rdx+8]
       jmp       near ptr M00_L00
M00_L07:
       vpaddq    xmm0,xmm6,xmm9
       vprolq    xmm0,xmm0,17
       vpsllq    xmm1,xmm7,11
       vpxor     xmm8,xmm6,xmm8
       vpxor     xmm9,xmm7,xmm9
       vpxor     xmm7,xmm7,xmm8
       vpaddq    xmm0,xmm6,xmm0
       vpxor     xmm6,xmm6,xmm9
       vpxor     xmm8,xmm8,xmm1
       vprolq    xmm9,xmm9,2D
       vmovups   [rcx],xmm0
       add       rcx,10
       inc       eax
       jmp       near ptr M00_L03
M00_L08:
       vpaddq    xmm0,xmm6,xmm9
       vprolq    xmm0,xmm0,17
       vpsllq    xmm1,xmm7,11
       vpxor     xmm8,xmm6,xmm8
       vpxor     xmm9,xmm7,xmm9
       vpxor     xmm7,xmm7,xmm8
       vpaddq    xmm0,xmm6,xmm0
       vmovaps   [rsp+20],xmm0
       vpxor     xmm6,xmm6,xmm9
       vpxor     xmm8,xmm8,xmm1
       vprolq    xmm9,xmm9,2D
       lea       rdx,[rsp+20]
       call      qword ptr [7FF91F986508]
       jmp       near ptr M00_L04
; Total bytes of code 588
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
       sub       rsp,78
       vmovaps   [rsp+60],xmm6
       vmovaps   [rsp+50],xmm7
       vmovaps   [rsp+40],xmm8
       vmovaps   [rsp+30],xmm9
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
       shr       edx,1
       lea       eax,[rdx+rdx]
       sub       r8d,eax
       vmovups   xmm6,[rsi+28]
       vmovups   xmm7,[rsi+38]
       vmovups   xmm8,[rsi+48]
       vmovups   xmm9,[rsi+58]
       xor       eax,eax
M00_L01:
       lea       r10d,[rax+4]
       cmp       r10d,edx
       jg        near ptr M00_L03
       vpaddq    xmm0,xmm6,xmm9
       vprolq    xmm0,xmm0,17
       vpsllq    xmm1,xmm7,11
       vpxor     xmm2,xmm6,xmm8
       vpxor     xmm3,xmm7,xmm9
       vpxor     xmm4,xmm7,xmm2
       vpaddq    xmm0,xmm6,xmm0
       vpxor     xmm5,xmm6,xmm3
       vpxor     xmm2,xmm2,xmm1
       vprolq    xmm3,xmm3,2D
       vpsrlq    xmm0,xmm0,0B
       vcvtuqq2pd xmm0,xmm0
       vmovddup  xmm1,qword ptr [7FF91F5DCAF0]
       vmulpd    xmm0,xmm0,xmm1
       vpaddq    xmm16,xmm3,xmm5
       vprolq    xmm16,xmm16,17
       vpsllq    xmm17,xmm4,11
       vpxor     xmm2,xmm2,xmm5
       vpxor     xmm3,xmm3,xmm4
       vpxor     xmm4,xmm2,xmm4
       vpaddq    xmm16,xmm5,xmm16
       vpxor     xmm5,xmm3,xmm5
       vpxord    xmm2,xmm2,xmm17
       vprolq    xmm3,xmm3,2D
       vpsrlq    xmm16,xmm16,0B
       vcvtuqq2pd xmm16,xmm16
       vmulpd    xmm16,xmm16,xmm1
       vpaddq    xmm17,xmm3,xmm5
       vprolq    xmm17,xmm17,17
       vpsllq    xmm18,xmm4,11
       vpxor     xmm2,xmm2,xmm5
       vpxor     xmm3,xmm3,xmm4
       vpxor     xmm4,xmm2,xmm4
       vpaddq    xmm17,xmm5,xmm17
       vpxor     xmm5,xmm3,xmm5
       vpxord    xmm2,xmm2,xmm18
       vprolq    xmm3,xmm3,2D
       vpsrlq    xmm17,xmm17,0B
       vcvtuqq2pd xmm17,xmm17
       vmulpd    xmm17,xmm17,xmm1
       vpaddq    xmm18,xmm3,xmm5
       vprolq    xmm18,xmm18,17
       vpsllq    xmm19,xmm4,11
       vpxor     xmm8,xmm2,xmm5
       vpxor     xmm9,xmm3,xmm4
       vpxor     xmm7,xmm4,xmm8
       vpaddq    xmm2,xmm5,xmm18
       vpxor     xmm6,xmm5,xmm9
       vpxord    xmm8,xmm8,xmm19
       vprolq    xmm9,xmm9,2D
       vpsrlq    xmm2,xmm2,0B
       vcvtuqq2pd xmm2,xmm2
       vmulpd    xmm1,xmm2,xmm1
       vmovups   [rcx],xmm0
       vmovups   [rcx+10],xmm16
       vmovups   [rcx+20],xmm17
       vmovups   [rcx+30],xmm1
       add       rcx,40
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
       vmovups   [rsi+28],xmm6
       vmovups   [rsi+38],xmm7
       vmovups   [rsi+48],xmm8
       vmovups   [rsi+58],xmm9
M00_L05:
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F956370]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
       nop
       vmovaps   xmm6,[rsp+60]
       vmovaps   xmm7,[rsp+50]
       vmovaps   xmm8,[rsp+40]
       vmovaps   xmm9,[rsp+30]
       add       rsp,78
       pop       rbx
       pop       rsi
       ret
M00_L06:
       vpaddq    xmm0,xmm6,xmm9
       vprolq    xmm0,xmm0,17
       vpsllq    xmm1,xmm7,11
       vpxor     xmm8,xmm6,xmm8
       vpxor     xmm9,xmm7,xmm9
       vpxor     xmm7,xmm7,xmm8
       vpaddq    xmm0,xmm6,xmm0
       vpxor     xmm6,xmm6,xmm9
       vpxor     xmm8,xmm8,xmm1
       vprolq    xmm9,xmm9,2D
       vpsrlq    xmm0,xmm0,0B
       vcvtuqq2pd xmm0,xmm0
       vmovddup  xmm1,qword ptr [7FF91F5DCAF0]
       vmulpd    xmm1,xmm0,xmm1
       vmovups   [rcx],xmm1
       add       rcx,10
       inc       eax
       jmp       near ptr M00_L03
M00_L07:
       vpaddq    xmm0,xmm6,xmm9
       vprolq    xmm0,xmm0,17
       vpsllq    xmm1,xmm7,11
       vpxor     xmm8,xmm6,xmm8
       vpxor     xmm9,xmm7,xmm9
       vpxor     xmm7,xmm7,xmm8
       vpaddq    xmm0,xmm6,xmm0
       vpxor     xmm6,xmm6,xmm9
       vpxor     xmm8,xmm8,xmm1
       vprolq    xmm9,xmm9,2D
       vpsrlq    xmm0,xmm0,0B
       vcvtuqq2pd xmm0,xmm0
       vmovddup  xmm1,qword ptr [7FF91F5DCAF0]
       vmulpd    xmm0,xmm0,xmm1
       vmovaps   [rsp+20],xmm0
       lea       rdx,[rsp+20]
       call      qword ptr [7FF91F956478]
       jmp       near ptr M00_L04
; Total bytes of code 716
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
       call      qword ptr [7FF91F946340]; Anastasya.Metaheuristics.Core.Randomness.RandomSource.Fill(System.Span`1<Int32>, Int32, Int32)
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
       mov       rax,0C410AD8FFCF7
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
       vmovups   xmm0,[rcx+28]
       vmovups   xmm1,[rcx+38]
       vmovups   xmm2,[rcx+48]
       vmovups   xmm3,[rcx+58]
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
       sub       rsp,10
       lea       rax,[rsp+20]
       vxorps    xmm4,xmm4,xmm4
       vmovups   [rax],xmm4
       xor       ebx,ebx
       cmp       ebx,r11d
       jge       short M01_L04
M01_L02:
       vpaddq    xmm4,xmm0,xmm3
       vprolq    xmm4,xmm4,17
       vpsllq    xmm5,xmm1,11
       vpxor     xmm2,xmm0,xmm2
       vpxor     xmm3,xmm1,xmm3
       vpxor     xmm1,xmm2,xmm1
       vpaddq    xmm4,xmm0,xmm4
       vpxor     xmm0,xmm0,xmm3
       vpxor     xmm2,xmm2,xmm5
       vprolq    xmm3,xmm3,2D
       vmovups   [rax],xmm4
       xor       esi,esi
       cmp       ebx,r11d
       jl        short M01_L10
M01_L03:
       cmp       ebx,r11d
       jl        short M01_L02
M01_L04:
       vmovups   [rcx+28],xmm0
       vmovups   [rcx+38],xmm1
       vmovups   [rcx+48],xmm2
       vmovups   [rcx+58],xmm3
M01_L05:
       mov       r8,0C410AD8FFCF7
       cmp       [rbp],r8
       je        short M01_L06
       call      CORINFO_HELP_FAIL_FAST
M01_L06:
       nop
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
       jge       short M01_L03
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
       cmp       esi,2
       jl        short M01_L09
       mov       ebx,edi
       jmp       near ptr M01_L03
M01_L13:
       mov       rcx,offset MT_System.ArgumentException
       call      CORINFO_HELP_NEWSFAST
       mov       rbx,rax
       mov       ecx,0B5
       mov       rdx,7FF91F919F90
       call      qword ptr [7FF91F777798]
       mov       rsi,rax
       mov       ecx,5B
       mov       rdx,7FF91F919F90
       call      qword ptr [7FF91F777798]
       mov       r8,rax
       mov       rdx,rsi
       mov       rcx,rbx
       call      qword ptr [7FF91F8B5E48]
       mov       rcx,rbx
       call      CORINFO_HELP_THROW
       int       3
; Total bytes of code 468
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
       call      qword ptr [7FF91F956358]; Anastasya.Metaheuristics.Core.Randomness.StandardNormal.Fill(Anastasya.Metaheuristics.Core.Randomness.RandomSource, System.Span`1<Double>)
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F956370]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
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
       sub       rsp,0C0
       vmovaps   [rsp+0B0],xmm6
       vmovaps   [rsp+0A0],xmm7
       vmovaps   [rsp+90],xmm8
       vmovaps   [rsp+80],xmm9
       lea       rbp,[rsp+20]
       xor       eax,eax
       mov       [rbp+10],rax
       mov       [rbp+18],rax
       mov       rax,711E2F797CFA
       mov       [rbp+8],rax
       mov       rbx,rcx
       mov       rsi,[rdx]
       mov       edi,[rdx+8]
       test      rbx,rbx
       je        near ptr M01_L10
       test      edi,edi
       je        near ptr M01_L08
       test      [rsp],esp
       sub       rsp,10
       lea       rdx,[rsp+20]
       vxorps    xmm0,xmm0,xmm0
       vmovups   [rdx],xmm0
       mov       r14,rdx
       test      [rsp],esp
       sub       rsp,10
       lea       rdx,[rsp+20]
       vmovups   [rdx],xmm0
       mov       r15,rdx
       test      [rsp],esp
       sub       rsp,10
       lea       rdx,[rsp+20]
       vmovups   [rdx],xmm0
       test      [rsp],esp
       sub       rsp,10
       lea       rcx,[rsp+20]
       vmovups   [rcx],xmm0
       mov       r13,rcx
       xor       ecx,ecx
       jmp       short M01_L02
       nop       dword ptr [rax]
M01_L00:
       xor       r8d,r8d
M01_L01:
       mov       [rax],r8
       inc       ecx
       cmp       ecx,2
       jge       short M01_L03
M01_L02:
       lea       rax,[rdx+rcx*8]
       test      cl,1
       jne       short M01_L00
       mov       r8,0FFFFFFFFFFFFFFFF
       jmp       short M01_L01
M01_L03:
       vmovups   xmm6,[rdx]
       vmovddup  xmm7,qword ptr [7FF91F5D87C0]
       vmovddup  xmm8,qword ptr [7FF91F5D87C8]
       test      edi,edi
       jle       near ptr M01_L08
M01_L04:
       vmovups   xmm0,[rbx+28]
       vmovups   xmm1,[rbx+38]
       vmovups   xmm2,[rbx+48]
       vmovups   xmm3,[rbx+58]
       vpaddq    xmm4,xmm0,xmm3
       vprolq    xmm4,xmm4,17
       vpsllq    xmm5,xmm1,11
       vpxor     xmm2,xmm0,xmm2
       vpxor     xmm3,xmm1,xmm3
       vpxor     xmm1,xmm2,xmm1
       vpaddq    xmm4,xmm0,xmm4
       vpxor     xmm0,xmm0,xmm3
       vpxor     xmm2,xmm2,xmm5
       vprolq    xmm3,xmm3,2D
       vmovups   [rbx+28],xmm0
       vmovups   [rbx+38],xmm1
       vmovups   [rbx+48],xmm2
       vmovups   [rbx+58],xmm3
       vpsrlq    xmm0,xmm4,0B
       vcvtuqq2pd xmm0,xmm0
       vmulpd    xmm0,xmm0,qword bcst [7FF91F5D87D0]
       xor       edx,edx
M01_L05:
       lea       rcx,[rdx*8]
       lea       rax,[r14+rcx]
       mov       r8d,edx
       and       r8d,0FFFFFFFE
       cmp       r8d,2
       jae       near ptr M01_L11
       vmovaps   [rbp+10],xmm0
       vmovsd    xmm1,qword ptr [rbp+r8*8+10]
       vmovsd    qword ptr [rax],xmm1
       add       rcx,r15
       mov       eax,edx
       or        eax,1
       cmp       eax,2
       jae       near ptr M01_L11
       vmovaps   [rbp+10],xmm0
       vmovsd    xmm1,qword ptr [rbp+rax*8+10]
       vmovsd    qword ptr [rcx],xmm1
       inc       edx
       cmp       edx,2
       jl        short M01_L05
       vmovddup  xmm0,qword ptr [7FF91F5D87D8]
       vsubpd    xmm0,xmm0,[r14]
       vmovaps   [rbp+20],xmm0
       lea       rdx,[rbp+20]
       lea       rcx,[rbp+50]
       call      qword ptr [7FF91F956760]; System.Runtime.Intrinsics.VectorMath.LogDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.UInt64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovaps   xmm9,[rbp+50]
       vmulpd    xmm0,xmm8,[r15]
       vmovaps   [rbp+20],xmm0
       lea       rdx,[rbp+20]
       lea       rcx,[rbp+30]
       call      qword ptr [7FF91F956790]; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   xmm0,[rbp+30]
       vmovups   xmm1,[rbp+40]
       vpternlogq xmm1,xmm0,xmm6,0E4
       vmulpd    xmm0,xmm9,xmm7
       vsqrtpd   xmm0,xmm0
       vmulpd    xmm0,xmm1,xmm0
       cmp       edi,2
       jl        short M01_L06
       vmovups   [rsi],xmm0
       add       rsi,10
       sub       edi,2
       test      edi,edi
       jg        near ptr M01_L04
       jmp       short M01_L08
M01_L06:
       vmovups   [r13],xmm0
       xor       ecx,ecx
       nop       dword ptr [rax]
M01_L07:
       movsxd    rdx,ecx
       vmovsd    xmm0,qword ptr [r13+rdx*8]
       vmovsd    qword ptr [rsi+rdx*8],xmm0
       inc       ecx
       cmp       ecx,edi
       jl        short M01_L07
M01_L08:
       mov       r8,711E2F797CFA
       cmp       [rbp+8],r8
       je        short M01_L09
       call      CORINFO_HELP_FAIL_FAST
M01_L09:
       nop
       vmovaps   xmm6,[rbp+90]
       vmovaps   xmm7,[rbp+80]
       vmovaps   xmm8,[rbp+70]
       vmovaps   xmm9,[rbp+60]
       lea       rsp,[rbp+0A0]
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
       mov       rdx,7FF91F929F90
       call      qword ptr [7FF91F787798]
       mov       rcx,rax
       call      qword ptr [7FF91F956850]
       int       3
M01_L11:
       call      CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION
       int       3
; Total bytes of code 698
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
       vmovups   xmm0,[rdx]
       vmovaps   xmm1,xmm0
       vpaddq    xmm2,xmm0,qword bcst [7FF91F5DC6B0]
       vpcmpnltuq k1,xmm2,qword bcst [7FF91F5DC6B8]
       vpmovm2q  xmm2,k1
       vptest    xmm2,xmm2
       jne       near ptr M03_L01
M03_L00:
       vmovups   xmm0,[rdx]
       vpaddq    xmm0,xmm0,qword bcst [7FF91F5DC6C0]
       vpsraq    xmm3,xmm0,34
       vcvtqq2pd xmm3,xmm3
       vpandq    xmm0,xmm0,qword bcst [7FF91F5DC6C8]
       vpaddq    xmm0,xmm0,qword bcst [7FF91F5DC6D0]
       vsubpd    xmm0,xmm0,qword bcst [7FF91F5DC6D8]
       vmulpd    xmm4,xmm0,xmm0
       vmulpd    xmm5,xmm4,xmm4
       vmulpd    xmm16,xmm5,xmm5
       vmulpd    xmm17,xmm16,xmm16
       vmovddup  xmm18,qword ptr [7FF91F5DC6E0]
       vfmadd213pd xmm18,xmm0,qword bcst [7FF91F5DC6E8]
       vmovddup  xmm19,qword ptr [7FF91F5DC6F0]
       vfmadd213pd xmm19,xmm0,qword bcst [7FF91F5DC6F8]
       vfmadd213pd xmm18,xmm4,xmm19
       vfmadd231pd xmm18,xmm5,qword bcst [7FF91F5DC700]
       vmovddup  xmm19,qword ptr [7FF91F5DC708]
       vfmadd213pd xmm19,xmm0,qword bcst [7FF91F5DC710]
       vmovddup  xmm20,qword ptr [7FF91F5DC718]
       vfmadd213pd xmm20,xmm0,qword bcst [7FF91F5DC720]
       vfmadd213pd xmm19,xmm4,xmm20
       vmovddup  xmm20,qword ptr [7FF91F5DC728]
       vfmadd213pd xmm20,xmm0,qword bcst [7FF91F5DC730]
       vmovddup  xmm21,qword ptr [7FF91F5DC738]
       vfmadd213pd xmm21,xmm0,qword bcst [7FF91F5DC740]
       vfmadd213pd xmm20,xmm4,xmm21
       vfmadd231pd xmm20,xmm19,xmm5
       vmovddup  xmm19,qword ptr [7FF91F5DC748]
       vfmadd213pd xmm19,xmm0,qword bcst [7FF91F5DC750]
       vmovddup  xmm21,qword ptr [7FF91F5DC758]
       vfmadd213pd xmm21,xmm0,qword bcst [7FF91F5DC760]
       vfmadd213pd xmm19,xmm4,xmm21
       vmovddup  xmm21,qword ptr [7FF91F5DC768]
       vfmadd213pd xmm21,xmm0,qword bcst [7FF91F5DC770]
       vfmadd213pd xmm4,xmm21,xmm0
       vfmadd213pd xmm5,xmm19,xmm4
       vfmadd213pd xmm16,xmm20,xmm5
       vfmadd213pd xmm17,xmm18,xmm16
       vmovaps   xmm0,xmm3
       vfmadd132pd xmm0,xmm17,qword bcst [7FF91F5DC778]
       vfmadd132pd xmm3,xmm0,qword bcst [7FF91F5DC780]
       vpternlogq xmm2,xmm3,xmm1,0AC
       vmovups   [rcx],xmm2
       mov       rax,rcx
       ret
M03_L01:
       vxorps    xmm3,xmm3,xmm3
       vpcmpgtq  xmm3,xmm3,xmm0
       vpternlogq xmm1,xmm3,qword bcst [7FF91F5DC788],0B8
       vxorps    xmm4,xmm4,xmm4
       vcmpeqpd  xmm4,xmm4,xmm0
       vpternlogq xmm1,xmm4,qword bcst [7FF91F5DC6B0],0B8
       vcmpneqpd xmm5,xmm0,xmm0
       vpternlogq xmm4,xmm5,xmm3,0FE
       vpcmpeqq  xmm3,xmm0,[7FF91F5DC790]
       vorpd     xmm3,xmm3,xmm4
       vandnpd   xmm2,xmm3,xmm2
       vmulpd    xmm4,xmm0,qword bcst [7FF91F5DC7A0]
       vpaddq    xmm4,xmm4,qword bcst [7FF91F5DC7A8]
       vpternlogq xmm2,xmm4,xmm0,0CA
       vmovups   [rdx],xmm2
       vmovaps   xmm2,xmm3
       jmp       near ptr M03_L00
; Total bytes of code 515
```
```assembly
; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       sub       rsp,58
       vmovaps   [rsp+40],xmm6
       vmovaps   [rsp+30],xmm7
       vmovaps   [rsp+20],xmm8
       vmovaps   [rsp+10],xmm9
       vmovaps   [rsp],xmm10
       vmovups   xmm6,[rdx]
       vandpd    xmm7,xmm6,qword bcst [7FF91F5DCB80]
       vmovddup  xmm0,qword ptr [7FF91F5DCB88]
       vpcmpgtq  xmm0,xmm0,xmm7
       vpcmpeqd  xmm1,xmm1,xmm1
       vptest    xmm0,xmm1
       jb        near ptr M04_L01
       vmovddup  xmm0,qword ptr [7FF91F5DCB90]
       vpcmpgtq  xmm0,xmm0,xmm7
       vpcmpeqd  xmm1,xmm1,xmm1
       vptest    xmm0,xmm1
       jae       near ptr M04_L03
       vmovddup  xmm0,qword ptr [7FF91F5DCB98]
       vmovaps   xmm1,xmm0
       vfmadd231pd xmm1,xmm7,qword bcst [7FF91F5DCBA0]
       vsubpd    xmm0,xmm1,xmm0
       vmovaps   xmm2,xmm7
       vfmadd231pd xmm2,xmm0,qword bcst [7FF91F5DCBA8]
       vmulpd    xmm3,xmm0,qword bcst [7FF91F5DCBB0]
       vsubpd    xmm4,xmm2,xmm3
       vsubpd    xmm2,xmm2,xmm4
       vsubpd    xmm3,xmm2,xmm3
       vmovddup  xmm2,qword ptr [7FF91F5DCBB8]
       vxorpd    xmm3,xmm3,xmm2
       vfmadd132pd xmm0,xmm3,qword bcst [7FF91F5DCBC0]
       vmovaps   xmm3,xmm0
       vsubpd    xmm0,xmm4,xmm3
       vsubpd    xmm4,xmm4,xmm0
       vsubpd    xmm3,xmm4,xmm3
       vmulpd    xmm4,xmm0,xmm0
       vmovaps   xmm5,xmm4
       vmulpd    xmm16,xmm0,xmm5
       vmulpd    xmm17,xmm5,xmm5
       vmovaps   xmm18,xmm17
       vmovddup  xmm19,qword ptr [7FF91F5DCBC8]
       vmulpd    xmm20,xmm18,xmm18
       vmovddup  xmm21,qword ptr [7FF91F5DCBD0]
       vfmadd213pd xmm21,xmm5,qword bcst [7FF91F5DCBD8]
       vmovddup  xmm22,qword ptr [7FF91F5DCBE0]
       vfmadd213pd xmm22,xmm5,qword bcst [7FF91F5DCBE8]
       vfmadd213pd xmm18,xmm21,xmm22
       vfmadd231pd xmm18,xmm20,qword bcst [7FF91F5DCBF0]
       vmulpd    xmm18,xmm18,xmm16
       vxorpd    xmm18,xmm18,xmm2
       vfmadd231pd xmm18,xmm19,xmm3
       vxorpd    xmm21,xmm2,xmm3
       vfmadd213pd xmm5,xmm18,xmm21
       vfmadd231pd xmm5,xmm16,qword bcst [7FF91F5DCBF8]
       vsubpd    xmm5,xmm0,xmm5
       vmovaps   xmm16,xmm4
       vmulpd    xmm16,xmm19,xmm16
       vmovddup  xmm8,qword ptr [7FF91F5DCC00]
       vsubpd    xmm18,xmm16,xmm8
       vmovddup  xmm19,qword ptr [7FF91F5DCC08]
       vfmadd213pd xmm19,xmm4,qword bcst [7FF91F5DCC10]
       vmovddup  xmm21,qword ptr [7FF91F5DCC18]
       vfmadd213pd xmm21,xmm4,qword bcst [7FF91F5DCC20]
       vmovddup  xmm22,qword ptr [7FF91F5DCC28]
       vfmadd213pd xmm4,xmm22,qword bcst [7FF91F5DCC30]
       vfmadd231pd xmm4,xmm17,xmm21
       vfmadd213pd xmm19,xmm20,xmm4
       vaddpd    xmm4,xmm8,xmm18
       vsubpd    xmm4,xmm4,xmm16
       vfmadd213pd xmm0,xmm3,xmm4
       vfmadd213pd xmm19,xmm17,xmm0
       vsubpd    xmm0,xmm19,xmm18
       vmovddup  xmm3,qword ptr [7FF91F5DCC38]
       vpand     xmm4,xmm3,xmm1
       vptestnmq k1,xmm4,xmm4
       vpblendmq xmm9{k1},xmm0,xmm5
       vpblendmq xmm10{k1},xmm5,xmm0
       vpsrlq    xmm0,xmm6,3F
       vpsrlq    xmm4,xmm1,1
       vpternlogq xmm5,xmm4,xmm0,11
       vpternlogq xmm5,xmm4,xmm0,0F8
       vpand     xmm0,xmm5,xmm3
       vxorps    xmm4,xmm4,xmm4
       vpcmpeqq  xmm0,xmm4,xmm0
       vxorpd    xmm4,xmm2,xmm9
       vblendvpd xmm9,xmm9,xmm4,xmm0
       vpaddq    xmm0,xmm3,xmm1
       vpandq    xmm0,xmm0,qword bcst [7FF91F5DCC40]
       vxorps    xmm1,xmm1,xmm1
       vpcmpeqq  xmm0,xmm1,xmm0
       vxorpd    xmm1,xmm2,xmm10
       vblendvpd xmm10,xmm1,xmm10,xmm0
M04_L00:
       vpcmpgtq  k1,xmm7,qword bcst [7FF91F5DCC48]
       vpblendmq xmm9{k1},xmm6,xmm9
       vpblendmq xmm10{k1},xmm8,xmm10
       vmovups   [rcx],xmm9
       vmovups   [rcx+10],xmm10
       mov       rax,rcx
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       ret
M04_L01:
       vmulpd    xmm0,xmm6,xmm6
       vmovaps   xmm10,xmm0
       vpcmpgtq  xmm1,xmm7,[7FF91F5DCC50]
       vptest    xmm1,xmm1
       je        near ptr M04_L02
       vmovaps   xmm1,xmm0
       vmulpd    xmm9,xmm6,xmm1
       vmulpd    xmm2,xmm1,xmm1
       vmovaps   xmm3,xmm2
       vmovddup  xmm4,qword ptr [7FF91F5DCBF0]
       vfmadd213pd xmm4,xmm1,qword bcst [7FF91F5DCBD0]
       vmulpd    xmm5,xmm3,xmm3
       vmovddup  xmm16,qword ptr [7FF91F5DCBD8]
       vfmadd213pd xmm16,xmm1,qword bcst [7FF91F5DCBE0]
       vmovddup  xmm17,qword ptr [7FF91F5DCBE8]
       vfmadd213pd xmm1,xmm17,qword bcst [7FF91F5DCC60]
       vfmadd213pd xmm3,xmm16,xmm1
       vfmadd213pd xmm4,xmm5,xmm3
       vfmadd213pd xmm9,xmm4,xmm6
       vmovddup  xmm1,qword ptr [7FF91F5DCC08]
       vfmadd213pd xmm1,xmm0,qword bcst [7FF91F5DCC10]
       vmovddup  xmm3,qword ptr [7FF91F5DCC18]
       vfmadd213pd xmm3,xmm0,qword bcst [7FF91F5DCC20]
       vmovddup  xmm4,qword ptr [7FF91F5DCC28]
       vfmadd213pd xmm0,xmm4,qword bcst [7FF91F5DCC30]
       vfmadd213pd xmm2,xmm3,xmm0
       vfmadd213pd xmm1,xmm5,xmm2
       vfmadd213pd xmm1,xmm10,qword bcst [7FF91F5DCC68]
       vmovddup  xmm8,qword ptr [7FF91F5DCC00]
       vfmadd213pd xmm10,xmm1,xmm8
       jmp       near ptr M04_L00
M04_L02:
       vmulpd    xmm0,xmm6,xmm10
       vmovaps   xmm9,xmm6
       vfmadd231pd xmm9,xmm0,qword bcst [7FF91F5DCC60]
       vmovddup  xmm8,qword ptr [7FF91F5DCC00]
       vfmadd132pd xmm10,xmm8,qword bcst [7FF91F5DCC68]
       jmp       near ptr M04_L00
M04_L03:
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       jmp       qword ptr [7FF91F95CE10]
; Total bytes of code 947
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.VectorApi()
       mov       rax,[rcx+10]
       vmovups   xmm0,[rax+28]
       vmovups   xmm1,[rax+38]
       vmovups   xmm2,[rax+48]
       vmovups   xmm3,[rax+58]
       vpaddq    xmm4,xmm0,xmm3
       vprolq    xmm4,xmm4,17
       vpsllq    xmm5,xmm1,11
       vpxor     xmm2,xmm0,xmm2
       vpxor     xmm3,xmm1,xmm3
       vpxor     xmm1,xmm2,xmm1
       vpaddq    xmm4,xmm0,xmm4
       vpxor     xmm0,xmm0,xmm3
       vpxor     xmm2,xmm2,xmm5
       vprolq    xmm3,xmm3,2D
       vmovups   [rax+28],xmm0
       vmovups   [rax+38],xmm1
       vmovups   [rax+48],xmm2
       vmovups   [rax+58],xmm3
       vmovq     rax,xmm4
       ret
; Total bytes of code 97
```

