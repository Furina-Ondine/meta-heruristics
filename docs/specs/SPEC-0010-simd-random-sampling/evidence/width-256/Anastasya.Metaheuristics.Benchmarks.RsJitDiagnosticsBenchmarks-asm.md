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
       vpsllq    ymm1,ymm0,17
       vpsrlq    ymm0,ymm0,29
       vpor      ymm0,ymm0,ymm1
       vpsllq    ymm1,ymm7,11
       vpxor     ymm2,ymm6,ymm8
       vpxor     ymm3,ymm7,ymm9
       vpxor     ymm4,ymm7,ymm2
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm5,ymm3,ymm6
       vpxor     ymm2,ymm2,ymm1
       vpsllq    ymm1,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpor      ymm3,ymm3,ymm1
       vpaddq    ymm1,ymm3,ymm5
       vpsllq    ymm16,ymm1,17
       vpsrlq    ymm1,ymm1,29
       vpord     ymm1,ymm1,ymm16
       vpsllq    ymm16,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm1,ymm5,ymm1
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm16
       vpsllq    ymm16,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpord     ymm3,ymm3,ymm16
       vpaddq    ymm16,ymm3,ymm5
       vpsllq    ymm17,ymm16,17
       vpsrlq    ymm16,ymm16,29
       vpord     ymm16,ymm16,ymm17
       vpsllq    ymm17,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm16,ymm5,ymm16
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm17
       vpsllq    ymm17,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpord     ymm3,ymm3,ymm17
       vpaddq    ymm17,ymm3,ymm5
       vpsllq    ymm18,ymm17,17
       vpsrlq    ymm17,ymm17,29
       vpord     ymm17,ymm17,ymm18
       vpsllq    ymm18,ymm4,11
       vpxor     ymm8,ymm2,ymm5
       vpxor     ymm9,ymm3,ymm4
       vpxor     ymm7,ymm4,ymm8
       vpaddq    ymm2,ymm5,ymm17
       vpxor     ymm6,ymm5,ymm9
       vpxord    ymm8,ymm8,ymm18
       vpsllq    ymm3,ymm9,2D
       vpsrlq    ymm4,ymm9,13
       vpor      ymm9,ymm4,ymm3
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
       call      qword ptr [7FF91F986580]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(UInt64[])
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
       vpsllq    ymm1,ymm0,17
       vpsrlq    ymm0,ymm0,29
       vpor      ymm0,ymm0,ymm1
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vpsllq    ymm1,ymm9,2D
       vpsrlq    ymm2,ymm9,13
       vpor      ymm9,ymm2,ymm1
       vmovups   [rcx],ymm0
       add       rcx,20
       inc       eax
       jmp       near ptr M00_L03
M00_L08:
       vpaddq    ymm0,ymm6,ymm9
       vpsllq    ymm1,ymm0,17
       vpsrlq    ymm0,ymm0,29
       vpor      ymm0,ymm0,ymm1
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vmovups   [rsp+20],ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vpsllq    ymm0,ymm9,2D
       vpsrlq    ymm1,ymm9,13
       vpor      ymm9,ymm1,ymm0
       lea       rdx,[rsp+20]
       vextractf128 xmm10,ymm6,1
       vextractf128 xmm11,ymm7,1
       vextractf128 xmm12,ymm9,1
       vextractf128 xmm13,ymm8,1
       call      qword ptr [7FF91F986628]
       vinsertf128 ymm6,ymm6,xmm10,1
       vinsertf128 ymm7,ymm7,xmm11,1
       vinsertf128 ymm9,ymm9,xmm12,1
       vinsertf128 ymm8,ymm8,xmm13,1
       jmp       near ptr M00_L04
; Total bytes of code 841
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
       vpsllq    ymm1,ymm0,17
       vpsrlq    ymm0,ymm0,29
       vpor      ymm0,ymm0,ymm1
       vpsllq    ymm1,ymm7,11
       vpxor     ymm2,ymm6,ymm8
       vpxor     ymm3,ymm7,ymm9
       vpxor     ymm4,ymm7,ymm2
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm5,ymm3,ymm6
       vpxor     ymm2,ymm2,ymm1
       vpsllq    ymm1,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpor      ymm3,ymm3,ymm1
       vpsrlq    ymm0,ymm0,0B
       vcvtuqq2pd ymm0,ymm0
       vbroadcastsd ymm1,qword ptr [7FF91F60D250]
       vmulpd    ymm0,ymm0,ymm1
       vpaddq    ymm16,ymm3,ymm5
       vpsllq    ymm17,ymm16,17
       vpsrlq    ymm16,ymm16,29
       vpord     ymm16,ymm16,ymm17
       vpsllq    ymm17,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm16,ymm5,ymm16
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm17
       vpsllq    ymm17,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpord     ymm3,ymm3,ymm17
       vpsrlq    ymm16,ymm16,0B
       vcvtuqq2pd ymm16,ymm16
       vmulpd    ymm16,ymm16,ymm1
       vpaddq    ymm17,ymm3,ymm5
       vpsllq    ymm18,ymm17,17
       vpsrlq    ymm17,ymm17,29
       vpord     ymm17,ymm17,ymm18
       vpsllq    ymm18,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm17,ymm5,ymm17
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm18
       vpsllq    ymm18,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpord     ymm3,ymm3,ymm18
       vpsrlq    ymm17,ymm17,0B
       vcvtuqq2pd ymm17,ymm17
       vmulpd    ymm17,ymm17,ymm1
       vpaddq    ymm18,ymm3,ymm5
       vpsllq    ymm19,ymm18,17
       vpsrlq    ymm18,ymm18,29
       vpord     ymm18,ymm18,ymm19
       vpsllq    ymm19,ymm4,11
       vpxor     ymm8,ymm2,ymm5
       vpxor     ymm9,ymm3,ymm4
       vpxor     ymm7,ymm4,ymm8
       vpaddq    ymm2,ymm5,ymm18
       vpxor     ymm6,ymm5,ymm9
       vpxord    ymm8,ymm8,ymm19
       vpsllq    ymm3,ymm9,2D
       vpsrlq    ymm4,ymm9,13
       vpor      ymm9,ymm4,ymm3
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
       call      qword ptr [7FF91F996568]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
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
       vpsllq    ymm1,ymm0,17
       vpsrlq    ymm0,ymm0,29
       vpor      ymm0,ymm0,ymm1
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vpsllq    ymm1,ymm9,2D
       vpsrlq    ymm2,ymm9,13
       vpor      ymm9,ymm2,ymm1
       vpsrlq    ymm0,ymm0,0B
       vcvtuqq2pd ymm0,ymm0
       vbroadcastsd ymm1,qword ptr [7FF91F60D250]
       vmulpd    ymm1,ymm0,ymm1
       vmovups   [rcx],ymm1
       add       rcx,20
       inc       eax
       jmp       near ptr M00_L03
