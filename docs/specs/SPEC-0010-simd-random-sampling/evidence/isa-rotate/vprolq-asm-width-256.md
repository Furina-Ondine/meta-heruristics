## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.RawFill()
       push      rsi
       push      rbx
       sub       rsp,0C8
       vmovaps   [rsp+0B0],xmm6
       vmovaps   [rsp+0A0],xmm7
       vmovaps   [rsp+90],xmm8
       vmovaps   [rsp+80],xmm9
       vmovaps   [rsp+70],xmm10
       vmovaps   [rsp+60],xmm11
       vmovaps   [rsp+50],xmm12
       vmovaps   [rsp+40],xmm13
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
       shr       edx,2
       lea       eax,[rdx*4]
       sub       r8d,eax
       vmovups   ymm6,[rsi+28]
       vmovups   ymm7,[rsi+48]
       vmovups   ymm8,[rsi+68]
       vmovups   ymm9,[rsi+88]
       xor       eax,eax
       jmp       near ptr M00_L02
M00_L01:
       vpaddq    ymm0,ymm6,ymm9
       vprolq    ymm0,ymm0,17
       vpsllq    ymm1,ymm7,11
       vpxor     ymm2,ymm6,ymm8
       vpxor     ymm3,ymm7,ymm9
       vpxor     ymm4,ymm7,ymm2
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm5,ymm6,ymm3
       vpxor     ymm2,ymm2,ymm1
       vprolq    ymm3,ymm3,2D
       vpaddq    ymm1,ymm3,ymm5
       vprolq    ymm1,ymm1,17
       vpsllq    ymm16,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm1,ymm5,ymm1
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm16
       vprolq    ymm3,ymm3,2D
       vpaddq    ymm16,ymm3,ymm5
       vprolq    ymm16,ymm16,17
       vpsllq    ymm17,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm16,ymm5,ymm16
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm17
       vprolq    ymm3,ymm3,2D
       vpaddq    ymm17,ymm3,ymm5
       vprolq    ymm17,ymm17,17
       vpsllq    ymm18,ymm4,11
       vpxor     ymm8,ymm2,ymm5
       vpxor     ymm9,ymm3,ymm4
       vpxor     ymm7,ymm4,ymm8
       vpaddq    ymm2,ymm5,ymm17
       vpxor     ymm6,ymm5,ymm9
       vpxord    ymm8,ymm8,ymm18
       vprolq    ymm9,ymm9,2D
       vmovups   [rcx],ymm0
       vmovups   [rcx+20],ymm1
       vmovups   [rcx+40],ymm16
       vmovups   [rcx+60],ymm2
       add       rcx,80
       mov       eax,r10d
M00_L02:
       lea       r10d,[rax+4]
       cmp       r10d,edx
       jle       near ptr M00_L01
M00_L03:
       cmp       eax,edx
       jl        near ptr M00_L07
       test      r8d,r8d
       jne       near ptr M00_L08
M00_L04:
       vmovups   [rsi+28],ymm6
       vmovups   [rsi+48],ymm7
       vmovups   [rsi+68],ymm8
       vmovups   [rsi+88],ymm9
M00_L05:
       mov       rcx,[rbx+20]
       call      qword ptr [7FF91F976580]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(UInt64[])
       nop
       vzeroupper
       vmovaps   xmm6,[rsp+0B0]
       vmovaps   xmm7,[rsp+0A0]
       vmovaps   xmm8,[rsp+90]
       vmovaps   xmm9,[rsp+80]
       vmovaps   xmm10,[rsp+70]
       vmovaps   xmm11,[rsp+60]
       vmovaps   xmm12,[rsp+50]
       vmovaps   xmm13,[rsp+40]
       add       rsp,0C8
       pop       rbx
       pop       rsi
       ret
M00_L06:
       lea       rcx,[rdx+10]
       mov       r8d,[rdx+8]
       jmp       near ptr M00_L00
M00_L07:
       vpaddq    ymm0,ymm6,ymm9
       vprolq    ymm0,ymm0,17
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vprolq    ymm9,ymm9,2D
       vmovups   [rcx],ymm0
       add       rcx,20
       inc       eax
       jmp       near ptr M00_L03
M00_L08:
       vpaddq    ymm0,ymm6,ymm9
       vprolq    ymm0,ymm0,17
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vmovups   [rsp+20],ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vprolq    ymm9,ymm9,2D
       lea       rdx,[rsp+20]
       vextractf128 xmm10,ymm6,1
       vextractf128 xmm11,ymm7,1
       vextractf128 xmm12,ymm9,1
       vextractf128 xmm13,ymm8,1
       call      qword ptr [7FF91F976628]
       vinsertf128 ymm6,ymm6,xmm10,1
       vinsertf128 ymm7,ymm7,xmm11,1
       vinsertf128 ymm9,ymm9,xmm12,1
       vinsertf128 ymm8,ymm8,xmm13,1
       jmp       near ptr M00_L04
; Total bytes of code 727
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
       sub       rsp,0C8
       vmovaps   [rsp+0B0],xmm6
       vmovaps   [rsp+0A0],xmm7
       vmovaps   [rsp+90],xmm8
       vmovaps   [rsp+80],xmm9
       vmovaps   [rsp+70],xmm10
       vmovaps   [rsp+60],xmm11
       vmovaps   [rsp+50],xmm12
       vmovaps   [rsp+40],xmm13
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
       shr       edx,2
       lea       eax,[rdx*4]
       sub       r8d,eax
       vmovups   ymm6,[rsi+28]
       vmovups   ymm7,[rsi+48]
       vmovups   ymm8,[rsi+68]
       vmovups   ymm9,[rsi+88]
       xor       eax,eax
M00_L01:
       lea       r10d,[rax+4]
       cmp       r10d,edx
       jg        near ptr M00_L03
       vpaddq    ymm0,ymm6,ymm9
       vprolq    ymm0,ymm0,17
       vpsllq    ymm1,ymm7,11
       vpxor     ymm2,ymm6,ymm8
       vpxor     ymm3,ymm7,ymm9
       vpxor     ymm4,ymm7,ymm2
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm5,ymm6,ymm3
       vpxor     ymm2,ymm2,ymm1
       vprolq    ymm3,ymm3,2D
       vpsrlq    ymm0,ymm0,0B
       vcvtuqq2pd ymm0,ymm0
       vbroadcastsd ymm1,qword ptr [7FF91F5CD198]
       vmulpd    ymm0,ymm0,ymm1
       vpaddq    ymm16,ymm3,ymm5
       vprolq    ymm16,ymm16,17
       vpsllq    ymm17,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm16,ymm5,ymm16
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm17
       vprolq    ymm3,ymm3,2D
       vpsrlq    ymm16,ymm16,0B
       vcvtuqq2pd ymm16,ymm16
       vmulpd    ymm16,ymm16,ymm1
       vpaddq    ymm17,ymm3,ymm5
       vprolq    ymm17,ymm17,17
       vpsllq    ymm18,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm17,ymm5,ymm17
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm18
       vprolq    ymm3,ymm3,2D
       vpsrlq    ymm17,ymm17,0B
       vcvtuqq2pd ymm17,ymm17
       vmulpd    ymm17,ymm17,ymm1
       vpaddq    ymm18,ymm3,ymm5
       vprolq    ymm18,ymm18,17
       vpsllq    ymm19,ymm4,11
       vpxor     ymm8,ymm2,ymm5
       vpxor     ymm9,ymm3,ymm4
       vpxor     ymm7,ymm4,ymm8
       vpaddq    ymm2,ymm5,ymm18
       vpxor     ymm6,ymm5,ymm9
       vpxord    ymm8,ymm8,ymm19
       vprolq    ymm9,ymm9,2D
       vpsrlq    ymm2,ymm2,0B
       vcvtuqq2pd ymm2,ymm2
       vmulpd    ymm1,ymm2,ymm1
       vmovups   [rcx],ymm0
       vmovups   [rcx+20],ymm16
       vmovups   [rcx+40],ymm17
       vmovups   [rcx+60],ymm1
       add       rcx,80
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
       vmovups   [rsi+28],ymm6
       vmovups   [rsi+48],ymm7
       vmovups   [rsi+68],ymm8
       vmovups   [rsi+88],ymm9
M00_L05:
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F946490]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
       nop
       vzeroupper
       vmovaps   xmm6,[rsp+0B0]
       vmovaps   xmm7,[rsp+0A0]
       vmovaps   xmm8,[rsp+90]
       vmovaps   xmm9,[rsp+80]
       vmovaps   xmm10,[rsp+70]
       vmovaps   xmm11,[rsp+60]
       vmovaps   xmm12,[rsp+50]
       vmovaps   xmm13,[rsp+40]
       add       rsp,0C8
       pop       rbx
       pop       rsi
       ret
