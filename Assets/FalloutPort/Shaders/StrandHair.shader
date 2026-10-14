Shader "FalloutPort/StrandHair" {
 Properties {_BaseMap("Hair colour",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1)}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"}
 Pass {Tags {"LightMode"="UniversalForward"} Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;float4 c:COLOR;};
 struct V {float4 p:SV_POSITION;float3 n:TEXCOORD0;float2 uv:TEXCOORD1;float4 c:COLOR;};
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial) float4 _BaseColor; CBUFFER_END
 V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.n=TransformObjectToWorldNormal(a.n);o.uv=a.uv;o.c=a.c;return o;}
 half4 frag(V i):SV_Target{
 float u=i.uv.x+sin(i.uv.y*8+i.c.r*15)*.007;
 float strand=floor(u*24);float seed=frac(sin(strand*127.1+i.c.g*43)*43758.54);
 float width=abs(u-.5)*2;float tip=.91+seed*.09;clip(tip-i.uv.y);
 clip(1-width-(.015+seed*.055));
 float grooves=.68+.32*pow(abs(sin(u*24*3.14159)),.45);
 half3 base=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,float2(u,i.uv.y)).rgb*_BaseColor.rgb;
 Light light=GetMainLight();float diffuse=.58+max(0,dot(normalize(i.n),light.direction))*.45;
 return half4(base*grooves*diffuse*lerp(.85,1.22,i.c.r),1);
 }
 ENDHLSL
 }
 }
}