M00_L07:
       vpaddq    ymm0,ymm6,ymm9
       vpsllq    ymm1,ymm0,17
       vpsrlq    ymm0,ymm0,29
       vpor      ymm0,ymm0,ymm1
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vpsllq    ymm1,ymm9,2D
       vpsrlq    ymm2,ymm9,13
       vpor      ymm9,ymm2,ymm1
       vpsrlq    ymm0,ymm0,0B
       vcvtuqq2pd ymm0,ymm0
       vbroadcastsd ymm1,qword ptr [7FF91F60D250]
       vmulpd    ymm0,ymm0,ymm1
       vmovups   [rsp+20],ymm0
       lea       rdx,[rsp+20]
       vextractf128 xmm10,ymm6,1
       vextractf128 xmm11,ymm7,1
       vextractf128 xmm12,ymm9,1
       vextractf128 xmm13,ymm8,1
       call      qword ptr [7FF91F996670]
       vinsertf128 ymm6,ymm6,xmm10,1
       vinsertf128 ymm7,ymm7,xmm11,1
       vinsertf128 ymm9,ymm9,xmm12,1
       vinsertf128 ymm8,ymm8,xmm13,1
       jmp       near ptr M00_L04
; Total bytes of code 970
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
       call      qword ptr [7FF91F986550]; Anastasya.Metaheuristics.Core.Randomness.RandomSource.Fill(System.Span`1<Int32>, Int32, Int32)
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
       mov       rax,7D4A4BDB8CBC
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
       mov       [rbp+18],rdx
       test      [rsp],esp
       sub       rsp,20
       lea       rax,[rsp+20]
       vxorps    ymm4,ymm4,ymm4
       vmovdqu   ymmword ptr [rax],ymm4
       mov       rbx,rax
       xor       esi,esi
       cmp       esi,r11d
       jge       short M01_L04
M01_L02:
       vpaddq    ymm4,ymm0,ymm3
       vpsllq    ymm5,ymm4,17
       vpsrlq    ymm4,ymm4,29
       vpor      ymm4,ymm4,ymm5
       vpsllq    ymm5,ymm1,11
       vpxor     ymm2,ymm0,ymm2
       vpxor     ymm3,ymm1,ymm3
       vpxor     ymm1,ymm2,ymm1
       vpaddq    ymm4,ymm0,ymm4
       vpxor     ymm0,ymm3,ymm0
       vpxor     ymm2,ymm2,ymm5
       vpsllq    ymm5,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpor      ymm3,ymm3,ymm5
       vmovups   [rbx],ymm4
       xor       edi,edi
       cmp       esi,r11d
       jl        short M01_L10
M01_L03:
       cmp       esi,r11d
       jl        short M01_L02
M01_L04:
       vmovups   [rcx+28],ymm0
       vmovups   [rcx+48],ymm1
       vmovups   [rcx+68],ymm2
       vmovups   [rcx+88],ymm3
M01_L05:
       mov       r8,7D4A4BDB8CBC
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
       jge       short M01_L03
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
       cmp       edi,4
       jl        short M01_L09
       mov       esi,r14d
       jmp       near ptr M01_L03
M01_L13:
       mov       rcx,offset MT_System.ArgumentException
       call      CORINFO_HELP_NEWSFAST
       mov       rbx,rax
       mov       ecx,0B5
       mov       rdx,7FF91F959FF8
       call      qword ptr [7FF91F7A7798]
       mov       rsi,rax
       mov       ecx,5B
       mov       rdx,7FF91F959FF8
       call      qword ptr [7FF91F7A7798]
       mov       r8,rax
       mov       rdx,rsi
       mov       rcx,rbx
       call      qword ptr [7FF91F8F5E48]
       mov       rcx,rbx
       call      CORINFO_HELP_THROW
       int       3
; Total bytes of code 502
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
       call      qword ptr [7FF91F976568]; Anastasya.Metaheuristics.Core.Randomness.StandardNormal.Fill(Anastasya.Metaheuristics.Core.Randomness.RandomSource, System.Span`1<Double>)
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F976580]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
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
       mov       rax,65306026F64C
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
       vbroadcastsd ymm7,qword ptr [7FF91F5E8D00]
       vbroadcastsd ymm8,qword ptr [7FF91F5E8D08]
       test      edi,edi
       jle       near ptr M01_L08
M01_L04:
       vmovups   ymm0,[rbx+28]
       vmovups   ymm1,[rbx+48]
       vmovups   ymm2,[rbx+68]
       vmovups   ymm3,[rbx+88]
       vpaddq    ymm4,ymm0,ymm3
       vpsllq    ymm5,ymm4,17
       vpsrlq    ymm4,ymm4,29
       vpor      ymm4,ymm4,ymm5
       vpsllq    ymm5,ymm1,11
       vpxor     ymm2,ymm0,ymm2
       vpxor     ymm3,ymm1,ymm3
       vpxor     ymm1,ymm2,ymm1
       vpaddq    ymm4,ymm0,ymm4
       vpxor     ymm0,ymm3,ymm0
       vpxor     ymm2,ymm2,ymm5
       vpsllq    ymm5,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpor      ymm3,ymm3,ymm5
       vmovups   [rbx+28],ymm0
       vmovups   [rbx+48],ymm1
       vmovups   [rbx+68],ymm2
       vmovups   [rbx+88],ymm3
       vpsrlq    ymm0,ymm4,0B
       vcvtuqq2pd ymm0,ymm0
       vmulpd    ymm0,ymm0,qword bcst [7FF91F5E8D10]
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
       vbroadcastsd ymm0,qword ptr [7FF91F5E8D18]
       vsubpd    ymm0,ymm0,[r14]
       vmovups   [rbp+30],ymm0
       lea       rdx,[rbp+30]
       lea       rcx,[rbp+90]
       vextractf128 xmm9,ymm6,1
       vextractf128 xmm10,ymm7,1
       vextractf128 xmm11,ymm8,1
       call      qword ptr [7FF91F976880]; System.Runtime.Intrinsics.VectorMath.LogDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.UInt64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   ymm12,[rbp+90]
       vinsertf128 ymm8,ymm8,xmm11,1
       vmulpd    ymm0,ymm8,[r15]
       vmovups   [rbp+30],ymm0
       lea       rdx,[rbp+30]
       lea       rcx,[rbp+50]
       vextractf128 xmm13,ymm12,1
       vextractf128 xmm11,ymm8,1
       call      qword ptr [7FF91F9768B0]; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
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
       nop       dword ptr [rax]
M01_L07:
       movsxd    rdx,ecx
       vmovsd    xmm0,qword ptr [r13+rdx*8]
       vmovsd    qword ptr [rsi+rdx*8],xmm0
       inc       ecx
       cmp       ecx,edi
       jl        short M01_L07