M00_L06:
       vpaddq    ymm0,ymm6,ymm9
       vprolq    ymm0,ymm0,17
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vprolq    ymm9,ymm9,2D
       vpsrlq    ymm0,ymm0,0B
       vcvtuqq2pd ymm0,ymm0
       vbroadcastsd ymm1,qword ptr [7FF91F5CD198]
       vmulpd    ymm1,ymm0,ymm1
       vmovups   [rcx],ymm1
       add       rcx,20
       inc       eax
       jmp       near ptr M00_L03
M00_L07:
       vpaddq    ymm0,ymm6,ymm9
       vprolq    ymm0,ymm0,17
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vprolq    ymm9,ymm9,2D
       vpsrlq    ymm0,ymm0,0B
       vcvtuqq2pd ymm0,ymm0
       vbroadcastsd ymm1,qword ptr [7FF91F5CD198]
       vmulpd    ymm0,ymm0,ymm1
       vmovups   [rsp+20],ymm0
       lea       rdx,[rsp+20]
       vextractf128 xmm10,ymm6,1
       vextractf128 xmm11,ymm7,1
       vextractf128 xmm12,ymm9,1
       vextractf128 xmm13,ymm8,1
       call      qword ptr [7FF91F946598]
       vinsertf128 ymm6,ymm6,xmm10,1
       vinsertf128 ymm7,ymm7,xmm11,1
       vinsertf128 ymm9,ymm9,xmm12,1
       vinsertf128 ymm8,ymm8,xmm13,1
       jmp       near ptr M00_L04
; Total bytes of code 854
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
       call      qword ptr [7FF91F956460]; Anastasya.Metaheuristics.Core.Randomness.RandomSource.Fill(System.Span`1<Int32>, Int32, Int32)
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
       mov       rax,0B8CC1DE494C4
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
       vmovups   ymm0,[rcx+28]
       vmovups   ymm1,[rcx+48]
       vmovups   ymm2,[rcx+68]
       vmovups   ymm3,[rcx+88]
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
       sub       rsp,20
       lea       rax,[rsp+20]
       vxorps    ymm4,ymm4,ymm4
       vmovdqu   ymmword ptr [rax],ymm4
       xor       ebx,ebx
       cmp       ebx,r11d
       jge       short M01_L04
M01_L02:
       vpaddq    ymm4,ymm0,ymm3
       vprolq    ymm4,ymm4,17
       vpsllq    ymm5,ymm1,11
       vpxor     ymm2,ymm0,ymm2
       vpxor     ymm3,ymm1,ymm3
       vpxor     ymm1,ymm2,ymm1
       vpaddq    ymm4,ymm0,ymm4
       vpxor     ymm0,ymm0,ymm3
       vpxor     ymm2,ymm2,ymm5
       vprolq    ymm3,ymm3,2D
       vmovups   [rax],ymm4
       xor       esi,esi
       cmp       ebx,r11d
       jl        short M01_L10
M01_L03:
       cmp       ebx,r11d
       jl        short M01_L02
M01_L04:
       vmovups   [rcx+28],ymm0
       vmovups   [rcx+48],ymm1
       vmovups   [rcx+68],ymm2
       vmovups   [rcx+88],ymm3
M01_L05:
       mov       r8,0B8CC1DE494C4
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
       cmp       esi,4
       jl        short M01_L09
       mov       ebx,edi
       jmp       near ptr M01_L03
M01_L13:
       mov       rcx,offset MT_System.ArgumentException
       call      CORINFO_HELP_NEWSFAST
       mov       rbx,rax
       mov       ecx,0B5
       mov       rdx,7FF91F929FF8
       call      qword ptr [7FF91F787798]
       mov       rsi,rax
       mov       ecx,5B
       mov       rdx,7FF91F929FF8
       call      qword ptr [7FF91F787798]
       mov       r8,rax
       mov       rdx,rsi
       mov       rcx,rbx
       call      qword ptr [7FF91F8C5E48]
       mov       rcx,rbx
       call      CORINFO_HELP_THROW
       int       3
; Total bytes of code 477
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
       call      qword ptr [7FF91F956568]; Anastasya.Metaheuristics.Core.Randomness.StandardNormal.Fill(Anastasya.Metaheuristics.Core.Randomness.RandomSource, System.Span`1<Double>)
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F956580]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
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
       sub       rsp,150
       vmovaps   [rsp+140],xmm6
       vmovaps   [rsp+130],xmm7
       vmovaps   [rsp+120],xmm8
       vmovaps   [rsp+110],xmm9
       vmovaps   [rsp+100],xmm10
       vmovaps   [rsp+0F0],xmm11
       vmovaps   [rsp+0E0],xmm12
       vmovaps   [rsp+0D0],xmm13
       lea       rbp,[rsp+20]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp+10],ymm4
       mov       rax,0DA90CD7BB0FF
       mov       [rbp+8],rax
       mov       rbx,rcx
       mov       rsi,[rdx]
       mov       edi,[rdx+8]
       test      rbx,rbx
       je        near ptr M01_L10
       test      edi,edi
       je        near ptr M01_L08
       test      [rsp],esp
       sub       rsp,20
       lea       rdx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rdx],ymm0
       mov       r14,rdx
       test      [rsp],esp
       sub       rsp,20
       lea       rdx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rdx],ymm0
       mov       r15,rdx
       test      [rsp],esp
       sub       rsp,20
       lea       rdx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rdx],ymm0
       test      [rsp],esp
       sub       rsp,20
       lea       rcx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rcx],ymm0
       mov       r13,rcx
       xor       ecx,ecx
       jmp       short M01_L02
M01_L00:
       xor       r8d,r8d
M01_L01:
       mov       [rax],r8
       inc       ecx
       cmp       ecx,4
       jge       short M01_L03
M01_L02:
       lea       rax,[rdx+rcx*8]
       test      cl,1
       jne       short M01_L00
       mov       r8,0FFFFFFFFFFFFFFFF
       jmp       short M01_L01
M01_L03:
       vmovups   ymm6,[rdx]
       vbroadcastsd ymm7,qword ptr [7FF91F5D8CE8]
       vbroadcastsd ymm8,qword ptr [7FF91F5D8CF0]
       test      edi,edi
       jle       near ptr M01_L08
M01_L04:
       vmovups   ymm0,[rbx+28]
       vmovups   ymm1,[rbx+48]
       vmovups   ymm2,[rbx+68]
       vmovups   ymm3,[rbx+88]
       vpaddq    ymm4,ymm0,ymm3
       vprolq    ymm4,ymm4,17
       vpsllq    ymm5,ymm1,11
       vpxor     ymm2,ymm0,ymm2
       vpxor     ymm3,ymm1,ymm3
       vpxor     ymm1,ymm2,ymm1
       vpaddq    ymm4,ymm0,ymm4
       vpxor     ymm0,ymm0,ymm3
       vpxor     ymm2,ymm2,ymm5
       vprolq    ymm3,ymm3,2D
       vmovups   [rbx+28],ymm0
       vmovups   [rbx+48],ymm1
       vmovups   [rbx+68],ymm2
       vmovups   [rbx+88],ymm3
       vpsrlq    ymm0,ymm4,0B
       vcvtuqq2pd ymm0,ymm0
       vmulpd    ymm0,ymm0,qword bcst [7FF91F5D8CF8]
       xor       edx,edx
