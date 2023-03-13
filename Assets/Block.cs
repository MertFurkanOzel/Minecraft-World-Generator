using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    [SerializeField] Material mat;
    public BlockType bType;                        /* Projede dünyayý oluþturmak için hazýr küp kullanmak performans açýsýndan */
    private Vector3 position;                      /* çok kötü bir fikir. Bunu þu þekilde açýklamak istiyorum: Projede default */
    private GameObject Parent;                     /* 4096 chunk oluþturuluyor ve her chunk 256 blok içeriyor bu da toplam 1.048.576 */
    private bool isSolid;                          /* adet küp demek, unityde sadece bu küpleri oluþturmasý bile yaklaþýk 40 dakika */
    private Chunk owner;                           /* sürüyor. Bu yüzden küp oluþturmak yerine küpün yüzeylerini(kare) çizdirmek ve */
    enum quadside //Küpün yüzeyleri                /* birleþtirmek çok daha iyi fikir. Bu þekilde tam bir küp oluþturmuyoruz sadece küpün */
    {                                              /* yüzünün normaline yani küpe baktýðýmýz tarafa bir mesh uygulamýþ oluyoruz ve normal */
        front,                                     /* bir küp gibi görünüyor, dahasý birleþtirme iþlemi yaparken küpün asla görünmeyecek */
        back,                                      /* kýsýmlarýna ihtiyaç duymadýðýmýz için bunlarý hiç çizdirmiyoruz bile bu sayede */
        top,                                       /* tüm dünyanýn oluþmasý yaklaþýk 1 dakikadan az sürüyor. */
        bottom,
        right,
        left
    }

    public Block(BlockType blockType,Vector3 pos,GameObject parent,Chunk owner) //Yapýcý metot
    {
        this.owner =owner; bType = blockType; position = pos; Parent = parent; isSolid = bType != BlockType.AIR;
    }
    public enum BlockType { GRASS, DIRT, STONE,LAVA,DIAMOND,IRON,BEDROCK,AIR}; //Bloklar
    Vector2[,] blockUVs = { 
		/*Grass Top*/		{new Vector2( 0.125f, 0.375f ), new Vector2( 0.1875f, 0.375f),          /* Bloklarýn texture atlastaki vektörel konumlarý */
                                new Vector2( 0.125f, 0.4375f ),new Vector2( 0.1875f, 0.4375f )},    /* Her texture arasý 1/16(atlas 16*16) 0.0625 birim */
		/*Grass */		    {new Vector2( 0.1875f, 0.9375f ), new Vector2( 0.25f, 0.9375f),         /* Texture atlas kullanýmý render iþlemini hafifletmek için. */
                                new Vector2( 0.1875f, 1.0f ),new Vector2( 0.25f, 1.0f )},
		/*Dirt*/			{new Vector2( 0.125f, 0.9375f ), new Vector2( 0.1875f, 0.9375f),
                                new Vector2( 0.125f, 1.0f ),new Vector2( 0.1875f, 1.0f )},
		/*Stone*/			{new Vector2( 0.0625f, 0.9375f ), new Vector2( 0.125f, 0.9375f),
                                new Vector2( 0.0625f, 1.0f ),new Vector2( 0.125f, 1.0f )},
        /*Lava*/			{new Vector2( 0.875f, 0.0625f ), new Vector2( 0.9375f, 0.0625f),
                                new Vector2( 0.875f, 0.125f ),new Vector2( 0.9375f, 0.125f )},
        /*Diamond*/         {new Vector2( 0.125f, 0.75f ), new Vector2( 0.1875f, 0.75f),
                                new Vector2( 0.125f, 0.8125f ),new Vector2( 0.1875f, 0.8125f )},
        /*Iron*/            {new Vector2( 0.0625f, 0.8125f ), new Vector2( 0.125f, 0.8125f),
                                new Vector2( 0.0625f, 0.875f ),new Vector2( 0.125f, 0.875f )},
        /*Bedrock*/         {new Vector2( 0.0625f, 0.875f ), new Vector2( 0.125f, 0.875f),
                                new Vector2( 0.0625f, 0.9375f ),new Vector2( 0.125f, 0.9375f )},
                        };
    public void Draw()                                                                              
    {                                                                                               
        if (bType == BlockType.AIR)                                                          /* Küpün yüzeylerini çizdiriyoruz, burada dikkat etmek gereken þey*/
            return;                                                                          /* oluþturmak istediðimiz bloðun yanlarýnda baþka blok var mý? */
        if (!has_solid_neighboor((int)position.x, (int)position.y, (int)position.z + 1))     /* Eðer varsa çizdirmeye gerek yok çünkü o yüzeyi zaten göremeyeceðiz.*/
            CreateQuad(quadside.front);                                                      /* Blok kýrma gibi olaylar eklendikten sonra bazý yüzeylerin çizdirilmesi gerekecek.*/

        if (!has_solid_neighboor((int)position.x, (int)position.y, (int)position.z - 1))
            CreateQuad(quadside.back);

        if (!has_solid_neighboor((int)position.x, (int)position.y - 1, (int)position.z))
            CreateQuad(quadside.bottom);

        if (!has_solid_neighboor((int)position.x, (int)position.y + 1, (int)position.z))
            CreateQuad(quadside.top);

        if (!has_solid_neighboor((int)position.x + 1, (int)position.y, (int)position.z))
            CreateQuad(quadside.right);

        if (!has_solid_neighboor((int)position.x - 1, (int)position.y, (int)position.z))
            CreateQuad(quadside.left);
    }
    private void CreateQuad(quadside side)
    {                                                    /* Yüzeyi çizdirme iþlemi */
        Mesh mesh = new Mesh();                          /* Meshi yüzeye uygulamak için normal,uv,köþe vektörlerinin ayarlanmasý*/
        mesh.name = "M_Mesh";                            /* Her mesh üçgenlerden oluþur, her yüzey iki üçgen içerir*/

        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] triangles = new int[6];

        Vector3 p0 = new Vector3(-0.5f, -0.5f, 0.5f);
        Vector3 p1 = new Vector3(0.5f, -0.5f, 0.5f);
        Vector3 p2 = new Vector3(0.5f, -0.5f, -0.5f);
        Vector3 p3 = new Vector3(-0.5f, -0.5f, -0.5f);
        Vector3 p4 = new Vector3(-0.5f, 0.5f, 0.5f);
        Vector3 p5 = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 p6 = new Vector3(0.5f, 0.5f, -0.5f);
        Vector3 p7 = new Vector3(-0.5f, 0.5f, -0.5f);

        Vector2 uv00;
        Vector2 uv10;
        Vector2 uv01;
        Vector2 uv11;

        if (bType == BlockType.GRASS && side == quadside.top)   // Çim bloðu diðer bloklardan farklý olduðu için iki duruma ayrýlýyor.
        {
            uv00 = blockUVs[0, 0];
            uv10 = blockUVs[0, 1];
            uv01 = blockUVs[0, 2];
            uv11 = blockUVs[0, 3];
        }

        else if (bType == BlockType.GRASS && side == quadside.bottom)
        {
            uv00 = blockUVs[(int)(BlockType.DIRT + 1), 0];
            uv10 = blockUVs[(int)(BlockType.DIRT + 1), 1];
            uv01 = blockUVs[(int)(BlockType.DIRT + 1), 2];
            uv11 = blockUVs[(int)(BlockType.DIRT + 1), 3];
        }
        else                                            
        {                                           //Diðer bloklar. Çim bloðu iki ayrý blok tipi içerdiði için diðer bloklara +1 eklemek gerekiyor.
            uv00 = blockUVs[(int)(bType + 1), 0];    
            uv10 = blockUVs[(int)(bType + 1), 1];
            uv01 = blockUVs[(int)(bType + 1), 2];
            uv11 = blockUVs[(int)(bType + 1), 3];
        }
        uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
        triangles = new int[] { 3, 1, 0, 3, 2, 1 };

        switch (side)
        {
            case quadside.front:
                vertices = new Vector3[] { p4, p5, p1, p0 };
                normals = new Vector3[] {Vector3.forward,
                                 Vector3.forward,
                                 Vector3.forward,
                                 Vector3.forward};
                break;
            case quadside.back:
                vertices = new Vector3[] { p6, p7, p3, p2 };
                normals = new Vector3[] {Vector3.back,
                                 Vector3.back,
                                 Vector3.back,
                                 Vector3.back};
                break;
            case quadside.top:
                vertices = new Vector3[] { p7, p6, p5, p4 };
                normals = new Vector3[] {Vector3.up,
                                 Vector3.up,
                                 Vector3.up,
                                 Vector3.up};
                break;
            case quadside.bottom:
                vertices = new Vector3[] { p0, p1, p2, p3 };
                normals = new Vector3[] {Vector3.down,
                                 Vector3.down,
                                 Vector3.down,
                                 Vector3.down};
                break;
            case quadside.right:
                vertices = new Vector3[] { p5, p6, p2, p1 };
                normals = new Vector3[] {Vector3.right,
                                 Vector3.right,
                                 Vector3.right,
                                 Vector3.right};
                break;
            case quadside.left:
                vertices = new Vector3[] { p7, p4, p0, p3 };
                normals = new Vector3[] {Vector3.left,
                                 Vector3.left,
                                 Vector3.left,
                                 Vector3.left};
                break;
            default:
                break;
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.RecalculateBounds();

        GameObject cubequad = new GameObject();
        cubequad.transform.position = position;
        cubequad.transform.parent = Parent.transform;
        MeshFilter mf = cubequad.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        MeshRenderer mr = cubequad.AddComponent<MeshRenderer>();
        mr.material = mat;
    }

    public bool has_solid_neighboor(int x, int y, int z)
    {
        Block[,,] chunks;

        if (x < 0 || x >= World.chunk_size ||
           y < 0 || y >= World.chunk_size ||                                /* Çizdireceðimiz küp yüzeyinin yanýnda baþka bir küp olma durumuna bakýyoruz.*/
           z < 0 || z >= World.chunk_size)                                  /* Burada da dikkat etmek gereken yandaki küp baþka chunka ait olabilir*/
        {                                                                   /* bu durumu kontrol etmek gerekiyor.*/
            Vector3 neighbourChunksPos = Parent.transform.position +                  
                new Vector3(
                (x - (int)position.x) * World.chunk_size,
                (y - (int)position.y) * World.chunk_size,
                (z - (int)position.z) * World.chunk_size);

            string bName = World.build_chunk_name(neighbourChunksPos);

            x = ConvertBlockIndexToLocal(x);
            y = ConvertBlockIndexToLocal(y);
            z = ConvertBlockIndexToLocal(z);

            Chunk bChunk;
            if (World.world_Chunk.TryGetValue(bName, out bChunk))
            {
                chunks = bChunk.Chunk_data;
            }
            else
                return false;

        }
        else
            chunks = owner.Chunk_data;

        try
        {
            return chunks[x, y, z].isSolid;
        }
        catch (System.IndexOutOfRangeException ex) { }

        return false;
    }
    int ConvertBlockIndexToLocal(int i)
    {
        if (i == -1)
            i = World.chunk_size - 1;
        else if (i == World.chunk_size)
            i = 0;

        return i;
    }
}
