using UnityEngine;
using System.Collections;

public class NoiseManager : MonoBehaviour
{
    public int ImageWidth = 32;
    public int ImageHeight = 32;
    public float Scale = 3;
    public int Layers = 12;
    public float Offset = 300;

    float[] current_noise;
    
    // Recalculate Cached Noise
    public void NewCachedNoise(){
        current_noise = GetNoise(ImageWidth, ImageHeight, Scale, Layers, Offset);
    }

    // Convert Noise to an Image for display
    public Texture2D NoiseAsImage(float[] noise, int width, int height){
        Color[] final_pixels = new Color[width * height];
        for(int i = 0; i < width * height; i++){
            float colorval = Mathf.Clamp(((noise[i] + 1) / 2), 0f, 1f);
            final_pixels[i] = new Color(colorval,colorval,colorval,1f);
        }
        Texture2D noise_tex = new Texture2D(ImageWidth, ImageHeight);
        noise_tex.filterMode = FilterMode.Point;
        noise_tex.SetPixels(final_pixels);
        noise_tex.Apply();
        return noise_tex; 
    }

    // Calculate a Noise
    public float[] GetNoise(int width, int height, float scale, int layers, float offset){
        float[] random_vals = RandomiseOffsets(offset, layers);

        float[] pixels = new float[width * height];
        
        for(int i = 0; i < width * height; i++){
            pixels[i] = 0;
        }
        for(int count = 1; count <= layers; count++){
            float amplitude = 1f / (float)count;
            float[] pixadd = new float[width * height];
            
            for (float y = 0.0f; y < height; y++)
            {
                for (float x = 0.0f; x < width; x++)
                {
                    float xCoord = random_vals[(count * 2) - 2] + x / width * scale * count;
                    float yCoord = random_vals[(count * 2) - 1] + y / height * scale * count;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    pixadd[(int)y * width + (int)x] = Mathf.Clamp((sample * 2f) - 1f, -1f, 1f);
                }
            }
            for(int i = 0; i < width * height; i++){
                pixels[i] = pixels[i] + (pixadd[i] * amplitude);
            }
        }

        return pixels;
    }

    public float[] GetCachedNoise(){
        return current_noise;
    }

    // Randomises each Layer
    float[] RandomiseOffsets(float offset, int layers){
        float[] random_vals = new float[layers * 2];
        for(int i = 0; i < layers * 2; i++){
            random_vals[i] = Random.Range(-offset, offset);
        }
        return random_vals;
    }
}