M01_L08:
       mov       r8,65306026F64C
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
       mov       rdx,7FF91F949FF8
       call      qword ptr [7FF91F797798]
       mov       rcx,rax
       call      qword ptr [7FF91F976970]
       int       3
M01_L11:
       call      CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION
       int       3
; Total bytes of code 861
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
       vpaddq    ymm2,ymm0,qword bcst [7FF91F5EE840]
       vpcmpnltuq k1,ymm2,qword bcst [7FF91F5EE848]
       vpmovm2q  ymm2,k1
       vptest    ymm2,ymm2
       jne       near ptr M03_L01
M03_L00:
       vmovups   ymm0,[rdx]
       vpaddq    ymm0,ymm0,qword bcst [7FF91F5EE850]
       vpsraq    ymm3,ymm0,34
       vcvtqq2pd ymm3,ymm3
       vpandq    ymm0,ymm0,qword bcst [7FF91F5EE858]
       vpaddq    ymm0,ymm0,qword bcst [7FF91F5EE860]
       vsubpd    ymm0,ymm0,qword bcst [7FF91F5EE868]
       vmulpd    ymm4,ymm0,ymm0
       vmulpd    ymm5,ymm4,ymm4
       vmulpd    ymm16,ymm5,ymm5
       vmulpd    ymm17,ymm16,ymm16
       vbroadcastsd ymm18,qword ptr [7FF91F5EE870]
       vfmadd213pd ymm18,ymm0,qword bcst [7FF91F5EE878]
       vbroadcastsd ymm19,qword ptr [7FF91F5EE880]
       vfmadd213pd ymm19,ymm0,qword bcst [7FF91F5EE888]
       vfmadd213pd ymm18,ymm4,ymm19
       vfmadd231pd ymm18,ymm5,qword bcst [7FF91F5EE890]
       vbroadcastsd ymm19,qword ptr [7FF91F5EE898]
       vfmadd213pd ymm19,ymm0,qword bcst [7FF91F5EE8A0]
       vbroadcastsd ymm20,qword ptr [7FF91F5EE8A8]
       vfmadd213pd ymm20,ymm0,qword bcst [7FF91F5EE8B0]
       vfmadd213pd ymm19,ymm4,ymm20
       vbroadcastsd ymm20,qword ptr [7FF91F5EE8B8]
       vfmadd213pd ymm20,ymm0,qword bcst [7FF91F5EE8C0]
       vbroadcastsd ymm21,qword ptr [7FF91F5EE8C8]
       vfmadd213pd ymm21,ymm0,qword bcst [7FF91F5EE8D0]
       vfmadd213pd ymm20,ymm4,ymm21
       vfmadd231pd ymm20,ymm19,ymm5
       vbroadcastsd ymm19,qword ptr [7FF91F5EE8D8]
       vfmadd213pd ymm19,ymm0,qword bcst [7FF91F5EE8E0]
       vbroadcastsd ymm21,qword ptr [7FF91F5EE8E8]
       vfmadd213pd ymm21,ymm0,qword bcst [7FF91F5EE8F0]
       vfmadd213pd ymm19,ymm4,ymm21
       vbroadcastsd ymm21,qword ptr [7FF91F5EE8F8]
       vfmadd213pd ymm21,ymm0,qword bcst [7FF91F5EE900]
       vfmadd213pd ymm4,ymm21,ymm0
       vfmadd213pd ymm5,ymm19,ymm4
       vfmadd213pd ymm16,ymm20,ymm5
       vfmadd213pd ymm17,ymm18,ymm16
       vmovaps   ymm0,ymm3
       vfmadd132pd ymm0,ymm17,qword bcst [7FF91F5EE908]
       vfmadd132pd ymm3,ymm0,qword bcst [7FF91F5EE910]
       vpternlogq ymm2,ymm3,ymm1,0AC
       vmovups   [rcx],ymm2
       mov       rax,rcx
       vzeroupper
       ret
M03_L01:
       vxorps    ymm3,ymm3,ymm3
       vpcmpgtq  ymm3,ymm3,ymm0
       vpternlogq ymm1,ymm3,qword bcst [7FF91F5EE918],0B8
       vxorps    ymm4,ymm4,ymm4
       vcmpeqpd  ymm4,ymm4,ymm0
       vpternlogq ymm1,ymm4,qword bcst [7FF91F5EE840],0B8
       vcmpneqpd ymm5,ymm0,ymm0
       vpternlogq ymm4,ymm5,ymm3,0FE
       vpcmpeqq  ymm3,ymm0,[7FF91F5EE920]
       vorpd     ymm3,ymm3,ymm4
       vandnpd   ymm2,ymm3,ymm2
       vmulpd    ymm4,ymm0,qword bcst [7FF91F5EE940]
       vpaddq    ymm4,ymm4,qword bcst [7FF91F5EE948]
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
       vandpd    ymm7,ymm6,qword bcst [7FF91F5EEDA0]
       vbroadcastsd ymm0,qword ptr [7FF91F5EEDA8]
       vpcmpgtq  ymm0,ymm0,ymm7
       vpcmpeqd  ymm1,ymm1,ymm1
       vptest    ymm0,ymm1
       jb        near ptr M04_L01
       vbroadcastsd ymm0,qword ptr [7FF91F5EEDB0]
       vpcmpgtq  ymm0,ymm0,ymm7
       vpcmpeqd  ymm1,ymm1,ymm1
       vptest    ymm0,ymm1
       jae       near ptr M04_L03
       vbroadcastsd ymm0,qword ptr [7FF91F5EEDB8]
       vmovaps   ymm1,ymm0
       vfmadd231pd ymm1,ymm7,qword bcst [7FF91F5EEDC0]
       vsubpd    ymm0,ymm1,ymm0
       vmovaps   ymm2,ymm7
       vfmadd231pd ymm2,ymm0,qword bcst [7FF91F5EEDC8]
       vmulpd    ymm3,ymm0,qword bcst [7FF91F5EEDD0]
       vsubpd    ymm4,ymm2,ymm3
       vsubpd    ymm2,ymm2,ymm4
       vsubpd    ymm3,ymm2,ymm3
       vbroadcastsd ymm2,qword ptr [7FF91F5EEDD8]
       vxorpd    ymm3,ymm3,ymm2
       vfmadd132pd ymm0,ymm3,qword bcst [7FF91F5EEDE0]
       vmovaps   ymm3,ymm0
       vsubpd    ymm0,ymm4,ymm3
       vsubpd    ymm4,ymm4,ymm0
       vsubpd    ymm3,ymm4,ymm3
       vmulpd    ymm4,ymm0,ymm0
       vmovaps   ymm5,ymm4
       vmulpd    ymm16,ymm0,ymm5
       vmulpd    ymm17,ymm5,ymm5
       vmovaps   ymm18,ymm17
       vbroadcastsd ymm19,qword ptr [7FF91F5EEDE8]
       vmulpd    ymm20,ymm18,ymm18
       vbroadcastsd ymm21,qword ptr [7FF91F5EEDF0]
       vfmadd213pd ymm21,ymm5,qword bcst [7FF91F5EEDF8]
       vbroadcastsd ymm22,qword ptr [7FF91F5EEE00]
       vfmadd213pd ymm22,ymm5,qword bcst [7FF91F5EEE08]
       vfmadd213pd ymm18,ymm21,ymm22
       vfmadd231pd ymm18,ymm20,qword bcst [7FF91F5EEE10]
       vmulpd    ymm18,ymm18,ymm16
       vxorpd    ymm18,ymm18,ymm2
       vfmadd231pd ymm18,ymm19,ymm3
       vxorpd    ymm21,ymm2,ymm3
       vfmadd213pd ymm5,ymm18,ymm21
       vfmadd231pd ymm5,ymm16,qword bcst [7FF91F5EEE18]
       vsubpd    ymm5,ymm0,ymm5
       vmovaps   ymm16,ymm4
       vmulpd    ymm16,ymm19,ymm16
       vbroadcastsd ymm8,qword ptr [7FF91F5EEE20]
       vsubpd    ymm18,ymm16,ymm8
       vbroadcastsd ymm19,qword ptr [7FF91F5EEE28]
       vfmadd213pd ymm19,ymm4,qword bcst [7FF91F5EEE30]
       vbroadcastsd ymm21,qword ptr [7FF91F5EEE38]
       vfmadd213pd ymm21,ymm4,qword bcst [7FF91F5EEE40]
       vbroadcastsd ymm22,qword ptr [7FF91F5EEE48]
       vfmadd213pd ymm4,ymm22,qword bcst [7FF91F5EEE50]
       vfmadd231pd ymm4,ymm17,ymm21
       vfmadd213pd ymm19,ymm20,ymm4
       vaddpd    ymm4,ymm8,ymm18
       vsubpd    ymm4,ymm4,ymm16
       vfmadd213pd ymm0,ymm3,ymm4
       vfmadd213pd ymm19,ymm17,ymm0
       vsubpd    ymm0,ymm19,ymm18
       vbroadcastsd ymm3,qword ptr [7FF91F5EEE58]
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
       vpandq    ymm0,ymm0,qword bcst [7FF91F5EEE60]
       vxorps    ymm1,ymm1,ymm1
       vpcmpeqq  ymm0,ymm1,ymm0
       vxorpd    ymm1,ymm2,ymm10
       vblendvpd ymm10,ymm1,ymm10,ymm0
