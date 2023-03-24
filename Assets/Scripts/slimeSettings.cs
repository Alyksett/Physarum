using System.Collections;
using System.IO;
using System.Collections.Generic;

using UnityEngine;


[System.Serializable]
public class slimeSettings
{
    public int height = 720;
    public int width = 720;
    public int NUMAGENTS = 500000;
    public float speed = 0;
    public float blurSpread = 1.0f;
    public float blurAmountHeight = 3.0f;
    public float blurAmount = 1.0f;
    public float speciesOpacity = 1.0f;
    public float heightSpeciesOpacity = 1.0f;
    public float steerControl = 100f;
    public float accelerationScale = 8f;
    public float velocityScale = 8f;
    public float sensorDistance = 5f;
    public float maxSensorDistance = 60f;
    public float minSensorDistance = 1f;
    public float sensorOffset = 1f;
    public float foodPickupRate = 1f;
    public float rotateAmount = 1f;
    public float hungerDecay = 1f;
    public float sensorOffsets = 1f;
    public float turnAngle = 1f;
    public float noiseScale = 1f;
    public float specLine = .5f;
    public int drawRadius = 5;
    public bool useNoise = true;
    public bool useSensors = false;
    public bool useInertia = false;
    public bool useScaler = false;
    public bool useAcceleration = false;
    public bool useCircleSense = false;
    public bool reset = false;
    public bool saveCSV = false;
    public bool twoSpecies = false;
    private int noiseSeed = 0;
    
    public Color endingSpec = new Color(1, 0, 0, 1);
    public Color startingSpec = new Color(0, 1, 1, 0);
    public Color heightSpec1 = new Color(1, 0, 0, 0);
    public Color heightSpec2 = new Color(0, 1, 0, 0);
    public Color fieldColor = new Color(0, 1, 1, 0);
    public bool middleStart = false;
    public bool displayHeightMap = false;
    public bool randomColors = false;
    public slimeSettings(slimeSettings sets){
        this.height = sets.height;
        this.width = sets.width;
        this.NUMAGENTS = sets.NUMAGENTS;
        this.speed = sets.speed;
        this.blurSpread = sets.blurSpread;
        this.blurAmountHeight = sets.blurAmountHeight;
        this.blurAmount = sets.blurAmount;
        this.speciesOpacity = sets.speciesOpacity;
        this.heightSpeciesOpacity = sets.heightSpeciesOpacity;
        this.steerControl = sets.steerControl;
        this.velocityScale = sets.velocityScale;
        this.accelerationScale = sets.accelerationScale;
        this.sensorDistance = sets.sensorDistance;
        this.maxSensorDistance = sets.maxSensorDistance;
        this.minSensorDistance = sets.minSensorDistance;
        this.sensorOffset = sets.sensorOffset;
        this.foodPickupRate = sets.foodPickupRate;
        this.rotateAmount = sets.rotateAmount;
        this.hungerDecay = sets.hungerDecay;
        this.sensorOffsets = sets.sensorOffsets;
        this.turnAngle = sets.turnAngle;
        this.noiseScale = sets.noiseScale;
        this.specLine = sets.specLine;
        this.drawRadius = sets.drawRadius;
        this.useNoise = sets.useNoise;
        this.noiseSeed = sets.noiseSeed;
        this.endingSpec = sets.endingSpec;
        this.startingSpec = sets.startingSpec;
        this.heightSpec1 = sets.heightSpec1;
        this.heightSpec2 = sets.heightSpec2;
        this.fieldColor = sets.fieldColor;
        this.middleStart = sets.middleStart;
        this.displayHeightMap = sets.displayHeightMap;
        this.randomColors = sets.randomColors;
        this.useSensors = sets.useSensors;
        this.useInertia = sets.useInertia;
        this.useScaler = sets.useScaler;
        this.useAcceleration = sets.useAcceleration;
        this.useCircleSense = sets.useCircleSense;
        this.reset = sets.reset;
        this.saveCSV = sets.saveCSV;
        this.twoSpecies = sets.twoSpecies;
        
    }
    public slimeSettings(){}
}
