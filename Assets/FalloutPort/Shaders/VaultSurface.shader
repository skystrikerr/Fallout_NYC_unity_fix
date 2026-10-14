Shader "FalloutPort/VaultSurface"
{
 Properties { _BaseMap("Original texture",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1) _Emission("Emission",Color)=(0,0,0,1) _VertexColor("Vertex colors",Float)=1 _Cutoff("Alpha cutoff",Float)=0.05 }
 SubShader {
 Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
 Pass {
 Tags { "LightMode"="UniversalForward" }
 Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct V {float4 positionCS:SV_POSITION;float3 normalWS:TEXCOORD0;float2 uv:TEXCOORD1;float4 color:COLOR;};
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor,_Emission,_BaseMap_ST;float _VertexColor,_Cutoff;
 CBUFFER_END
 V vert(A a){V o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.normalWS=TransformObjectToWorldNormal(a.normalOS);o.uv=TRANSFORM_TEX(a.uv,_BaseMap);o.color=a.color;return o;}
 half4 frag(V i):SV_Target{half4 tex=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(tex.a-_Cutoff);half3 albedo=tex.rgb*_BaseColor.rgb*lerp(half3(1,1,1),i.color.rgb,_VertexColor);Light key=GetMainLight();half3 light=half3(.48,.50,.53)+key.color*saturate(dot(normalize(i.normalWS),key.direction))*.48;return half4(albedo*light+tex.rgb*_Emission.rgb,1);}
 ENDHLSL
 }
 }
}
