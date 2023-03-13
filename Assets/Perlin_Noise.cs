using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perlin_Noise : MonoBehaviour
{
    static readonly int max_height = 150;
    static readonly float smooth = 0.006f;
    static readonly int octaves = 4;
    static readonly float persistance = .5f;
    private static float seed()
    {     
        return World.seed*10*smooth;
    }
    public static int generate_height(float x,float z)//  x ve z koordinatýna sahip küplerin tepe noktasýný yani en üstteki küpün y deðerini döndürüyor.
    {
        float height = Map(0, max_height, 0, 1, FBM(x * smooth, z * smooth, octaves, persistance));
        return (int)height;
    }
    public static int generate_stone_height(float x, float z,int min_height=0,int max_height=150) // Taþ,demir,elmas bloklarý için yükseklik deðeri döndürüyor.
    {
        float height = Map(min_height, max_height, 0, 1, FBM(x * smooth * 2f, z * smooth * 2f, octaves + 1, persistance));
        return (int)height;
    }
    private static float Map(float new_min,float new_max,float origin_min,float origin_max,float val)// Noise fonksiyonundan gelen deðere göre yükseklik döndürüyor.
    {
        return Mathf.Lerp(new_min, new_max, Mathf.InverseLerp(origin_min, origin_max, val)); 
    }
    private static float FBM(float x, float z, int oct, float pers)  // Noise fonksiyonu, bloklarýn x ve z deðerlerine göre baþka bir deðer döndürüyor,
    {                                                                /* deðerler ayný olduðu taktirde farklý çýktýlar elde edilmiyor ve deðer 0 ile 1 arasýnda.*/
        if (x == 0)                                                  /* Oktav deðeri arttýkça deðiþen frekans ve genlik sayesinde daha doðal gürültü elde ediliyor. */
            x = smooth;                                              /* Persistance deðeri, genliði belli oranda büyültüp küçültmek için kullanýlýyor.*/
        if (z == 0)                                                  /* Smooth deðeri gürültünün frekansýna direkt etki ediyor.*/
            z = smooth;                                              /*Deðerler ile oynayýp daha güzel sonuçlar elde etmek mümkün*/
        float total = 0;
        float frequency = 1f;
        float amplitude = 0.5f;
        float max_value = 0;
        float Seed = seed();
        for (int i = 0; i < oct; i++)
        {
            total += Mathf.PerlinNoise((x + Seed) * frequency, (z + Seed) * frequency) * amplitude; // parametreler int olduðu zaman hep ayný deðer dönüyor(0.4652731).
            max_value += amplitude;
            amplitude *= pers;
            frequency *= 2;
        }
        return total / max_value;
    }
    public static float FBM_3D(float x,float y, float z,float smoothness,int octaves)  //3 boyutlu noise fonksiyonu. Ayrýyeten y koordinatýna da baðlý deðer döndürüyor.
    {                                                                                  
        float xy = FBM(x * smoothness, y * smoothness, octaves, 0.8f);
        float yz = FBM(y * smoothness, z * smoothness, octaves, 0.2f);
        float xz = FBM(x * smoothness, z * smoothness, octaves, 0.5f);

        float yx = FBM(y * smoothness, x * smoothness, octaves, 0.5f);
        float zy = FBM(z * smoothness, y * smoothness, octaves, 0.7f);
        float zx = FBM(z * smoothness, x * smoothness, octaves, 0.2f);
        return (xy + yz + xz + yx + zy + zx) / 6;
    }
   
}