M01_L05:
       lea       rcx,[rdx*8]
       lea       rax,[r14+rcx]
       mov       r8d,edx
       and       r8d,0FFFFFFFE
       cmp       r8d,4
       jae       near ptr M01_L11
       vmovups   [rbp+10],ymm0
       vmovsd    xmm1,qword ptr [rbp+r8*8+10]
       vmovsd    qword ptr [rax],xmm1
       add       rcx,r15
       mov       eax,edx
       or        eax,1
       cmp       eax,4
       jae       near ptr M01_L11
       vmovups   [rbp+10],ymm0
       vmovsd    xmm1,qword ptr [rbp+rax*8+10]
       vmovsd    qword ptr [rcx],xmm1
       inc       edx
       cmp       edx,4
       jl        short M01_L05
       vbroadcastsd ymm0,qword ptr [7FF91F5D8D00]
       vsubpd    ymm0,ymm0,[r14]
       vmovups   [rbp+30],ymm0
       lea       rdx,[rbp+30]
       lea       rcx,[rbp+90]
       vextractf128 xmm9,ymm6,1
       vextractf128 xmm10,ymm7,1
       vextractf128 xmm11,ymm8,1
       call      qword ptr [7FF91F956928]; System.Runtime.Intrinsics.VectorMath.LogDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.UInt64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   ymm12,[rbp+90]
       vinsertf128 ymm8,ymm8,xmm11,1
       vmulpd    ymm0,ymm8,[r15]
       vmovups   [rbp+30],ymm0
       lea       rdx,[rbp+30]
       lea       rcx,[rbp+50]
       vextractf128 xmm13,ymm12,1
       vextractf128 xmm11,ymm8,1
       call      qword ptr [7FF91F956958]; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   ymm0,[rbp+50]
       vmovups   ymm1,[rbp+70]
       vinsertf128 ymm6,ymm6,xmm9,1
       vpternlogq ymm1,ymm0,ymm6,0E4
       vinsertf128 ymm12,ymm12,xmm13,1
       vinsertf128 ymm7,ymm7,xmm10,1
       vmulpd    ymm0,ymm12,ymm7
       vsqrtpd   ymm0,ymm0
       vmulpd    ymm0,ymm1,ymm0
       cmp       edi,4
       vinsertf128 ymm8,ymm8,xmm11,1
       jl        short M01_L06
       vmovups   [rsi],ymm0
       add       rsi,20
       sub       edi,4
       test      edi,edi
       jg        near ptr M01_L04
       jmp       short M01_L08
M01_L06:
       vmovups   [r13],ymm0
       xor       ecx,ecx
M01_L07:
       movsxd    rdx,ecx
       vmovsd    xmm0,qword ptr [r13+rdx*8]
       vmovsd    qword ptr [rsi+rdx*8],xmm0
       inc       ecx
       cmp       ecx,edi
       jl        short M01_L07
M01_L08:
       mov       r8,0DA90CD7BB0FF
       cmp       [rbp+8],r8
       je        short M01_L09
       call      CORINFO_HELP_FAIL_FAST
M01_L09:
       nop
       vzeroupper
       vmovaps   xmm6,[rbp+120]
       vmovaps   xmm7,[rbp+110]
       vmovaps   xmm8,[rbp+100]
       vmovaps   xmm9,[rbp+0F0]
       vmovaps   xmm10,[rbp+0E0]
       vmovaps   xmm11,[rbp+0D0]
       vmovaps   xmm12,[rbp+0C0]
       vmovaps   xmm13,[rbp+0B0]
       lea       rsp,[rbp+130]
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
       mov       rdx,7FF91F929FF8
       call      qword ptr [7FF91F787798]
       mov       rcx,rax
       call      qword ptr [7FF91F956A18]
       int       3
M01_L11:
       call      CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION
       int       3
; Total bytes of code 840
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
       vmovups   ymm0,[rdx]
       vmovaps   ymm1,ymm0
       vpaddq    ymm2,ymm0,qword bcst [7FF91F5DE840]
       vpcmpnltuq k1,ymm2,qword bcst [7FF91F5DE848]
       vpmovm2q  ymm2,k1
       vptest    ymm2,ymm2
       jne       near ptr M03_L01
M03_L00:
       vmovups   ymm0,[rdx]
       vpaddq    ymm0,ymm0,qword bcst [7FF91F5DE850]
       vpsraq    ymm3,ymm0,34
       vcvtqq2pd ymm3,ymm3
       vpandq    ymm0,ymm0,qword bcst [7FF91F5DE858]
       vpaddq    ymm0,ymm0,qword bcst [7FF91F5DE860]
       vsubpd    ymm0,ymm0,qword bcst [7FF91F5DE868]
       vmulpd    ymm4,ymm0,ymm0
       vmulpd    ymm5,ymm4,ymm4
       vmulpd    ymm16,ymm5,ymm5
       vmulpd    ymm17,ymm16,ymm16
       vbroadcastsd ymm18,qword ptr [7FF91F5DE870]
       vfmadd213pd ymm18,ymm0,qword bcst [7FF91F5DE878]
       vbroadcastsd ymm19,qword ptr [7FF91F5DE880]
       vfmadd213pd ymm19,ymm0,qword bcst [7FF91F5DE888]
       vfmadd213pd ymm18,ymm4,ymm19
       vfmadd231pd ymm18,ymm5,qword bcst [7FF91F5DE890]
       vbroadcastsd ymm19,qword ptr [7FF91F5DE898]
       vfmadd213pd ymm19,ymm0,qword bcst [7FF91F5DE8A0]
       vbroadcastsd ymm20,qword ptr [7FF91F5DE8A8]
       vfmadd213pd ymm20,ymm0,qword bcst [7FF91F5DE8B0]
       vfmadd213pd ymm19,ymm4,ymm20
       vbroadcastsd ymm20,qword ptr [7FF91F5DE8B8]
       vfmadd213pd ymm20,ymm0,qword bcst [7FF91F5DE8C0]
       vbroadcastsd ymm21,qword ptr [7FF91F5DE8C8]
       vfmadd213pd ymm21,ymm0,qword bcst [7FF91F5DE8D0]
       vfmadd213pd ymm20,ymm4,ymm21
       vfmadd231pd ymm20,ymm19,ymm5
       vbroadcastsd ymm19,qword ptr [7FF91F5DE8D8]
       vfmadd213pd ymm19,ymm0,qword bcst [7FF91F5DE8E0]
       vbroadcastsd ymm21,qword ptr [7FF91F5DE8E8]
       vfmadd213pd ymm21,ymm0,qword bcst [7FF91F5DE8F0]
       vfmadd213pd ymm19,ymm4,ymm21
       vbroadcastsd ymm21,qword ptr [7FF91F5DE8F8]
       vfmadd213pd ymm21,ymm0,qword bcst [7FF91F5DE900]
       vfmadd213pd ymm4,ymm21,ymm0
       vfmadd213pd ymm5,ymm19,ymm4
       vfmadd213pd ymm16,ymm20,ymm5
       vfmadd213pd ymm17,ymm18,ymm16
       vmovaps   ymm0,ymm3
       vfmadd132pd ymm0,ymm17,qword bcst [7FF91F5DE908]
       vfmadd132pd ymm3,ymm0,qword bcst [7FF91F5DE910]
       vpternlogq ymm2,ymm3,ymm1,0AC
       vmovups   [rcx],ymm2
       mov       rax,rcx
       vzeroupper
       ret
M03_L01:
       vxorps    ymm3,ymm3,ymm3
       vpcmpgtq  ymm3,ymm3,ymm0
       vpternlogq ymm1,ymm3,qword bcst [7FF91F5DE918],0B8
       vxorps    ymm4,ymm4,ymm4
       vcmpeqpd  ymm4,ymm4,ymm0
       vpternlogq ymm1,ymm4,qword bcst [7FF91F5DE840],0B8
       vcmpneqpd ymm5,ymm0,ymm0
       vpternlogq ymm4,ymm5,ymm3,0FE
       vpcmpeqq  ymm3,ymm0,[7FF91F5DE920]
       vorpd     ymm3,ymm3,ymm4
       vandnpd   ymm2,ymm3,ymm2
       vmulpd    ymm4,ymm0,qword bcst [7FF91F5DE940]
       vpaddq    ymm4,ymm4,qword bcst [7FF91F5DE948]
       vpternlogq ymm2,ymm4,ymm0,0CA
       vmovups   [rdx],ymm2
       vmovaps   ymm2,ymm3
       jmp       near ptr M03_L00