M04_L00:
       vpcmpgtq  k1,ymm7,qword bcst [7FF91F5EEE68]
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
       vpcmpgtq  ymm1,ymm7,[7FF91F5EEE80]
       vptest    ymm1,ymm1
       je        near ptr M04_L02
       vmovaps   ymm1,ymm0
       vmulpd    ymm9,ymm6,ymm1
       vmulpd    ymm2,ymm1,ymm1
       vmovaps   ymm3,ymm2
       vbroadcastsd ymm4,qword ptr [7FF91F5EEE10]
       vfmadd213pd ymm4,ymm1,qword bcst [7FF91F5EEDF0]
       vmulpd    ymm5,ymm3,ymm3
       vbroadcastsd ymm16,qword ptr [7FF91F5EEDF8]
       vfmadd213pd ymm16,ymm1,qword bcst [7FF91F5EEE00]
       vbroadcastsd ymm17,qword ptr [7FF91F5EEE08]
       vfmadd213pd ymm1,ymm17,qword bcst [7FF91F5EEEA0]
       vfmadd213pd ymm3,ymm16,ymm1
       vfmadd213pd ymm4,ymm5,ymm3
       vfmadd213pd ymm9,ymm4,ymm6
       vbroadcastsd ymm1,qword ptr [7FF91F5EEE28]
       vfmadd213pd ymm1,ymm0,qword bcst [7FF91F5EEE30]
       vbroadcastsd ymm3,qword ptr [7FF91F5EEE38]
       vfmadd213pd ymm3,ymm0,qword bcst [7FF91F5EEE40]
       vbroadcastsd ymm4,qword ptr [7FF91F5EEE48]
       vfmadd213pd ymm0,ymm4,qword bcst [7FF91F5EEE50]
       vfmadd213pd ymm2,ymm3,ymm0
       vfmadd213pd ymm1,ymm5,ymm2
       vfmadd213pd ymm1,ymm10,qword bcst [7FF91F5EEEA8]
       vbroadcastsd ymm8,qword ptr [7FF91F5EEE20]
       vfmadd213pd ymm10,ymm1,ymm8
       jmp       near ptr M04_L00
M04_L02:
       vmulpd    ymm0,ymm6,ymm10
       vmovaps   ymm9,ymm6
       vfmadd231pd ymm9,ymm0,qword bcst [7FF91F5EEEA0]
       vbroadcastsd ymm8,qword ptr [7FF91F5EEE20]
       vfmadd132pd ymm10,ymm8,qword bcst [7FF91F5EEEA8]
       jmp       near ptr M04_L00
M04_L03:
       vzeroupper
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       jmp       qword ptr [7FF91F976AD8]
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
       vpsllq    ymm5,ymm4,17
       vpsrlq    ymm4,ymm4,29
       vpor      ymm4,ymm4,ymm5
       vpsllq    ymm5,ymm1,11
       vpxor     ymm2,ymm0,ymm2
       vpxor     ymm3,ymm1,ymm3
       vpxor     ymm1,ymm2,ymm1
       vpaddq    ymm4,ymm0,ymm4
       vpxor     ymm0,ymm3,ymm0
       vpxor     ymm2,ymm2,ymm5
       vpsllq    ymm5,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpor      ymm3,ymm3,ymm5
       vmovups   [rax+28],ymm0
       vmovups   [rax+48],ymm1
       vmovups   [rax+68],ymm2
       vmovups   [rax+88],ymm3
       vmovq     rax,xmm4
       vzeroupper
       ret
