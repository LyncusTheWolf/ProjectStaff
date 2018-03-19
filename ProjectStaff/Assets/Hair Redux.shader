Shader "Custom/Fur Redux" {
	Properties {
		_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Root("Root dimming", Range(0.0, 1.0)) = 1
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex("Noise", 2D) = "white" {}
		_HeightTex("Height Map", 2D) = "white" {}
		_Length ("Length", Range(0.0, 20.0)) = 1
		_Gravity("Gravity", Range(0.0, 20.0)) = 1
		_Inter("Interpolation Factor", Range(0.0, 70)) = 4
		_Step("Detail", Range(1, 10)) = 8 
		_DistortionIntensity("Distortion", Range(0.0, 10.0)) = 1
	}

	SubShader {
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
		LOD 100

		//Perform the base mesh pass
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform float4 _Color;

			float4 _LightColor0;

			struct appdata{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f{
				float4 vertex : SV_POSITION;
				float4 col : COLOR;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _Root;

			v2f vert(appdata v){
				v2f o;

				float3 normalDirection = normalize(mul(float4(v.normal, 0), unity_WorldToObject).xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float atten = 1.0;

				float3 diffuseReflection = atten * _LightColor0.xyz * max(0.0, dot(normalDirection, lightDirection));
				float3 lightFinal = diffuseReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;

				o.col = float4(lightFinal * _Color.rgb, 1.0);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * (1 - _Root);
				return i.col * col;
			}
			ENDCG
		}
		
		//Perform the hair strand pass
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2g {
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
				fixed3 col : COLOR;
				float2 heightUV: TEXCOORD1;
			};

			struct g2f {
				float4 vertex : SV_POSITION;
				//UNITY_FOG_COORDS(1)
				//float2 uv : TEXCOORD0;
				//float3 normal: TEXCOORD1;
				fixed3 baseCol : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;

			sampler2D _HeightTex;
			float4 _HeightTex_ST;

			uniform float4 _Color;
			fixed _Root;
			float4 _LightColor0;
			float _Length;
			float _Gravity;
			int _Inter;
			int _Step;
			float _DistortionIntensity;
			
			v2g vert (appdata v) {
				v2g o;
				o.vertex = v.vertex;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.heightUV = TRANSFORM_TEX(v.uv, _HeightTex);

				float3 noise = tex2Dlod(_NoiseTex, float4(o.uv, 0, 0)).xyz;
				float3 noiseNormal = (noise * 2 - 1) * _DistortionIntensity;
				o.normal = normalize(float4((v.normal + noiseNormal).xyz, 0));

				float3 normalDirection = normalize(mul(float4(v.normal.xyz, 0), unity_WorldToObject).xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float atten = 1.0;

				float3 diffuseReflection = atten * _LightColor0.xyz * max(0.0, dot(normalDirection, lightDirection));
				float3 lightFinal = diffuseReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;

				o.col = lightFinal;

				//o.normal = v.normal;				
				return o;
			}

			/*float4 bezier(float4 p0, float4 p1, float4 p2, float t) {
				float oneMinusT = 1.0 - t;
				return
					oneMinusT * oneMinusT * p0 +
					2.0f * oneMinusT * t * p1 +
					t * t * p2;
			}*/

			void renderStrand(triangle v2g input[3], float weight1, float weight2, float weight3, inout LineStream<g2f> OutputStream) {
				g2f pIn;

				float stepCache = 1.0f / _Step;

				float4 pA = ((input[0].vertex * weight1) + (input[1].vertex * weight2) + (input[2].vertex * weight3));
				half4 nA = ((input[0].normal * weight1) + (input[1].normal * weight2) + (input[2].normal * weight3));
				//pIn.normal = nA;

				float2 uvA = ((input[0].uv * weight1) + (input[1].uv * weight2) + (input[2].uv * weight3));
				//pIn.uv = uvA;

				float2 uvHA = ((input[0].heightUV * weight1) + (input[1].heightUV * weight2) + (input[2].heightUV * weight3));
				float v = tex2Dlod(_HeightTex, float4(uvHA, 0.0f, 0.0f)).r * _Length;

				//pIn.baseCol = tex2Dlod(_MainTex, float4(uvA, 0.0f, 0.0f));
				fixed3 colA = ((input[0].col * weight1) + (input[1].col * weight2) + (input[2].col * weight3));
				fixed3 baseColTemp = tex2Dlod(_MainTex, float4(uvA, 0.0f, 0.0f)).rgb * colA;

				//Get a relative normal point;
				//float4 pN = pA + (nA * v * 0.5f);
				//float4 pG = pN - float4(0.0f, v * 0.5f, 0.0f, 0.0f);
				//Construct hair strip
				float t0 = 0.0f;
				float gravEffect = _Gravity / (v + 0.0001f);
				
				for (int j = 0; j < _Step; j++) {
					float t1 = (float)(j + 1) * stepCache;

					float4 subP0 = normalize(nA - (float4(0, v * t0, 0, 0) * gravEffect * t0)) * (v * t0);
					float4 subP1 = normalize(nA - (float4(0, v * t1, 0, 0) * gravEffect * t1)) * (v * t1);
					//float4 subP0 = bezier(pA, pN, pG, t0);
					//float4 subP1 = bezier(pA, pN, pG, t1);

					//float4 w0 = t * lerp(_Width, 0, t0);
					//float4 w1 = t * lerp(_Width, 0, t1);

					pIn.vertex = UnityObjectToClipPos(subP0 + pA);
					pIn.baseCol = baseColTemp * (1- (1 - t0) * _Root);
					//pIn.vertex = UnityObjectToClipPos(subP0);
					OutputStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(subP1 + pA);
					pIn.baseCol = baseColTemp * (1 - (1 - t1) * _Root);
					//pIn.vertex = UnityObjectToClipPos(subP1);
					OutputStream.Append(pIn);
					OutputStream.RestartStrip();
					t0 = t1;
				}
			}

			[maxvertexcount(140)]
			void geom(triangle v2g input[3], inout LineStream<g2f> OutputStream){
				
				//float3 normal = normalize(cross(input[1].worldPosition.xyz - input[0].worldPosition.xyz, input[2].worldPosition.xyz - input[0].worldPosition.xyz));

				float sample1, sample2, sample3;

				for (int i = 0; i < _Inter; i++) {
					//Gibberish random samples
					sample1 = ((i * 249.9845f) % 127.5634f) / 127.5634f;
					sample2 = (((i * 513.4719f) % 9.4193f) / 9.4193f) * (1.0f - sample1);
					sample3 = (1.0f - (sample1 + sample2));
					renderStrand(input, sample1, sample2, sample3, OutputStream);
				}
			}
			
			fixed4 frag (g2f i) : SV_Target {
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				//float3 lightFinal = saturate(dot(i.normal, _LightColor0.xyz)) * _LightColor0 + UNITY_LIGHTMODEL_AMBIENT.xyz;
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return float4(i.baseCol * _Color, 1.0f);
			}

			ENDCG
		}
	}
}
