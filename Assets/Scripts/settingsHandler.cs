using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

using UnityEngine;


public class settingsHandler
{   

    [NonSerialized] private slimeSettings settings;
    private int currentProfile;
    
    public struct userData{
        public string favoriteProfile;
    }
    public userData userDat;

    public settingsHandler(){

    }

    public slimeSettings onStart(){
        getStartingData();
        slimeSettings sets = initFromJson();
        return sets;
    }

    public slimeSettings initFromJson(){
        string json = "";
        string profile = userDat.favoriteProfile;
        currentProfile = Int32.Parse(userDat.favoriteProfile);
        
        try{
            json = File.ReadAllText("Assets/Settings/Profiles/Profile" + profile + ".txt");
        }
        catch {
            Debug.Log("File not found... creating new file");
            json = File.ReadAllText("Assets/Settings/Profiles/Profile1.txt");        
        }
        settings = JsonUtility.FromJson<slimeSettings>(json);
        settings = new slimeSettings(settings);
        Debug.Log("Profile " + profile + " loaded.");
        return new slimeSettings(settings);
    }

    
    public void getStartingData(){
        string json = "";
        json = File.ReadAllText("Assets/Settings/UserData/userDat.txt");
        userDat = JsonUtility.FromJson<userData>(json);
    }

    public void saveAsProfile(slimeSettings newSettings){
        string settingsToJson = JsonUtility.ToJson(newSettings, true);
        int latestProfile = getLatestProfile() + 1;
        currentProfile = latestProfile;
        string path = "Assets/Settings/Profiles/Profile" + latestProfile + ".txt";
        File.WriteAllText(path, settingsToJson);
        Debug.Log("Saved new profile.");
    }

    public void updateCurrentProfileData(slimeSettings newSettings){
        string settingsToJson = JsonUtility.ToJson(newSettings, true);
        string path = "Assets/Settings/Profiles/Profile" + currentProfile + ".txt";
        File.WriteAllText(path, settingsToJson);
        Debug.Log("Saved current settings to profile." + currentProfile);
    }

    private int getLatestProfile(){
        string targetDir = "Assets/Settings/Profiles/";
        string modelName = "Profile*.txt";
        string[] files = Directory.GetFiles(targetDir, modelName);

        int latestProf = files.Length;
        return latestProf;
    }

    private slimeSettings getProfile(string path){
        slimeSettings profile = new slimeSettings();
        string json = "";
        try{
            json = File.ReadAllText(path);
        }
        catch(FileNotFoundException){
            return null;
        }
        profile = JsonUtility.FromJson<slimeSettings>(json);
        return profile;
    }

    public void switchProfiles(int option){
        string path = "Assets/Settings/Profiles/Profile" + option + ".txt";
        slimeSettings profile = getProfile(path);
        if(profile == null){
            Debug.Log("Profile doesn't exist");
            return;
        }
        settings = profile;
        currentProfile = option;
        Debug.Log("Current profile: " + currentProfile);
    }

    public slimeSettings getSettings(){
        return settings;
    }
    public bool checkAgents(slimeSettings sets){
        return (sets.NUMAGENTS != settings.NUMAGENTS);
    }
    public bool checkResolution(slimeSettings sets){
        return (sets.height != settings.height || sets.width != settings.width);
    }
}