; Total bytes of code 120
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
       vpsllq    ymm1,ymm0,17
       vpsrlq    ymm0,ymm0,29
       vpor      ymm0,ymm0,ymm1
       vpsllq    ymm1,ymm7,11
       vpxor     ymm2,ymm6,ymm8
       vpxor     ymm3,ymm7,ymm9
       vpxor     ymm4,ymm7,ymm2
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm5,ymm3,ymm6
       vpxor     ymm2,ymm2,ymm1
       vpsllq    ymm1,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpor      ymm3,ymm3,ymm1
       vpaddq    ymm1,ymm3,ymm5
       vpsllq    ymm16,ymm1,17
       vpsrlq    ymm1,ymm1,29
       vpord     ymm1,ymm1,ymm16
       vpsllq    ymm16,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm1,ymm5,ymm1
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm16
       vpsllq    ymm16,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpord     ymm3,ymm3,ymm16
       vpaddq    ymm16,ymm3,ymm5
       vpsllq    ymm17,ymm16,17
       vpsrlq    ymm16,ymm16,29
       vpord     ymm16,ymm16,ymm17
       vpsllq    ymm17,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm16,ymm5,ymm16
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm17
       vpsllq    ymm17,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpord     ymm3,ymm3,ymm17
       vpaddq    ymm17,ymm3,ymm5
       vpsllq    ymm18,ymm17,17
       vpsrlq    ymm17,ymm17,29
       vpord     ymm17,ymm17,ymm18
       vpsllq    ymm18,ymm4,11
       vpxor     ymm8,ymm2,ymm5
       vpxor     ymm9,ymm3,ymm4
       vpxor     ymm7,ymm4,ymm8
       vpaddq    ymm2,ymm5,ymm17
       vpxor     ymm6,ymm5,ymm9
       vpxord    ymm8,ymm8,ymm18
       vpsllq    ymm3,ymm9,2D
       vpsrlq    ymm4,ymm9,13
       vpor      ymm9,ymm4,ymm3
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
       call      qword ptr [7FF91F996580]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(UInt64[])
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
       vpsllq    ymm1,ymm0,17
       vpsrlq    ymm0,ymm0,29
       vpor      ymm0,ymm0,ymm1
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vpsllq    ymm1,ymm9,2D
       vpsrlq    ymm2,ymm9,13
       vpor      ymm9,ymm2,ymm1
       vmovups   [rcx],ymm0
       add       rcx,20
       inc       eax
       jmp       near ptr M00_L03
M00_L08:
       vpaddq    ymm0,ymm6,ymm9
       vpsllq    ymm1,ymm0,17
       vpsrlq    ymm0,ymm0,29
       vpor      ymm0,ymm0,ymm1
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vmovups   [rsp+20],ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vpsllq    ymm0,ymm9,2D
       vpsrlq    ymm1,ymm9,13
       vpor      ymm9,ymm1,ymm0
       lea       rdx,[rsp+20]
       vextractf128 xmm10,ymm6,1
       vextractf128 xmm11,ymm7,1
       vextractf128 xmm12,ymm9,1
       vextractf128 xmm13,ymm8,1
       call      qword ptr [7FF91F996628]
       vinsertf128 ymm6,ymm6,xmm10,1
       vinsertf128 ymm7,ymm7,xmm11,1
       vinsertf128 ymm9,ymm9,xmm12,1
       vinsertf128 ymm8,ymm8,xmm13,1
       jmp       near ptr M00_L04
; Total bytes of code 841
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
       vpsllq    ymm1,ymm0,17
       vpsrlq    ymm0,ymm0,29
       vpor      ymm0,ymm0,ymm1
       vpsllq    ymm1,ymm7,11
       vpxor     ymm2,ymm6,ymm8
       vpxor     ymm3,ymm7,ymm9
       vpxor     ymm4,ymm7,ymm2
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm5,ymm3,ymm6
       vpxor     ymm2,ymm2,ymm1
       vpsllq    ymm1,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpor      ymm3,ymm3,ymm1
       vpsrlq    ymm0,ymm0,0B
       vcvtuqq2pd ymm0,ymm0
       vbroadcastsd ymm1,qword ptr [7FF91F5ED250]
       vmulpd    ymm0,ymm0,ymm1
       vpaddq    ymm16,ymm3,ymm5
       vpsllq    ymm17,ymm16,17
       vpsrlq    ymm16,ymm16,29
       vpord     ymm16,ymm16,ymm17
       vpsllq    ymm17,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm16,ymm5,ymm16
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm17
       vpsllq    ymm17,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpord     ymm3,ymm3,ymm17
       vpsrlq    ymm16,ymm16,0B
       vcvtuqq2pd ymm16,ymm16
       vmulpd    ymm16,ymm16,ymm1
       vpaddq    ymm17,ymm3,ymm5
       vpsllq    ymm18,ymm17,17
       vpsrlq    ymm17,ymm17,29
       vpord     ymm17,ymm17,ymm18
       vpsllq    ymm18,ymm4,11
       vpxor     ymm2,ymm2,ymm5
       vpxor     ymm3,ymm3,ymm4
       vpxor     ymm4,ymm2,ymm4
       vpaddq    ymm17,ymm5,ymm17
       vpxor     ymm5,ymm3,ymm5
       vpxord    ymm2,ymm2,ymm18
       vpsllq    ymm18,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpord     ymm3,ymm3,ymm18
       vpsrlq    ymm17,ymm17,0B
       vcvtuqq2pd ymm17,ymm17
       vmulpd    ymm17,ymm17,ymm1
       vpaddq    ymm18,ymm3,ymm5
       vpsllq    ymm19,ymm18,17
       vpsrlq    ymm18,ymm18,29
       vpord     ymm18,ymm18,ymm19
       vpsllq    ymm19,ymm4,11
       vpxor     ymm8,ymm2,ymm5
       vpxor     ymm9,ymm3,ymm4
       vpxor     ymm7,ymm4,ymm8
       vpaddq    ymm2,ymm5,ymm18
       vpxor     ymm6,ymm5,ymm9
       vpxord    ymm8,ymm8,ymm19
       vpsllq    ymm3,ymm9,2D
       vpsrlq    ymm4,ymm9,13
       vpor      ymm9,ymm4,ymm3
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
       call      qword ptr [7FF91F976580]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
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
       vpsllq    ymm1,ymm0,17
       vpsrlq    ymm0,ymm0,29
       vpor      ymm0,ymm0,ymm1
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vpsllq    ymm1,ymm9,2D
       vpsrlq    ymm2,ymm9,13
       vpor      ymm9,ymm2,ymm1
       vpsrlq    ymm0,ymm0,0B
       vcvtuqq2pd ymm0,ymm0
       vbroadcastsd ymm1,qword ptr [7FF91F5ED250]
       vmulpd    ymm1,ymm0,ymm1
       vmovups   [rcx],ymm1
       add       rcx,20
       inc       eax
       jmp       near ptr M00_L03