; Total bytes of code 518
```
```assembly
; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       sub       rsp,58
       vmovaps   [rsp+40],xmm6
       vmovaps   [rsp+30],xmm7
       vmovaps   [rsp+20],xmm8
       vmovaps   [rsp+10],xmm9
       vmovaps   [rsp],xmm10
       vmovups   ymm6,[rdx]
       vandpd    ymm7,ymm6,qword bcst [7FF91F5DEDA0]
       vbroadcastsd ymm0,qword ptr [7FF91F5DEDA8]
       vpcmpgtq  ymm0,ymm0,ymm7
       vpcmpeqd  ymm1,ymm1,ymm1
       vptest    ymm0,ymm1
       jb        near ptr M04_L01
       vbroadcastsd ymm0,qword ptr [7FF91F5DEDB0]
       vpcmpgtq  ymm0,ymm0,ymm7
       vpcmpeqd  ymm1,ymm1,ymm1
       vptest    ymm0,ymm1
       jae       near ptr M04_L03
       vbroadcastsd ymm0,qword ptr [7FF91F5DEDB8]
       vmovaps   ymm1,ymm0
       vfmadd231pd ymm1,ymm7,qword bcst [7FF91F5DEDC0]
       vsubpd    ymm0,ymm1,ymm0
       vmovaps   ymm2,ymm7
       vfmadd231pd ymm2,ymm0,qword bcst [7FF91F5DEDC8]
       vmulpd    ymm3,ymm0,qword bcst [7FF91F5DEDD0]
       vsubpd    ymm4,ymm2,ymm3
       vsubpd    ymm2,ymm2,ymm4
       vsubpd    ymm3,ymm2,ymm3
       vbroadcastsd ymm2,qword ptr [7FF91F5DEDD8]
       vxorpd    ymm3,ymm3,ymm2
       vfmadd132pd ymm0,ymm3,qword bcst [7FF91F5DEDE0]
       vmovaps   ymm3,ymm0
       vsubpd    ymm0,ymm4,ymm3
       vsubpd    ymm4,ymm4,ymm0
       vsubpd    ymm3,ymm4,ymm3
       vmulpd    ymm4,ymm0,ymm0
       vmovaps   ymm5,ymm4
       vmulpd    ymm16,ymm0,ymm5
       vmulpd    ymm17,ymm5,ymm5
       vmovaps   ymm18,ymm17
       vbroadcastsd ymm19,qword ptr [7FF91F5DEDE8]
       vmulpd    ymm20,ymm18,ymm18
       vbroadcastsd ymm21,qword ptr [7FF91F5DEDF0]
       vfmadd213pd ymm21,ymm5,qword bcst [7FF91F5DEDF8]
       vbroadcastsd ymm22,qword ptr [7FF91F5DEE00]
       vfmadd213pd ymm22,ymm5,qword bcst [7FF91F5DEE08]
       vfmadd213pd ymm18,ymm21,ymm22
       vfmadd231pd ymm18,ymm20,qword bcst [7FF91F5DEE10]
       vmulpd    ymm18,ymm18,ymm16
       vxorpd    ymm18,ymm18,ymm2
       vfmadd231pd ymm18,ymm19,ymm3
       vxorpd    ymm21,ymm2,ymm3
       vfmadd213pd ymm5,ymm18,ymm21
       vfmadd231pd ymm5,ymm16,qword bcst [7FF91F5DEE18]
       vsubpd    ymm5,ymm0,ymm5
       vmovaps   ymm16,ymm4
       vmulpd    ymm16,ymm19,ymm16
       vbroadcastsd ymm8,qword ptr [7FF91F5DEE20]
       vsubpd    ymm18,ymm16,ymm8
       vbroadcastsd ymm19,qword ptr [7FF91F5DEE28]
       vfmadd213pd ymm19,ymm4,qword bcst [7FF91F5DEE30]
       vbroadcastsd ymm21,qword ptr [7FF91F5DEE38]
       vfmadd213pd ymm21,ymm4,qword bcst [7FF91F5DEE40]
       vbroadcastsd ymm22,qword ptr [7FF91F5DEE48]
       vfmadd213pd ymm4,ymm22,qword bcst [7FF91F5DEE50]
       vfmadd231pd ymm4,ymm17,ymm21
       vfmadd213pd ymm19,ymm20,ymm4
       vaddpd    ymm4,ymm8,ymm18
       vsubpd    ymm4,ymm4,ymm16
       vfmadd213pd ymm0,ymm3,ymm4
       vfmadd213pd ymm19,ymm17,ymm0
       vsubpd    ymm0,ymm19,ymm18
       vbroadcastsd ymm3,qword ptr [7FF91F5DEE58]
       vpand     ymm4,ymm3,ymm1
       vptestnmq k1,ymm4,ymm4
       vpblendmq ymm9{k1},ymm0,ymm5
       vpblendmq ymm10{k1},ymm5,ymm0
       vpsrlq    ymm0,ymm6,3F
       vpsrlq    ymm4,ymm1,1
       vpternlogq ymm5,ymm4,ymm0,11
       vpternlogq ymm5,ymm4,ymm0,0F8
       vpand     ymm0,ymm5,ymm3
       vxorps    ymm4,ymm4,ymm4
       vpcmpeqq  ymm0,ymm4,ymm0
       vxorpd    ymm4,ymm2,ymm9
       vblendvpd ymm9,ymm9,ymm4,ymm0
       vpaddq    ymm0,ymm3,ymm1
       vpandq    ymm0,ymm0,qword bcst [7FF91F5DEE60]
       vxorps    ymm1,ymm1,ymm1
       vpcmpeqq  ymm0,ymm1,ymm0
       vxorpd    ymm1,ymm2,ymm10
       vblendvpd ymm10,ymm1,ymm10,ymm0
M04_L00:
       vpcmpgtq  k1,ymm7,qword bcst [7FF91F5DEE68]
       vpblendmq ymm9{k1},ymm6,ymm9
       vpblendmq ymm10{k1},ymm8,ymm10
       vmovups   [rcx],ymm9
       vmovups   [rcx+20],ymm10
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
       vmulpd    ymm0,ymm6,ymm6
       vmovaps   ymm10,ymm0
       vpcmpgtq  ymm1,ymm7,[7FF91F5DEE80]
       vptest    ymm1,ymm1
       je        near ptr M04_L02
       vmovaps   ymm1,ymm0
       vmulpd    ymm9,ymm6,ymm1
       vmulpd    ymm2,ymm1,ymm1
       vmovaps   ymm3,ymm2
       vbroadcastsd ymm4,qword ptr [7FF91F5DEE10]
       vfmadd213pd ymm4,ymm1,qword bcst [7FF91F5DEDF0]
       vmulpd    ymm5,ymm3,ymm3
       vbroadcastsd ymm16,qword ptr [7FF91F5DEDF8]
       vfmadd213pd ymm16,ymm1,qword bcst [7FF91F5DEE00]
       vbroadcastsd ymm17,qword ptr [7FF91F5DEE08]
       vfmadd213pd ymm1,ymm17,qword bcst [7FF91F5DEEA0]
       vfmadd213pd ymm3,ymm16,ymm1
       vfmadd213pd ymm4,ymm5,ymm3
       vfmadd213pd ymm9,ymm4,ymm6
       vbroadcastsd ymm1,qword ptr [7FF91F5DEE28]
       vfmadd213pd ymm1,ymm0,qword bcst [7FF91F5DEE30]
       vbroadcastsd ymm3,qword ptr [7FF91F5DEE38]
       vfmadd213pd ymm3,ymm0,qword bcst [7FF91F5DEE40]
       vbroadcastsd ymm4,qword ptr [7FF91F5DEE48]
       vfmadd213pd ymm0,ymm4,qword bcst [7FF91F5DEE50]
       vfmadd213pd ymm2,ymm3,ymm0
       vfmadd213pd ymm1,ymm5,ymm2
       vfmadd213pd ymm1,ymm10,qword bcst [7FF91F5DEEA8]
       vbroadcastsd ymm8,qword ptr [7FF91F5DEE20]
       vfmadd213pd ymm10,ymm1,ymm8
       jmp       near ptr M04_L00
M04_L02:
       vmulpd    ymm0,ymm6,ymm10
       vmovaps   ymm9,ymm6
       vfmadd231pd ymm9,ymm0,qword bcst [7FF91F5DEEA0]
       vbroadcastsd ymm8,qword ptr [7FF91F5DEE20]
       vfmadd132pd ymm10,ymm8,qword bcst [7FF91F5DEEA8]
       jmp       near ptr M04_L00
M04_L03:
       vzeroupper
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       jmp       qword ptr [7FF91F956B80]
; Total bytes of code 965
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.VectorApi()
       mov       rax,[rcx+10]
       vmovups   ymm0,[rax+28]
       vmovups   ymm1,[rax+48]
       vmovups   ymm2,[rax+68]
       vmovups   ymm3,[rax+88]
       vpaddq    ymm4,ymm0,ymm3
       vprolq    ymm4,ymm4,17
       vpsllq    ymm5,ymm1,11
       vpxor     ymm2,ymm0,ymm2
       vpxor     ymm3,ymm1,ymm3
       vpxor     ymm1,ymm2,ymm1
       vpaddq    ymm4,ymm0,ymm4
       vpxor     ymm0,ymm0,ymm3
       vpxor     ymm2,ymm2,ymm5
       vprolq    ymm3,ymm3,2D
       vmovups   [rax+28],ymm0
       vmovups   [rax+48],ymm1
       vmovups   [rax+68],ymm2
       vmovups   [rax+88],ymm3
       vmovq     rax,xmm4
       vzeroupper
       ret
