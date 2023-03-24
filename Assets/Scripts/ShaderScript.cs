// using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using System.Text;
using System.Net;
using System.IO;
using System.Linq;
using UnityEngine;

using System.Net.Http;

using TMPro;



public class ShaderScript : MonoBehaviour 
{
    public settingsHandler handler;
    [SerializeField] public slimeSettings localSettings; 
    private int clock = 1;
    private int changeTimer = 1;
    private Agent[] agentList;
    private Cell[] cellList;
    public ComputeShader shader;
    private RenderTexture heightMap;
    private RenderTexture trailTexture;
    private RenderTexture speciesTexture;
    
    private ComputeBuffer buffer;
    private ComputeBuffer cellBuffer;
    private bool mouseState;
    private bool saveCSV;
    private Vector3 mousePos;


    private int mainKernal;
    private int updateKernal;
    private int heightMapKernal;
    private int layKernal;
    private int blurKernal;
    private int fillTextureKernal;



    public float timer, refresh, avgFramerate;
    string display = "{0} FPS";
    public TextMeshProUGUI m_Text;

    private bool fpsFlag = false;
    
    public struct Agent{
        public Vector2 pos;
        public Vector2 v;
        public Vector4 species; 
        public float speed;
        public float angle;
        public float sensorDist;
        public float foodState;
        public Vector4 speciesIndex;
        public int hasFood;
    }

    public struct Cell{
        public Vector2 vel;
        public float amount;
        public uint foodAmount;

        public Vector4 speciesDensity;
    }   

    private void allocateSpace(){
        int positionSize = sizeof(float) * 2;
        int velocitySize = sizeof(float) * 2;
        int colorSize = sizeof(float) * 4;
        int speedSize = sizeof(float);
        int angleSize = sizeof(float);
        int sensorDistSize = sizeof(float);
        int foodSize = sizeof(float);
        int speciesIndexSize = sizeof(float) * 4;
        int hasFoodFlagSize = sizeof(int);

        int cellVSize = sizeof(float) * 2;
        int cellSpecSize = sizeof(float) * 4;
        int cellSSize = sizeof(float);
        int foodAmountSize = sizeof(uint);
        
        int totalSize = positionSize + velocitySize + colorSize + speedSize + angleSize + sensorDistSize + foodSize + speciesIndexSize + hasFoodFlagSize; 
        int totalCellSize = cellVSize + cellSSize + foodAmountSize + cellSpecSize;


        buffer = new ComputeBuffer(agentList.Length, totalSize);
        buffer.SetData(agentList);
        shader.SetBuffer(mainKernal, "Agents", buffer);
        shader.SetBuffer(blurKernal, "Agents", buffer);
        shader.SetBuffer(updateKernal, "Agents", buffer);
        shader.SetBuffer(layKernal, "Agents", buffer);
        

        cellBuffer = new ComputeBuffer(cellList.Length, totalCellSize);
        cellBuffer.SetData(cellList);
        shader.SetBuffer(mainKernal, "Cells", cellBuffer);
        shader.SetBuffer(blurKernal, "Cells", cellBuffer);
        shader.SetBuffer(updateKernal, "Cells", cellBuffer);
        
    }

    private void setShaderData(){
        shader.SetFloat("speed", localSettings.speed);
        
        shader.SetFloat("control1", localSettings.blurSpread);
        shader.SetFloat("control2", localSettings.blurAmount);
        shader.SetFloat("maxSensorDistance", localSettings.maxSensorDistance);
        shader.SetFloat("minSensorDistance", localSettings.minSensorDistance);
        shader.SetFloat("sensorOffset", localSettings.sensorOffset);
        shader.SetFloat("blurAmountHeight", localSettings.blurAmountHeight);
        shader.SetFloat("speciesOpacity", localSettings.speciesOpacity);
        shader.SetFloat("heightSpeciesOpacity", localSettings.heightSpeciesOpacity);

        shader.SetFloat("steerControl", localSettings.steerControl);
        shader.SetFloat("accelerationScale", localSettings.accelerationScale);
        shader.SetFloat("velocityScale", localSettings.velocityScale);
        shader.SetFloat("sensorDistance", localSettings.sensorDistance);
        shader.SetFloat("rotateAmount", localSettings.rotateAmount);
        shader.SetFloat("hungerDecay", localSettings.hungerDecay);
        shader.SetFloat("foodPickupRate", localSettings.foodPickupRate);
        shader.SetFloat("sensorOffsets", localSettings.sensorOffsets);
        shader.SetFloat("turnAngle", localSettings.turnAngle);
        shader.SetFloat("specLine", localSettings.specLine);
        
        shader.SetFloat("noiseScale", localSettings.noiseScale);
        shader.SetInt("drawRadius", localSettings.drawRadius);
        
        shader.SetVector("startingSpecies", localSettings.startingSpec);
        shader.SetVector("endingSpecies", localSettings.endingSpec);
        shader.SetVector("heightSpec1", localSettings.heightSpec1);
        shader.SetVector("heightSpec2", localSettings.heightSpec2);
        shader.SetVector("fieldColor", localSettings.fieldColor);

        shader.SetBool("middleStart", localSettings.middleStart);
        shader.SetBool("useSensors", localSettings.useSensors);
        shader.SetBool("useInertia", localSettings.useInertia);
        shader.SetBool("useScaler", localSettings.useScaler);
        shader.SetBool("useAcceleration", localSettings.useAcceleration);
        shader.SetBool("useCircleSense", localSettings.useCircleSense);
        shader.SetBool("reset", localSettings.reset);
        
        shader.SetInt("WIDTH", localSettings.width);
        shader.SetInt("HEIGHT", localSettings.height);
        shader.SetFloat("DTime", Time.deltaTime);
        shader.SetInt("clock", clock);
    }

