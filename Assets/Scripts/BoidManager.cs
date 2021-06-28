using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Boid
{
    public Vector2 position;
    public float orientation;
    public float intention;
};

public class BoidManager : MonoBehaviour
{
    public ComputeShader TextureShader;
    public ComputeShader BoidShader;
    public int BoidQty;

    private RenderTexture _renderTexture;
    private Camera _camera;
    private ComputeBuffer _boidBuffer;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        InitializeBoids();
    }
    private void OnDisable()
    {
        if (_boidBuffer != null)
            _boidBuffer.Release();
    }

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void InitializeBoids()
    {
        List<Boid> boids = new List<Boid>();

        // Add a number of random boids
        for (int i = 0; i < BoidQty; i++)
        {
            Boid boid = new Boid();

            // Randomize boid position
            boid.position.x = Random.Range(0, 1023);
            boid.position.y = Random.Range(0, 1023);

            // Randomize boid orientation and intention
            boid.orientation = Random.Range(-Mathf.PI, Mathf.PI);
            boid.intention = Random.Range(-Mathf.PI, Mathf.PI);

            boids.Add(boid);
        }

        // Assign to compute buffer - stride is 16 due to the use of 4 different 4-byte floats
        _boidBuffer = new ComputeBuffer(boids.Count, 16);
        _boidBuffer.SetData(boids);
    }

    private void InitializeRenderTexture()
    {
        if (    ( _renderTexture == null                 )
             || ( _renderTexture.width != Screen.width   )
             || ( _renderTexture.height != Screen.height ) )
        {
            // Release render texture if we already have one
            if (_renderTexture != null)
            {
                _renderTexture.Release();
            }

            // Get a render target
            _renderTexture = new RenderTexture(Screen.width,
                                               Screen.height,
                                               0,
                                               RenderTextureFormat.ARGBFloat,
                                               RenderTextureReadWrite.Linear);

            _renderTexture.enableRandomWrite = true;

            _renderTexture.Create();
        }
    }

    private void SetShaderParameters()
    {
        TextureShader.SetTexture(0, "Result", _renderTexture);

        BoidShader.SetTexture(0, "Result", _renderTexture);

        BoidShader.SetBuffer(0, "Boids", _boidBuffer);
        BoidShader.SetInt("BoidQty", BoidQty);
    }

    private void Render(RenderTexture destination)
    {
        int textureGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int textureGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        int boidGroupsSqrt = Mathf.CeilToInt(Mathf.Sqrt(BoidQty) / 8.0f);

        // Make sure we have a current render target
        InitializeRenderTexture();

        // Set the shader parameters
        SetShaderParameters();

        TextureShader.Dispatch(0, textureGroupsX, textureGroupsY, 1);

        BoidShader.Dispatch(0, boidGroupsSqrt, boidGroupsSqrt, 1);

        // Blit the result texture to the screen
        Graphics.Blit(_renderTexture, destination);
    }

    private void OnRenderImage(RenderTexture source,
                               RenderTexture destination)
    {
        Render(destination);
    }
}