; Total bytes of code 106
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.RawFill()
       push      rsi
       push      rbx
       sub       rsp,0C8
       vmovaps   [rsp+0B0],xmm6
       vmovaps   [rsp+0A0],xmm7
       vmovaps   [rsp+90],xmm8
       vmovaps   [rsp+80],xmm9
       vmovaps   [rsp+70],xmm10
       vmovaps   [rsp+60],xmm11
       vmovaps   [rsp+50],xmm12
       vmovaps   [rsp+40],xmm13
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
       shr       edx,2
       lea       eax,[rdx*4]
       sub       r8d,eax
       vmovups   ymm6,[rsi+28]
       vmovups   ymm7,[rsi+48]
       vmovups   ymm8,[rsi+68]
       vmovups   ymm9,[rsi+88]
       xor       eax,eax
       jmp       near ptr M00_L02
M00_L01:
       vpaddq    ymm0,ymm6,ymm9
       vprolq    ymm0,ymm0,17
       vpsllq    ymm1,ymm7,11
       vpxor     ymm2,ymm6,ymm8
       vpxor     ymm3,ymm7,ymm9
       vpxor     ymm4,ymm7,ymm2
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm5,ymm6,ymm3
       vpxor     ymm2,ymm2,ymm1
       vprolq    ymm3,ymm3,2D
       vpaddq    ymm1,ymm3,ymm5
       vprolq    ymm1,ymm1,17
       vpsllq    ymm16,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm1,ymm5,ymm1
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm16
       vprolq    ymm3,ymm3,2D
       vpaddq    ymm16,ymm3,ymm5
       vprolq    ymm16,ymm16,17
       vpsllq    ymm17,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm16,ymm5,ymm16
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm17
       vprolq    ymm3,ymm3,2D
       vpaddq    ymm17,ymm3,ymm5
       vprolq    ymm17,ymm17,17
       vpsllq    ymm18,ymm4,11
       vpxor     ymm8,ymm2,ymm5
       vpxor     ymm9,ymm3,ymm4
       vpxor     ymm7,ymm4,ymm8
       vpaddq    ymm2,ymm5,ymm17
       vpxor     ymm6,ymm5,ymm9
       vpxord    ymm8,ymm8,ymm18
       vprolq    ymm9,ymm9,2D
       vmovups   [rcx],ymm0
       vmovups   [rcx+20],ymm1
       vmovups   [rcx+40],ymm16
       vmovups   [rcx+60],ymm2
       add       rcx,80
       mov       eax,r10d
M00_L02:
       lea       r10d,[rax+4]
       cmp       r10d,edx
       jle       near ptr M00_L01
M00_L03:
       cmp       eax,edx
       jl        near ptr M00_L07
       test      r8d,r8d
       jne       near ptr M00_L08
M00_L04:
       vmovups   [rsi+28],ymm6
       vmovups   [rsi+48],ymm7
       vmovups   [rsi+68],ymm8
       vmovups   [rsi+88],ymm9
M00_L05:
       mov       rcx,[rbx+20]
       call      qword ptr [7FF91F956490]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(UInt64[])
       nop
       vzeroupper
       vmovaps   xmm6,[rsp+0B0]
       vmovaps   xmm7,[rsp+0A0]
       vmovaps   xmm8,[rsp+90]
       vmovaps   xmm9,[rsp+80]
       vmovaps   xmm10,[rsp+70]
       vmovaps   xmm11,[rsp+60]
       vmovaps   xmm12,[rsp+50]
       vmovaps   xmm13,[rsp+40]
       add       rsp,0C8
       pop       rbx
       pop       rsi
       ret
M00_L06:
       lea       rcx,[rdx+10]
       mov       r8d,[rdx+8]
       jmp       near ptr M00_L00
M00_L07:
       vpaddq    ymm0,ymm6,ymm9
       vprolq    ymm0,ymm0,17
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vprolq    ymm9,ymm9,2D
       vmovups   [rcx],ymm0
       add       rcx,20
       inc       eax
       jmp       near ptr M00_L03
M00_L08:
       vpaddq    ymm0,ymm6,ymm9
       vprolq    ymm0,ymm0,17
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vmovups   [rsp+20],ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vprolq    ymm9,ymm9,2D
       lea       rdx,[rsp+20]
       vextractf128 xmm10,ymm6,1
       vextractf128 xmm11,ymm7,1
       vextractf128 xmm12,ymm9,1
       vextractf128 xmm13,ymm8,1
       call      qword ptr [7FF91F956538]
       vinsertf128 ymm6,ymm6,xmm10,1
       vinsertf128 ymm7,ymm7,xmm11,1
       vinsertf128 ymm9,ymm9,xmm12,1
       vinsertf128 ymm8,ymm8,xmm13,1
       jmp       near ptr M00_L04
; Total bytes of code 727
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
       sub       rsp,0C8
       vmovaps   [rsp+0B0],xmm6
       vmovaps   [rsp+0A0],xmm7
       vmovaps   [rsp+90],xmm8
       vmovaps   [rsp+80],xmm9
       vmovaps   [rsp+70],xmm10
       vmovaps   [rsp+60],xmm11
       vmovaps   [rsp+50],xmm12
       vmovaps   [rsp+40],xmm13
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
       shr       edx,2
       lea       eax,[rdx*4]
       sub       r8d,eax
       vmovups   ymm6,[rsi+28]
       vmovups   ymm7,[rsi+48]
       vmovups   ymm8,[rsi+68]
       vmovups   ymm9,[rsi+88]
       xor       eax,eax
M00_L01:
       lea       r10d,[rax+4]
       cmp       r10d,edx
       jg        near ptr M00_L03
       vpaddq    ymm0,ymm6,ymm9
       vprolq    ymm0,ymm0,17
       vpsllq    ymm1,ymm7,11
       vpxor     ymm2,ymm6,ymm8
       vpxor     ymm3,ymm7,ymm9
       vpxor     ymm4,ymm7,ymm2
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm5,ymm6,ymm3
       vpxor     ymm2,ymm2,ymm1
       vprolq    ymm3,ymm3,2D
       vpsrlq    ymm0,ymm0,0B
       vcvtuqq2pd ymm0,ymm0
       vbroadcastsd ymm1,qword ptr [7FF91F5CD198]
       vmulpd    ymm0,ymm0,ymm1
       vpaddq    ymm16,ymm3,ymm5
       vprolq    ymm16,ymm16,17
       vpsllq    ymm17,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm16,ymm5,ymm16
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm17
       vprolq    ymm3,ymm3,2D
       vpsrlq    ymm16,ymm16,0B
       vcvtuqq2pd ymm16,ymm16
       vmulpd    ymm16,ymm16,ymm1
       vpaddq    ymm17,ymm3,ymm5
       vprolq    ymm17,ymm17,17
       vpsllq    ymm18,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm17,ymm5,ymm17
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm18
       vprolq    ymm3,ymm3,2D
       vpsrlq    ymm17,ymm17,0B
       vcvtuqq2pd ymm17,ymm17
       vmulpd    ymm17,ymm17,ymm1
       vpaddq    ymm18,ymm3,ymm5
       vprolq    ymm18,ymm18,17
       vpsllq    ymm19,ymm4,11
       vpxor     ymm8,ymm2,ymm5
       vpxor     ymm9,ymm3,ymm4
       vpxor     ymm7,ymm4,ymm8
       vpaddq    ymm2,ymm5,ymm18
       vpxor     ymm6,ymm5,ymm9
       vpxord    ymm8,ymm8,ymm19
       vprolq    ymm9,ymm9,2D
       vpsrlq    ymm2,ymm2,0B
       vcvtuqq2pd ymm2,ymm2
       vmulpd    ymm1,ymm2,ymm1
       vmovups   [rcx],ymm0
       vmovups   [rcx+20],ymm16
       vmovups   [rcx+40],ymm17
       vmovups   [rcx+60],ymm1
       add       rcx,80
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
       vmovups   [rsi+28],ymm6
       vmovups   [rsi+48],ymm7
       vmovups   [rsi+68],ymm8
       vmovups   [rsi+88],ymm9
M00_L05:
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F946580]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
       nop
       vzeroupper
       vmovaps   xmm6,[rsp+0B0]
       vmovaps   xmm7,[rsp+0A0]
       vmovaps   xmm8,[rsp+90]
       vmovaps   xmm9,[rsp+80]
       vmovaps   xmm10,[rsp+70]
       vmovaps   xmm11,[rsp+60]
       vmovaps   xmm12,[rsp+50]
       vmovaps   xmm13,[rsp+40]
       add       rsp,0C8
       pop       rbx
       pop       rsi
       ret