M00_L07:
       vpaddq    ymm0,ymm6,ymm9
       vpsllq    ymm1,ymm0,17
       vpsrlq    ymm0,ymm0,29
       vpor      ymm0,ymm0,ymm1
       vpsllq    ymm1,ymm7,11
       vpxor     ymm8,ymm6,ymm8
       vpxor     ymm9,ymm7,ymm9
       vpxor     ymm7,ymm7,ymm8
       vpaddq    ymm0,ymm6,ymm0
       vpxor     ymm6,ymm6,ymm9
       vpxor     ymm8,ymm8,ymm1
       vpsllq    ymm1,ymm9,2D
       vpsrlq    ymm2,ymm9,13
       vpor      ymm9,ymm2,ymm1
       vpsrlq    ymm0,ymm0,0B
       vcvtuqq2pd ymm0,ymm0
       vbroadcastsd ymm1,qword ptr [7FF91F5ED250]
       vmulpd    ymm0,ymm0,ymm1
       vmovups   [rsp+20],ymm0
       lea       rdx,[rsp+20]
       vextractf128 xmm10,ymm6,1
       vextractf128 xmm11,ymm7,1
       vextractf128 xmm12,ymm9,1
       vextractf128 xmm13,ymm8,1
       call      qword ptr [7FF91F976688]
       vinsertf128 ymm6,ymm6,xmm10,1
       vinsertf128 ymm7,ymm7,xmm11,1
       vinsertf128 ymm9,ymm9,xmm12,1
       vinsertf128 ymm8,ymm8,xmm13,1
       jmp       near ptr M00_L04
; Total bytes of code 970
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
       call      qword ptr [7FF91F996538]; Anastasya.Metaheuristics.Core.Randomness.RandomSource.Fill(System.Span`1<Int32>, Int32, Int32)
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
       mov       rax,4A75B7A471FB
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
       mov       [rbp+18],rdx
       test      [rsp],esp
       sub       rsp,20
       lea       rax,[rsp+20]
       vxorps    ymm4,ymm4,ymm4
       vmovdqu   ymmword ptr [rax],ymm4
       mov       rbx,rax
       xor       esi,esi
       cmp       esi,r11d
       jge       short M01_L04
M01_L02:
       vpaddq    ymm4,ymm0,ymm3
       vpsllq    ymm5,ymm4,17
       vpsrlq    ymm4,ymm4,29
       vpor      ymm4,ymm4,ymm5
       vpsllq    ymm5,ymm1,11
       vpxor     ymm2,ymm0,ymm2
       vpxor     ymm3,ymm1,ymm3
       vpxor     ymm1,ymm2,ymm1
       vpaddq    ymm4,ymm0,ymm4
       vpxor     ymm0,ymm3,ymm0
       vpxor     ymm2,ymm2,ymm5
       vpsllq    ymm5,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpor      ymm3,ymm3,ymm5
       vmovups   [rbx],ymm4
       xor       edi,edi
       cmp       esi,r11d
       jl        short M01_L10
M01_L03:
       cmp       esi,r11d
       jl        short M01_L02
M01_L04:
       vmovups   [rcx+28],ymm0
       vmovups   [rcx+48],ymm1
       vmovups   [rcx+68],ymm2
       vmovups   [rcx+88],ymm3
M01_L05:
       mov       r8,4A75B7A471FB
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
       jge       short M01_L03
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
       cmp       edi,4
       jl        short M01_L09
       mov       esi,r14d
       jmp       near ptr M01_L03
M01_L13:
       mov       rcx,offset MT_System.ArgumentException
       call      CORINFO_HELP_NEWSFAST
       mov       rbx,rax
       mov       ecx,0B5
       mov       rdx,7FF91F969FF8
       call      qword ptr [7FF91F7B7798]
       mov       rsi,rax
       mov       ecx,5B
       mov       rdx,7FF91F969FF8
       call      qword ptr [7FF91F7B7798]
       mov       r8,rax
       mov       rdx,rsi
       mov       rcx,rbx
       call      qword ptr [7FF91F905E48]
       mov       rcx,rbx
       call      CORINFO_HELP_THROW
       int       3
; Total bytes of code 502
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
       call      qword ptr [7FF91F996568]; Anastasya.Metaheuristics.Core.Randomness.StandardNormal.Fill(Anastasya.Metaheuristics.Core.Randomness.RandomSource, System.Span`1<Double>)
       mov       rcx,[rbx+28]
       call      qword ptr [7FF91F996580]; Anastasya.Metaheuristics.Benchmarks.RandomSourceComparisonBenchmarksBase.Last(Double[])
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
       mov       rax,705A7BBA93FA
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
       vbroadcastsd ymm7,qword ptr [7FF91F608D00]
       vbroadcastsd ymm8,qword ptr [7FF91F608D08]
       test      edi,edi
       jle       near ptr M01_L08
