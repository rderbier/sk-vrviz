using StereoKit;

namespace RDR
{
    class MeshUtils
	{
		static public Mesh createArrow(float dx, float dy, float dz)
		{
			Vertex[] verts = new Vertex[5];
			verts[0] = new Vertex(new Vec3(0, 0, dz), new Vec3(0, 0, dz));
			verts[1] = new Vertex(new Vec3(0, dy, 0), new Vec3(0, dy, 0));
			verts[2] = new Vertex(new Vec3(dx, 0, 0), new Vec3(dx, 0, 0));
			verts[3] = new Vertex(new Vec3(0, -dy, 0), new Vec3(0, -dy, 0));
			verts[4] = new Vertex(new Vec3(-dx, 0, 0), new Vec3(-dx, 0, 0));

			uint[] inds = new uint[] { 0, 2, 1, 0, 1, 4, 0, 4, 3, 0, 3, 2, 4, 1, 2, 4, 2, 3 };

			Mesh m = new Mesh();
			m.SetVerts(verts);
			m.SetInds(inds);
			return m;
		}
		static public Mesh generateUVSphere(float diameter, uint segments, uint rings)
		{
			// rings >= 3 
			Vertex[] verts = new Vertex[2 + (rings - 1) * (segments+1)];

			verts[0] = new Vertex(new Vec3(0, diameter / 2.0f, 0 ), Vec3.Up, new Vec2(.5f,0));
			verts[1 + (rings - 1) * (segments+1)] = new Vertex(new Vec3(0, -diameter / 2.0f,0), -Vec3.Up, new Vec2(.5f, 1));
			int ind = 1;
			float beta;
			Vec3 p;
			for (int r = 1; r < rings ; r++)
			{
				float alpha = SKMath.Pi / rings * r;
				for (int s = 0; s < segments; s++)
				{
					beta = (2.0f * SKMath.Pi / segments) * s;
					p = new Vec3(
						 SKMath.Sin(alpha) * SKMath.Cos(beta),
						  SKMath.Cos(alpha),
						 SKMath.Sin(alpha) * SKMath.Sin(beta)
						
						);
					// Normal set vertice and normal 
					verts[ind] = new Vertex(diameter /2.0f * p, p, new Vec2(1.0f - (1.0f/segments)*s,1.0f/rings * r));
					ind += 1;
				}
				// add a final point at the same physical location of the first one but different UV map.
				beta = 0f;
				p = new Vec3(
					 SKMath.Sin(alpha) * SKMath.Cos(beta),
					  SKMath.Cos(alpha),
					 SKMath.Sin(alpha) * SKMath.Sin(beta)

					);
				verts[ind] = new Vertex(diameter / 2.0f * p, p, new Vec2(0f, (1.0f / rings) * r));
				ind += 1;

			}

			// segments facets on poles and 2 facets per square for each segments on rest of rings.
			uint[] inds = new uint[(2 * (segments+1) + (rings - 2) * (segments+1) * 2) * 3];
			int j = 0;
			for (uint i = 0; i < segments ; i++)
			{
				inds[j] = 0;
				inds[j + 1] = i + 2;
				inds[j + 2] = i+1;
				
				j += 3;
			}
			
	
			for (uint r = 1; r < rings-1 ; r++)
			{
				for (uint i = 0; i < segments ; i++)
				{
					inds[j] = 1 + (r - 1) * (segments+1) + i;
					inds[j + 1] = 1 + (r) * (segments+1) + i + 1; 
					inds[j + 2] = 1 + (r) * (segments+1) + i;
					j += 3;
					inds[j] = 1 + (r - 1) * (segments+1) + i;
					inds[j + 1] = 1 + (r - 1) * (segments+1) + i + 1; 
					inds[j + 2] = 1 + (r) * (segments+1) + i + 1;
					j += 3;
				}
				
			}
			uint lastIndex = 1 + (rings - 1) * (segments+1);
			for (uint i = 0; i < segments ; i++)
			{
				inds[j] = lastIndex;
				inds[j + 2] = lastIndex-i-1;
				inds[j + 1] = lastIndex-i-2;
				j += 3;
			}
		


			Mesh m = new Mesh();
			m.SetVerts(verts);
			m.SetInds(inds);
			return m;
		}
	}
}