M00_L06:
       vpaddq    ymm0,ymm6,ymm9
       vprolq    ymm0,ymm0,17
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vprolq    ymm9,ymm9,2D
       vpsrlq    ymm0,ymm0,0B
       vcvtuqq2pd ymm0,ymm0
       vbroadcastsd ymm1,qword ptr [7FF91F5CD198]
       vmulpd    ymm1,ymm0,ymm1
       vmovups   [rcx],ymm1
       add       rcx,20
       inc       eax
       jmp       near ptr M00_L03
M00_L07:
       vpaddq    ymm0,ymm6,ymm9
       vprolq    ymm0,ymm0,17
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vprolq    ymm9,ymm9,2D
       vpsrlq    ymm0,ymm0,0B
       vcvtuqq2pd ymm0,ymm0
       vbroadcastsd ymm1,qword ptr [7FF91F5CD198]
       vmulpd    ymm0,ymm0,ymm1
       vmovups   [rsp+20],ymm0
       lea       rdx,[rsp+20]
       vextractf128 xmm10,ymm6,1
       vextractf128 xmm11,ymm7,1
       vextractf128 xmm12,ymm9,1
       vextractf128 xmm13,ymm8,1
       call      qword ptr [7FF91F946688]
       vinsertf128 ymm6,ymm6,xmm10,1
       vinsertf128 ymm7,ymm7,xmm11,1
       vinsertf128 ymm9,ymm9,xmm12,1
       vinsertf128 ymm8,ymm8,xmm13,1
       jmp       near ptr M00_L04
; Total bytes of code 854
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
       call      qword ptr [7FF91F946460]; Anastasya.Metaheuristics.Core.Randomness.RandomSource.Fill(System.Span`1<Int32>, Int32, Int32)
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
       mov       rax,0F9F26D7F893F
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
       vmovups   ymm0,[rcx+28]
       vmovups   ymm1,[rcx+48]
       vmovups   ymm2,[rcx+68]
       vmovups   ymm3,[rcx+88]
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
       sub       rsp,20
       lea       rax,[rsp+20]
       vxorps    ymm4,ymm4,ymm4
       vmovdqu   ymmword ptr [rax],ymm4
       xor       ebx,ebx
       cmp       ebx,r11d
       jge       short M01_L04
M01_L02:
       vpaddq    ymm4,ymm0,ymm3
       vprolq    ymm4,ymm4,17
       vpsllq    ymm5,ymm1,11
       vpxor     ymm2,ymm0,ymm2
       vpxor     ymm3,ymm1,ymm3
       vpxor     ymm1,ymm2,ymm1
       vpaddq    ymm4,ymm0,ymm4
       vpxor     ymm0,ymm0,ymm3
       vpxor     ymm2,ymm2,ymm5
       vprolq    ymm3,ymm3,2D
       vmovups   [rax],ymm4
       xor       esi,esi
       cmp       ebx,r11d
       jl        short M01_L10
M01_L03:
       cmp       ebx,r11d
       jl        short M01_L02
M01_L04:
       vmovups   [rcx+28],ymm0
       vmovups   [rcx+48],ymm1
       vmovups   [rcx+68],ymm2
       vmovups   [rcx+88],ymm3
M01_L05:
       mov       r8,0F9F26D7F893F
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
       cmp       esi,4
       jl        short M01_L09
       mov       ebx,edi
       jmp       near ptr M01_L03
M01_L13:
       mov       rcx,offset MT_System.ArgumentException
       call      CORINFO_HELP_NEWSFAST
       mov       rbx,rax
       mov       ecx,0B5
       mov       rdx,7FF91F919FF8
       call      qword ptr [7FF91F777798]
       mov       rsi,rax
       mov       ecx,5B
       mov       rdx,7FF91F919FF8
       call      qword ptr [7FF91F777798]
       mov       r8,rax
       mov       rdx,rsi
       mov       rcx,rbx
       call      qword ptr [7FF91F8B5E48]
       mov       rcx,rbx
       call      CORINFO_HELP_THROW
       int       3
; Total bytes of code 477
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
       call      qword ptr [7FF91F956478]; Anastasya.Metaheuristics.Core.Randomness.StandardNormal.Fill(Anastasya.Metaheuristics.Core.Randomness.RandomSource, System.Span`1<Double>)
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F956490]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
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
       sub       rsp,150
       vmovaps   [rsp+140],xmm6
       vmovaps   [rsp+130],xmm7
       vmovaps   [rsp+120],xmm8
       vmovaps   [rsp+110],xmm9
       vmovaps   [rsp+100],xmm10
       vmovaps   [rsp+0F0],xmm11
       vmovaps   [rsp+0E0],xmm12
       vmovaps   [rsp+0D0],xmm13
       lea       rbp,[rsp+20]
       vxorps    xmm4,xmm4,xmm4
       vmovdqu   ymmword ptr [rbp+10],ymm4
       mov       rax,43FFF22D8DBC
       mov       [rbp+8],rax
       mov       rbx,rcx
       mov       rsi,[rdx]
       mov       edi,[rdx+8]
       test      rbx,rbx
       je        near ptr M01_L10
       test      edi,edi
       je        near ptr M01_L08
       test      [rsp],esp
       sub       rsp,20
       lea       rdx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rdx],ymm0
       mov       r14,rdx
       test      [rsp],esp
       sub       rsp,20
       lea       rdx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rdx],ymm0
       mov       r15,rdx
       test      [rsp],esp
       sub       rsp,20
       lea       rdx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rdx],ymm0
       test      [rsp],esp
       sub       rsp,20
       lea       rcx,[rsp+20]
       vxorps    ymm0,ymm0,ymm0
       vmovdqu   ymmword ptr [rcx],ymm0
       mov       r13,rcx
       xor       ecx,ecx
       jmp       short M01_L02
M01_L00:
       xor       r8d,r8d
M01_L01:
       mov       [rax],r8
       inc       ecx
       cmp       ecx,4
       jge       short M01_L03
M01_L02:
       lea       rax,[rdx+rcx*8]
       test      cl,1
       jne       short M01_L00
       mov       r8,0FFFFFFFFFFFFFFFF
       jmp       short M01_L01
M01_L03:
       vmovups   ymm6,[rdx]
       vbroadcastsd ymm7,qword ptr [7FF91F5D8CE8]
       vbroadcastsd ymm8,qword ptr [7FF91F5D8CF0]
       test      edi,edi
       jle       near ptr M01_L08
M01_L04:
       vmovups   ymm0,[rbx+28]
       vmovups   ymm1,[rbx+48]
       vmovups   ymm2,[rbx+68]
       vmovups   ymm3,[rbx+88]
       vpaddq    ymm4,ymm0,ymm3
       vprolq    ymm4,ymm4,17
       vpsllq    ymm5,ymm1,11
       vpxor     ymm2,ymm0,ymm2
       vpxor     ymm3,ymm1,ymm3
       vpxor     ymm1,ymm2,ymm1
       vpaddq    ymm4,ymm0,ymm4
       vpxor     ymm0,ymm0,ymm3
       vpxor     ymm2,ymm2,ymm5
       vprolq    ymm3,ymm3,2D
       vmovups   [rbx+28],ymm0
       vmovups   [rbx+48],ymm1
       vmovups   [rbx+68],ymm2
       vmovups   [rbx+88],ymm3
       vpsrlq    ymm0,ymm4,0B
       vcvtuqq2pd ymm0,ymm0
       vmulpd    ymm0,ymm0,qword bcst [7FF91F5D8CF8]
       xor       edx,edx