    private void createAgents(){
        agentList = new Agent[localSettings.NUMAGENTS];
        Vector2 center = new Vector2(localSettings.width/2, localSettings.height/2);
        for(int i = 0; i<localSettings.NUMAGENTS; i++){
            Agent agent = new Agent();

            agent.pos = new Vector2(Random.Range(0, localSettings.width), Random.Range(0, localSettings.height));
            float circleDist = 200.0f;
            float randLen = Random.Range(circleDist - 30, circleDist);
            // float randLen = circleDist;
            float randAngle = Random.Range(0, 6.2831f);
            float x = Mathf.Cos(randAngle) * randLen + localSettings.width/2;
            float y = Mathf.Sin(randAngle) * randLen + localSettings.height/2;
            float circleAngle = Mathf.Atan2(localSettings.height/2 - y, localSettings.width/2 - x);

            // agent.v = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
            agent.v = new Vector2(0,0);
            
            agent.speed = 40f;
            agent.angle = Random.Range(0.0f,6.2831f);
            
            agent.species = localSettings.startingSpec;
            agent.sensorDist = 15f;
            agent.speed = 40f;
            agent.foodState = 0;
            agent.speciesIndex = new Vector4(0, 0, 0, 0);

            if(localSettings.middleStart){
                // agent.pos = new Vector2(50, Random.Range(0, localSettings.height));
                // agent.pos = new Vector2(localSettings.width/2, localSettings.height/2);
                agent.pos = new Vector2(x, y);
                agent.angle = circleAngle;
                // agent.angle = 0.0f;
            }
            if(localSettings.twoSpecies){
                if(i % 2 == 0){
                    agent.species = localSettings.endingSpec;
                    agent.speciesIndex = new Vector4(-1, 0, 0, 0);
                }
            }
            
            agentList[i] = agent;
        }
        
    }

    private void createCells(){
        cellList = new Cell[localSettings.width*localSettings.height];
        for(int i = 0; i<localSettings.width*localSettings.height; i++){
            Vector2 v = new Vector2(0,0);
            Cell c = new Cell();
            c.vel = v;
            c.foodAmount = 0;
            cellList[i] = c;
        }
    }

    private Color[] generateColors(int am){
        Color[] res = new Color[am];
        for(int i = 0; i<am; i++){
            float r = Random.Range(0f, 1f);
            float g = Random.Range(0f, 1f);
            float b = Random.Range(0f, 1f);
            Color c = new Color(r, g, b);
            res[i] = c;
        }

        localSettings.startingSpec = res[0];
        // localSettings.endingSpec = new Vector4(1-col.r, 1-col.g, 1-col.b, 1-col.a);
        localSettings.endingSpec = new Vector4(
            1-localSettings.startingSpec.r,
            1-localSettings.startingSpec.g,
            1-localSettings.startingSpec.b,
            1-localSettings.startingSpec.a);
        return res;
    }

