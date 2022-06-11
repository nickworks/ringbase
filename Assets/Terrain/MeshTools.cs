using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshTools
{
    
    /// <summary>
    /// This class stores multiple mesh instance lists. Having multiple lists
    /// allows us to keep the instances separated by submesh. This way the final
    /// output can support having multiple material channels.
    /// </summary>
    public class MeshBuilder {
        private Dictionary<int, List<CombineInstance>> instances = new Dictionary<int, List<CombineInstance>>();
        public void AddMesh(Mesh mesh, Matrix4x4 xform, int submesh = 0) {
            Poke(submesh);
            instances[submesh].Add(new CombineInstance() { mesh = mesh, transform = xform });
        }

        public Mesh CombineAll() {
            List<Mesh> meshes = new List<Mesh>();
            foreach (KeyValuePair<int, List<CombineInstance>> pair in instances) {
                meshes.Add(MeshTools.MeshFromInstanceList(pair.Value));
            }
            return MeshTools.MeshFromMeshes(meshes.ToArray());
        }
        public void Poke(int submesh) {
            if (!instances.ContainsKey(submesh)) 
                instances.Add(submesh, new List<CombineInstance>() {
                    new CombineInstance() {
                        mesh = MeshTools.MakeQuad(.1f),
                        transform = Matrix4x4.identity
                    }
                });
        }
    }

    public static Mesh MeshFromInstanceList(List<CombineInstance> list, bool mergeSubmeshes = true) {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.CombineMeshes(list.ToArray(), mergeSubmeshes);
        return mesh;
    }
    public static Mesh MeshFromMeshes(Mesh[] meshes, bool mergeSubmeshes = false) {

        List<CombineInstance> list = new List<CombineInstance>();
        foreach (Mesh m in meshes) list.Add(new CombineInstance() { mesh = m, transform = Matrix4x4.identity });

        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        finalMesh.CombineMeshes(list.ToArray(), mergeSubmeshes);
        return finalMesh;
    }

    public static Mesh MakeQuad(float s = 1) {

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<int> tris = new List<int>();

        s /= 2;

        verts.Add(new Vector3(-s, 0, -s));
        verts.Add(new Vector3(-s, 0, +s));
        verts.Add(new Vector3(+s, 0, +s));
        verts.Add(new Vector3(+s, 0, -s));
        normals.Add(new Vector3(0, +1, 0));
        normals.Add(new Vector3(0, +1, 0));
        normals.Add(new Vector3(0, +1, 0));
        normals.Add(new Vector3(0, +1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        tris.Add(0);
        tris.Add(1);
        tris.Add(2);
        tris.Add(2);
        tris.Add(3);
        tris.Add(0);

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(normals);
        mesh.SetTriangles(tris, 0);
        return mesh;
    }

    /// <summary>
    /// This function generates and returns a 1m cube mesh. The anchor point is at the bottom of the mesh.
    /// </summary>
    /// <returns>A mesh object with normals and uvs. No color information has been set.</returns>
    public static Mesh MakeCube(float x = 1, float y = 1, float z = 1) {

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<int> tris = new List<int>();

        x /= 2;
        z /= 2;


        // Front face
        verts.Add(new Vector3(-x, 0, -z));
        verts.Add(new Vector3(-x, y, -z));
        verts.Add(new Vector3(+x, y, -z));
        verts.Add(new Vector3(+x, 0, -z));
        normals.Add(new Vector3(0, 0, -1));
        normals.Add(new Vector3(0, 0, -1));
        normals.Add(new Vector3(0, 0, -1));
        normals.Add(new Vector3(0, 0, -1));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        tris.Add(0);
        tris.Add(1);
        tris.Add(2);
        tris.Add(2);
        tris.Add(3);
        tris.Add(0);

        // Back face
        verts.Add(new Vector3(-x, 0, +z));
        verts.Add(new Vector3(+x, 0, +z));
        verts.Add(new Vector3(+x, y, +z));
        verts.Add(new Vector3(-x, y, +z));
        normals.Add(new Vector3(0, 0, +1));
        normals.Add(new Vector3(0, 0, +1));
        normals.Add(new Vector3(0, 0, +1));
        normals.Add(new Vector3(0, 0, +1));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        tris.Add(4);
        tris.Add(5);
        tris.Add(6);
        tris.Add(6);
        tris.Add(7);
        tris.Add(4);

        // Left face 
        verts.Add(new Vector3(-x, 0, -z));
        verts.Add(new Vector3(-x, 0, +z));
        verts.Add(new Vector3(-x, y, +z));
        verts.Add(new Vector3(-x, y, -z));
        normals.Add(new Vector3(-1, 0, 0));
        normals.Add(new Vector3(-1, 0, 0));
        normals.Add(new Vector3(-1, 0, 0));
        normals.Add(new Vector3(-1, 0, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        tris.Add(8);
        tris.Add(9);
        tris.Add(10);
        tris.Add(10);
        tris.Add(11);
        tris.Add(8);

        // Right face
        verts.Add(new Vector3(+x, 0, -z));
        verts.Add(new Vector3(+x, y, -z));
        verts.Add(new Vector3(+x, y, +z));
        verts.Add(new Vector3(+x, 0, +z));
        normals.Add(new Vector3(+1, 0, 0));
        normals.Add(new Vector3(+1, 0, 0));
        normals.Add(new Vector3(+1, 0, 0));
        normals.Add(new Vector3(+1, 0, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        tris.Add(12);
        tris.Add(13);
        tris.Add(14);
        tris.Add(14);
        tris.Add(15);
        tris.Add(12);

        // Top face
        verts.Add(new Vector3(-x, y, -z));
        verts.Add(new Vector3(-x, y, +z));
        verts.Add(new Vector3(+x, y, +z));
        verts.Add(new Vector3(+x, y, -z));
        normals.Add(new Vector3(0, +1, 0));
        normals.Add(new Vector3(0, +1, 0));
        normals.Add(new Vector3(0, +1, 0));
        normals.Add(new Vector3(0, +1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        tris.Add(16);
        tris.Add(17);
        tris.Add(18);
        tris.Add(18);
        tris.Add(19);
        tris.Add(16);

        // Bottom face 
        verts.Add(new Vector3(-x, 0, -z));
        verts.Add(new Vector3(+x, 0, -z));
        verts.Add(new Vector3(+x, 0, +z));
        verts.Add(new Vector3(-x, 0, +z));
        normals.Add(new Vector3(0, -1, 0));
        normals.Add(new Vector3(0, -1, 0));
        normals.Add(new Vector3(0, -1, 0));
        normals.Add(new Vector3(0, -1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        tris.Add(20);
        tris.Add(21);
        tris.Add(22);
        tris.Add(22);
        tris.Add(23);
        tris.Add(20);

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(normals);
        mesh.SetTriangles(tris, 0);
        return mesh;
    }
    /// <summary>
    /// This function generates and returns a 1m cube mesh. The anchor point is at the bottom of the mesh. This mesh has NO duplicate vertices. So shading will look a little odd due to unrealistic normals.
    /// </summary>
    /// <returns>A mesh with normals set. There are no UVs and no vertex colors on this mesh.</returns>
    public static Mesh MakeSmoothCube() {
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<int> tris = new List<int>();

        verts.Add(new Vector3(-0.5f, 0, -0.5f));
        verts.Add(new Vector3(+0.5f, 0, -0.5f));
        verts.Add(new Vector3(+0.5f, 0, +0.5f));
        verts.Add(new Vector3(-0.5f, 0, +0.5f));

        verts.Add(new Vector3(-0.5f, 1, -0.5f));
        verts.Add(new Vector3(+0.5f, 1, -0.5f));
        verts.Add(new Vector3(+0.5f, 1, +0.5f));
        verts.Add(new Vector3(-0.5f, 1, +0.5f));

        tris.Add(0);
        tris.Add(1);
        tris.Add(2);

        tris.Add(0);
        tris.Add(2);
        tris.Add(3);

        tris.Add(1);
        tris.Add(6);
        tris.Add(2);

        tris.Add(1);
        tris.Add(5);
        tris.Add(6);

        tris.Add(0);
        tris.Add(5);
        tris.Add(1);

        tris.Add(0);
        tris.Add(4);
        tris.Add(5);


        tris.Add(3);
        tris.Add(4);
        tris.Add(0);

        tris.Add(3);
        tris.Add(7);
        tris.Add(4);

        tris.Add(7);
        tris.Add(5);
        tris.Add(4);

        tris.Add(7);
        tris.Add(6);
        tris.Add(5);

        tris.Add(2);
        tris.Add(7);
        tris.Add(3);

        tris.Add(2);
        tris.Add(6);
        tris.Add(7);

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        return mesh;
    }
    /// <summary>
    /// Generates the mesh data to make a 1m sized cylinder mesh. Anchor point in in the bottom center of the mesh.
    /// </summary>
    /// <param name="points">The number of points the object has.</param>
    /// <returns></returns>
    public static Mesh MakeCylinder(int points) {
        List<Vector3> verts = new List<Vector3>();
        //List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<int> tris = new List<int>();

        //Generate vertices based on number of points & set normals:
        //Set start points in middle:
        verts.Add(new Vector3(0, 0, 0));                //verts(0) on Bottom Center
        normals.Add(new Vector3(0, -1, 0));                          //normals for verts(0)

        verts.Add(new Vector3(0, 1, 0));                //verts(1) on Top Center
        normals.Add(new Vector3(0, +1, 0));              //normals for verts(1)

        //Get points for all sides(bottom, then top); the sides will start at verts(2):
        for (int i = 0; i < points; i++) {
            //find angle to spawn verts at:
            float angleDegrees = (i == 0) ? 0 : 360 / ((float)points / (float)i);
            float angleRadians = angleDegrees * (Mathf.PI / 180);
            float radius = .5f;

            //find x and z positions of created vertices:
            float vertX = Mathf.Sin(angleRadians) * radius;
            float vertZ = Mathf.Cos(angleRadians) * radius;

            //set bottom vert & normals
            verts.Add(new Vector3(vertX, 0, vertZ));
            int point1Bottom = verts.Count - 1;
            normals.Add(Vector3.Normalize(verts[point1Bottom] - verts[0]));
            //set top vert & normals
            verts.Add(new Vector3(vertX, 1, vertZ));
            int point1Top = verts.Count - 1;
            normals.Add(Vector3.Normalize(verts[point1Top] - verts[1]));
        }

        //TODO: Generate UV values:

        //Generate triangles based on the created vertices
        //Side Trianges:
        //set up left and right index values:
        int leftBottomIndex = 2;
        int leftTopIndex = 3;

        int rightBottomIndex;
        int rightTopIndex;

        for (int i = 1; i <= points; i++) {
            if (i != points) {
                //set index values for the right vertices:
                rightBottomIndex = leftBottomIndex + 2;
                rightTopIndex = leftTopIndex + 2;


            } else {
                //set right vertices to loop back to the starting side vertices:
                rightBottomIndex = 2;
                rightTopIndex = 3;
            }

            //create triangles:
            tris.Add(leftBottomIndex);
            tris.Add(rightTopIndex);
            tris.Add(leftTopIndex);

            tris.Add(rightTopIndex);
            tris.Add(leftBottomIndex);
            tris.Add(rightBottomIndex);

            //Set index values for next side.
            leftBottomIndex = rightBottomIndex;
            leftTopIndex = rightTopIndex;
        }

        //Bottom Triangles (Even index values):
        for (int bv = 2; bv <= verts.Count - 2; bv += 2) {
            if (bv != verts.Count - 2) {
                //grab vertices from index
                //start tri at bv (Bottom Vertex)
                tris.Add(bv);
                //Set vertex at index 0
                tris.Add(0);
                //End at vertex bv + 2;
                tris.Add(bv + 2);
            } else {
                //Set last triangle using the center and point 1:
                tris.Add(bv);
                tris.Add(0);            //Bottom Center Index
                tris.Add(2);            //Bottom Point 1 Index
            }
        }

        //Top Triangles (Odd index values):
        for (int tv = 1; tv <= verts.Count - 1; tv += 2) {
            if (tv != verts.Count - 1) {
                //set tris from vertices at index
                //Set at vertex tv + 2
                tris.Add(tv + 2);
                //Set at vertex at index 1
                tris.Add(1);
                //start tri at tv (Top Vertex)
                tris.Add(tv);
            } else {
                //Set last triangle using the center and point 1:
                tris.Add(3);
                tris.Add(1);                //Top Center Index
                tris.Add(tv);                //Top Point 1 Index
            }
        }

        //Generate mesh based on verts, tris, and normals
        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        //mesh.SetUVs(uvs);
        mesh.SetNormals(normals);
        mesh.SetTriangles(tris, 0);
        return mesh;
    }

}
