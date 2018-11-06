Shader "Voxel/VoxelShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		// No culling or depth
		//Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
            
            #pragma require geometry

			#pragma vertex VS_Main
			#pragma fragment FS_Main
            #pragma geometry GS_Main

            #pragma target 4.0
			
			#include "UnityCG.cginc"

            #define TAM 36

            /**
             * The input data that goes into the Vertex Shader.
             * This is typically passed from the main program
             */
			struct VS_INPUT {
				float4 vertex : POSITION;
			};

            /**
             * The input data that goes into the Geometry Shader.
             * This is typically passed from the vertex shader
             */
            struct GS_INPUT {
                float4 pos : SV_POSITION;
                uint morton_key : TEXCOORD;
            };

            /**
             * The input data that goes into the Fragment Shader.
             * This is typically passed from the geometry shader
             */
            struct FS_INPUT {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            /**
             * VERTEX SHADER
             */
            const uint morton_part(uint n) {
                // morton n-key
                n = (n ^ (n << 16)) & 0xff0000ff;
                n = (n ^ (n << 8)) & 0x0300f00f;
                n = (n ^ (n << 4)) & 0x030c30c3;
                n = (n ^ (n << 2)) & 0x09249249;

                return n;
            }

            const uint morton_key(uint x, uint y, uint z) {
                uint cx = morton_part(x);
                uint cy = morton_part(y);
                uint cz = morton_part(z);

                return (cz << 2) + (cy << 1) + cx;
            }

			GS_INPUT VS_Main(VS_INPUT v) {
				GS_INPUT OUT;

                // calculate the morton key
                uint _x = (uint)v.vertex.x;
                uint _y = (uint)v.vertex.y;
                uint _z = (uint)v.vertex.z;

                OUT.morton_key = morton_key(_x, _y, _z);
				OUT.pos = v.vertex;
				return OUT;
			}

            /**
             * GEOMETRY SHADER
             */
            [maxvertexcount(TAM)]
            void GS_Main(triangle GS_INPUT IN[3], inout TriangleStream<FS_INPUT> triStream) {
                float f = 5.0/20.0f; //half size
                 
                 const float4 vc[TAM] = { float4( -f,  f,  f, 0.0f), float4(  f,  f,  f, 0.0f), float4(  f,  f, -f, 0.0f),    //Top                                 
                                          float4(  f,  f, -f, 0.0f), float4( -f,  f, -f, 0.0f), float4( -f,  f,  f, 0.0f),    //Top
                                          
                                          float4(  f,  f, -f, 0.0f), float4(  f,  f,  f, 0.0f), float4(  f, -f,  f, 0.0f),     //Right
                                          float4(  f, -f,  f, 0.0f), float4(  f, -f, -f, 0.0f), float4(  f,  f, -f, 0.0f),     //Right
                                          
                                          float4( -f,  f, -f, 0.0f), float4(  f,  f, -f, 0.0f), float4(  f, -f, -f, 0.0f),     //Front
                                          float4(  f, -f, -f, 0.0f), float4( -f, -f, -f, 0.0f), float4( -f,  f, -f, 0.0f),     //Front
                                          
                                          float4( -f, -f, -f, 0.0f), float4(  f, -f, -f, 0.0f), float4(  f, -f,  f, 0.0f),    //Bottom                                         
                                          float4(  f, -f,  f, 0.0f), float4( -f, -f,  f, 0.0f), float4( -f, -f, -f, 0.0f),     //Bottom
                                          
                                          float4( -f,  f,  f, 0.0f), float4( -f,  f, -f, 0.0f), float4( -f, -f, -f, 0.0f),    //Left
                                          float4( -f, -f, -f, 0.0f), float4( -f, -f,  f, 0.0f), float4( -f,  f,  f, 0.0f),    //Left
                                          
                                          float4( -f,  f,  f, 0.0f), float4( -f, -f,  f, 0.0f), float4(  f, -f,  f, 0.0f),    //Back
                                          float4(  f, -f,  f, 0.0f), float4(  f,  f,  f, 0.0f), float4( -f,  f,  f, 0.0f)     //Back
                                          };
                                          
                 
                 const float2 UV1[TAM] = { float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),         //Esta em uma ordem
                                           float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),         //aleatoria qualquer.
                                           
                                           float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), 
                                           float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
                                           
                                           float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), 
                                           float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
                                           
                                           float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), 
                                           float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
                                           
                                           float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), 
                                           float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ),
                                           
                                           float2( 0.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), 
                                           float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f ), float2( 1.0f,    0.0f )                                            
                                             };    
                                                             
                 const int TRI_STRIP[TAM]  = {  0, 1, 2,  3, 4, 5,
                                                6, 7, 8,  9,10,11,
                                               12,13,14, 15,16,17,
                                               18,19,20, 21,22,23,
                                               24,25,26, 27,28,29,
                                               30,31,32, 33,34,35  
                                               }; 
                                                             
                 FS_INPUT v[TAM];
                 int i;
                 
                 // Assign new vertices positions 
                 for (i=0;i<TAM;i++) { 
                    v[i].pos = IN[0].pos + vc[i]; 
                    v[i].uv = float2(0.0f, 0.0f);
                 }
                 
                 // Position in view space
                 for (i=0;i<TAM;i++) { 
                    v[i].pos = UnityObjectToClipPos(v[i].pos); 
                 }
                     
                 // Build the cube tile by submitting triangle strip vertices
                 for (i=0;i<TAM/3;i++)
                 { 
                     triStream.Append(v[TRI_STRIP[i*3+0]]);
                     triStream.Append(v[TRI_STRIP[i*3+1]]);
                     triStream.Append(v[TRI_STRIP[i*3+2]]);    
                                     
                     triStream.RestartStrip();
                 }
            }

            /**
             * FRAGMENT SHADER
             */
			sampler2D _MainTex;

			fixed4 FS_Main(FS_INPUT i) : SV_Target {
				return tex2D(_MainTex, i.uv);
			}
            
			ENDCG
		}
	}
}
