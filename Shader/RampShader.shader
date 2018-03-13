Shader "Unlit/RampShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ToonMap("Ramp Map",2D) = "white"{}
		_ToonEffect("Toon Effect",Range(0,2)) = 0.5
		_Color("Main Color",Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags {"RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Tags{"LightMode"="ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			
			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD1;
				float3 light : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			sampler2D _ToonMap;
			float _ToonEffect;

			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.normal = v.normal;
				o.light = ObjSpaceLightDir(v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 c = 1;
				// apply fog
				float3 N = normalize(i.normal);
				float3 lightDir = normalize(i.light);
				
				float diff = dot(i.normal, i.light);
				diff = diff * 0.5 + 0.5;

				float3  toon = tex2D(_ToonMap, float2(diff,diff)).rgb;
				diff = lerp(diff, toon, _ToonEffect);
				c.rgb = col.rgb * _LightColor0.rgb * toon * _ToonEffect * _Color.rgb;
				return c;
			}
			ENDCG
		}
	}
}