M01_L04:
       vmovups   ymm0,[rbx+28]
       vmovups   ymm1,[rbx+48]
       vmovups   ymm2,[rbx+68]
       vmovups   ymm3,[rbx+88]
       vpaddq    ymm4,ymm0,ymm3
       vpsllq    ymm5,ymm4,17
       vpsrlq    ymm4,ymm4,29
       vpor      ymm4,ymm4,ymm5
       vpsllq    ymm5,ymm1,11
       vpxor     ymm2,ymm0,ymm2
       vpxor     ymm3,ymm1,ymm3
       vpxor     ymm1,ymm2,ymm1
       vpaddq    ymm4,ymm0,ymm4
       vpxor     ymm0,ymm3,ymm0
       vpxor     ymm2,ymm2,ymm5
       vpsllq    ymm5,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpor      ymm3,ymm3,ymm5
       vmovups   [rbx+28],ymm0
       vmovups   [rbx+48],ymm1
       vmovups   [rbx+68],ymm2
       vmovups   [rbx+88],ymm3
       vpsrlq    ymm0,ymm4,0B
       vcvtuqq2pd ymm0,ymm0
       vmulpd    ymm0,ymm0,qword bcst [7FF91F608D10]
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
       vbroadcastsd ymm0,qword ptr [7FF91F608D18]
       vsubpd    ymm0,ymm0,[r14]
       vmovups   [rbp+30],ymm0
       lea       rdx,[rbp+30]
       lea       rcx,[rbp+90]
       vextractf128 xmm9,ymm6,1
       vextractf128 xmm10,ymm7,1
       vextractf128 xmm11,ymm8,1
       call      qword ptr [7FF91F996880]; System.Runtime.Intrinsics.VectorMath.LogDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.UInt64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
       vmovups   ymm12,[rbp+90]
       vinsertf128 ymm8,ymm8,xmm11,1
       vmulpd    ymm0,ymm8,[r15]
       vmovups   [rbp+30],ymm0
       lea       rdx,[rbp+30]
       lea       rcx,[rbp+50]
       vextractf128 xmm13,ymm12,1
       vextractf128 xmm11,ymm8,1
       call      qword ptr [7FF91F9968B0]; System.Runtime.Intrinsics.VectorMath.SinCosDouble[[System.Numerics.Vector`1[[System.Double, System.Private.CoreLib]], System.Private.CoreLib],[System.Numerics.Vector`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib]](System.Numerics.Vector`1<Double>)
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
       nop       dword ptr [rax]
M01_L07:
       movsxd    rdx,ecx
       vmovsd    xmm0,qword ptr [r13+rdx*8]
       vmovsd    qword ptr [rsi+rdx*8],xmm0
       inc       ecx
       cmp       ecx,edi
       jl        short M01_L07
M01_L08:
       mov       r8,705A7BBA93FA
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
       mov       rdx,7FF91F969FF8
       call      qword ptr [7FF91F7B7798]
       mov       rcx,rax
       call      qword ptr [7FF91F996970]
       int       3
M01_L11:
       call      CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION
       int       3
; Total bytes of code 861
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
       vpaddq    ymm2,ymm0,qword bcst [7FF91F60E840]
       vpcmpnltuq k1,ymm2,qword bcst [7FF91F60E848]
       vpmovm2q  ymm2,k1
       vptest    ymm2,ymm2
       jne       near ptr M03_L01
M03_L00:
       vmovups   ymm0,[rdx]
       vpaddq    ymm0,ymm0,qword bcst [7FF91F60E850]
       vpsraq    ymm3,ymm0,34
       vcvtqq2pd ymm3,ymm3
       vpandq    ymm0,ymm0,qword bcst [7FF91F60E858]
       vpaddq    ymm0,ymm0,qword bcst [7FF91F60E860]
       vsubpd    ymm0,ymm0,qword bcst [7FF91F60E868]
       vmulpd    ymm4,ymm0,ymm0
       vmulpd    ymm5,ymm4,ymm4
       vmulpd    ymm16,ymm5,ymm5
       vmulpd    ymm17,ymm16,ymm16
       vbroadcastsd ymm18,qword ptr [7FF91F60E870]
       vfmadd213pd ymm18,ymm0,qword bcst [7FF91F60E878]
       vbroadcastsd ymm19,qword ptr [7FF91F60E880]
       vfmadd213pd ymm19,ymm0,qword bcst [7FF91F60E888]
       vfmadd213pd ymm18,ymm4,ymm19
       vfmadd231pd ymm18,ymm5,qword bcst [7FF91F60E890]
       vbroadcastsd ymm19,qword ptr [7FF91F60E898]
       vfmadd213pd ymm19,ymm0,qword bcst [7FF91F60E8A0]
       vbroadcastsd ymm20,qword ptr [7FF91F60E8A8]
       vfmadd213pd ymm20,ymm0,qword bcst [7FF91F60E8B0]
       vfmadd213pd ymm19,ymm4,ymm20
       vbroadcastsd ymm20,qword ptr [7FF91F60E8B8]
       vfmadd213pd ymm20,ymm0,qword bcst [7FF91F60E8C0]
       vbroadcastsd ymm21,qword ptr [7FF91F60E8C8]
       vfmadd213pd ymm21,ymm0,qword bcst [7FF91F60E8D0]
       vfmadd213pd ymm20,ymm4,ymm21
       vfmadd231pd ymm20,ymm19,ymm5
       vbroadcastsd ymm19,qword ptr [7FF91F60E8D8]
       vfmadd213pd ymm19,ymm0,qword bcst [7FF91F60E8E0]
       vbroadcastsd ymm21,qword ptr [7FF91F60E8E8]
       vfmadd213pd ymm21,ymm0,qword bcst [7FF91F60E8F0]
       vfmadd213pd ymm19,ymm4,ymm21
       vbroadcastsd ymm21,qword ptr [7FF91F60E8F8]
       vfmadd213pd ymm21,ymm0,qword bcst [7FF91F60E900]
       vfmadd213pd ymm4,ymm21,ymm0
       vfmadd213pd ymm5,ymm19,ymm4
       vfmadd213pd ymm16,ymm20,ymm5
       vfmadd213pd ymm17,ymm18,ymm16
       vmovaps   ymm0,ymm3
       vfmadd132pd ymm0,ymm17,qword bcst [7FF91F60E908]
       vfmadd132pd ymm3,ymm0,qword bcst [7FF91F60E910]
       vpternlogq ymm2,ymm3,ymm1,0AC
       vmovups   [rcx],ymm2
       mov       rax,rcx
       vzeroupper
       ret
M03_L01:
       vxorps    ymm3,ymm3,ymm3
       vpcmpgtq  ymm3,ymm3,ymm0
       vpternlogq ymm1,ymm3,qword bcst [7FF91F60E918],0B8
       vxorps    ymm4,ymm4,ymm4
       vcmpeqpd  ymm4,ymm4,ymm0
       vpternlogq ymm1,ymm4,qword bcst [7FF91F60E840],0B8
       vcmpneqpd ymm5,ymm0,ymm0
       vpternlogq ymm4,ymm5,ymm3,0FE
       vpcmpeqq  ymm3,ymm0,[7FF91F60E920]
       vorpd     ymm3,ymm3,ymm4
       vandnpd   ymm2,ymm3,ymm2
       vmulpd    ymm4,ymm0,qword bcst [7FF91F60E940]
       vpaddq    ymm4,ymm4,qword bcst [7FF91F60E948]
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
       vandpd    ymm7,ymm6,qword bcst [7FF91F60EDA0]
       vbroadcastsd ymm0,qword ptr [7FF91F60EDA8]
       vpcmpgtq  ymm0,ymm0,ymm7
       vpcmpeqd  ymm1,ymm1,ymm1
       vptest    ymm0,ymm1
       jb        near ptr M04_L01
       vbroadcastsd ymm0,qword ptr [7FF91F60EDB0]
       vpcmpgtq  ymm0,ymm0,ymm7
       vpcmpeqd  ymm1,ymm1,ymm1
       vptest    ymm0,ymm1
       jae       near ptr M04_L03
       vbroadcastsd ymm0,qword ptr [7FF91F60EDB8]
       vmovaps   ymm1,ymm0
       vfmadd231pd ymm1,ymm7,qword bcst [7FF91F60EDC0]
       vsubpd    ymm0,ymm1,ymm0
       vmovaps   ymm2,ymm7
       vfmadd231pd ymm2,ymm0,qword bcst [7FF91F60EDC8]
       vmulpd    ymm3,ymm0,qword bcst [7FF91F60EDD0]
       vsubpd    ymm4,ymm2,ymm3
       vsubpd    ymm2,ymm2,ymm4
       vsubpd    ymm3,ymm2,ymm3
       vbroadcastsd ymm2,qword ptr [7FF91F60EDD8]
       vxorpd    ymm3,ymm3,ymm2
       vfmadd132pd ymm0,ymm3,qword bcst [7FF91F60EDE0]
       vmovaps   ymm3,ymm0
       vsubpd    ymm0,ymm4,ymm3
       vsubpd    ymm4,ymm4,ymm0
       vsubpd    ymm3,ymm4,ymm3
       vmulpd    ymm4,ymm0,ymm0
       vmovaps   ymm5,ymm4
       vmulpd    ymm16,ymm0,ymm5
       vmulpd    ymm17,ymm5,ymm5
       vmovaps   ymm18,ymm17
       vbroadcastsd ymm19,qword ptr [7FF91F60EDE8]
       vmulpd    ymm20,ymm18,ymm18
       vbroadcastsd ymm21,qword ptr [7FF91F60EDF0]
       vfmadd213pd ymm21,ymm5,qword bcst [7FF91F60EDF8]
       vbroadcastsd ymm22,qword ptr [7FF91F60EE00]
       vfmadd213pd ymm22,ymm5,qword bcst [7FF91F60EE08]
       vfmadd213pd ymm18,ymm21,ymm22
       vfmadd231pd ymm18,ymm20,qword bcst [7FF91F60EE10]
       vmulpd    ymm18,ymm18,ymm16
       vxorpd    ymm18,ymm18,ymm2
       vfmadd231pd ymm18,ymm19,ymm3
       vxorpd    ymm21,ymm2,ymm3
       vfmadd213pd ymm5,ymm18,ymm21
       vfmadd231pd ymm5,ymm16,qword bcst [7FF91F60EE18]
       vsubpd    ymm5,ymm0,ymm5
       vmovaps   ymm16,ymm4
       vmulpd    ymm16,ymm19,ymm16
       vbroadcastsd ymm8,qword ptr [7FF91F60EE20]
       vsubpd    ymm18,ymm16,ymm8
       vbroadcastsd ymm19,qword ptr [7FF91F60EE28]
       vfmadd213pd ymm19,ymm4,qword bcst [7FF91F60EE30]
       vbroadcastsd ymm21,qword ptr [7FF91F60EE38]
       vfmadd213pd ymm21,ymm4,qword bcst [7FF91F60EE40]
       vbroadcastsd ymm22,qword ptr [7FF91F60EE48]
       vfmadd213pd ymm4,ymm22,qword bcst [7FF91F60EE50]
       vfmadd231pd ymm4,ymm17,ymm21
       vfmadd213pd ymm19,ymm20,ymm4
       vaddpd    ymm4,ymm8,ymm18
       vsubpd    ymm4,ymm4,ymm16
       vfmadd213pd ymm0,ymm3,ymm4
       vfmadd213pd ymm19,ymm17,ymm0
       vsubpd    ymm0,ymm19,ymm18
       vbroadcastsd ymm3,qword ptr [7FF91F60EE58]
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
       vpandq    ymm0,ymm0,qword bcst [7FF91F60EE60]
       vxorps    ymm1,ymm1,ymm1
       vpcmpeqq  ymm0,ymm1,ymm0
       vxorpd    ymm1,ymm2,ymm10
       vblendvpd ymm10,ymm1,ymm10,ymm0
M04_L00:
       vpcmpgtq  k1,ymm7,qword bcst [7FF91F60EE68]
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
       vpcmpgtq  ymm1,ymm7,[7FF91F60EE80]
       vptest    ymm1,ymm1
       je        near ptr M04_L02
       vmovaps   ymm1,ymm0
       vmulpd    ymm9,ymm6,ymm1
       vmulpd    ymm2,ymm1,ymm1
       vmovaps   ymm3,ymm2
       vbroadcastsd ymm4,qword ptr [7FF91F60EE10]
       vfmadd213pd ymm4,ymm1,qword bcst [7FF91F60EDF0]
       vmulpd    ymm5,ymm3,ymm3
       vbroadcastsd ymm16,qword ptr [7FF91F60EDF8]
       vfmadd213pd ymm16,ymm1,qword bcst [7FF91F60EE00]
       vbroadcastsd ymm17,qword ptr [7FF91F60EE08]
       vfmadd213pd ymm1,ymm17,qword bcst [7FF91F60EEA0]
       vfmadd213pd ymm3,ymm16,ymm1
       vfmadd213pd ymm4,ymm5,ymm3
       vfmadd213pd ymm9,ymm4,ymm6
       vbroadcastsd ymm1,qword ptr [7FF91F60EE28]
       vfmadd213pd ymm1,ymm0,qword bcst [7FF91F60EE30]
       vbroadcastsd ymm3,qword ptr [7FF91F60EE38]
       vfmadd213pd ymm3,ymm0,qword bcst [7FF91F60EE40]
       vbroadcastsd ymm4,qword ptr [7FF91F60EE48]
       vfmadd213pd ymm0,ymm4,qword bcst [7FF91F60EE50]
       vfmadd213pd ymm2,ymm3,ymm0
       vfmadd213pd ymm1,ymm5,ymm2
       vfmadd213pd ymm1,ymm10,qword bcst [7FF91F60EEA8]
       vbroadcastsd ymm8,qword ptr [7FF91F60EE20]
       vfmadd213pd ymm10,ymm1,ymm8
       jmp       near ptr M04_L00
M04_L02:
       vmulpd    ymm0,ymm6,ymm10
       vmovaps   ymm9,ymm6
       vfmadd231pd ymm9,ymm0,qword bcst [7FF91F60EEA0]
       vbroadcastsd ymm8,qword ptr [7FF91F60EE20]
       vfmadd132pd ymm10,ymm8,qword bcst [7FF91F60EEA8]
       jmp       near ptr M04_L00
M04_L03:
       vzeroupper
       vmovaps   xmm6,[rsp+40]
       vmovaps   xmm7,[rsp+30]
       vmovaps   xmm8,[rsp+20]
       vmovaps   xmm9,[rsp+10]
       vmovaps   xmm10,[rsp]
       add       rsp,58
       jmp       qword ptr [7FF91F996AD8]
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
       vpsllq    ymm5,ymm4,17
       vpsrlq    ymm4,ymm4,29
       vpor      ymm4,ymm4,ymm5
       vpsllq    ymm5,ymm1,11
       vpxor     ymm2,ymm0,ymm2
       vpxor     ymm3,ymm1,ymm3
       vpxor     ymm1,ymm2,ymm1
       vpaddq    ymm4,ymm0,ymm4
       vpxor     ymm0,ymm3,ymm0
       vpxor     ymm2,ymm2,ymm5
       vpsllq    ymm5,ymm3,2D
       vpsrlq    ymm3,ymm3,13
       vpor      ymm3,ymm3,ymm5
       vmovups   [rax+28],ymm0
       vmovups   [rax+48],ymm1
       vmovups   [rax+68],ymm2
       vmovups   [rax+88],ymm3
       vmovq     rax,xmm4
       vzeroupper
       ret
; Total bytes of code 120
```