    void Start()
    {
        handler = new settingsHandler();
        handler.getStartingData();
        localSettings = handler.initFromJson();
        mainKernal = shader.FindKernel("Main");
        blurKernal = shader.FindKernel("blur");
        updateKernal = shader.FindKernel("Update");
        heightMapKernal = shader.FindKernel("heightMapBlur");
        layKernal = shader.FindKernel("lay");
        fillTextureKernal = shader.FindKernel("fillTexture");

        createTextures();

        float noiseSeed = Random.Range(0, 10000);
        shader.SetFloat("noiseSeed", noiseSeed);
        shader.SetInt("NumAgents", localSettings.NUMAGENTS);
        setShaderData();

        
        if(localSettings.randomColors){
            Color[] res = generateColors(2);
        }
        createAgents();
        createCells();
        allocateSpace();
        // updateUserSettings();
        m_Text.transform.position = new Vector3(localSettings.width, localSettings.height);
    }
    void createTextures(){
        heightMap = new RenderTexture(localSettings.width, localSettings.height, 24, RenderTextureFormat.ARGB32);
        heightMap.enableRandomWrite = true;
        heightMap.Create();

        trailTexture = new RenderTexture(localSettings.width, localSettings.height, 24, RenderTextureFormat.ARGB32);
        trailTexture.enableRandomWrite = true;
        trailTexture.Create();

        speciesTexture = new RenderTexture(localSettings.width, localSettings.height, 24);
        speciesTexture.enableRandomWrite = true;
        speciesTexture.Create();
        
        shader.SetTexture(mainKernal, "HeightMap", heightMap);
        shader.SetTexture(updateKernal, "HeightMap", heightMap);
        shader.SetTexture(layKernal, "HeightMap", heightMap);
        shader.SetTexture(blurKernal, "HeightMap", heightMap);

        shader.SetTexture(mainKernal, "TrailMap", trailTexture);
        shader.SetTexture(updateKernal, "TrailMap", trailTexture);
        shader.SetTexture(layKernal, "TrailMap", trailTexture);
        shader.SetTexture(blurKernal, "TrailMap", trailTexture);
        shader.SetTexture(fillTextureKernal, "TrailMap", trailTexture);

        shader.SetTexture(updateKernal, "SpeciesMap", speciesTexture);
        shader.SetTexture(updateKernal, "SpeciesMap", speciesTexture);
        shader.SetTexture(layKernal, "SpeciesMap", speciesTexture);

        shader.SetTexture(heightMapKernal, "HeightMap", heightMap);   
        
    }

    void restart(){
        this.localSettings = handler.getSettings();
        createTextures();
        createAgents();
        createCells();
        allocateSpace();
    }

    void Update(){
        mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mouseState = (Input.GetMouseButton(0));
        float timelapse = Time.smoothDeltaTime;
        timer = timer <= 0 ? refresh : timer -= timelapse;
 
        if(timer <= 0) avgFramerate = (int) (1f / timelapse);

        handleDataUpdates();
        
        if (Input.GetKeyDown("r") && clock%2 == 0){
            string path = "C:/Users/User 1/Desktop/Generative/Physarum/New/";
            SaveTexture(trailTexture, path);
            Debug.Log("Saved Image");
        }
        if (Input.GetKeyDown("p") && clock%2 == 0){
            resetPositions();

        }
        if (Input.GetKeyDown("b") && clock%2 == 0){
            string path = "C:/Users/User 1/Desktop/Code Stuff/ScratchFiles/imgs/";
            SaveTexture(heightMap, path);
            Debug.Log("Saved Image");
        }
        
        if(localSettings.saveCSV){
            if(clock%30 == 0){
                Debug.Log("Image Saved");
                string path = "C:/Users/User 1/Desktop/Code Stuff/ScratchFiles/imgs/";
                SaveTexture(heightMap, path);
                Debug.Log("Image Captured");
            }
        }
        
    }

    private void resetPositions(){
        for(int i = 0; i<localSettings.NUMAGENTS-1; i++){
            Agent a = agentList[i];
            a.pos = new Vector2(Random.Range(0, localSettings.width), Random.Range(0, localSettings.height));
            a.v = new Vector2(0.0f, 0.0f);
            agentList[i] = a;
        }
        buffer.SetData(agentList);
        GL.Clear(false, true, Color.clear);
    }

    

