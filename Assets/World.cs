using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class World : MonoBehaviour
{
    [SerializeField] Material Atlas;
    [SerializeField] GameObject player;
    [SerializeField] GameObject camera0;
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI seed_text;
    [SerializeField] GameObject canvas;
    public static int world_height =16;
    public static int chunk_size =16;
    public static int world_size =16;
    public static Dictionary<string, Chunk> world_Chunk;
    private float process = 0;
    public static int seed = 0;
    public void generate_button_click()
    {
        create_seed();
        slider.gameObject.SetActive(true);
        world_Chunk = new Dictionary<string, Chunk>();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        StartCoroutine(Build_world_height());
    }
    private void game_start()
    {
        player.SetActive(true);
        camera0.SetActive(false);
        canvas.SetActive(false);
    }
    private void create_seed()      // Stringi hash'e dönüþtürüp rakamlarý toplamýný seed deðiþkenine atýyor.
    {
        if(!(seed_text.text==""))
        {
            int num = Mathf.Abs(seed_text.text.GetHashCode());
            int total = 0;
            while (true)
            {
                total += num % 10;
                if (num / 10 > 0)
                {
                    num -= num % 10;
                    num /= 10;
                }
                else
                    break;
            }
            seed = total;
        }
    }
    public static string build_chunk_name(Vector3 pos)     //Chunk pozisyonunu parametre olarak alýp chunk ismini döndürüyor(Chunk ismi dictionary'de key).  
    {
        return $"{pos.x}_{pos.y}_{pos.z}";
    }
    IEnumerator Build_world_height()   //Chunklarý oluþturduðumuz fonksiyon.
    {
        int chunk_count = world_height * world_size * world_size;
        for (int z = 0; z < world_size; z++)
            for (int x = 0; x < world_size; x++)
        for (int y = 0; y < world_height; y++)
        {
            Vector3 chunk_pos = new Vector3(x*chunk_size,y*chunk_size,z*chunk_size);
            Chunk c = new Chunk(chunk_pos, Atlas);
                    c.chunk.transform.parent = transform;
            world_Chunk.Add(c.chunk.name, c);
                    process++;
                    slider.value = process / chunk_count * 100;
                    yield return null;
        }
        game_start();
        foreach (KeyValuePair<string,Chunk> item in world_Chunk)
        {
            item.Value.Draw_chunk();
            yield return null;
        }
    }
}