M01_L05:
       lea       rcx,[rdx*8]
       lea       rax,[r14+rcx]
       mov       r8d,edx
       and       r8d,0FFFFFFFE
       cmp       r8d,4
       jae       near ptr M01_L11
       vmovups   [rbp+10],ymm0
       vmovsd    xmm1,qword ptr [rbp+r8*8+10]
       vmovsd    qword ptr [rax],xmm1
       add       rcx,r15
       mov       eax,edx
       or        eax,1
       cmp       eax,4
       jae       near ptr M01_L11
       vmovups   [rbp+10],ymm0
       vmovsd    xmm1,qword ptr [rbp+rax*8+10]
       vmovsd    qword ptr [rcx],xmm1
       inc       edx
       cmp       edx,4
       jl        short M01_L05
       vbroadcastsd ymm0,qword ptr [7FF91F5D8D00]
       vsubpd    ymm0,ymm0,[r14]
       vmovups   [rbp+30],ymm0
       lea       rdx,[rbp+30]
       lea       rcx,[rbp+90]
       vextractf128 xmm9,ymm6,1
       vextractf128 xmm10,ymm7,1
       vextractf128 xmm11,ymm8,1
       call      qword ptr [7FF91F956838]; System.Runtime.Intrinsics.VectorMath.LogDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.UInt64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   ymm12,[rbp+90]
       vinsertf128 ymm8,ymm8,xmm11,1
       vmulpd    ymm0,ymm8,[r15]
       vmovups   [rbp+30],ymm0
       lea       rdx,[rbp+30]
       lea       rcx,[rbp+50]
       vextractf128 xmm13,ymm12,1
       vextractf128 xmm11,ymm8,1
       call      qword ptr [7FF91F956868]; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   ymm0,[rbp+50]
       vmovups   ymm1,[rbp+70]
       vinsertf128 ymm6,ymm6,xmm9,1
       vpternlogq ymm1,ymm0,ymm6,0E4
       vinsertf128 ymm12,ymm12,xmm13,1
       vinsertf128 ymm7,ymm7,xmm10,1
       vmulpd    ymm0,ymm12,ymm7
       vsqrtpd   ymm0,ymm0
       vmulpd    ymm0,ymm1,ymm0
       cmp       edi,4
       vinsertf128 ymm8,ymm8,xmm11,1
       jl        short M01_L06
       vmovups   [rsi],ymm0
       add       rsi,20
       sub       edi,4
       test      edi,edi
       jg        near ptr M01_L04
       jmp       short M01_L08
M01_L06:
       vmovups   [r13],ymm0
       xor       ecx,ecx
M01_L07:
       movsxd    rdx,ecx
       vmovsd    xmm0,qword ptr [r13+rdx*8]
       vmovsd    qword ptr [rsi+rdx*8],xmm0
       inc       ecx
       cmp       ecx,edi
       jl        short M01_L07
M01_L08:
       mov       r8,43FFF22D8DBC
       cmp       [rbp+8],r8
       je        short M01_L09
       call      CORINFO_HELP_FAIL_FAST
M01_L09:
       nop
       vzeroupper
       vmovaps   xmm6,[rbp+120]
       vmovaps   xmm7,[rbp+110]
       vmovaps   xmm8,[rbp+100]
       vmovaps   xmm9,[rbp+0F0]
       vmovaps   xmm10,[rbp+0E0]
       vmovaps   xmm11,[rbp+0D0]
       vmovaps   xmm12,[rbp+0C0]
       vmovaps   xmm13,[rbp+0B0]
       lea       rsp,[rbp+130]
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
       mov       rdx,7FF91F929FF8
       call      qword ptr [7FF91F787798]
       mov       rcx,rax
       call      qword ptr [7FF91F956928]
       int       3
M01_L11:
       call      CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION
       int       3
; Total bytes of code 840
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
       vmovups   ymm0,[rdx]
       vmovaps   ymm1,ymm0
       vpaddq    ymm2,ymm0,qword bcst [7FF91F5DE840]
       vpcmpnltuq k1,ymm2,qword bcst [7FF91F5DE848]
       vpmovm2q  ymm2,k1
       vptest    ymm2,ymm2
       jne       near ptr M03_L01
M03_L00:
       vmovups   ymm0,[rdx]
       vpaddq    ymm0,ymm0,qword bcst [7FF91F5DE850]
       vpsraq    ymm3,ymm0,34
       vcvtqq2pd ymm3,ymm3
       vpandq    ymm0,ymm0,qword bcst [7FF91F5DE858]
       vpaddq    ymm0,ymm0,qword bcst [7FF91F5DE860]
       vsubpd    ymm0,ymm0,qword bcst [7FF91F5DE868]
       vmulpd    ymm4,ymm0,ymm0
       vmulpd    ymm5,ymm4,ymm4
       vmulpd    ymm16,ymm5,ymm5
       vmulpd    ymm17,ymm16,ymm16
       vbroadcastsd ymm18,qword ptr [7FF91F5DE870]
       vfmadd213pd ymm18,ymm0,qword bcst [7FF91F5DE878]
       vbroadcastsd ymm19,qword ptr [7FF91F5DE880]
       vfmadd213pd ymm19,ymm0,qword bcst [7FF91F5DE888]
       vfmadd213pd ymm18,ymm4,ymm19
       vfmadd231pd ymm18,ymm5,qword bcst [7FF91F5DE890]
       vbroadcastsd ymm19,qword ptr [7FF91F5DE898]
       vfmadd213pd ymm19,ymm0,qword bcst [7FF91F5DE8A0]
       vbroadcastsd ymm20,qword ptr [7FF91F5DE8A8]
       vfmadd213pd ymm20,ymm0,qword bcst [7FF91F5DE8B0]
       vfmadd213pd ymm19,ymm4,ymm20
       vbroadcastsd ymm20,qword ptr [7FF91F5DE8B8]
       vfmadd213pd ymm20,ymm0,qword bcst [7FF91F5DE8C0]
       vbroadcastsd ymm21,qword ptr [7FF91F5DE8C8]
       vfmadd213pd ymm21,ymm0,qword bcst [7FF91F5DE8D0]
       vfmadd213pd ymm20,ymm4,ymm21
       vfmadd231pd ymm20,ymm19,ymm5
       vbroadcastsd ymm19,qword ptr [7FF91F5DE8D8]
       vfmadd213pd ymm19,ymm0,qword bcst [7FF91F5DE8E0]
       vbroadcastsd ymm21,qword ptr [7FF91F5DE8E8]
       vfmadd213pd ymm21,ymm0,qword bcst [7FF91F5DE8F0]
       vfmadd213pd ymm19,ymm4,ymm21
       vbroadcastsd ymm21,qword ptr [7FF91F5DE8F8]
       vfmadd213pd ymm21,ymm0,qword bcst [7FF91F5DE900]
       vfmadd213pd ymm4,ymm21,ymm0
       vfmadd213pd ymm5,ymm19,ymm4
       vfmadd213pd ymm16,ymm20,ymm5
       vfmadd213pd ymm17,ymm18,ymm16
       vmovaps   ymm0,ymm3
       vfmadd132pd ymm0,ymm17,qword bcst [7FF91F5DE908]
       vfmadd132pd ymm3,ymm0,qword bcst [7FF91F5DE910]
       vpternlogq ymm2,ymm3,ymm1,0AC
       vmovups   [rcx],ymm2
       mov       rax,rcx
       vzeroupper
       ret
M03_L01:
       vxorps    ymm3,ymm3,ymm3
       vpcmpgtq  ymm3,ymm3,ymm0
       vpternlogq ymm1,ymm3,qword bcst [7FF91F5DE918],0B8
       vxorps    ymm4,ymm4,ymm4
       vcmpeqpd  ymm4,ymm4,ymm0
       vpternlogq ymm1,ymm4,qword bcst [7FF91F5DE840],0B8
       vcmpneqpd ymm5,ymm0,ymm0
       vpternlogq ymm4,ymm5,ymm3,0FE
       vpcmpeqq  ymm3,ymm0,[7FF91F5DE920]
       vorpd     ymm3,ymm3,ymm4
       vandnpd   ymm2,ymm3,ymm2
       vmulpd    ymm4,ymm0,qword bcst [7FF91F5DE940]
       vpaddq    ymm4,ymm4,qword bcst [7FF91F5DE948]
       vpternlogq ymm2,ymm4,ymm0,0CA
       vmovups   [rdx],ymm2
       vmovaps   ymm2,ymm3
       jmp       near ptr M03_L00
