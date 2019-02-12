using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour {

    public Dropdown dropdownObject;
    public string playerPref;

    // Use this for initialization
    void Start () {
        int savedValue = getPlayerPref(playerPref);
        dropdownObject.value = savedValue;

        dropdownObject.onValueChanged.AddListener(delegate {
            onValueChanged(dropdownObject);
        });
    }

    public void onValueChanged(Dropdown dd)
    {
        PlayerPrefs.SetInt(playerPref, dd.value);
    }

    //defaultValue is -1 if value has never been saved to player preferences. Use -1 as a sanity check in any code dealing with loaded pref values.
    public static int getPlayerPref(string pref)
    {
        return PlayerPrefs.GetInt(pref, -1);   
    }

    public static float getPlayerPrefFloat(string pref)
    {
        return PlayerPrefs.GetFloat(pref, -1);
    }

    public static void setPlayerPrefFloat(string pref, float value)
    {
        PlayerPrefs.SetFloat(pref, value);
    }

    void OnDestroy()
    {
        dropdownObject.onValueChanged.RemoveAllListeners();
    }

	// Update is called once per frame
	void Update () {
	
	}
}
