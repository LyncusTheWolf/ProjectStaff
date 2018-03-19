Shader "Custom/Fur" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex("Noise", 2D) = "white" {}
		_HeightTex("Height Map", 2D) = "white" {}
		_Length ("Length", Range(0.0, 20.0)) = 1
		_Gravity("Gravity", Range(0.0, 20.0)) = 1
		_Step("Detail", Range(1, 10)) = 8 
		_DistortionIntensity("Distortion", Range(0.0, 10.0)) = 1
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				float2 heightUV: TEXCOORD1;
			};

			struct g2f {
				float4 vertex : SV_POSITION;
				//UNITY_FOG_COORDS(1)
				float2 uv : TEXCOORD0;
				fixed4 baseCol : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;

			sampler2D _HeightTex;
			float4 _HeightTex_ST;

			float4 _LightColor0;
			float _Length;
			float _Gravity;
			int _Step;
			float _DistortionIntensity;
			
			v2g vert (appdata v) {
				v2g o;
				o.vertex = v.vertex;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.heightUV = TRANSFORM_TEX(v.uv, _HeightTex);

				float3 noise = tex2Dlod(_NoiseTex, float4(v.uv, 0, 0)).xyz;
				float3 noiseNormal = (noise * 2 - 1) * _DistortionIntensity;
				o.normal = normalize(float4((v.normal + noiseNormal).xyz, 0));
				//o.normal = v.normal;				
				return o;
			}

			[maxvertexcount(64)]
			void geom(triangle v2g input[3], inout LineStream<g2f> OutputStream)
			{
				
				//float3 normal = normalize(cross(input[1].worldPosition.xyz - input[0].worldPosition.xyz, input[2].worldPosition.xyz - input[0].worldPosition.xyz));
				
				float4 p1 = input[0].vertex;
				float4 p2 = input[1].vertex;
				float4 p3 = input[2].vertex;

				float4 n1 = input[0].normal;
				float4 n2 = input[1].normal;
				float4 n3 = input[2].normal;

				g2f pIn;

				//pIn.uv = input[0].uv;
				float stepCache = 1.0f / _Step;

				int splitSize = 5;

				for (int i = 0.0f; i < splitSize; i++) {
					//Generate some average position data
					int temp1 = i * splitSize;
					int temp2 = splitSize;
					int temp3 = ((splitSize - 1) - i) * splitSize;

					int tempTotal = temp1 + temp2 + temp3;

					float weight1 = (float)temp1 / tempTotal;
					float weight2 = (float)temp2 / tempTotal;
					float weight3 = (float)temp3 / tempTotal;

					//Pick an average point and normal between the provided vertices
					float4 pA = ((p1 * weight1) + (p2 * weight2) + (p3 * weight3));
					float4 nA = ((n1 * weight1) + (n2 * weight2) + (n3 * weight3));

					float2 uvA = ((input[0].uv * weight1) + (input[1].uv * weight2) + (input[2].uv * weight3));
					pIn.uv = uvA;

					//Apply some noise distortion
					//float3 noise = tex2Dlod(_NoiseTex, float4(uvA, 0, 0)).xyz;
					//float3 noiseNormal = (noise * 2 - 1) * _DistortionIntensity;
					//nA = normalize(float4((nA + noiseNormal).xyz, 0));

					float2 uvHA = ((input[0].heightUV * weight1) + (input[1].heightUV * weight2) + (input[2].heightUV * weight3));
					float v = tex2Dlod(_HeightTex, float4(uvHA, 0.0f, 0.0f)).r * _Length;
					pIn.baseCol = tex2Dlod(_MainTex, float4(uvA, 0.0f, 0.0f));

					//Construct hair strip
					float t0 = 0.0f;
					for (int j = 0; j < _Step; j++) {
						float t1 = (float)(j + 1) * stepCache;

						float4 subP0 = normalize(nA - (float4(0, v * t0, 0, 0) * _Gravity * t0)) * (v * t0);
						float4 subP1 = normalize(nA - (float4(0, v * t1, 0, 0) * _Gravity * t1)) * (v * t1);

						//float4 w0 = t * lerp(_Width, 0, t0);
						//float4 w1 = t * lerp(_Width, 0, t1);
				
						pIn.vertex = UnityObjectToClipPos(subP0+ pA);
						OutputStream.Append(pIn);

						pIn.vertex = UnityObjectToClipPos(subP1 + pA);
						OutputStream.Append(pIn);
						OutputStream.RestartStrip();
						t0 = t1;

						//pIn.vertex = UnityObjectToClipPos(subP1 - w1 + pA);
						//OutputStream.Append(pIn);

						//pIn.vertex = UnityObjectToClipPos(subP1 + w1 + pA);
						//OutputStream.Append(pIn);
						//UnityObjectToClipPos()


						/*pIn.vertex = input[0].vertex;
						UNITY_TRANSFER_FOG(pIn, pIn.vertex);
						//pIn.uv = input[0].uv;
						pIn.normal = input[0].normal;
						pIn.worldPosition = input[0].worldPosition;
						OutputStream.Append(pIn);

						pIn.vertex = input[0].vertex + input[0].normal;
						UNITY_TRANSFER_FOG(pIn, pIn.vertex);
						pIn.uv = input[0].uv;
						pIn.normal = input[0].normal;
						pIn.worldPosition = input[0].worldPosition;
						OutputStream.Append(pIn);*/
					}
				}
			}
			
			fixed4 frag (g2f i) : SV_Target {
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				float3 lightFinal = (_LightColor0.xyz) + UNITY_LIGHTMODEL_AMBIENT.xyz;
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return float4(lightFinal * i.baseCol, 1.0f);
			}

			ENDCG
		}
	}
}
