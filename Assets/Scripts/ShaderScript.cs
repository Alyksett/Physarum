using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderScript : MonoBehaviour 
{

    public int height = 1080;
    public int width = 1920;
    private int NUMAGENTS = 2073600;
    // private int NUMAGENTS = 1200000;
    public float speed = 0;
    public int brightness = 200;
    
    public float control1 = 1.0f;
    public float control2 = 1.0f;
    public float control3 = 0.0005f;
    public float steerControl = 100f;
    public float accelerationScale = 8f;
    

    public float R = 0.0f;
    public float G = 0.0f;
    public float B = 0.0f;
    public int clock = 1;
    private Agent[] agentList;
    public ComputeShader shader;
    private RenderTexture agentTexture;
    private RenderTexture trailTexture;

    private ComputeBuffer buffer;


    private int mainKernal;
    private int updateKernal;
    private int worldKernal;

    public struct Agent{
        public Vector2 pos;
        public Vector2 v;
        public Vector4 species; 
        public float speed;
        public float angle;
    }

    

    private void allocateSpace(){
        int positionSize = sizeof(float) * 2;
        int velocitySize = sizeof(float) * 2;
        int colorSize = sizeof(float) * 4;
        int speedSize = sizeof(float);
        int angleSize = sizeof(float);

        
        
        int totalSize = positionSize + velocitySize + colorSize + speedSize + angleSize; 

        buffer = new ComputeBuffer(agentList.Length, totalSize);
        buffer.SetData(agentList);
        shader.SetBuffer(mainKernal, "Agents", buffer);
        shader.SetBuffer(updateKernal, "Agents", buffer);

    }

    private void createAgents(){
        agentList = new Agent[NUMAGENTS];
        float circleDist = 100.0f;
        Vector2 center = new Vector2(width/2, height/2);
        for(int i = 0; i<NUMAGENTS; i++){
            Agent agent = new Agent();

            agent.pos = new Vector2(Random.Range(0, width), Random.Range(0, height));
            // agent.pos = new Vector2(Random.Range(0, width/2), Random.Range(0, height));
            // agent.pos = new Vector2(width/2, height/2);
            float x = Mathf.Cos(Random.Range(0, 6.2831f)) * circleDist;
            float y = Mathf.Sin(Random.Range(0, 6.2831f)) * circleDist;
            // agent.pos = new Vector2(x + center.x, y + center.y);



            agent.v = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
            agent.speed = 40f;
            agent.angle = Random.Range(0.0f,6.2831f);
            agent.species = new Vector4(1, 0, 0, 0);
            if(i%2==0){
                // agent.pos = new Vector2(Random.Range(width/2, width), Random.Range(0, height));
                agent.species = new Vector4(0, 0, 1, 0);
                agent.speed = 40f;
            }
            // if(i%3==0){
            //     // agent.pos = new Vector2(Random.Range(width/2, width), Random.Range(0, height));
            //     agent.species = new Vector4(0, 1, 0, 0);
            //     agent.speed = 40f;
            // }
            agentList[i] = agent;

        }
        
        
    }

    void Start()
    {
        mainKernal = shader.FindKernel("Main");
        updateKernal = shader.FindKernel("Update");


        agentTexture = new RenderTexture(width, height, 24);
        agentTexture.enableRandomWrite = true;
        agentTexture.Create();

        trailTexture = new RenderTexture(width, height, 24);
        trailTexture.enableRandomWrite = true;
        trailTexture.Create();

        
        shader.SetTexture(mainKernal, "AgentMap", agentTexture);
        shader.SetTexture(updateKernal, "AgentMap", agentTexture);
        shader.SetTexture(mainKernal, "TrailMap", trailTexture);
        shader.SetTexture(updateKernal, "TrailMap", trailTexture);

        shader.SetInt("HEIGHT", height);
        shader.SetInt("WIDTH", width);
        shader.SetInt("NumAgents", NUMAGENTS);
        createAgents();
        allocateSpace();

    }


    public void OnRenderImage(RenderTexture src, RenderTexture dest) {
        shader.SetFloat("DTime", Time.deltaTime);
        shader.SetInt("clock", clock);
        
        shader.SetFloat("speed", speed);
        
        shader.SetFloat("control1", control1);
        shader.SetFloat("control2", control2);
        shader.SetFloat("randControl", control3);
        shader.SetFloat("steerControl", steerControl);
        shader.SetFloat("accelerationScale", accelerationScale);

        shader.SetFloat("R", R);
        shader.SetFloat("G", G);
        shader.SetFloat("B", B);

        shader.SetTexture(mainKernal, "AgentMap", agentTexture);
        shader.SetTexture(updateKernal, "AgentMap", agentTexture);
        shader.SetTexture(mainKernal, "TrailMap", trailTexture);
        shader.SetTexture(updateKernal, "TrailMap", trailTexture);
        
        // buffer.SetData(agentList);
        
        shader.Dispatch(mainKernal, agentTexture.width/16, agentTexture.height/16, 1);
        shader.Dispatch(updateKernal, agentTexture.width/16, agentTexture.height/16, 1);

        buffer.GetData(agentList);
        // Debug.Log(agentList[100].angle);
        
        Graphics.Blit(trailTexture, dest);
        // Graphics.Blit(agentTexture, dest);
        // agentTexture.Release();
        
        clock++;
    }
}
