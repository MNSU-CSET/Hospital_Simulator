Shader "Custom/EasyColliderShader"
{
    // Attempted to write this shader as human readable as possible in case someone is looking to modify it.
    Properties
    {
        _Color ("Color", Color) = (0,1,0,1)
        _Size ("Size", float) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass 
        {
            // Always draw selected vertex things on top.
            ZTest Always
            CGPROGRAM
            #pragma require geometry
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            // In regular graphics shaders the compute buffer support requires minimum shader model 4.5.
            #pragma target 4.5
            #include "UnityCG.cginc"

            // default values
            float4 _Color = float4(0,1,0,1);
            float _Size = 0.01;

            // struct of data we pass in from the compute script.
            struct worldPosition {
                float3 position;
            };

            // buffer of all our world points passed from our compute script.
            StructuredBuffer<worldPosition> worldPositions;
        
            // output from vertex shader, goes into the geometry shader, and the fragment shader.
            struct vertOutput {
                float4 position: SV_POSITION;
            };

            // for the vertex shader, we just need to vertex id, which matches with the worldPoints buffer of data
            vertOutput vert(uint id: SV_VertexID)
            {
                // Create a new vertexOutput, and set the position on it.
                vertOutput o;
                o.position = float4(worldPositions[id].position, 1.0f);
                return o;
            }

            // 36 vertices to make the box that is displayed over vertices.
            #define totalVerts 36
            // geometry shader creates a box at each vertOutput passed in.
            [maxvertexcount(totalVerts)]
            void geom(point vertOutput p[1], inout TriangleStream<vertOutput> triStream)
            {
                // dimensions of length & width are the size / 2.
                float d = _Size/2;
                // scale of the square will be based on camera distance,
                // so that as you get closer or further, the displayed cube remains the same size on screen.
                float scale = distance(_WorldSpaceCameraPos, p[0].position);
                if (unity_OrthoParams.w ==1) {
                  scale = unity_OrthoParams.y*2;
                }
                // verts:
                // by x (negative z): ---, -+-, +--, ++-
                // (positive z): --+, -++, +-+, +++
                // create a square, order of vertices in triangles matters for normals.
                const float4 square[totalVerts] = {
                    // z+
                    float4(-d,d,d,0), float4(-d,-d,d,0), float4(d,-d,d,0),
                    float4(d,d,d,0), float4(-d,d,d,0), float4(d,-d,d,0),
                    // z-
                    float4(d,-d,-d,0), float4(-d,-d,-d,0), float4(-d,d,-d,0),
                    float4(d,d,-d,0), float4(d,-d,-d,0),  float4(-d,d,-d,0), 
                    // y+
                    float4(-d,d,d,0), float4(d,d,d,0), float4(d,d,-d,0),
                    float4(d,d,-d,0), float4(-d,d,-d,0), float4(-d,d,d,0),
                    // y-
                    float4(-d,-d,d,0), float4(d,-d,-d,0), float4(d,-d,d,0), 
                    float4(d,-d,-d,0),  float4(-d,-d,d,0), float4(-d,-d,-d,0),
                    // x+ 
                    float4(d,d,-d,0), float4(d,d,d,0),   float4(d,-d,d,0), 
                    float4(d,d,-d,0), float4(d,-d,d,0), float4(d,-d,-d,0), 
                    // x-
                     float4(-d,d,-d,0), float4(-d,-d,d,0), float4(-d,d,d,0), 
                    float4(-d,d,-d,0), float4(-d,-d,-d,0), float4(-d,-d,d,0),
                };

                // array of vertices to make the triangles with
                vertOutput Vertex[totalVerts];
                
                // build the cube
                for(int i=0; i<totalVerts; i+=3) {
                    // get vertex positions for every 3 points on the cube
                    Vertex[i].position = UnityObjectToClipPos(p[0].position + square[i]*scale);
                    Vertex[i+1].position = UnityObjectToClipPos(p[0].position + square[i+1]*scale);
                    Vertex[i+2].position = UnityObjectToClipPos(p[0].position + square[i+2]*scale);
                    // create a triangle for every 3 outputs.
                    triStream.Append(Vertex[i]);   
                    triStream.Append(Vertex[i+1]);
                    triStream.Append(Vertex[i+2]);
                    // allows for unconnected triangles to be output.
                    triStream.RestartStrip();
                }
            }

            // just returns the color for all fragments. simple.
            float4 frag(vertOutput i) : COLOR
            {
                return _Color;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
