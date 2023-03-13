using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Chunk
{
    [SerializeField] Material material;
    public Block[,,] Chunk_data;
    public GameObject chunk;

    void Build_Chunk()         /* Noise fonksiyonlarýndan gelen deðere göre blok tipi belirlenip, blok oluþturuluyor.*/
    {
        Chunk_data = new Block[World.chunk_size, World.chunk_size, World.chunk_size];      
        int block_count = World.chunk_size * World.chunk_size * World.chunk_size;
        for (int x = 0; x < World.chunk_size; x++)
            for (int y = 0; y < World.chunk_size; y++)           
                for (int z = 0; z < World.chunk_size; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    int worldx = (int)(x + chunk.transform.position.x);
                    int worldy = (int)(y + chunk.transform.position.y);
                    int worldz = (int)(z+ chunk.transform.position.z);

                    if(worldy==0)
                    {
                        Chunk_data[x, y, z] = new Block(Block.BlockType.BEDROCK, pos, chunk, this);
                    }
                    else if (worldy == Perlin_Noise.generate_height(worldx, worldz))
                    {
                        Chunk_data[x, y, z] = new Block(Block.BlockType.GRASS, pos, chunk.gameObject, this);
                    }
                    else if (Perlin_Noise.FBM_3D(worldx,worldy,worldz,0.044f,2)<0.44f)
                    {
                        Chunk_data[x, y, z] = new Block(Block.BlockType.AIR, pos, chunk, this);
                    }
                    else if (worldy < Perlin_Noise.generate_stone_height(worldx, worldz, 5, 20) && worldy >= 4)
                    {
                        if (Perlin_Noise.FBM_3D(worldx, worldy, worldz, 0.12f, 5) < 0.388f)
                        {
                            Chunk_data[x, y, z] = new Block(Block.BlockType.DIAMOND, pos, chunk, this);
                        }
                        else
                            Chunk_data[x, y, z] = new Block(Block.BlockType.STONE, pos, chunk, this);
                    }
                    else if (worldy <= Perlin_Noise.generate_stone_height(worldx, worldz, 0, 120)&& worldy+5 < Perlin_Noise.generate_height(worldx, worldz))
                    {
                        if (Perlin_Noise.FBM_3D(worldx, worldy, worldz, 0.2f, 3) < 0.374f)
                        {
                            Chunk_data[x, y, z] = new Block(Block.BlockType.IRON, pos, chunk, this);
                        }
                        else
                            Chunk_data[x, y, z] = new Block(Block.BlockType.STONE, pos, chunk, this);
                    }
                    else if (worldy < Perlin_Noise.generate_height(worldx, worldz))
                    {
                        Chunk_data[x, y, z] = new Block(Block.BlockType.DIRT, pos, chunk, this);
                    }
                    else
                    {
                        Chunk_data[x, y, z] = new Block(Block.BlockType.AIR, pos, chunk,this);
                    }                    
                }
    }

    public void Draw_chunk()
    {                                                                /* Ýlgili Chunktaki tüm küpler çizdirildikten sonra küpler birleþtirilip tek bir collider ve*/
        for (int z = 0; z < World.chunk_size; z++)                                                 /*mesh filter uygulanýyor*/
            for (int y = 0; y < World.chunk_size; y++)
                for (int x = 0; x < World.chunk_size; x++)
                {
                    Chunk_data[x, y, z].Draw();
                }
        combine_quad();
        MeshCollider mc = chunk.AddComponent<MeshCollider>();
        mc.sharedMesh = chunk.GetComponent<MeshFilter>().mesh;
    }
    public Chunk(Vector3 pos,Material mat)            // Yapýcý metot
    {
        chunk = new GameObject(World.build_chunk_name(pos));
        chunk.transform.position = pos;
        material = mat;
        Build_Chunk();
    }
    private void combine_quad()       //CombineInstance ile mesh birleþtirme, birleþtirme yapýldýktan sonra chunktaki tüm blok objelerini kaldýr.
    {                                 //Böylelikle performans kazanmýþ oluyoruz.
        MeshFilter[] mfs =chunk.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combines = new CombineInstance[mfs.Length];
        int i = 0;
        while (i < mfs.Length)
        {
            combines[i].mesh = mfs[i].sharedMesh;
            combines[i].transform = mfs[i].transform.localToWorldMatrix;
            i++;
        }
        MeshFilter mf = chunk.AddComponent<MeshFilter>();
        mf.mesh = new Mesh();
        mf.mesh.CombineMeshes(combines);
        MeshRenderer mr = chunk.AddComponent<MeshRenderer>();
        mr.material = material;
        foreach (Transform item in chunk.transform)
        {
            Object.Destroy(item.gameObject);
        }
    }
}