    public void SaveTexture (RenderTexture rt, string path) {
        
        var num = FindNextFileNumber(path);
        
        RenderTexture.active = rt;
		Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false, false);
		texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
		texture.Apply();
		byte[] bytes = texture.EncodeToJPG();
		UnityEngine.Object.Destroy(texture);
		System.IO.File.WriteAllBytes(path+"img"+num+".jpg", bytes);
    }

    public static string ConvertByteToString(byte[] source)
    {
        return source != null ? System.Text.Encoding.UTF8.GetString(source) : null;
    }

    string GetString(byte[] bytes)
    {
        char[] chars = new char[bytes.Length / sizeof(char)];
        System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
        return new string(chars);
    }

    int FindNextFileNumber(string folderPath)
    {
        // Get all the files in the specified folder
        string[] files = Directory.GetFiles(folderPath);

        // Initialize the greatest integer found to be 0
        int greatestInt = 0;

        // Loop through all the files
        foreach (string file in files)
        {
            // Get the file name without the extension
            string fName = Path.GetFileNameWithoutExtension(file);
            var builder = new StringBuilder();

            // Check if the file name ends with an integer
            if (int.TryParse(fName.Substring(fName.Length - 1), out int fileNumber))
            {
                int l = fName.Length;
                for(int i = l-1 ; i>=0 ; i--){
                    if(char.IsDigit(fName[i])){
                        builder.Insert(0, fName[i]);
                    }
                }   
            }
            int num = 0;
            var what = int.TryParse(builder.ToString(), out num);
            if(num > greatestInt){
                greatestInt = num;
            }
        }

        // Return the greatest integer found plus 1
        return greatestInt + 1;
    }

    private bool handleProfileSwitch(){
        KeyCode[] keyCodes = {
                KeyCode.Alpha1,
                KeyCode.Alpha2,
                KeyCode.Alpha3,
                KeyCode.Alpha4,
                KeyCode.Alpha5,
                KeyCode.Alpha6,
                KeyCode.Alpha7,
                KeyCode.Alpha8,
                KeyCode.Alpha9,
            };

        for(int i = 0 ; i < keyCodes.Length; i ++ ){
            if(Input.GetKeyDown(keyCodes[i])){
                int numberPressed = i+1;
                handler.switchProfiles(numberPressed);
                updateUserSettings();
                return true;
            }
        }
        return false;
    }

    public void updateUserSettings(){
        slimeSettings newSettings = handler.getSettings();
        if (handler.checkAgents(localSettings)){
            restart();
        }
            //true if different resolution
        if (handler.checkResolution(localSettings)){
            restart();
        }
        Debug.Log(localSettings.speed);
        localSettings = handler.getSettings();
        Debug.Log(localSettings.speed);
    }
    private void handleDataUpdates(){
        if (Input.GetKeyDown("s") && changeTimer < 3){
            handler.saveAsProfile(localSettings);
            changeTimer = 0;
        }

        if (Input.GetKeyDown("u") && changeTimer < 3){
            handler.updateCurrentProfileData(this.localSettings);
            Debug.Log(this.localSettings.speed + " FUCKFUCKFUCK");
            changeTimer = 0;
        }

        if (Input.GetKeyDown("f") && changeTimer < 3){
            fpsFlag = !fpsFlag;
            changeTimer = 0;
        }
   
        // if (Input.GetKeyDown("b") && changeTimer < 3){
        //     ConvertByteArrayToTextFile();
        // }
        
        handleProfileSwitch();
        
        if(fpsFlag){
            m_Text.text = string.Format(display,avgFramerate.ToString());
        }
        else{
            m_Text.text = "";
        }
    }
    public int thread = 64;


    public void OnRenderImage(RenderTexture src, RenderTexture dest) {
        setShaderData();
        Vector2 p = mousePos;
        p.x *= localSettings.width;
        p.y *= localSettings.height;
        shader.SetVector("mousePos", p);
        
        shader.SetBool("mouseActive", mouseState);
        shader.SetBool("useNoise", localSettings.useNoise);
        
        shader.SetTexture(mainKernal, "HeightMap", heightMap);
        shader.SetTexture(updateKernal, "HeightMap", heightMap);
        shader.SetTexture(heightMapKernal, "HeightMap", heightMap);
        shader.SetTexture(layKernal, "HeightMap", heightMap);
        shader.SetTexture(blurKernal, "HeightMap", heightMap);

        shader.SetTexture(mainKernal, "TrailMap", trailTexture);
        shader.SetTexture(updateKernal, "TrailMap", trailTexture);
        shader.SetTexture(layKernal, "TrailMap", trailTexture);
        shader.SetTexture(blurKernal, "TrailMap", trailTexture);
        shader.SetTexture(updateKernal, "SpeciesMap", speciesTexture);
        
        
        int div = 16;
        int textureThreadsX = heightMap.width/div;
        int textureThreadsY = heightMap.height/div;
        
        int updateThreads = 64;

        // shader.Dispatch(mainKernal, textureThreadsX, textureThreadsY, 1);

        shader.Dispatch(updateKernal, updateThreads, updateThreads, 1);
        shader.Dispatch(layKernal, updateThreads, updateThreads, 1);
        shader.Dispatch(heightMapKernal, textureThreadsX, textureThreadsY, 1);
        shader.Dispatch(blurKernal, textureThreadsX, textureThreadsY, 1);

        if(localSettings.displayHeightMap){
            Graphics.Blit(heightMap, dest);

            // if(clock%15==0){
            //     heightMap.Release();
            // }

        }
        else{
            Graphics.Blit(trailTexture, dest);
        }
        
        // int count = localSettings.NUMAGENTS;
        // for(int i = localSettings.NUMAGENTS-1; i>1; i--){
        //     if(agentList[i].hasFood != 100){
        //         count--;
        //     }
        // }    
        
        // Debug.Log(count);

        clock++;
        changeTimer++;
    }
}
