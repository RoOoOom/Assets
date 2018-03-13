Shader "Unlit/FloorToonShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ToonEffect("Toon Effect",Range(0,1)) = 0.5
		_Steps("Steps of Toon",Range(0,9)) = 3 
	}
	SubShader
	{
		Tags {"Queue"="Geometry" "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Tags{"LightMode" = "ForwardBase"}
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
			    float4 color :COLOR;
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD1;
				float3 light : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _ToonEffect;
			float _Steps;
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.normal = v.normal;
				o.light = normalize(ObjSpaceLightDir(v.vertex));
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				i.color = i.color * 2;
				float N = normalize(i.normal);
				float diff = max(0, dot(N, i.light));
				diff = (diff + 1) / 2;
				diff = smoothstep(0, 1, diff);
				float toon = floor(diff * _Steps) / _Steps;
				diff = lerp(diff, toon, _ToonEffect);
				col *= _LightColor0*(diff);
				return col;
			}
			ENDCG
		}
	}
}