; Total bytes of code 518
```
```assembly
; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       sub       rsp,58
       vmovaps   [rsp+40],xmm6
       vmovaps   [rsp+30],xmm7
       vmovaps   [rsp+20],xmm8
       vmovaps   [rsp+10],xmm9
       vmovaps   [rsp],xmm10
       vmovups   ymm6,[rdx]
       vandpd    ymm7,ymm6,qword bcst [7FF91F5DEDA0]
       vbroadcastsd ymm0,qword ptr [7FF91F5DEDA8]
       vpcmpgtq  ymm0,ymm0,ymm7
       vpcmpeqd  ymm1,ymm1,ymm1
       vptest    ymm0,ymm1
       jb        near ptr M04_L01
       vbroadcastsd ymm0,qword ptr [7FF91F5DEDB0]
       vpcmpgtq  ymm0,ymm0,ymm7
       vpcmpeqd  ymm1,ymm1,ymm1
       vptest    ymm0,ymm1
       jae       near ptr M04_L03
       vbroadcastsd ymm0,qword ptr [7FF91F5DEDB8]
       vmovaps   ymm1,ymm0
       vfmadd231pd ymm1,ymm7,qword bcst [7FF91F5DEDC0]
       vsubpd    ymm0,ymm1,ymm0
       vmovaps   ymm2,ymm7
       vfmadd231pd ymm2,ymm0,qword bcst [7FF91F5DEDC8]
       vmulpd    ymm3,ymm0,qword bcst [7FF91F5DEDD0]
       vsubpd    ymm4,ymm2,ymm3
       vsubpd    ymm2,ymm2,ymm4
       vsubpd    ymm3,ymm2,ymm3
       vbroadcastsd ymm2,qword ptr [7FF91F5DEDD8]
       vxorpd    ymm3,ymm3,ymm2
       vfmadd132pd ymm0,ymm3,qword bcst [7FF91F5DEDE0]
       vmovaps   ymm3,ymm0
       vsubpd    ymm0,ymm4,ymm3
       vsubpd    ymm4,ymm4,ymm0
       vsubpd    ymm3,ymm4,ymm3
       vmulpd    ymm4,ymm0,ymm0
       vmovaps   ymm5,ymm4
       vmulpd    ymm16,ymm0,ymm5
       vmulpd    ymm17,ymm5,ymm5
       vmovaps   ymm18,ymm17
       vbroadcastsd ymm19,qword ptr [7FF91F5DEDE8]
       vmulpd    ymm20,ymm18,ymm18
       vbroadcastsd ymm21,qword ptr [7FF91F5DEDF0]
       vfmadd213pd ymm21,ymm5,qword bcst [7FF91F5DEDF8]
       vbroadcastsd ymm22,qword ptr [7FF91F5DEE00]
       vfmadd213pd ymm22,ymm5,qword bcst [7FF91F5DEE08]
       vfmadd213pd ymm18,ymm21,ymm22
       vfmadd231pd ymm18,ymm20,qword bcst [7FF91F5DEE10]
       vmulpd    ymm18,ymm18,ymm16
       vxorpd    ymm18,ymm18,ymm2
       vfmadd231pd ymm18,ymm19,ymm3
       vxorpd    ymm21,ymm2,ymm3
       vfmadd213pd ymm5,ymm18,ymm21
       vfmadd231pd ymm5,ymm16,qword bcst [7FF91F5DEE18]
       vsubpd    ymm5,ymm0,ymm5
       vmovaps   ymm16,ymm4
       vmulpd    ymm16,ymm19,ymm16
       vbroadcastsd ymm8,qword ptr [7FF91F5DEE20]
       vsubpd    ymm18,ymm16,ymm8
       vbroadcastsd ymm19,qword ptr [7FF91F5DEE28]
       vfmadd213pd ymm19,ymm4,qword bcst [7FF91F5DEE30]
       vbroadcastsd ymm21,qword ptr [7FF91F5DEE38]
       vfmadd213pd ymm21,ymm4,qword bcst [7FF91F5DEE40]
       vbroadcastsd ymm22,qword ptr [7FF91F5DEE48]
       vfmadd213pd ymm4,ymm22,qword bcst [7FF91F5DEE50]
       vfmadd231pd ymm4,ymm17,ymm21
       vfmadd213pd ymm19,ymm20,ymm4
       vaddpd    ymm4,ymm8,ymm18
       vsubpd    ymm4,ymm4,ymm16
       vfmadd213pd ymm0,ymm3,ymm4
       vfmadd213pd ymm19,ymm17,ymm0
       vsubpd    ymm0,ymm19,ymm18
       vbroadcastsd ymm3,qword ptr [7FF91F5DEE58]
       vpand     ymm4,ymm3,ymm1
       vptestnmq k1,ymm4,ymm4
       vpblendmq ymm9{k1},ymm0,ymm5
       vpblendmq ymm10{k1},ymm5,ymm0
       vpsrlq    ymm0,ymm6,3F
       vpsrlq    ymm4,ymm1,1
       vpternlogq ymm5,ymm4,ymm0,11
       vpternlogq ymm5,ymm4,ymm0,0F8
       vpand     ymm0,ymm5,ymm3
       vxorps    ymm4,ymm4,ymm4
       vpcmpeqq  ymm0,ymm4,ymm0
       vxorpd    ymm4,ymm2,ymm9
       vblendvpd ymm9,ymm9,ymm4,ymm0
       vpaddq    ymm0,ymm3,ymm1
       vpandq    ymm0,ymm0,qword bcst [7FF91F5DEE60]
       vxorps    ymm1,ymm1,ymm1
       vpcmpeqq  ymm0,ymm1,ymm0
       vxorpd    ymm1,ymm2,ymm10
       vblendvpd ymm10,ymm1,ymm10,ymm0
M04_L00:
       vpcmpgtq  k1,ymm7,qword bcst [7FF91F5DEE68]
       vpblendmq ymm9{k1},ymm6,ymm9
       vpblendmq ymm10{k1},ymm8,ymm10
       vmovups   [rcx],ymm9
       vmovups   [rcx+20],ymm10
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
       vmulpd    ymm0,ymm6,ymm6
       vmovaps   ymm10,ymm0
       vpcmpgtq  ymm1,ymm7,[7FF91F5DEE80]
       vptest    ymm1,ymm1
       je        near ptr M04_L02
       vmovaps   ymm1,ymm0
       vmulpd    ymm9,ymm6,ymm1
       vmulpd    ymm2,ymm1,ymm1
       vmovaps   ymm3,ymm2
       vbroadcastsd ymm4,qword ptr [7FF91F5DEE10]
       vfmadd213pd ymm4,ymm1,qword bcst [7FF91F5DEDF0]
       vmulpd    ymm5,ymm3,ymm3
       vbroadcastsd ymm16,qword ptr [7FF91F5DEDF8]
       vfmadd213pd ymm16,ymm1,qword bcst [7FF91F5DEE00]
       vbroadcastsd ymm17,qword ptr [7FF91F5DEE08]
       vfmadd213pd ymm1,ymm17,qword bcst [7FF91F5DEEA0]
       vfmadd213pd ymm3,ymm16,ymm1
       vfmadd213pd ymm4,ymm5,ymm3
       vfmadd213pd ymm9,ymm4,ymm6
       vbroadcastsd ymm1,qword ptr [7FF91F5DEE28]
       vfmadd213pd ymm1,ymm0,qword bcst [7FF91F5DEE30]
       vbroadcastsd ymm3,qword ptr [7FF91F5DEE38]
       vfmadd213pd ymm3,ymm0,qword bcst [7FF91F5DEE40]
       vbroadcastsd ymm4,qword ptr [7FF91F5DEE48]
       vfmadd213pd ymm0,ymm4,qword bcst [7FF91F5DEE50]
       vfmadd213pd ymm2,ymm3,ymm0
       vfmadd213pd ymm1,ymm5,ymm2
       vfmadd213pd ymm1,ymm10,qword bcst [7FF91F5DEEA8]
       vbroadcastsd ymm8,qword ptr [7FF91F5DEE20]
       vfmadd213pd ymm10,ymm1,ymm8
       jmp       near ptr M04_L00
M04_L02:
       vmulpd    ymm0,ymm6,ymm10
       vmovaps   ymm9,ymm6
       vfmadd231pd ymm9,ymm0,qword bcst [7FF91F5DEEA0]
       vbroadcastsd ymm8,qword ptr [7FF91F5DEE20]
       vfmadd132pd ymm10,ymm8,qword bcst [7FF91F5DEEA8]
       jmp       near ptr M04_L00
M04_L03:
       vzeroupper
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       jmp       qword ptr [7FF91F956A90]
; Total bytes of code 965
```

## .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v4 (Job: Job-HHSXOG(IterationCount=12, WarmupCount=5))

```assembly
; Anastasya.Metaheuristics.Benchmarks.RsJitDiagnosticsBenchmarks.VectorApi()
       mov       rax,[rcx+10]
       vmovups   ymm0,[rax+28]
       vmovups   ymm1,[rax+48]
       vmovups   ymm2,[rax+68]
       vmovups   ymm3,[rax+88]
       vpaddq    ymm4,ymm0,ymm3
       vprolq    ymm4,ymm4,17
       vpsllq    ymm5,ymm1,11
       vpxor     ymm2,ymm0,ymm2
       vpxor     ymm3,ymm1,ymm3
       vpxor     ymm1,ymm2,ymm1
       vpaddq    ymm4,ymm0,ymm4
       vpxor     ymm0,ymm0,ymm3
       vpxor     ymm2,ymm2,ymm5
       vprolq    ymm3,ymm3,2D
       vmovups   [rax+28],ymm0
       vmovups   [rax+48],ymm1
       vmovups   [rax+68],ymm2
       vmovups   [rax+88],ymm3
       vmovq     rax,xmm4
       vzeroupper
       ret
; Total bytes of code 106
```

